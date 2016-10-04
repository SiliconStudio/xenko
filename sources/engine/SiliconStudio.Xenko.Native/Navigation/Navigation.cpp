#include "../XenkoNative.h"
#include "../../../../deps/NativePath/NativePath.h"
#include "Navigation.hpp"
#include "NavigationBuilder.hpp"
#include "Navmesh.hpp"

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
	DLL_EXPORT_API void xnNavigationSetAgentSettings(NavigationBuilder* nav, AgentSettings* agentSettings)
	{
		nav->SetAgentSettings(*agentSettings);
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
		Navmesh* navmesh = new Navmesh();
		if (!navmesh->Init(cellTileSize))
		{
			delete navmesh;
			navmesh = nullptr;
		}
		return navmesh;
	}
	DLL_EXPORT_API void xnNavigationDestroyNavmesh(Navmesh* navmesh)
	{
		delete navmesh;
	}

	DLL_EXPORT_API bool xnNavigationAddTile(Navmesh* navmesh, Point tileCoordinate, uint8_t* data, int dataLength)
	{
		return navmesh->LoadTile(tileCoordinate, data, dataLength);
	}
	DLL_EXPORT_API bool xnNavigationRemoveTile(Navmesh* navmesh, Point tileCoordinate)
	{
		return navmesh->RemoveTile(tileCoordinate);
	}
	DLL_EXPORT_API NavmeshQueryResult* xnNavigationQuery(Navmesh* navmesh, NavmeshQuery query)
	{
		return navmesh->Query(query);
	}
}
