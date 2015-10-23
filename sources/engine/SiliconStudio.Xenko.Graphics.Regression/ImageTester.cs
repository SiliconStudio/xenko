// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public static partial class ImageTester
    {
        public const string XenkoImageServerHost = "XenkoBuild.siliconstudio.co.jp";
        public const int XenkoImageServerPort = 1832;

        public static ImageTestResultConnection ImageTestResultConnection = PlatformPermutator.GetDefaultImageTestResultConnection();
    }
}