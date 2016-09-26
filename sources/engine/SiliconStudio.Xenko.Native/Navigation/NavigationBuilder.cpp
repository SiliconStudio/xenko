#include "../../../../deps/Recast/include/DetourNavMeshBuilder.h"
#include "../XenkoNative.h"

#include "../../../../deps/NativePath/NativePath.h"
#include "Navigation.hpp"
#include "NavigationBuilder.hpp"
#include "../../../../deps/NativePath/NativeTime.h"

// TODO: Remove this
#ifdef _WIN32
#define WINAPI __stdcall
#define WINBASEAPI __declspec(dllimport)
extern "C"
{
	WINBASEAPI uint32_t WINAPI AllocConsole();
	WINBASEAPI uint32_t WINAPI FreeConsole();
	WINBASEAPI uint32_t WINAPI AttachConsole(uint32_t dwProcessId);
	WINBASEAPI void* WINAPI GetConsoleWindow(void);
	WINBASEAPI uint32_t WINAPI CloseWindow(void* hWnd);
	WINBASEAPI uint32_t WINAPI GetCurrentProcessId(void);
	WINBASEAPI void* __cdecl freopen(
			char const* _FileName,
			char const* _Mode,
			void*       _Stream
		);
	WINBASEAPI void* __cdecl __acrt_iob_func(unsigned); 
	#define stdin (__acrt_iob_func(0))
	#define stdout (__acrt_iob_func(1))
	#define stderr (__acrt_iob_func(2))
}
class DebugConsole
{
	DebugConsole()
	{
		AllocConsole();
		AttachConsole(GetCurrentProcessId());
		freopen("CON", "w", stdout);
	}
public:
	~DebugConsole()
	{
		void* consoleWindow = GetConsoleWindow();
		FreeConsole();
		CloseWindow(consoleWindow);
	}
	static DebugConsole& Get()
	{
		static DebugConsole console;
		return console;
	}
};
#endif

NavigationBuilder::NavigationBuilder()
{
	m_context = new rcContext(false);
#ifdef _WIN32
	DebugConsole::Get();
#endif
}
NavigationBuilder::~NavigationBuilder()
{
	delete m_context;
	Cleanup();
}
void NavigationBuilder::Cleanup()
{
	if(m_solid)
	{
		rcFreeHeightField(m_solid);
		m_solid = nullptr;
	}
	if(m_triareas)
	{
		delete[] m_triareas;
		m_triareas = nullptr;
	}
	if(m_chf)
	{
		rcFreeCompactHeightfield(m_chf);
		m_chf = nullptr;
	}
	if(m_pmesh)
	{
		rcFreePolyMesh(m_pmesh);
		m_pmesh = nullptr;
	}
	if(m_dmesh)
	{
		rcFreePolyMeshDetail(m_dmesh);
		m_dmesh = nullptr;
	}
}
GeneratedData* NavigationBuilder::BuildNavmesh(Vector3* vertices, int numVertices, int* indices, int numIndices)
{
	printf(" -- Generating navmesh --\n");
	printf("Input Vertices: %d\n", numVertices);
	printf("Input Indices: %d\n", numIndices);
	printf("CS: %f  CH: %f\n", m_config.cs, m_config.ch);
	printf("Bounds: \n\t{%f, %f, %f}\n\t{%f, %f, %f}\n", 
		m_config.bmin[0], m_config.bmin[1], m_config.bmin[2],
		m_config.bmax[0], m_config.bmax[1], m_config.bmax[2]);
	printf("Agent Radius: %f  Height: %f\n", m_buildSettings.agentSettings.radius, m_buildSettings.agentSettings.height);

	double totalTime = npSeconds();
	GeneratedData* ret = &m_result;
	ret->success = false;

	// Make sure state is clean
	Cleanup();

	if(numIndices == 0 || numVertices == 0)
		return ret;

	m_solid = rcAllocHeightfield();
	if(!rcCreateHeightfield(m_context, *m_solid, m_config.width, m_config.height, m_config.bmin, m_config.bmax, m_config.cs, m_config.ch))
	{
		return ret;
	}

	int numTriangles = numIndices / 3;

	// Allocate array that can hold triangle area types.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	m_triareas = new uint8_t[numTriangles];
	if(!m_triareas)
	{
		return ret;
	}
	// Find triangles which are walkable based on their slope and rasterize them.
	// If your input data is multiple meshes, you can transform them here, calculate
	// the are type for each of the meshes and rasterize them.
	double rasterizationTime = npSeconds();
	memset(m_triareas, 0, numTriangles * sizeof(unsigned char));
	rcMarkWalkableTriangles(m_context, m_config.walkableSlopeAngle, (float*)vertices, numVertices, indices, numTriangles, m_triareas);
	if(!rcRasterizeTriangles(m_context, (float*)vertices, numVertices, indices, m_triareas, numTriangles, *m_solid, m_config.walkableClimb))
	{
		return ret;
	}
	rasterizationTime = npSeconds() - rasterizationTime;

	//
	// Step 3. Filter walkables surfaces.
	//

	// Once all geoemtry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.
	rcFilterLowHangingWalkableObstacles(m_context, m_config.walkableClimb, *m_solid);
	rcFilterLedgeSpans(m_context, m_config.walkableHeight, m_config.walkableClimb, *m_solid);
	rcFilterWalkableLowHeightSpans(m_context, m_config.walkableHeight, *m_solid);

	//
	// Step 4. Partition walkable surface to simple regions.
	//

	// Compact the heightfield so that it is faster to handle from now on.
	// This will result more cache coherent data as well as the neighbours
	// between walkable cells will be calculated.
	double buildHeightFieldTime = npSeconds();
	m_chf = rcAllocCompactHeightfield();
	if(!m_chf)
	{
		return ret;
	}
	if(!rcBuildCompactHeightfield(m_context, m_config.walkableHeight, m_config.walkableClimb, *m_solid, *m_chf))
	{
		return ret;
	}

	{
		rcFreeHeightField(m_solid);
		m_solid = 0;
	}
	buildHeightFieldTime = npSeconds() - buildHeightFieldTime;

	// Erode the walkable area by agent radius.
	if(!rcErodeWalkableArea(m_context, m_config.walkableRadius, *m_chf))
	{
		return ret;
	}

	// (Optional) Mark areas.
	Vector3 rootVolume;
	rcMarkBoxArea(m_context, m_config.bmin, m_config.bmax, 1, *m_chf);

	// Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
	// There are 3 partitioning methods, each with some pros and cons:
	// 1) Watershed partitioning
	//   - the classic Recast partitioning
	//   - creates the nicest tessellation
	//   - usually slowest
	//   - partitions the heightfield into nice regions without holes or overlaps
	//   - the are some corner cases where this method creates produces holes and overlaps
	//      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
	//      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
	//   * generally the best choice if you precompute the navmesh, use this if you have large open areas
	// 2) Monotone partioning
	//   - fastest
	//   - partitions the heightfield into regions without holes and overlaps (guaranteed)
	//   - creates long thin polygons, which sometimes causes paths with detours
	//   * use this if you want fast navmesh generation
	// 3) Layer partitoining
	//   - quite fast
	//   - partitions the heighfield into non-overlapping regions
	//   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
	//   - produces better triangles than monotone partitioning
	//   - does not have the corner cases of watershed partitioning
	//   - can be slow and create a bit ugly tessellation (still better than monotone)
	//     if you have large open areas with small obstacles (not a problem if you use tiles)
	//   * good choice to use for tiled navmesh with medium and small sized tiles

	double regionsTime = npSeconds();
	//if (m_partitionType == SAMPLE_PARTITION_WATERSHED)
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if(!rcBuildDistanceField(m_context, *m_chf))
		{
			return ret;
		}

		// Partition the walkable surface into simple regions without holes.
		if(!rcBuildRegions(m_context, *m_chf, 0, m_config.minRegionArea, m_config.mergeRegionArea))
		{
			return ret;
		}
	}
	//else if (m_partitionType == SAMPLE_PARTITION_MONOTONE)
	//{
	//	// Partition the walkable surface into simple regions without holes.
	//	// Monotone partitioning does not need distancefield.
	//	if (!rcBuildRegionsMonotone(context, *m_chf, 0, config.minRegionArea, config.mergeRegionArea))
	//	{
	//		context->log(RC_LOG_ERROR, "buildNavigation: Could not build monotone regions.");
	//		return false;
	//	}
	//}
	//else // SAMPLE_PARTITION_LAYERS
	//{
	//	// Partition the walkable surface into simple regions without holes.
	//	if (!rcBuildLayerRegions(context, *m_chf, 0, config.minRegionArea))
	//	{
	//		context->log(RC_LOG_ERROR, "buildNavigation: Could not build layer regions.");
	//		return false;
	//	}
	//}
	regionsTime = npSeconds() - regionsTime;

	//
	// Step 5. Trace and simplify region contours.
	//

	// Create contours.
	double contoursTime = npSeconds();
	m_cset = rcAllocContourSet();
	if(!m_cset)
	{
		return ret;
	}
	if(!rcBuildContours(m_context, *m_chf, m_config.maxSimplificationError, m_config.maxEdgeLen, *m_cset))
	{
		return ret;
	}
	contoursTime = npSeconds() - contoursTime;

	//
	// Step 6. Build polygons mesh from contours.
	//

	// Build polygon navmesh from the contours.
	double polyMeshTime = npSeconds();
	m_pmesh = rcAllocPolyMesh();
	if(!m_pmesh)
	{
		return ret;
	}
	if(!rcBuildPolyMesh(m_context, *m_cset, m_config.maxVertsPerPoly, *m_pmesh))
	{
		return ret;
	}
	polyMeshTime = npSeconds() - polyMeshTime;

	//
	// Step 7. Create detail mesh which allows to access approximate height on each polygon.
	//

	double detailMeshTime = npSeconds();
	m_dmesh = rcAllocPolyMeshDetail();
	if(!m_dmesh)
	{
		return ret;
	}

	if(!rcBuildPolyMeshDetail(m_context, *m_pmesh, *m_chf, m_config.detailSampleDist, m_config.detailSampleMaxError, *m_dmesh))
	{
		return ret;
	}
	detailMeshTime = npSeconds() - detailMeshTime;

	{
		rcFreeCompactHeightfield(m_chf);
		m_chf = 0;
		rcFreeContourSet(m_cset);
		m_cset = 0;
	}

	// Generate native navmesh format and store the data pointers in the return structure
	GenerateNavMeshVertices();
	ret->navmeshVertices = m_navmeshVertices.data();
	ret->numNavmeshVertices = m_navmeshVertices.size();
	double createMeshTime = npSeconds();
	if(!CreateDetourMesh())
		return ret;
	createMeshTime = npSeconds() - createMeshTime;
	ret->navmeshData = m_navmeshData.data();
	ret->navmeshDataLength = m_navmeshData.size();
	ret->success = true;

	totalTime = npSeconds() - totalTime;

	printf("Navmesh build timings\n");
	printf("Rasterization: %.03f\n", rasterizationTime);
	printf("Heightfield: %.03f\n", buildHeightFieldTime);
	printf("Regions: %.03f\n", regionsTime);
	printf("Contours: %.03f\n", contoursTime);
	printf("Poly Mesh: %.03f\n", polyMeshTime);
	printf("Detail Mesh: %.03f\n", detailMeshTime);
	printf("Create Mesh: %.03f\n", createMeshTime);
	printf("Total Duration: %.03f\n", totalTime);

	return ret;
}
void NavigationBuilder::SetSettings(BuildSettings buildSettings)
{
	// Copy this to have access to original settings
	m_buildSettings = buildSettings;

	// TODO: Expose these settings
	float regionMinSize = 0.1f;
	float regionMergeSize = 20;
	float edgeMaxLen = 12.0f;
	float edgeMaxError = 1.3f;
	float detailSampleDist = 6.0f;
	float detailSampleMaxError = 1.0f;

	m_config = {0};
	memcpy(m_config.bmin, &buildSettings.boundingBox.minimum, sizeof(float) * 3);
	memcpy(m_config.bmax, &buildSettings.boundingBox.maximum, sizeof(float) * 3);
	m_config.cs = buildSettings.cellSize;
	m_config.ch = buildSettings.cellHeight;
	m_config.walkableSlopeAngle = buildSettings.agentSettings.maxSlope;
	m_config.walkableHeight = (int)ceilf(buildSettings.agentSettings.height / m_config.ch);
	m_config.walkableClimb = (int)floorf(buildSettings.agentSettings.maxClimb / m_config.ch);
	m_config.walkableRadius = (int)ceilf(buildSettings.agentSettings.radius / m_config.cs);
	m_config.maxEdgeLen = (int)(edgeMaxLen / m_config.cs);
	m_config.maxSimplificationError = edgeMaxError;
	m_config.minRegionArea = (int)rcSqr(regionMinSize); // Note: area = size*size
	m_config.mergeRegionArea = (int)rcSqr(regionMergeSize); // Note: area = size*size
	m_config.maxVertsPerPoly = 6;
	m_config.detailSampleDist = detailSampleDist < 0.9f ? 0 : m_config.cs * detailSampleDist;
	m_config.detailSampleMaxError = m_config.ch * detailSampleMaxError;

	rcCalcGridSize(m_config.bmin, m_config.bmax, m_config.cs, &m_config.width, &m_config.height);
}
void NavigationBuilder::GenerateNavMeshVertices()
{
	rcPolyMesh& mesh = *m_pmesh;
	if(!m_pmesh)
		return;

	Vector3 origin;
	memcpy(&origin, m_config.bmin, sizeof(float) * 3);

	m_navmeshVertices.clear();
	for(int i = 0; i < m_pmesh->npolys; i++)
	{
		const unsigned short* p = &mesh.polys[i * mesh.nvp * 2];

		unsigned short vi[3];
		for(int j = 2; j < mesh.nvp; ++j)
		{
			if(p[j] == RC_MESH_NULL_IDX) break;
			vi[0] = p[0];
			vi[1] = p[j - 1];
			vi[2] = p[j];
			for(int k = 0; k < 3; ++k)
			{
				const unsigned short* v = &mesh.verts[vi[k] * 3];
				const float x = origin.X + (float)v[0] * m_config.cs;
				const float y = origin.Y + (float)(v[1] + 1) * m_config.ch;
				const float z = origin.Z + (float)v[2] * m_config.cs;
				m_navmeshVertices.push_back(Vector3{x, y, z});
			}
		}
	}
}
bool NavigationBuilder::CreateDetourMesh()
{
	dtNavMeshCreateParams params;
	memset(&params, 0, sizeof(params));
	params.verts = m_pmesh->verts;
	params.vertCount = m_pmesh->nverts;
	params.polys = m_pmesh->polys;
	params.polyAreas = m_pmesh->areas;
	params.polyFlags = m_pmesh->flags;
	params.polyCount = m_pmesh->npolys;
	params.nvp = m_pmesh->nvp;
	params.detailMeshes = m_dmesh->meshes;
	params.detailVerts = m_dmesh->verts;
	params.detailVertsCount = m_dmesh->nverts;
	params.detailTris = m_dmesh->tris;
	params.detailTriCount = m_dmesh->ntris;
	// TODO: Support off-mesh connections
	params.offMeshConVerts = nullptr;
	params.offMeshConRad = nullptr;
	params.offMeshConDir = nullptr;
	params.offMeshConAreas = nullptr;
	params.offMeshConFlags = nullptr;
	params.offMeshConUserID = nullptr;
	params.offMeshConCount = 0;
	params.walkableHeight = m_config.walkableHeight;
	params.walkableRadius = m_config.walkableRadius;
	params.walkableClimb = m_config.walkableClimb;
	rcVcopy(params.bmin, m_pmesh->bmin);
	rcVcopy(params.bmax, m_pmesh->bmax);
	params.cs = m_config.cs;
	params.ch = m_config.ch;
	params.buildBvTree = true;

	uint8_t* navData = nullptr;
	int navDataLength = 0;

	if(!dtCreateNavMeshData(&params, &navData, &navDataLength))
	{
		dtFree(navData);
		return false;
	}

	if(navDataLength == 0)
		return false;

	// Copy generated navmesh data to local array
	m_navmeshData.resize(navDataLength);
	memcpy(m_navmeshData.data(), navData, navDataLength);

	dtFree(navData);

	return true;
}
