using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    public class TestImageEffect : TestGameBase
    {
        private ImageEffectContext imageEffectContext;

        private ColorTransformGroup colorTransformGroup;

        private float switchEffectLevel;


        public TestImageEffect()
        {
            CurrentVersion = 1;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawCustomEffect).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();


            imageEffectContext = new ImageEffectContext(this);

            colorTransformGroup = new ColorTransformGroup(imageEffectContext);

            var redColorTransform = new ColorTransform("MyCustomColorTransformShader");
            redColorTransform.Parameters.Set(MyCustomColorTransformShaderKeys.Color, Color.Maroon);
            colorTransformGroup.Transforms.Add(redColorTransform);
             
            var greenColorTransform = new ColorTransform("MyCustomColorTransformShader");
            greenColorTransform.Parameters.Set(MyCustomColorTransformShaderKeys.Color, Color.Green);
            colorTransformGroup.Transforms.Add(greenColorTransform);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawCustomEffect();
        }

        private void DrawCustomEffect()
        {
            colorTransformGroup.GammaTransform.Enabled = Input.IsKeyDown(Keys.Space);

            colorTransformGroup.SetOutput(GraphicsDevice.BackBuffer);
            colorTransformGroup.Draw();
        }

        public static void Main()
        {
            using (var game = new TestImageEffect())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunImageEffect()
        {
            RunGameTest(new TestImageEffect());
        }
    }
}