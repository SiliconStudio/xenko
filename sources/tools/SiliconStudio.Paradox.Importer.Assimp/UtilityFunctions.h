// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#include "Stdafx.h"


using namespace System;
using namespace SiliconStudio::Paradox::Animations;
using namespace SiliconStudio::Core::Diagnostics;
using namespace SiliconStudio::Core::Mathematics;

// Assimp types convertion
String^ aiStringToString(aiString str);
Color aiColor4ToColor(aiColor4D color);
Color3 aiColor3ToColor3(aiColor3D color);
Color4 aiColor3ToColor4(aiColor3D color);
Matrix aiMatrixToMatrix(aiMatrix4x4 mat);
Vector2 aiVector2ToVector2(aiVector2D vec);
Vector3 aiVector3ToVector3(aiVector3D vec);
Quaternion aiQuaternionToQuaternion(aiQuaterniont<float> quat);
CompressedTimeSpan aiTimeToPdxTimeSpan(double time, double tickPerSecond);

// Others
Vector3 QuaternionToEulerAngles(Quaternion q);
Vector3 FlipYZAxis(Vector3 input, bool shouldFlip);
