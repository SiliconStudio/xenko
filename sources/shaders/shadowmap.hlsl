[Stream("Input", "Vertex")]
[Stream("Output", "Vertex")]
struct VS_OUTPUT
{
    float4 vShadowMapPos : SHADOWMAP_POS;
};

cbuffer PerFrame
{
    [Link("ShadowMap.WorldViewProjection")] matrix g_mShadowMapVP;
};

void VSMerge(in VS_INPUT input, out VS_OUTPUT output)
{
    output.vShadowMapPos = mul(input.vPosition, g_mShadowMapVP);
}

[Link("ShadowMap.Sampler")]
SamplerComparisonState shadowMapSampler
{
    ComparisonFunc = Less;
    Filter = COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
    AddressU = Border; AddressV = Border;
    BorderColor = float4(1,1,1,1);
};

[Stream("Output", "Pixel")]
struct PS_OUTPUT
{
    float4 vColor : SV_Target;
};

[Link("ShadowMap.Texture")]
Texture2D shadowMapTexture;

void PSMerge(in VS_OUTPUT input, out PS_OUTPUT output)
{
    output.vColor.rgb *= shadowMapTexture.SampleCmpLevelZero(shadowMapSampler,
        ((input.vShadowMapPos.xy/input.vShadowMapPos.w) * float2(0.5f, -0.5f)) + 0.5f, (input.vShadowMapPos.z - 0.005f)/input.vShadowMapPos.w).r;
}
