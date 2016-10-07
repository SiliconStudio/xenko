#include "../XenkoNative.h"
#include "../../../../deps/NativePath/NativePath.h"
#include "Navigation.hpp"
#include "NavigationBuilder.hpp"
#include "NavigationMesh.hpp"

extern "C"
{
	// Navmesh Builder
	DLL_EXPORT_API NavigationBuilder* xnNavigationCreateBuilder()
	{
		return new NavigationBuilder();
	}
	DLL_EXPORT_API void xnNavigationDestroyBuilder(NavigationBuilder* nav)
	{
		delete nav;
	}
	DLL_EXPORT_API void xnNavigationSetSettings(NavigationBuilder* nav, BuildSettings* buildSettings)
	{
		nav->SetSettings(*buildSettings);
	}
	DLL_EXPORT_API GeneratedData* xnNavigationBuildNavmesh(NavigationBuilder* nav,
		Vector3* vertices, int numVertices,
		int* indices, int numIndices)
	{
		return nav->BuildNavmesh(vertices, numVertices, indices, numIndices);
	}

	// Navmesh Query
	DLL_EXPORT_API void* xnNavigationCreateNavmesh(float cellTileSize)
	{
		NavigationMesh* navmesh = new NavigationMesh();
		if (!navmesh->Init(cellTileSize))
		{
			delete navmesh;
			navmesh = nullptr;
		}
		return navmesh;
	}
	DLL_EXPORT_API void xnNavigationDestroyNavmesh(NavigationMesh* navmesh)
	{
		delete navmesh;
	}

	DLL_EXPORT_API bool xnNavigationAddTile(NavigationMesh* navmesh, Point tileCoordinate, uint8_t* data, int dataLength)
	{
		return navmesh->LoadTile(tileCoordinate, data, dataLength);
	}
	DLL_EXPORT_API bool xnNavigationRemoveTile(NavigationMesh* navmesh, Point tileCoordinate)
	{
		return navmesh->RemoveTile(tileCoordinate);
	}
	DLL_EXPORT_API NavMeshPathfindResult* xnNavigationPathFindQuery(NavigationMesh* navmesh, NavMeshPathfindQuery query)
	{
		return navmesh->FindPath(query);
	}
	DLL_EXPORT_API NavMeshRaycastResult* xnNavigationRaycastQuery(NavigationMesh* navmesh, NavMeshRaycastQuery query)
	{
		return navmesh->Raycast(query);
	}
}
