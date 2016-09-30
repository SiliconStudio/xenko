// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#pragma once
#include "../../../../deps/Recast/include/DetourNavMesh.h"
#include "../../../../deps/Recast/include/DetourNavMeshQuery.h"

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
	tinystl::vector<Vector3> pathPoints;

	// Temporary navigation result
	NavmeshQueryResult m_result;

	// Stored navmesh data
	uint8_t* m_data = nullptr;
	int m_dataLength;
public:
	~Navmesh();
	bool Load(uint8_t* navData, int navDataLength);
	NavmeshQueryResult* Query(NavmeshQuery query);
};