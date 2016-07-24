// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;


namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    [Description("Check Dynamic Font")]
    public class TestDynamicSpriteFont : TestSpriteFont
    {
        public TestDynamicSpriteFont()
            : base("DynamicFonts/", "dyn")
        {
            CurrentVersion = 7; // Font names & sizes changed slightly
        }

        public static void Main()
        {
            using (var game = new TestDynamicSpriteFont())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestDynamicSpriteFont()
        {
            RunGameTest(new TestDynamicSpriteFont());
        }
    }
}
