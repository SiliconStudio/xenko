// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    class XenkoMotionState : BulletSharp.SharpMotionState
    {
        private readonly RigidbodyComponent rigidBody;

        public XenkoMotionState(RigidbodyComponent rb)
        {
            rigidBody = rb;
        }

        public override void GetWorldTransform(out Matrix transform)
        {
            if (rigidBody.GetWorldTransformCallback != null)
            {
                rigidBody.GetWorldTransformCallback(out transform);
            }
            else
            {
                transform = Matrix.Identity;
            }
        }

        public override void SetWorldTransform(Matrix transform)
        {
            rigidBody.SetWorldTransformCallback?.Invoke(transform);
        }
    }
}