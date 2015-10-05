using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
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
            return new Data
            {
                SpriteStudioComponent = entity.Get<SpriteStudioComponent>(),
                TransformComponent = entity.Transform,
            };
        }

        protected override void UpdateAssociatedData(Entity entity, ref Data associatedData)
        {
            associatedData.AnimationComponent = entity.Get<AnimationComponent>();
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

            //check if the sheet name dictionary has already been populated
            if (spriteStudioComponent.Sheet.Sprites == null)
            {
                spriteStudioComponent.Sheet.Sprites = new Dictionary<int, Sprite>();
                var index = 0;
                foreach (var sprite in spriteStudioComponent.Sheet.SpriteSheet.Sprites)
                {
                    spriteStudioComponent.Sheet.Sprites.Add(index, sprite);
                    index++;
                }
            }

            foreach (var node in nodes)
            {
                var nodeState = new SpriteStudioNodeState
                {
                    CurrentXyPrioAngle = node.BaseState.CurrentXyPrioAngle,
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

                Sprite sprite;
                nodeState.Sprite = spriteStudioComponent.Sheet.Sprites.TryGetValue(nodeState.SpriteId, out sprite) ? sprite : null;

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

        private static unsafe void UpdateNodes(IEnumerable<SpriteStudioNodeState> nodes, Data data)
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
                            var structureData = (float*)(bytes + channel.Offset);
                            if(structureData == null) continue;
                            if (structureData[0] == 0.0f) continue;

                            var valueFloat = *(structureData + 1);
                            var valueInt = *((int*)structureData + 1);
                            var valueVector4 = *((Vector4*)structureData + 1);

                            if (channel.PropertyName.StartsWith("posx"))
                            {
                                node.CurrentXyPrioAngle.X = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("posy"))
                            {
                                node.CurrentXyPrioAngle.Y = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("prio"))
                            {
                                node.CurrentXyPrioAngle.Z = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("rotz"))
                            {
                                node.CurrentXyPrioAngle.W = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("sclx"))
                            {
                                node.Scale.X = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("scly"))
                            {
                                node.Scale.Y = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("alph"))
                            {
                                node.Transparency = valueFloat;
                            }
                            else if (channel.PropertyName.StartsWith("hide"))
                            {
                                node.Hide = valueInt != 0;
                            }
                            else if (channel.PropertyName.StartsWith("flph"))
                            {
                                node.HFlipped = valueInt != 0;
                            }
                            else if (channel.PropertyName.StartsWith("flpv"))
                            {
                                node.VFlipped = valueInt != 0;
                            }
                            else if (channel.PropertyName.StartsWith("cell"))
                            {
                                var spriteIndex = valueInt;
                                node.SpriteId = spriteIndex;
                                Sprite sprite;
                                node.Sprite = data.SpriteStudioComponent.Sheet.Sprites.TryGetValue(spriteIndex, out sprite) ? sprite : null;
                            }
                            else if (channel.PropertyName.StartsWith("colb"))
                            {
                                node.BlendType = (SpriteStudioBlending)valueInt;
                            }
                            else if (channel.PropertyName.StartsWith("colv"))
                            {
                                Utilities.Read((IntPtr)(structureData + 1), ref node.BlendColor);
                            }
                            else if (channel.PropertyName.StartsWith("colf"))
                            {
                                node.BlendFactor = valueFloat;
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