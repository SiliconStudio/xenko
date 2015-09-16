using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    public class SpriteStudioProcessor : EntityProcessor<SpriteStudioProcessor.Data>
    {
        public readonly List<Data> Sprites = new List<Data>();

        public SpriteStudioProcessor()
            : base(SpriteStudioComponent.Key, TransformComponent.Key)
        {
        }

        public class Data
        {
            public SpriteStudioComponent SpriteStudioComponent;
            public TransformComponent TransformComponent;
            public AnimationComponent AnimationComponent;
            public SpriteStudioNodeState RootNode;
            public SpriteStudioSheet Sheet;
        }

        protected override Data GenerateAssociatedData(Entity entity)
        {
            var data = new Data
            {
                SpriteStudioComponent = entity.Get<SpriteStudioComponent>(),
                TransformComponent = entity.Transform,
                AnimationComponent = entity.Get<AnimationComponent>()
            };
            return data;
        }

        protected override void OnEntityAdding(Entity entity, Data data)
        {
            PrepareNodes(data);
        }

        protected override void OnEntityRemoved(Entity entity, Data data)
        {
            data.SpriteStudioComponent.Nodes.Clear();
        }

        internal static SpriteStudioNodeState InitializeNodes(SpriteStudioComponent spriteStudioComponent)
        {
            spriteStudioComponent.Nodes.Clear();

            var nodes = spriteStudioComponent.Sheet?.NodesInfo;
            if (nodes == null)
                return null;

            foreach (var node in nodes)
            {
                var nodeState = new SpriteStudioNodeState
                {
                    CurrentXyPrioAngle = node.BaseXyPrioAngle,
                    Sprite = node.Sprite,
                    BaseNode = node
                };
                spriteStudioComponent.Nodes.Add(nodeState);
            }

            SpriteStudioNodeState rootNode = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var nodeState = spriteStudioComponent.Nodes[i];
                var nodeAsset = nodes[i];

                if (nodeAsset.Id == -1)
                {
                    rootNode = nodeState;
                }
                else if (nodeAsset.Id > -1)
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

        private static unsafe void UpdateNodes(IList<SpriteStudioNodeState> nodes, Data data)
        {
            //foreach (var node in nodes)
            //{
            //    node.CurrentXyPrioAngle = node.BaseNode.BaseXyPrioAngle;
            //}

            var animComp = data.AnimationComponent;
            if (animComp != null && animComp.PlayingAnimations.Count > 0 && animComp.CurrentFrameResult != null)
            {
                fixed (byte* bytes = animComp.CurrentFrameResult.Data)
                {
                    foreach (var node in nodes)
                    {
                        //Process animations
                        var results = animComp.CurrentFrameResult;
                        var channels = results.Channels.Where(x => x.NodeName == node.BaseNode.Name);
                        foreach (var channel in channels)
                        {
                            if((channel.Offset + (sizeof(float)*2)) > animComp.CurrentFrameResult.Data.Length) throw new Exception("SpriteStudio anim data corruption.");
                            var structureData = (float*)(bytes + channel.Offset);
                            if(structureData == null) continue;
                            if (structureData[0] == 0.0f) continue;

                            var value = structureData[1];

                            if (channel.PropertyName.StartsWith("posx"))
                            {
                                node.CurrentXyPrioAngle.X = value;
                            }
                            else if (channel.PropertyName.StartsWith("posy"))
                            {
                                node.CurrentXyPrioAngle.Y = value;
                            }
                            else if (channel.PropertyName.StartsWith("prio"))
                            {
                                node.CurrentXyPrioAngle.Z = value;
                            }
                            else if (channel.PropertyName.StartsWith("angl"))
                            {
                                node.CurrentXyPrioAngle.W = value;
                            }
                        }
                    }
                }
            }
        }

        private static void SortNodes(Data data, IEnumerable<SpriteStudioNodeState> nodes)
        {
            // TODO: Avoid reallocating
            data.SpriteStudioComponent.SortedNodes = nodes.OrderBy(x => x.CurrentXyPrioAngle.Z).ToList();
        }

        public override void Draw(RenderContext context)
        {
            Sprites.Clear();
            foreach (var spriteStateKeyPair in enabledEntities)
            {
                if (!PrepareNodes(spriteStateKeyPair.Value))
                    continue;

                UpdateNodes(spriteStateKeyPair.Value.SpriteStudioComponent.Nodes, spriteStateKeyPair.Value);
                SortNodes(spriteStateKeyPair.Value, spriteStateKeyPair.Value.SpriteStudioComponent.Nodes);
                spriteStateKeyPair.Value.RootNode.UpdateTransformation();
                Sprites.Add(spriteStateKeyPair.Value);
            }
        }

        private static bool PrepareNodes(Data data)
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
    }
}