// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#pragma once
#include "../../../../deps/Recast/include/DetourNavMesh.h"
#include "../../../../deps/Recast/include/DetourNavMeshQuery.h"
#include "../../../../deps/NativePath/TINYSTL/unordered_set.h"

#pragma pack(4)
struct NavmeshQuery
{
	Vector3 source;
	Vector3 target;
};
struct NavmeshQueryResult
{
	bool pathFound = false;
	Vector3* pathPoints = nullptr;
	int numPathPoints = 0;
};

class Navmesh
{
private:
	dtNavMesh* m_navMesh = nullptr;
	dtNavMeshQuery* m_navQuery = nullptr;
	tinystl::vector<Vector3> m_pathPoints;
	tinystl::unordered_set<dtTileRef> m_tileRefs;

	// Temporary navigation result
	NavmeshQueryResult m_result;
public:
	Navmesh();
	~Navmesh();
	bool Init(float cellTileSize);
	bool LoadTile(Point tileCoordinate, uint8_t* navData, int navDataLength);
	bool RemoveTile(Point tileCoordinate);
	NavmeshQueryResult* Query(NavmeshQuery query);
};