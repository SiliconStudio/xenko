group GS
{
    static const float EPSILON = 1e-30;

	cbuffer PerFrame
	{
		[Link("PickPosition")] float2 g_pickPosition;
	};

	cbuffer PerMesh
	{
		[Link("MeshId")] int meshId;
	};

	[Stream("Input", "Geometry")]
	struct GS_INPUT
	{
		float4 vPosition : SV_POSITION;
	};

	[Stream("Output", "Geometry")]
	struct GS_OUTPUT
	{
		int hit : HITTEST;
	};

	[EntryPoint("Geometry")]
    [maxvertexcount(3)]
    void GS(triangle GS_INPUT input[3], inout PointStream<GS_OUTPUT> ptStream)
    {
		float3 pickPosition = float3(g_pickPosition.xy, 0.0f);

		float3 pos[3];

		// Clip any polygon that is behind projection near plane
		if (input[0].vPosition.z < 0.0f
			|| input[1].vPosition.z < 0.0f
			|| input[2].vPosition.z < 0.0f)
			return;

		// Divide by W component to get screen position
		pos[0] = float3(input[0].vPosition.xy / input[0].vPosition.w, 0.0f);
		pos[1] = float3(input[1].vPosition.xy / input[1].vPosition.w, 0.0f);
		pos[2] = float3(input[2].vPosition.xy / input[2].vPosition.w, 0.0f);

		// 2D Point in Triangle test,
		// Explanations: http://www.mochima.com/articles/cuj_geometry_article/cuj_geometry_article.html
		float3 o1 = sign(cross(pickPosition - pos[0], pos[1] - pos[0]));
		float3 o2 = sign(cross(pickPosition - pos[1], pos[2] - pos[1]));
		float3 o3 = sign(cross(pickPosition - pos[2], pos[0] - pos[2]));

		if (any(o1 != o2) || any(o1 != o3))
			return;

		// If test is passed, output triangle
		// TODO: We might want more information (intersection point?)
		GS_OUTPUT output;
		output.hit = meshId;
		ptStream.Append(output);
        ptStream.RestartStrip();
    }
}
