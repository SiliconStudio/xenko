group HS
{
	struct HS_INPUT
	{
	};

	struct HS_CONSTANT_DATA_OUTPUT
	{
	};

	struct HS_CONTROL_POINT_OUTPUT
	{
	};

	HS_CONSTANT_DATA_OUTPUT ConstantsHS( InputPatch<HS_INPUT, 3> p, uint PatchID : SV_PrimitiveID )
	{
	}

	[domain("tri")]
	[partitioning("fractional_odd")]
	[outputtopology("triangle_cw")]
	[outputcontrolpoints(3)]
	[patchconstantfunc("ConstantsHS")]
	[maxtessfactor(15.0)]
	HS_CONTROL_POINT_OUTPUT HS( InputPatch<HS_INPUT, 3> inputPatch, uint uCPID : SV_OutputControlPointID )
	{
	}
}

group DS
{
	struct DS_OUTPUT
	{
	};

	[domain("tri")]
	DS_OUTPUT DS( HS_CONSTANT_DATA_OUTPUT input, float3 BarycentricCoordinates : SV_DomainLocation, 
				 const OutputPatch<HS_CONTROL_POINT_OUTPUT, 3> TrianglePatch )
	{
	}
}
