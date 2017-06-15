// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/NativeMath.h"
#include "../../../../deps/NativePath/standard/math.h"
#include "../XenkoNative.h"

#ifdef __cplusplus
extern "C" {
#endif
	static VertexPositionNormalTexture BaseVertexBufferData[4] =
	{
		// Position		Normal		UV Coordinates
		{ { -1, 1, 0 },{ 0, 0, 1 },{ 0, 0 } },
		{ { 1, 1, 0 },{ 0, 0, 1 },{ 1, 0 } },
		{ { -1, -1, 0 },{ 0, 0, 1 },{ 0, 1 } },
		{ { 1, -1, 0 },{ 0, 0, 1 },{ 1, 1 } },
	};

	DLL_EXPORT_API void AppendTextToVertexBuffer(RectangleF* constantInfos, RectangleF* renderInfos, const char* textPointer, int** textLength, VertexPositionNormalTexture** vertexBufferPointer)
	{
		const float fX = renderInfos->x / renderInfos->width;
		const float fY = renderInfos->y / renderInfos->height;
		const float fW = constantInfos->x / renderInfos->width;
		const float fH = constantInfos->y / renderInfos->height;

		RectangleF destination = { fX, fY, fW, fH };
		RectangleF source = { 0.0f, 0.0f, constantInfos->x, constantInfos->y };

		// Copy the array length (since it may change during an iteration)
		const int textCharCount = **textLength;

		for (int i = 0; i < textCharCount; i++)
		{
			char currentChar = textPointer[i];

			if (currentChar == 11)
			{
				// Tabulation
				destination.x += 8 * fX;
				--**textLength;
				continue;
			}
			else if (currentChar >= 10 && currentChar <= 13)
			{
				// New Line
				destination.x = fX;
				destination.y += fH;
				--**textLength;
				continue;
			}
			else if (currentChar < 32 || currentChar > 126)
			{
				currentChar = 32;
			}

			source.x = ((float)(currentChar % 32)) * constantInfos->x;
			source.y = ((float)((currentChar / 32) % 4)) * constantInfos->y;

			for (int j = 0; j < 4; j++)
			{
				(*vertexBufferPointer)->Position.X = (destination.x * 2.0f - 1.0f) + BaseVertexBufferData[j].Position.X * destination.width;
				(*vertexBufferPointer)->Position.Y = -(destination.y * 2.0f - 1.0f) + BaseVertexBufferData[j].Position.Y * destination.height;
				(*vertexBufferPointer)->Position.Z = 0.0f;

				(*vertexBufferPointer)->TextureCoordinate.X = (source.x + BaseVertexBufferData[j].TextureCoordinate.X * source.width) / constantInfos->width;
				(*vertexBufferPointer)->TextureCoordinate.Y = (source.y + BaseVertexBufferData[j].TextureCoordinate.Y * source.height) / constantInfos->height;

				++(*vertexBufferPointer);
			}

			destination.x += destination.width;
		}
	}

#ifdef __cplusplus
}
#endif
