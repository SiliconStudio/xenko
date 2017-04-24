// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID
using NUnit.Framework.Internal;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public class TestNamespace : TestSuite
    {
        public TestNamespace(string @namespace)
            : base(@namespace)
        {
        }

        public override string TestType
        {
            get { return "Namespace"; }
        }
    }
}
#endif
