using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a way to sort RenderObject.
    /// </summary>
    [DataContract("SortMode")]
    public abstract class SortMode
    {
        public abstract unsafe void GenerateSortKey(RenderView renderView, RenderViewStage renderViewStage, SortKey* sortKeys);
    }

    public abstract class SortModeDistance : SortMode
    {
        private bool reverseDistance;
        protected int distancePosition = 32;
        protected int distancePrecision = 16;

        protected int statePosition = 0;
        protected int statePrecision = 32;

        protected SortModeDistance(bool reverseDistance)
        {
            this.reverseDistance = reverseDistance;
        }

        public static unsafe uint ComputeDistance(float distance)
        {
            // Compute uint sort key (http://aras-p.info/blog/2014/01/16/rough-sorting-by-depth/)
            var distanceI = *((uint*)&distance);
            return ((uint)(-(int)(distanceI >> 31)) | 0x80000000) ^ distanceI;
        }

        public static SortKey CreateSortKey(float distance)
        {
            var distanceI = ComputeDistance(distance);

            return new SortKey { Value = distanceI };
        }

        public override unsafe void GenerateSortKey(RenderView renderView, RenderViewStage renderViewStage, SortKey* sortKeys)
        {
            Matrix viewInverse = renderView.View;
            viewInverse.Invert();
            var plane = new Plane(viewInverse.Forward, Vector3.Dot(viewInverse.TranslationVector, viewInverse.Forward)); // TODO: Point-normal-constructor seems wrong. Check.

            var renderNodes = renderViewStage.RenderNodes;

            int distanceShift = 32 - distancePrecision;
            int stateShift = 32 - statePrecision;

            for (int i = 0; i < renderNodes.Count; ++i)
            {
                var renderNode = renderNodes[i];

                var renderObject = renderNode.RenderObject;
                var distance = CollisionHelper.DistancePlanePoint(ref plane, ref renderObject.BoundingBox.Center);
                var distanceI = ComputeDistance(distance);
                if (reverseDistance)
                    distanceI = ~distanceI;

                // Compute sort key
                sortKeys[i] = new SortKey { Value = ((ulong)renderNode.RootRenderFeature.SortKey << 56) | ((ulong)(distanceI >> distanceShift) << distancePosition) | ((ulong)(renderObject.StateSortKey >> stateShift) << statePosition), Index = i };
            }
        }
    }

    /// <summary>
    /// Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] RenderObject states 32 bits] [Distance front to back 16 bits]
    /// </summary>
    [DataContract("SortModeStateChange")]
    public class StateChangeSortMode : SortModeDistance
    {
        public StateChangeSortMode() : base(false)
        {
            statePosition = 32;
            distancePosition = 0;
        }
    }

    /// <summary>
    /// Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] [Distance front to back 16 bits] [RenderObject states 32 bits]
    /// </summary>
    [DataContract("FrontToBackSortMode")]
    public class FrontToBackSortMode : SortModeDistance
    {
        public FrontToBackSortMode() : base(false)
        {
        }
    }

    /// <summary>
    /// Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] [Distance back to front 32 bits] [RenderObject states 24 bits]
    /// </summary>
    [DataContract("BackToFrontSortMode")]
    public class BackToFrontSortMode : SortModeDistance
    {
        public BackToFrontSortMode() : base(true)
        {
            distancePrecision = 32;
            distancePosition = 24;

            statePrecision = 24;
        }
    }
}