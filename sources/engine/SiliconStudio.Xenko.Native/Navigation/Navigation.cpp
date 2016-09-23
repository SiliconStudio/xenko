#include "../../../../deps/Recast/include/Recast.h"
#include "../../../../deps/Recast/include/DetourNavMeshBuilder.h"
#include "../XenkoNative.h"

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/TINYSTL/vector.h"
#include "Navigation.hpp"
#include "NavigationBuilder.hpp"
#include "Navmesh.hpp"

extern "C"
{
	// Navmesh Builder
	NavigationBuilder* xnNavigationCreateBuilder()
	{
		return new NavigationBuilder();
	}
	void xnNavigationDestroyBuilder(NavigationBuilder* nav)
	{
		delete nav;
	}
	void xnNavigationSetSettings(NavigationBuilder* nav, BuildSettings* buildSettings)
	{
		nav->SetSettings(*buildSettings);
	}
	GeneratedData* xnNavigationBuildNavmesh(NavigationBuilder* nav,
		Vector3* vertices, int numVertices,
		int* indices, int numIndices)
	{
		return nav->BuildNavmesh(vertices, numVertices, indices, numIndices);
	}

	// Navmesh Query
	void* xnNavigationLoadNavmesh(uint8_t* data, int dataLength)
	{
		Navmesh* navmesh = new Navmesh();
		if (!navmesh->Load(data, dataLength))
		{
			delete navmesh;
			return nullptr;
		}
		return navmesh;
	}
	void xnNavigationDestroyNavmesh(Navmesh* navmesh)
	{
		delete navmesh;
	}
	NavmeshQueryResult* xnNavigationQuery(Navmesh* navmesh, NavmeshQuery query)
	{
		return navmesh->Query(query);
	}
}
