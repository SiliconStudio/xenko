using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    public class SpriteStudioProcessor : EntityProcessor<SpriteStudioComponent, RenderSpriteStudio>, IEntityComponentRenderProcessor
    {
        public SpriteStudioProcessor()
            : base(typeof(TransformComponent))
        {
            Order = 550;
        }

        protected override RenderSpriteStudio GenerateComponentData(Entity entity, SpriteStudioComponent component)
        {
            return new RenderSpriteStudio
            {
                SpriteStudioComponent = component,
                TransformComponent = entity.Transform
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, SpriteStudioComponent component, RenderSpriteStudio associatedData)
        {
            return
                component == associatedData.SpriteStudioComponent &&
                entity.Transform == associatedData.TransformComponent;
        }

        protected override void OnEntityComponentAdding(Entity entity, SpriteStudioComponent component, RenderSpriteStudio data)
        {
            PrepareNodes(data);
            VisibilityGroup.RenderObjects.Add(data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, SpriteStudioComponent component, RenderSpriteStudio data)
        {
            data.SpriteStudioComponent.Nodes.Clear();
            VisibilityGroup.RenderObjects.Remove(data);
        }

        internal static SpriteStudioNodeState InitializeNodes(SpriteStudioComponent spriteStudioComponent)
        {
            spriteStudioComponent.Nodes.Clear();

            var nodes = spriteStudioComponent.Sheet?.NodesInfo;
            if (nodes == null)
                return null;

            //check if the sheet name dictionary has already been populated
            if (spriteStudioComponent.Sheet.Sprites == null)
            {
                spriteStudioComponent.Sheet.Sprites = new Sprite[spriteStudioComponent.Sheet.SpriteSheet.Sprites.Count];
                for (int i = 0; i < spriteStudioComponent.Sheet.SpriteSheet.Sprites.Count; i++)
                {
                    spriteStudioComponent.Sheet.Sprites[i] = spriteStudioComponent.Sheet.SpriteSheet.Sprites[i];
                }
            }

            foreach (var node in nodes)
            {
                var nodeState = new SpriteStudioNodeState
                {
                    Position = node.BaseState.Position,
                    RotationZ = node.BaseState.RotationZ,
                    Priority = node.BaseState.Priority,
                    Scale = node.BaseState.Scale,
                    Transparency = node.BaseState.Transparency,
                    Hide = node.BaseState.Hide,
                    BaseNode = node,
                    HFlipped = node.BaseState.HFlipped,
                    VFlipped = node.BaseState.VFlipped,
                    SpriteId = node.BaseState.SpriteId,
                    BlendColor = node.BaseState.BlendColor,
                    BlendType = node.BaseState.BlendType,
                    BlendFactor = node.BaseState.BlendFactor
                };

                nodeState.Sprite = nodeState.SpriteId != -1 ? spriteStudioComponent.Sheet.Sprites[nodeState.SpriteId] : null;

                spriteStudioComponent.Nodes.Add(nodeState);
            }

            SpriteStudioNodeState rootNode = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var nodeState = spriteStudioComponent.Nodes[i];
                var nodeAsset = nodes[i];

                if (nodeAsset.ParentId == -1)
                {
                    rootNode = nodeState;
                }
                else
                {
                    nodeState.ParentNode = spriteStudioComponent.Nodes.FirstOrDefault(x => x.BaseNode.Id == nodeAsset.ParentId);
                }

                foreach (var subNode in spriteStudioComponent.Nodes.Where(subNode => subNode.BaseNode.ParentId == nodeAsset.Id))
                {
                    nodeState.ChildrenNodes.Add(subNode);
                }
            }

            return rootNode;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        // Enumerables are Evil
        private static void SortNodes(RenderSpriteStudio data, List<SpriteStudioNodeState> nodes)
        {
            data.SpriteStudioComponent.SortedNodes.Clear();
            var sortedNodes = nodes.OrderBy(x => x.Priority);
            foreach (var node in sortedNodes)
            {
                data.SpriteStudioComponent.SortedNodes.Add(node);
            }
        }

        public override void Draw(RenderContext context)
        {
            foreach (var spriteStateKeyPair in ComponentDatas)
            {
                var renderSpriteStudio = spriteStateKeyPair.Value;
                renderSpriteStudio.Enabled = renderSpriteStudio.SpriteStudioComponent.Enabled;

                if(!renderSpriteStudio.Enabled) continue;

                renderSpriteStudio.RenderGroup = renderSpriteStudio.SpriteStudioComponent.Entity.Group;

                if (!PrepareNodes(renderSpriteStudio))
                    continue;

                SortNodes(renderSpriteStudio, renderSpriteStudio.SpriteStudioComponent.Nodes);
                renderSpriteStudio.RootNode.UpdateTransformation();
            }
        }

        private static bool PrepareNodes(RenderSpriteStudio data)
        {
            var sheet = data.SpriteStudioComponent.Sheet;
            if (data.Sheet != sheet) // sheet changed? force pre-process
            {
                data.RootNode = null;
                data.SpriteStudioComponent.Nodes.Clear();
            }

            var assetNodes = sheet?.NodesInfo;
            if (assetNodes == null) return false;

            if (data.RootNode == null)
            {
                data.RootNode = InitializeNodes(data.SpriteStudioComponent);
                data.Sheet = sheet;
            }

            return (data.RootNode != null);
        }

        public VisibilityGroup VisibilityGroup { get; set; }
    }
}