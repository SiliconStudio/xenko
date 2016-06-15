using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SpriteEntity
{
    public class EnemyScript : SyncScript
    {
        public LogicScript Logic;

        public SpriteSheet SpriteSheet;

        private const float enemyInitPositionY = 8;

        // enemy age
        private const float enemyTimeToLive = 2.4f;   // seconds
        private const float enemyTimeToWait = -2f;    // seconds
        private float enemyAge;
        // enemy position
        private const float enemyDownSpeed = 8f;
        private const float floorPosiionY = 0f;
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;
        // enemy animation
        private const float enemyActiveFps = 2f;
        private const float enemyBlowupFps = 18f;
        private SpriteComponent enemySpriteComponent;
        private SpriteSheet spriteSheet;

        // random
        private static int seed = Environment.TickCount;
        private static Random enemyRandomLocal = new Random(seed);

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float elapsedTime;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        internal bool IsAlive { get; set; }

        public override void Start()
        {
            spriteSheet = SpriteSheet;
            enemySpriteComponent = Entity.Get<SpriteComponent>();
             
            if (!IsLiveReloading)
            {
                // Register our-self to the logic to detect collision
                Logic.WatchEnemy(Entity);

                Reset();
            }
        }

        public override void Update()
        {
            elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            enemyAge += elapsedTime;

            // Wait for the appearing
            if (enemyAge < 0f)
                return;

            if (enemyAge >= enemyTimeToLive)
            {
                // Die
                Reset();
                return;
            }

            if (!IsAlive)
            {
                // Let the explosion animation play
                return;
            }

            // Moving
            Entity.Transform.Position.Y -= enemyDownSpeed * elapsedTime;
            if (Entity.Transform.Position.Y <= floorPosiionY) Entity.Transform.Position.Y = floorPosiionY;
        }

        private void Reset()
        {
            IsAlive = true;
            Entity.Transform.Position.Y = enemyInitPositionY;

            var random = enemyRandomLocal;
            // Appearance position
            Entity.Transform.Position.X = (((float)(random.NextDouble())) * gameWidthX) - gameWidthHalfX;
            // Waiting time
            enemyAge = enemyTimeToWait - (((float)(random.NextDouble())));

            enemySpriteComponent.SpriteProvider = new SpriteFromSheet { Sheet = spriteSheet };
            SpriteAnimation.Play(enemySpriteComponent, spriteSheet.FindImageIndex("active0"), spriteSheet.FindImageIndex("active1"), AnimationRepeatMode.LoopInfinite, enemyActiveFps);
        }

        public void Explode()
        {
            IsAlive = false;
            enemySpriteComponent.SpriteProvider = new SpriteFromSheet { Sheet = spriteSheet };
            SpriteAnimation.Play(enemySpriteComponent, spriteSheet.FindImageIndex("blowup0"), spriteSheet.FindImageIndex("blowup7"), AnimationRepeatMode.LoopInfinite, enemyBlowupFps);
            enemyAge = enemyTimeToLive - 0.3f;
        }

        public RectangleF GetBoundingBox()
        {
            var result = spriteSheet.Sprites.First().Region;
            result.Width *= LogicScript.ScreenScale;
            result.Height *= LogicScript.ScreenScale;
            return result;
        }
    }
}
