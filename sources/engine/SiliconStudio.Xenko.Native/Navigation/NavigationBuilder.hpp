#pragma once

class NavigationBuilder
{
	rcHeightfield* m_solid = nullptr;
	uint8_t* m_triareas = nullptr;
	rcCompactHeightfield* m_chf = nullptr;
	rcContourSet* m_cset = nullptr;
	rcPolyMesh* m_pmesh = nullptr;
	rcPolyMeshDetail* m_dmesh = nullptr;
	rcConfig m_config;
	rcContext* m_context;

	// Calculated navmesh vertices
	tinystl::vector<Vector3> m_navmeshVertices;
	tinystl::vector<uint8_t> m_navmeshData;
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