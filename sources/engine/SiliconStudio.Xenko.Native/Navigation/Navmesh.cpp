// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../../deps/Recast/include/Recast.h"
#include "../XenkoNative.h"

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/TINYSTL/vector.h"
#include "Navigation.hpp"
#include "Navmesh.hpp"

Navmesh::Navmesh()
{
}

Navmesh::~Navmesh()
{
	// Cleanup allocated tiles
	for(auto tile : m_tileRefs)
	{
		uint8_t* deletedData;
		int deletedDataLength = 0;
		dtStatus status = m_navMesh->removeTile(tile, &deletedData, &deletedDataLength);
		if(dtStatusSucceed(status))
		{
			if (deletedData)
				delete[] deletedData;
		}
	}

	if(m_navQuery)
		dtFreeNavMeshQuery(m_navQuery);
	if(m_navMesh)
		dtFreeNavMesh(m_navMesh);
}

bool Navmesh::Init(float cellTileSize)
{
	// Allocate objects
	m_navMesh = dtAllocNavMesh();
	m_navQuery = dtAllocNavMeshQuery();

	if (!m_navMesh || !m_navQuery)
		return false;

	dtNavMeshParams params = { 0 };
	params.orig[0] = 0.0f;
	params.orig[1] = 0.0f;
	params.orig[2] = 0.0f;
	params.tileWidth = cellTileSize;
	params.tileHeight = cellTileSize;

	// TODO: Link these parameters to the builder
	int tileBits = 14;
	if (tileBits > 14) tileBits = 14;
	int polyBits = 22 - tileBits;
	params.maxTiles = 1 << tileBits;
	params.maxPolys = 1 << polyBits;

	dtStatus status = m_navMesh->init(&params);
	if (dtStatusFailed(status))
		return false;

	// Initialize the query object
	status = m_navQuery->init(m_navMesh, 2048);
	if (dtStatusFailed(status))
		return false;
	return true;
}

bool Navmesh::LoadTile(Point tileCoordinate, uint8_t* navData, int navDataLength)
{
	if (!m_navMesh || !m_navQuery)
		return false;
	if (!navData)
		return false;

	// Copy data
	uint8_t* dataCopy = new uint8_t[navDataLength];
	memcpy(dataCopy, navData, navDataLength);

	dtTileRef tileRef = 0;
	if(dtStatusSucceed(m_navMesh->addTile(dataCopy, navDataLength, 0, 0, &tileRef)))
	{
		m_tileRefs.insert(tileRef);
		return true;
	}

	delete[] dataCopy;
	return false;
}

bool Navmesh::RemoveTile(Point tileCoordinate)
{
	dtTileRef tileRef = m_navMesh->getTileRefAt(tileCoordinate.X, tileCoordinate.Y, 0);

	uint8_t* deletedData;
	int deletedDataLength = 0;
	dtStatus status = m_navMesh->removeTile(tileRef, &deletedData, &deletedDataLength);
	if(dtStatusSucceed(status))
	{
		if (deletedData)
			delete[] deletedData;
		m_tileRefs.erase(tileRef);
	}
	return false;
}

NavmeshQueryResult* Navmesh::Query(NavmeshQuery query)
{
	// Reset result
	m_result = NavmeshQueryResult();
	NavmeshQueryResult* res = &m_result;
	dtPolyRef startPoly, endPoly;
	Vector3 startPoint, endPoint;

	// Find the starting polygons and point on it to start from
	// TODO: Allow this to be user-specified
	const Vector3 extents = {2 ,4 ,2};
	dtQueryFilter filter;
	dtStatus status;
	status = m_navQuery->findNearestPoly(&query.source.X, &extents.X, &filter, &startPoly, &startPoint.X);
	if(dtStatusFailed(status))
		return res;
	status = m_navQuery->findNearestPoly(&query.target.X, &extents.X, &filter, &endPoly, &endPoint.X);
	if(dtStatusFailed(status))
		return res;

	const dtPoly* startPoly_, *endPoly_;
	const dtMeshTile* startTile, *endTile;
	m_navMesh->getTileAndPolyByRef(startPoly, &startTile, &startPoly_);
	m_navMesh->getTileAndPolyByRef(endPoly, &endTile, &endPoly_);

	// TODO: fix hardcoded limit
	dtPolyRef path[1024];
	int pathPointCount = 0;
	status = m_navQuery->findPath(startPoly, endPoly, &startPoint.X, &endPoint.X, 
		&filter, path, &pathPointCount, 1024);
	if (dtStatusFailed(status))
		return res;

	static const size_t maxStraightPathLength = 2048;
	Vector3 straightPath[maxStraightPathLength];
	uint8_t straightPathFlags[maxStraightPathLength];
	dtPolyRef straightPathRefs[maxStraightPathLength];
	int straightPathCount = 0;
	status = m_navQuery->findStraightPath(&startPoint.X, &endPoint.X, path, pathPointCount, &straightPath[0].X, straightPathFlags, straightPathRefs, &straightPathCount, maxStraightPathLength);
	if (dtStatusFailed(status))
		return res;

	m_pathPoints.clear();
	for(int i = 0; i < straightPathCount; i++)
	{
		m_pathPoints.push_back(straightPath[i]);
	}

	m_result.pathFound = true;
	m_result.numPathPoints = m_pathPoints.size();
	m_result.pathPoints = m_pathPoints.data();

	return res;
}
