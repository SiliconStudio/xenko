// From http://www.gamedev.net/reference/programming/features/simpleSSAO/ (MIT license)

group PSLightingSSAO
{
    [Link("SSAO.Texture")]
    Texture2D ssaoTexture;

    [PlaceHolderDestination("LightingFactor")]
    void PSMerge(in PS_INPUT input, out float3 lighting)
    {
        {
            float ssaoValue = ssaoTexture.Sample(mainSampler, input.vPosition.xy / float2(1024.0, 768.0));
            lighting *= ssaoValue.xxx;
        }
    }
}

group VS
{
	[Stream("Input", "Vertex")]
    struct VS_INPUT
    {
        float4 vPosition : POSITION;
        float2 vTexcoord : TEXCOORD0;
    };

	[Stream("Output", "Vertex")]
    struct VS_OUTPUT
    {
        float4 vPosition : SV_POSITION;
        float2 vTexcoord : TEXCOORD0;
    };

    [EntryPoint("Vertex")]
    VS_OUTPUT VSMain(in VS_INPUT input, out VS_OUTPUT output)
    {
        VS_OUTPUT result;
        output.vPosition = input.vPosition;
        output.vPosition.x = -output.vPosition.x;
        output.vTexcoord = input.vTexcoord;
        return output;
    }
}

group PS
{
	[Stream("Input", "Pixel")]
    struct PS_INPUT
    {
        float4 vPosition : SV_POSITION;
        float2 vTexcoord : TEXCOORD0;
    };

    cbuffer PerFrameSSAO
    {
        [Link("ProjScreenRay")] float2 g_mProjScreenRay;
        [Link("ScreenSize")] float2 g_screenSize;
        [Link("SSAO.RandomTextureSize")] float g_randomTextureSize;
        [Link("SSAO.SamplingRadius")] float g_samplingRadius = 5.0f;
        [Link("SSAO.Intensity")] float g_intensity = 4.0f;
        [Link("SSAO.Scale")] float g_scale = 0.1f;
        [Link("SSAO.Bias")] float g_bias = 0.05f;
        [Link("SSAO.SelfOcclusion")] float g_selfOcclusion = 0.3f;
    };

    [Link("RandomSampler")]
    SamplerState randomSampler
    {
        AddressU = WRAP; AddressV = WRAP;
    };

    [Link("GBufferSampler")]
    SamplerState gBufferSampler {  };

    [Link("GBufferTexture")]
    Texture2D gBufferTexture;

    [Link("RandomTexture")]
    Texture2D randomTexture;

    [Link("ForwardTexture")]
    Texture2D mainTexture;

    float2 getRandom(in float2 uv)
    {
        return normalize(randomTexture.Sample(randomSampler, g_screenSize * uv / g_randomTextureSize).xy * 2.0f - 1.0f);
    }

    float3 getPosition(float2 uv)
    {
        float depth = gBufferTexture.Sample(gBufferSampler, uv).w;
        return float3((1.0 - uv * 2.0) * g_mProjScreenRay, 1.0f) * depth;
    }

    float doAmbientOcclusion(in float2 tcoord,in float2 uv, in float3 p, in float3 cnorm)
    {
        float3 diff = getPosition(tcoord + uv) - p;
        const float3 v = normalize(diff);
        const float d = length(diff)*g_scale;
        return max(0.0 - g_selfOcclusion, dot(cnorm,v) - g_bias) * (1.0 / (1.0 + d)) * g_intensity;
    }

    [EntryPoint("Pixel")]
    float4 PSMain(PS_INPUT input) : SV_Target
    {
        float4 gBufferValue = gBufferTexture.Sample(gBufferSampler, input.vTexcoord);

        // Extract values from GBuffer
        float3 normal = gBufferValue.xyz * 2.0f - 1.0f;
        float depth = gBufferValue.w;
        float3 position = float3((1.0 - input.vTexcoord * 2.0) * g_mProjScreenRay, 1.0f) * depth;
        
        float2 rand = getRandom(input.vTexcoord);

        float ao = 0.0f;
        float rad = g_samplingRadius / position.z;

        // SSAO
        const float2 vec[4] = { float2(1,0), float2(-1,0), float2(0,1), float2(0,-1) };
        int iterations = 4;
        [unroll]
        for (int j = 0; j < iterations; ++j)
        {
            float2 coord1 = reflect(vec[j], rand) * rad;
            float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707, coord1.x*0.707 + coord1.y*0.707);
  
            ao += doAmbientOcclusion(input.vTexcoord, coord1 * 0.25, position, normal);
            ao += doAmbientOcclusion(input.vTexcoord, coord2 * 0.5, position, normal);
            ao += doAmbientOcclusion(input.vTexcoord, coord1 * 0.75, position, normal);
            ao += doAmbientOcclusion(input.vTexcoord, coord2, position, normal);
        }
        ao /= (float)iterations * 4.0;

        ao += g_selfOcclusion;

        return float4(1.0 - ao.xxxx);
    }
}

group VS
{
	[Stream("Input", "Vertex")]
    struct VS_INPUT
    {
        float4 vPosition : POSITION;
        float2 vTexcoord : TEXCOORD0;
    };

	[Stream("Output", "Vertex")]
	[Stream("Input", "Pixel")]
    struct VS_OUTPUT
    {
        float4 vPosition : SV_POSITION;
        float2 vTexcoord : TEXCOORD0;
    };

    [EntryPoint("Vertex")]
    VS_OUTPUT VSMain(in VS_INPUT input, out VS_OUTPUT output)
    {
        VS_OUTPUT result;
        output.vPosition = input.vPosition;
        output.vPosition.x = -output.vPosition.x;
        output.vTexcoord = input.vTexcoord;
        return output;
    }

    cbuffer PerFrameSSAO
    {
        [Link("ProjScreenRay")] float2 g_mProjScreenRay;
        [Link("ScreenSize")] float2 g_screenSize;
        [Link("SSAO.RandomTextureSize")] float g_randomTextureSize;
        [Link("SSAO.SamplingRadius")] float g_samplingRadius;
        [Link("SSAO.Intensity")] float g_intensity;
        [Link("SSAO.Scale")] float g_scale;
        [Link("SSAO.Bias")] float g_bias;
        [Link("SSAO.SelfOcclusion")] float g_selfOcclusion;
    };

    [Link("RandomSampler")]
    SamplerState randomSampler
    {
        AddressU = WRAP; AddressV = WRAP;
    };

    [Link("GBufferSampler")]
    SamplerState gBufferSampler {  };

    [Link("GBufferTexture")]
    Texture2D gBufferTexture;

    [Link("RandomTexture")]
    Texture2D randomTexture;

    [Link("ForwardTexture")]
    Texture2D mainTexture;

    float2 getRandom(in float2 uv)
    {
        return normalize(randomTexture.Sample(randomSampler, g_screenSize * uv / g_randomTextureSize).xy * 2.0f - 1.0f);
    }

    float3 getPosition(float2 uv)
    {
        float depth = gBufferTexture.Sample(gBufferSampler, uv).w;
        return float3((1.0 - uv * 2.0) * g_mProjScreenRay, 1.0f) * depth;
    }

    float doAmbientOcclusion(in float2 tcoord,in float2 uv, in float3 p, in float3 cnorm)
    {
        float3 diff = getPosition(tcoord + uv) - p;
        const float3 v = normalize(diff);
        const float d = length(diff)*g_scale;
        return max(0.0 - g_selfOcclusion, dot(cnorm,v) - g_bias) * (1.0 / (1.0 + d)) * g_intensity;
    }

    [EntryPoint("Pixel")]
    float4 PSMain(VS_OUTPUT input) : SV_Target
    {
        float4 gBufferValue = gBufferTexture.Sample(gBufferSampler, input.vTexcoord);

        // Extract values from GBuffer
        float3 normal = gBufferValue.xyz * 2.0f - 1.0f;
        float depth = gBufferValue.w;
        float3 position = float3((1.0 - input.vTexcoord * 2.0) * g_mProjScreenRay, 1.0f) * depth;
        
        float2 rand = getRandom(input.vTexcoord);

        float ao = 0.0f;
        float rad = g_samplingRadius / position.z;

        // SSAO
        const float2 vec[4] = { float2(1,0), float2(-1,0), float2(0,1), float2(0,-1) };
        int iterations = 4;
        [unroll]
        for (int j = 0; j < iterations; ++j)
        {
            float2 coord1 = reflect(vec[j], rand) * rad;
            float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707, coord1.x*0.707 + coord1.y*0.707);
  
            ao += doAmbientOcclusion(input.vTexcoord, coord1 * 0.25, position, normal);
            ao += doAmbientOcclusion(input.vTexcoord, coord2 * 0.5, position, normal);
            ao += doAmbientOcclusion(input.vTexcoord, coord1 * 0.75, position, normal);
            ao += doAmbientOcclusion(input.vTexcoord, coord2, position, normal);
        }
        ao /= (float)iterations * 4.0;

        ao += g_selfOcclusion;

        return float4(1.0 - ao.xxxx);
    }
}