// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    /// <summary>
    /// DEPRECATED. Precompiled fonts are not supported anymore and will be merged as a feature of the other fonts (Offline/SDF) soon
    /// </summary>
    public class TestPrecompiledSpriteFont : TestSpriteFont
    {
        public TestPrecompiledSpriteFont()
            : base("PrecompiledFonts/", "pre")
        {
            CurrentVersion = 7; // Font names & sizes changed slightly
        }

        public static void Main()
        {
            using (var game = new TestPrecompiledSpriteFont())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestPrecompiledSpriteFont()
        {
            RunGameTest(new TestPrecompiledSpriteFont());
        }
    }
}