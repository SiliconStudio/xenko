// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Audio.Tests.Engine
{
    /// <summary>
    /// Test the <see cref="ScriptContext"/> class augmented with the audio services.
    /// </summary>
    [TestFixture]
    class TestScriptContext
    {
        /// <summary>
        /// Utility class inheriting from <see cref="ScriptContext"/> used to perform our tests.
        /// </summary>
        class ScriptClass: ScriptContext
        {
            public ScriptClass(IServiceRegistry registry) : base(registry)
            {
            }

            public void AccessAudioService()
            {
                var audio = Audio;
            }

            public bool AudioServiceNotNull()
            {
                return Audio != null;
            }
        }

        /// <summary>
        /// Check that there are no problems during context construction and destruction.
        /// </summary>
        [Test]
        public void TestScriptCreationDestruction()
        {
            TestUtilities.CreateAndRunGame(TestScriptCreationDestructionImpl, TestUtilities.ExitGame);
        }

        private void TestScriptCreationDestructionImpl(Game game)
        {
            ScriptClass script = null;
            Assert.DoesNotThrow(() => script = new ScriptClass(game.Services), "Creation of the Script failed");
            Assert.DoesNotThrow(() => script.Dispose(), "Destruction of the script failed.");
        }

        /// <summary>
        /// Check that we can access to the audio service and that the service is valid.
        /// </summary>
        [Test]
        public void TestScriptAudioAccess()
        {
            TestUtilities.CreateAndRunGame(TestScriptAudioAccessImpl, TestUtilities.ExitGame);
        }

        private static void TestScriptAudioAccessImpl(Game game)
        {
            using (var script = new ScriptClass(game.Services))
            {
                Assert.DoesNotThrow(() => script.AccessAudioService(), "Access to the audio service failed.");
                Assert.IsTrue(script.AudioServiceNotNull(), "The Audio service is null.");
            }
        }
    }
}
