// SiliconStudio.Xenko.Native.cpp : Defines the exported functions for the DLL application.
//

#include "SiliconStudio.Xenko.Native.h"
#include <math.h>

void UpdateBufferValuesFromElementInfoNative(SpriteDrawInfo* drawInfo, VertexPositionColorTextureSwizzle* vertexPointer, void* indexPointer, int32_t vertexStartOffset)
{
	auto deltaX = 1.0f / drawInfo->TextureSize.X;
	auto deltaY = 1.0f / drawInfo->TextureSize.Y;

	Vector2 rotation;
	rotation.X = 1;
	rotation.Y = 0;

	if (fabs(drawInfo->Rotation) > 1e-6f)
	{
		rotation.X = cosf(drawInfo->Rotation);
		rotation.Y = sinf(drawInfo->Rotation);
	}

	auto origin = drawInfo->Origin;
	origin.X /= fmaxf(1e-6f, drawInfo->Source.width);
	origin.Y /= fmaxf(1e-6f, drawInfo->Source.height);

	const Vector2 cornerOffsets[] =
	{
		{0, 0 },
		{1, 0 },
		{1, 1 },
		{0, 1 }
	};

	for (int j = 0; j < 4; j++)
	{
		auto corner = cornerOffsets[j];
		Vector2 position;
		position.X = (corner.X - origin.X) * drawInfo->Destination.width;
		position.Y = (corner.Y - origin.Y) * drawInfo->Destination.height;

		vertexPointer->Position.X = drawInfo->Destination.x + (position.X * rotation.X) - (position.Y * rotation.Y);
		vertexPointer->Position.Y = drawInfo->Destination.y + (position.X * rotation.Y) + (position.Y * rotation.X);
		vertexPointer->Position.Z = drawInfo->Depth;
		vertexPointer->Position.W = 1.0f;
		vertexPointer->Color = drawInfo->Color;

		corner = cornerOffsets[((j ^ (int)drawInfo->SpriteEffects) + (int)drawInfo->Orientation) % 4];
		vertexPointer->TextureCoordinate.X = (drawInfo->Source.x + corner.X * drawInfo->Source.width) * deltaX;
		vertexPointer->TextureCoordinate.Y = (drawInfo->Source.y + corner.Y * drawInfo->Source.height) * deltaY;

		vertexPointer->Swizzle = (int)drawInfo->Swizzle;

		vertexPointer++;
	}
}
