// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the SILICONSTUDIOXENKONATIVE_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SILICONSTUDIOXENKONATIVE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef SILICONSTUDIOXENKONATIVE_EXPORTS
#define SILICONSTUDIOXENKONATIVE_API __declspec(dllexport)
#else
#define SILICONSTUDIOXENKONATIVE_API __declspec(dllimport)
#endif

#include <stdint.h>

#pragma pack(push, 4)
struct Vector2
{
	float X;
	float Y;
};

struct Vector4
{
	float X;
	float Y;
	float Z;
	float W;
};

struct Color4
{
	float R;
	float G;
	float B;
	float A;
};

struct RectangleF
{
	float x;
	float y;
	float width;
	float height;
};

struct VertexPositionColorTextureSwizzle
{
	Vector4 Position;
	Color4 Color;
	Vector2 TextureCoordinate;
	float Swizzle;
};
#pragma pack(pop)

#pragma pack(push, 8)
struct SpriteDrawInfo
{
	RectangleF Source;
	RectangleF Destination;
	Vector2 Origin;
	float Rotation;
	float Depth;
	int32_t SpriteEffects;
	Color4 Color;
	int32_t Swizzle;
	Vector2 TextureSize;
	int32_t Orientation;
};
#pragma pack(pop)

#ifdef __cplusplus
extern "C"
{
	SILICONSTUDIOXENKONATIVE_API void UpdateBufferValuesFromElementInfoNative(SpriteDrawInfo* elementInfo, VertexPositionColorTextureSwizzle* vertexPointer, void* indexPointer, int32_t vertexStartOffset);
}
#endif

