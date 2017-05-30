// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Graphics.Regression
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class RegressionTestAttribute : System.Attribute
    {
        private int frameIndex;

        public RegressionTestAttribute(int frame)
        {
            frameIndex = frame;
        }
    }
}
