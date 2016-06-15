using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SpriteEntity
{
    public class BeamScript : SyncScript
    {
        /// <summary>
        /// Direction of the beam along the X axis
        /// </summary>
        public float DirectionX { get; set; }

        /// <summary>
        /// Tells whether this bullet is alive.
        /// </summary>
        public bool IsAlive { get; set; }

        public SpriteSheet SpriteSheet;

        private const float beamSpeed = 14f;
        private const float maxWidthX = 8f + 2f;

        // TODO centralize
        private const float minWidthX = -8f - 2f;

        private Sprite beamNormalSprite;

        public BeamScript()
        {
            DirectionX = 1f;
            IsAlive = true;
        }

        public override void Update()
        {
            if (!IsAlive) 
                return;

            // Move
            Entity.Transform.Position.X += DirectionX * beamSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Entity went out the screen, mark it as dead
            if ((Entity.Transform.Position.X <= minWidthX) || (Entity.Transform.Position.X >= maxWidthX))
            {
                IsAlive = false;
            }
        }

        public RectangleF GetBoundingBox()
        {
            if (beamNormalSprite == null)
                beamNormalSprite = SpriteSheet["bullet"];

            var size = beamNormalSprite.SizeInPixels * LogicScript.ScreenScale;
            return new RectangleF(0, 0, size.X, size.Y);
        }
    }
}
