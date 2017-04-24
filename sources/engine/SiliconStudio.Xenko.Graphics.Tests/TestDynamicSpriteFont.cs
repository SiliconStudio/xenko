// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
