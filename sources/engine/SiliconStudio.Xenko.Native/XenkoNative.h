// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#pragma once

/*
* Some platforms requires a special declaration before the function declaration to export them
* in the shared library. Defining NEED_DLL_EXPORT will define DLL_EXPORT_API to do the right thing
* for those platforms.
*
* To export void foo(int a), do:
*
*   DLL_EXPORT_API void foo (int a);
*/
#ifdef NEED_DLL_EXPORT
#define DLL_EXPORT_API __declspec(dllexport)
#else
#define DLL_EXPORT_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

#pragma pack(push, 4)
typedef struct Vector2
{
	float X;
	float Y;
} Vector2;

typedef struct Vector3
{
	float X;
	float Y;
	float Z;
} Vector3;

typedef struct Vector4
{
	float X;
	float Y;
	float Z;
	float W;
} Vector4;

typedef struct Color4
{
	float R;
	float G;
	float B;
	float A;
} Color4;

typedef struct RectangleF
{
	float x;
	float y;
	float width;
	float height;
} RectangleF;

typedef struct BoundingBox
{
	Vector3 minimum;
	Vector3 maximum;
} BoundingBox;

typedef struct VertexPositionColorTextureSwizzle
{
	Vector4 Position;
	Color4 ColorScale;
	Color4 ColorAdd;
	Vector2 TextureCoordinate;
	float Swizzle;
} VertexPositionColorTextureSwizzle;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct SpriteDrawInfo
{
	RectangleF Source;
	RectangleF Destination;
	Vector2 Origin;
	float Rotation;
	float Depth;
	int SpriteEffects;
	Color4 ColorScale;
	Color4 ColorAdd;
	int Swizzle;
	Vector2 TextureSize;
	int Orientation;
} SpriteDrawInfo;
#pragma pack(pop)

#ifdef __cplusplus
}
#endif
