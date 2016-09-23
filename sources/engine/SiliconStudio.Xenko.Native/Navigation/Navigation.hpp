#pragma once
#pragma pack(4)
struct AgentSettings
{
	float height;
	float radius;
	float maxClimb;
	float maxSlope;
};
struct BuildSettings
{
	// Bounding box for the generated navigation mesh
	BoundingBox boundingBox;
	// Settings for agent
	AgentSettings agentSettings;
	float cellHeight;
	float cellSize;
};
struct GeneratedData
{
	bool success;
	Vector3* navmeshVertices = nullptr;
	int numNavmeshVertices = 0;
	uint8_t* navmeshData = nullptr;
	int navmeshDataLength = 0;
};