// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#pragma once
#include "stdafx.h"

using namespace System;
using namespace SiliconStudio::Core::Mathematics;
using namespace SiliconStudio::Paradox::Animations;

// conversion functions
Color4 FbxDouble3ToColor4(FbxDouble3 vector, float alphaValue = 1.0f);

Vector3 FbxDouble3ToVector3(FbxDouble3 vector);
Vector4 FbxDouble3ToVector4(FbxDouble3 vector, float wValue = 0.0f);

Vector4 FbxDouble4ToVector4(FbxDouble4 vector);

Matrix FBXMatrixToMatrix(FbxAMatrix& matrix);
FbxAMatrix MatrixToFBXMatrix(Matrix& matrix);

CompressedTimeSpan FBXTimeToTimeSpan(const FbxTime& time);

double FocalLengthToVerticalFov(double filmHeight, double focalLength);

// operators
FbxDouble3 operator*(double factor, FbxDouble3 vector);

// string manipulation
System::String^ ConvertToUTF8(std::string str);