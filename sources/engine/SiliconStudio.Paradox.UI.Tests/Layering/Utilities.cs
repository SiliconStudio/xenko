// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.UI.Tests.Layering
{
    public static class Utilities
    {
        // ReSharper disable UnusedParameter.Local
        public static void AssertAreNearlyEqual(float reference, float value)
        {
            Assert.IsTrue(Math.Abs(reference - value) < 0.001f);
        }

        public static void AssertAreNearlyEqual(Vector2 reference, Vector2 value)
        {
            Assert.IsTrue((reference - value).Length() < 0.001f);
        }

        public static void AssertAreNearlyEqual(Matrix reference, Matrix value)
        {
            var diffMat = reference - value;
            for (int i = 0; i < 16; i++)
                Assert.IsTrue(Math.Abs(diffMat[i]) < 0.001);
        }
        // ReSharper restore UnusedParameter.Local 

        public static void AreExactlyEqual(Vector3 left, Vector3 right)
        {
            Assert.AreEqual(left.X, right.X);
            Assert.AreEqual(left.Y, right.Y);
            Assert.AreEqual(left.Z, right.Z);
        }
    }
}