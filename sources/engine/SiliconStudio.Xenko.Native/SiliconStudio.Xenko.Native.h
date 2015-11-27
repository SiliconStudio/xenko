// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#pragma once

#include <NativeMath.h>
#include <NativeMemory.h>

#pragma pack(push, 4)
typedef struct Vector2
{
	float X;
	float Y;
} Vector2;

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

typedef struct VertexPositionColorTextureSwizzle
{
	Vector4 Position;
	Color4 Color;
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
	Color4 Color;
	int Swizzle;
	Vector2 TextureSize;
	int Orientation;
} SpriteDrawInfo;
#pragma pack(pop)
