// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#pragma once
#include "../../../deps/Recast/include/Recast.h"
#include "../../../deps/NativePath/TINYSTL/vector.h"

class NavigationBuilder
{
	rcHeightfield* m_solid = nullptr;
	uint8_t* m_triareas = nullptr;
	rcCompactHeightfield* m_chf = nullptr;
	rcContourSet* m_cset = nullptr;
	rcPolyMesh* m_pmesh = nullptr;
	rcPolyMeshDetail* m_dmesh = nullptr;
	BuildSettings m_buildSettings;
	rcContext* m_context;

	// Calculated navmesh vertices
	tinystl::vector<Vector3> m_navmeshVertices;
	// Detour returned navigation mesh data
	// free with dtFree()
	uint8_t* m_navmeshData = nullptr;
	int m_navmeshDataLength = 0;

	GeneratedData m_result;
public:
	NavigationBuilder();
	~NavigationBuilder();
	void Cleanup();
	GeneratedData* BuildNavmesh(Vector3* vertices, int numVertices, int* indices, int numIndices);
	void SetSettings(BuildSettings buildSettings);

private:
	void GenerateNavMeshVertices();
	bool CreateDetourMesh();
};
