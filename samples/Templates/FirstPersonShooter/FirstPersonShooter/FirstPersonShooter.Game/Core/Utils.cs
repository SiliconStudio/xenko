// Copyright (C) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed as part of the Xenko Game Studio Samples
// Detailed license can be found at: http://xenko.com/legal/eula/

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace FirstPersonShooter.Core
{
    public static class Utils
    {
        public static Vector3 LogicDirectionToWorldDirection(Vector2 logicDirection, CameraComponent camera, Vector3 upVector)
        {
            var inverseView = Matrix.Invert(camera.ViewMatrix);

            var forward = Vector3.Cross(upVector, inverseView.Right);
            forward.Normalize();

            var right = Vector3.Cross(forward, upVector);
            var worldDirection = forward * logicDirection.Y + right * logicDirection.X;
            worldDirection.Normalize();
            return worldDirection;
        }
    }
}
