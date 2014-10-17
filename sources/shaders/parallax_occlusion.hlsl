[Stream("Input", "Vertex")]
struct VS_INPUT
{
    float3 vNormal : NORMAL;
    float3 vTangent : TANGENT;
    float3 vBinormal : BINORMAL;
};

[Stream("Output", "Vertex")]
[Stream("Input", "Pixel")]
struct VS_OUTPUT
{        
    float3 vViewTS : VIEW_TS;
    float2 vParallaxOffsetTS : PARALLAX_OFFSET_TS;
    float3 vNormalWS : NORMAL_WS;
    float3 vViewWS : VIEW_WS;
};

cbuffer PerObject
{
    [Link("POM.HeightMapScale")] float g_fHeightMapScale;
    [Link("World")] matrix g_mWorld;
    [Link("Eye")] float4 g_vEye;
};

[PlaceHolderDestination("VSMain")]
void VSMerge(in VS_INPUT input, out VS_OUTPUT output)
{
    float3 vNormalWS = normalize(mul(input.vNormal, (float3x3)g_mWorld));
    float3 vTangentWS = normalize(mul(input.vTangent, (float3x3)g_mWorld));
    float3 vBinormalWS = normalize(mul(input.vBinormal, (float3x3)g_mWorld));
    output.vNormalWS = vNormalWS;
    float4 vPositionWS = mul(input.vPosition, g_mWorld);
    output.vViewWS = g_vEye - vPositionWS;
    float3x3 mWorldToTangent = float3x3(vTangentWS, vBinormalWS, vNormalWS);
    output.vViewTS  = mul(mWorldToTangent, output.vViewWS);
    float2 vParallaxDirection = normalize(output.vViewTS.xy);
    float fLength = length( output.vViewTS );
    float fParallaxLength = sqrt( fLength * fLength - output.vViewTS.z * output.vViewTS.z ) / output.vViewTS.z;
    output.vParallaxOffsetTS = vParallaxDirection * fParallaxLength;
    output.vParallaxOffsetTS *= g_fHeightMapScale;
}

cbuffer POM
{
    [Link("POM.TextureDims")] float4   g_vTextureDims;            // Specifies texture dimensions for computation of mip level at 
                                        // render time (width, height)
    [Link("POM.LODThreshold")] int      g_nLODThreshold;           // The mip level id for transitioning between the full computation
                                        // for parallax occlusion mapping and the bump mapping computation
    [Link("POM.MinSamples")] int      g_nMinSamples;             // The minimum number of samples for sampling the height field profile
    [Link("POM.MaxSamples")] int      g_nMaxSamples;             // The maximum number of samples for sampling the height field profile
};

[Link("HeightSampler")]
SamplerState normalMapSampler {};
[Link("HeightTexture")]
Texture2D normalMapTexture;

[PlaceHolderDestination("PSMain")]
void PSMerge(in VS_OUTPUT input, out PS_OUTPUT output)
{
    //  Normalize the interpolated vectors:
    float3 vViewTS   = normalize( input.vViewTS  );
    float3 vViewWS   = normalize( input.vViewWS  );
    //float3 vLightTS  = normalize( input.vLightTS );
    float3 vNormalWS = normalize( input.vNormalWS );
     
    float4 cResultColor = float4( 0, 0, 0, 1 );

    // Adaptive in-shader level-of-detail system implementation. Compute the 
    // current mip level explicitly in the pixel shader and use this information 
    // to transition between different levels of detail from the full effect to 
    // simple bump mapping. See the above paper for more discussion of the approach
    // and its benefits.
   
    // Compute the current gradients:
    float2 fTexCoordsPerSize = input.vTexcoord * g_vTextureDims;

    // Compute all 4 derivatives in x and y in a single instruction to optimize:
    float2 dxSize;
    float2 dySize;
    float2 dx;
    float2 dy;

    float4( dxSize, dx ) = ddx( float4( fTexCoordsPerSize, input.vTexcoord ) );
    float4( dySize, dy ) = ddy( float4( fTexCoordsPerSize, input.vTexcoord ) );
                  
    float  fMipLevel;      
    float  fMipLevelInt;    // mip level integer portion
    float  fMipLevelFrac;   // mip level fractional amount for blending in between levels

    float  fMinTexCoordDelta;
    float2 dTexCoords;

    // Find min of change in u and v across quad: compute du and dv magnitude across quad
    dTexCoords = dxSize * dxSize + dySize * dySize;

    // Standard mipmapping uses max here
    fMinTexCoordDelta = max( dTexCoords.x, dTexCoords.y );

    // Compute the current mip level  (* 0.5 is effectively computing a square root before )
    fMipLevel = max( 0.5 * log2( fMinTexCoordDelta ), 0 );
    
    // Start the current sample located at the input texture coordinate, which would correspond
    // to computing a bump mapping result:
    float2 texSample = input.vTexcoord;
   
    // Multiplier for visualizing the level of detail (see notes for 'nLODThreshold' variable
    // for how that is done visually)
    float4 cLODColoring = float4( 1, 1, 3, 1 );

    float fOcclusionShadow = 1.0;

    if ( fMipLevel <= (float) g_nLODThreshold )
    {
        //===============================================//
        // Parallax occlusion mapping offset computation //
        //===============================================//

        // Utilize dynamic flow control to change the number of samples per ray 
        // depending on the viewing angle for the surface. Oblique angles require 
        // smaller step sizes to achieve more accurate precision for computing displacement.
        // We express the sampling rate as a linear function of the angle between 
        // the geometric normal and the view direction ray:
        int nNumSteps = (int) lerp( g_nMaxSamples, g_nMinSamples, dot( vViewWS, vNormalWS ) );

        // Intersect the view ray with the height field profile along the direction of
        // the parallax offset ray (computed in the vertex shader. Note that the code is
        // designed specifically to take advantage of the dynamic flow control constructs
        // in HLSL and is very sensitive to specific syntax. When converting to other examples,
        // if still want to use dynamic flow control in the resulting assembly shader,
        // care must be applied.
        // 
        // In the below steps we approximate the height field profile as piecewise linear
        // curve. We find the pair of endpoints between which the intersection between the 
        // height field profile and the view ray is found and then compute line segment
        // intersection for the view ray and the line segment formed by the two endpoints.
        // This intersection is the displacement offset from the original texture coordinate.
        // See the above paper for more details about the process and derivation.
        //

        float fCurrHeight = 0.0;
        float fStepSize   = 1.0 / (float) nNumSteps;
        float fPrevHeight = 1.0;
        float fNextHeight = 0.0;

        int    nStepIndex = 0;
        bool   bCondition = true;

        float2 vTexOffsetPerStep = fStepSize * input.vParallaxOffsetTS;
        float2 vTexCurrentOffset = input.vTexcoord;
        float  fCurrentBound     = 1.0;
        float  fParallaxAmount   = 0.0;

        float2 pt1 = 0;
        float2 pt2 = 0;
       
        float2 texOffset2 = 0;

        while ( nStepIndex < nNumSteps ) 
        {
            vTexCurrentOffset -= vTexOffsetPerStep;

            // Sample height map which in this case is stored in the alpha channel of the normal map:
            fCurrHeight = normalMapTexture.SampleGrad(normalMapSampler, vTexCurrentOffset, dx, dy ).a;

            fCurrentBound -= fStepSize;

            if ( fCurrHeight > fCurrentBound ) 
            {   
            pt1 = float2( fCurrentBound, fCurrHeight );
            pt2 = float2( fCurrentBound + fStepSize, fPrevHeight );

            texOffset2 = vTexCurrentOffset - vTexOffsetPerStep;

            nStepIndex = nNumSteps + 1;
            fPrevHeight = fCurrHeight;
            }
            else
            {
            nStepIndex++;
            fPrevHeight = fCurrHeight;
            }
        }   

        float fDelta2 = pt2.x - pt2.y;
        float fDelta1 = pt1.x - pt1.y;
      
        float fDenominator = fDelta2 - fDelta1;
      
        // SM 3.0 requires a check for divide by zero, since that operation will generate
        // an 'Inf' number instead of 0, as previous models (conveniently) did:
        if ( fDenominator == 0.0f )
        {
            fParallaxAmount = 0.0f;
        }
        else
        {
            fParallaxAmount = (pt1.x * fDelta2 - pt2.x * fDelta1 ) / fDenominator;
        }
      
        float2 vParallaxOffset = input.vParallaxOffsetTS * (1 - fParallaxAmount );

        // The computed texture offset for the displaced point on the pseudo-extruded surface:
        float2 texSampleBase = input.vTexcoord - vParallaxOffset;
        texSample = texSampleBase;

        // Lerp to bump mapping only if we are in between, transition section:
        
        cLODColoring = float4( 1, 1, 1, 1 ); 

        if ( fMipLevel > (float)(g_nLODThreshold - 1) )
        {
            // Lerp based on the fractional part:
            fMipLevelFrac = modf( fMipLevel, fMipLevelInt );

            /*if ( g_bVisualizeLOD )
            {
            // For visualizing: lerping from regular POM-resulted color through blue color for transition layer:
            cLODColoring = float4( 1, 1, max( 1, 2 * fMipLevelFrac ), 1 ); 
            }*/

            // Lerp the texture coordinate from parallax occlusion mapped coordinate to bump mapping
            // smoothly based on the current mip level:
            texSample = lerp( texSampleBase, input.vTexcoord, fMipLevelFrac );

        }  
      
        /*if ( g_bDisplayShadows == true )
        {
        float2 vLightRayTS = vLightTS.xy * g_fHeightMapScale;
      
        // Compute the soft blurry shadows taking into account self-occlusion for 
        // features of the height field:
   
        float sh0 =  tex2Dgrad( tNormalHeightMap, texSampleBase, dx, dy ).a;
        float shA = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.88, dx, dy ).a - sh0 - 0.88 ) *  1 * g_fShadowSoftening;
        float sh9 = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.77, dx, dy ).a - sh0 - 0.77 ) *  2 * g_fShadowSoftening;
        float sh8 = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.66, dx, dy ).a - sh0 - 0.66 ) *  4 * g_fShadowSoftening;
        float sh7 = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.55, dx, dy ).a - sh0 - 0.55 ) *  6 * g_fShadowSoftening;
        float sh6 = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.44, dx, dy ).a - sh0 - 0.44 ) *  8 * g_fShadowSoftening;
        float sh5 = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.33, dx, dy ).a - sh0 - 0.33 ) * 10 * g_fShadowSoftening;
        float sh4 = (tex2Dgrad( tNormalHeightMap, texSampleBase + vLightRayTS * 0.22, dx, dy ).a - sh0 - 0.22 ) * 12 * g_fShadowSoftening;
   
        // Compute the actual shadow strength:
        fOcclusionShadow = 1 - max( max( max( max( max( max( shA, sh9 ), sh8 ), sh7 ), sh6 ), sh5 ), sh4 );
      
        // The previous computation overbrightens the image, let's adjust for that:
        fOcclusionShadow = fOcclusionShadow * 0.6 + 0.4;         
        }*/
    }

    // Compute resulting color for the pixel:
    //cResultColor = ComputeIllumination( texSample, vLightTS, vViewTS, fOcclusionShadow );
    float3 vNormalTS = normalize(normalMapTexture.Sample(normalMapSampler, texSample) * 2 - 1);
    float4 cBaseColor = mainTexture.Sample(mainSampler, texSample);
    output.vColor = cBaseColor;
              
    //if ( g_bVisualizeLOD )
    //{
    //   cResultColor *= cLODColoring;
    //}
   
    // Visualize currently computed mip level, tinting the color blue if we are in 
    // the region outside of the threshold level:
    //if ( g_bVisualizeMipLevel )
    //{
    //   cResultColor = fMipLevel.xxxx;      
    //}   
}
