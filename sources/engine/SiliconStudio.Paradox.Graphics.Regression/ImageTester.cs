// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Graphics.Regression
{
    public static partial class ImageTester
    {
        public const string ParadoxImageServerHost = "ParadoxBuild.siliconstudio.co.jp";
        public const int ParadoxImageServerPort = 1832;

        public static ImageTestResultConnection ImageTestResultConnection = PlatformPermutator.GetDefaultImageTestResultConnection();
    }
}