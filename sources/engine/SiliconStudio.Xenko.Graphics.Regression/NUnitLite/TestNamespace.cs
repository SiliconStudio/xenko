// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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