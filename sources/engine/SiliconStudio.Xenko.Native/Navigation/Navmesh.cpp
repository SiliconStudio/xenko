// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../../deps/Recast/include/Recast.h"
#include "../XenkoNative.h"

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/TINYSTL/vector.h"
#include "Navigation.hpp"
#include "Navmesh.hpp"

Navmesh::~Navmesh()
{
	if(m_navQuery)
		dtFreeNavMeshQuery(m_navQuery);
	if(m_navMesh)
		dtFreeNavMesh(m_navMesh);
}

bool Navmesh::Load(uint8_t* navData, int navDataLength)
{
	if (!navData)
		return false;

	// Copy data
	m_data = new uint8_t[m_dataLength = navDataLength];
	memcpy(m_data, navData, m_dataLength);

	// Allocate objects
	m_navMesh = dtAllocNavMesh();
	m_navQuery = dtAllocNavMeshQuery();
	if(!m_navMesh || !m_navQuery)
	{
		return false;
	}

	// Initialize a single tile navmesh
	dtStatus status;
	status = m_navMesh->init(m_data, navDataLength, DT_TILE_FREE_DATA);
	if (dtStatusFailed(status))
	{
		return false;
	}

	// Initialize the query object
	status = m_navQuery->init(m_navMesh, 2048);
	if (dtStatusFailed(status))
	{
		return false;
	}

	return true;
}

NavmeshQueryResult* Navmesh::Query(NavmeshQuery query)
{
	// Reset result
	m_result = NavmeshQueryResult();
	NavmeshQueryResult* res = &m_result;
	dtPolyRef startPoly, endPoly;
	Vector3 startPoint, endPoint;

	// Find the starting polygons and point on it to start from
	// TODO: Use something else for this
	const Vector3 extents = {10000.0f, 10000.0f, 10000.0f };
	dtQueryFilter filter;
	dtStatus status;
	status = m_navQuery->findNearestPoly(&query.source.X, &extents.X, &filter, &startPoly, &startPoint.X);
	if(dtStatusFailed(status))
		return res;
	status = m_navQuery->findNearestPoly(&query.target.X, &extents.X, &filter, &endPoly, &endPoint.X);
	if(dtStatusFailed(status))
		return res;

	// TODO: fix hardcoded limit
	dtPolyRef path[1024];
	int pathPointCount = 0;
	status = m_navQuery->findPath(startPoly, endPoly, &startPoint.X, &endPoint.X, 
		&filter, path, &pathPointCount, 1024);
	if (dtStatusFailed(status))
		return res;
	
	pathPoints.clear();
	Vector3 lastPoint = startPoint;
	pathPoints.push_back(startPoint);
	for(int i = 0; i < pathPointCount; i++)
	{
		Vector3 nextPoint;
		bool overPoly = false;
		status = m_navQuery->closestPointOnPoly(path[i], &lastPoint.X, &nextPoint.X, &overPoly);
		if (dtStatusFailed(status))
			return res; // Couldn't find next point on path
		pathPoints.push_back(nextPoint);
		lastPoint = nextPoint;
	}
	pathPoints.push_back(endPoint);

	res->pathFound = true;
	res->numPathPoints = pathPoints.size();
	res->pathPoints = pathPoints.data();

	return res;
}
