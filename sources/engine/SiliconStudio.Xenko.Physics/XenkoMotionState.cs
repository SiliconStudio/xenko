// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    class XenkoMotionState : BulletSharp.SharpMotionState
    {
        private RigidbodyComponent rigidBody;

        public XenkoMotionState(RigidbodyComponent rb)
        {
            rigidBody = rb;
        }

        public void Clear()
        {
            rigidBody = null;
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
