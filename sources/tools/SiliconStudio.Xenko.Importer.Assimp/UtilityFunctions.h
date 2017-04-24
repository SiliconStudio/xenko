// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#include "Stdafx.h"


using namespace System;
using namespace SiliconStudio::Xenko::Animations;
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
CompressedTimeSpan aiTimeToXkTimeSpan(double time, double tickPerSecond);

// Others
Vector3 QuaternionToEulerAngles(Quaternion q);
Vector3 FlipYZAxis(Vector3 input, bool shouldFlip);
