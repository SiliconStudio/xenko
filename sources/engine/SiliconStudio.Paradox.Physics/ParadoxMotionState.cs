// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Physics
{
    class ParadoxMotionState : BulletSharp.SharpMotionState
    {
        private readonly RigidBody mRigidBody;

        public ParadoxMotionState(RigidBody rigidBody)
        {
            mRigidBody = rigidBody;
        }

        public override void GetWorldTransform(out Matrix transform)
        {
            if (mRigidBody.GetWorldTransformCallback != null)
            {
                mRigidBody.GetWorldTransformCallback(out transform);
            }
            else
            {
                transform = Matrix.Identity;
            }
        }

        public override void SetWorldTransform(Matrix transform)
        {
            if (mRigidBody.SetWorldTransformCallback != null)
            {
                mRigidBody.SetWorldTransformCallback(transform);
            }
        }
    }
}