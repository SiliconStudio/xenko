using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SpriteEntity
{
    /// <summary>
    /// Watches bullets and enemies and detects collisions.
    /// </summary>
    public class LogicScript : SyncScript
    {
        public const float ScreenScale = 0.00625f;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private readonly List<Entity> bullets = new List<Entity>();

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private readonly List<Entity> enemies = new List<Entity>();

        public override void Update()
        {
            // For each bullet
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var bullet = bullets[i];
                var bulletScript = bullets[i].Get<BeamScript>();

                var bulletRectangleCollider = bulletScript.GetBoundingBox();
                bulletRectangleCollider.X = (int)bullet.Transform.Position.X - bulletRectangleCollider.Width / 2;
                bulletRectangleCollider.Y = (int)bullet.Transform.Position.Y - bulletRectangleCollider.Height / 2;

                if (bulletScript.IsAlive)
                {
                    // Checks for collision with enemies
                    foreach (var enemy in enemies)
                    {
                        var enemyScript = enemy.Get<EnemyScript>();

                        if (!enemyScript.IsAlive) continue;

                        var enemyRectangleCollider = enemyScript.GetBoundingBox();
                        enemyRectangleCollider.X = (int)enemy.Transform.Position.X - enemyRectangleCollider.Width / 2;
                        enemyRectangleCollider.Y = (int)enemy.Transform.Position.Y - enemyRectangleCollider.Height / 2;

                        if (!bulletRectangleCollider.Intersects(enemyRectangleCollider)) continue;

                        // Collision detected
                        bulletScript.IsAlive = false;
                        enemyScript.Explode();
                        break;
                    }

                }

                if (!bulletScript.IsAlive)
                {
                    // The bullet is dead, remove it
                    SceneSystem.SceneInstance.Scene.Entities.Remove(bullet);
                    bullets.Remove(bullet);
                }
            }
        }

        public override void Cancel()
        {
            if(!IsLiveReloading)
            {
                foreach (var bullet in bullets)
                    SceneSystem.SceneInstance.Scene.Entities.Remove(bullet);
            }
        }

        /// <summary>
        /// Adds a bullet we will monitor for collisions.
        /// </summary>
        /// <param name="bullet"></param>
        public void WatchBullet(Entity bullet)
        {
            bullets.Add(bullet);
        }

        /// <summary>
        /// Adds a enemy to monitor
        /// </summary>
        /// <param name="enemy"></param>
        public void WatchEnemy(Entity enemy)
        {
            enemies.Add(enemy);
        }
    }
}
