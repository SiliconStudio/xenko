using System;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Computes and uploads skinning info.
    /// </summary>
    public class SkinningRenderFeature : SubRenderFeature
    {
        private const int DefaultBufferSize = 128;

        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;
        private StaticObjectPropertyKey<SkinningInfo> skinningInfoKey;
        private ObjectPropertyKey<RenderModelFrameInfo> renderModelObjectInfoKey;

        private ConstantBufferOffsetReference blendMatrices;

        //private readonly FastList<NodeFrameInfo> nodeInfos = new FastList<NodeFrameInfo>();
        private readonly object nodeInfoLock = new object();
        private NodeFrameInfo[] nodeInfos = new NodeFrameInfo[DefaultBufferSize];
        private int nodeInfoCount;

        // Good number for low profiles?
        public int MaxBones { get; set; } = 56;

        struct NodeFrameInfo
        {
            // Copied during Extract
            public Matrix LinkToMeshMatrix;

            public Matrix NodeTransformation;
        }

        struct RenderModelFrameInfo
        {
            public int NodeInfoOffset;

            public int NodeInfoCount;
        }

        struct SkinningInfo
        {
            public ParameterCollection Parameters;
            public int PermutationCounter;

            public bool HasSkinningPosition;
            public bool HasSkinningNormal;
            public bool HasSkinningTangent;
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            renderModelObjectInfoKey = RootRenderFeature.RenderData.CreateObjectKey<RenderModelFrameInfo>();
            skinningInfoKey = RootRenderFeature.RenderData.CreateStaticObjectKey<SkinningInfo>();
            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;

            blendMatrices = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(TransformationSkinningKeys.BlendMatrixArray.Name);
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            var skinningInfos = RootRenderFeature.RenderData.GetData(skinningInfoKey);

            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            //foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            Dispatcher.ForEach(((RootEffectRenderFeature)RootRenderFeature).ObjectNodeReferences, objectNodeReference =>
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;
                var staticObjectNode = renderMesh.StaticObjectNode;

                var skinningInfo = skinningInfos[staticObjectNode];
                var parameters = renderMesh.Mesh.Parameters;
                if (parameters != skinningInfo.Parameters || parameters.PermutationCounter != skinningInfo.PermutationCounter)
                {
                    skinningInfo.Parameters = parameters;
                    skinningInfo.PermutationCounter = parameters.PermutationCounter;

                    skinningInfo.HasSkinningPosition = parameters.Get(MaterialKeys.HasSkinningPosition);
                    skinningInfo.HasSkinningNormal = parameters.Get(MaterialKeys.HasSkinningNormal);
                    skinningInfo.HasSkinningTangent = parameters.Get(MaterialKeys.HasSkinningTangent);

                    skinningInfos[staticObjectNode] = skinningInfo;
                }

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    if (renderMesh.Mesh.Skinning != null)
                    {
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasSkinningPosition, skinningInfo.HasSkinningPosition);
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasSkinningNormal, skinningInfo.HasSkinningNormal);
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasSkinningTangent, skinningInfo.HasSkinningTangent);

                        var skinningBones = Math.Max(MaxBones, renderMesh.Mesh.Skinning.Bones.Length);
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.SkinningMaxBones, skinningBones);
                    }
                }
            });
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            var renderModelObjectInfo = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            nodeInfoCount = 0;

            //foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            Dispatcher.ForEach(RootRenderFeature.ObjectNodeReferences, objectNodeReference =>
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;

                var skeleton = renderMesh.RenderModel.ModelComponent.Skeleton;
                var skinning = renderMesh.Mesh.Skinning;

                // Skip unskinned meshes
                if (skinning == null)
                {
                    renderModelObjectInfo[objectNodeReference] = new RenderModelFrameInfo();
                    return;
                }

                var bones = skinning.Bones;
                var boneCount = bones.Length;

                // Reserve space in the node buffer
                var newNodeInfoCount = Interlocked.Add(ref nodeInfoCount, boneCount);
                var nodeInfoOffset = newNodeInfoCount - boneCount;

                renderModelObjectInfo[objectNodeReference] = new RenderModelFrameInfo
                {
                    NodeInfoOffset = nodeInfoOffset,
                    NodeInfoCount = boneCount
                };

                // Ensure buffer capacity
                if (nodeInfos.Length < newNodeInfoCount)
                {
                    lock (nodeInfoLock)
                    {
                        if (nodeInfos.Length < newNodeInfoCount)
                            Array.Resize(ref nodeInfos, Math.Max(newNodeInfoCount, nodeInfos.Length * 2));
                    }
                }

                var nodeTransformations = skeleton.NodeTransformations;
                for (int index = 0; index < boneCount; index++)
                {
                    var nodeIndex = bones[index].NodeIndex;

                    nodeInfos[nodeInfoOffset + index] = new NodeFrameInfo
                    {
                        LinkToMeshMatrix = bones[index].LinkToMeshMatrix,
                        NodeTransformation = nodeTransformations[nodeIndex].WorldMatrix
                    };
                }
            });
        }

        /// <inheritdoc/>
        public override unsafe void Prepare(RenderDrawContext context)
        {
            var renderModelObjectInfoData = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            //foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).RenderNodes)
            Dispatcher.ForEach(((RootEffectRenderFeature)RootRenderFeature).RenderNodes, renderNode =>
            {
                var perDrawLayout = renderNode.RenderEffect.Reflection.PerDrawLayout;
                if (perDrawLayout == null)
                    return;

                var blendMatricesOffset = perDrawLayout.GetConstantBufferOffset(blendMatrices);
                if (blendMatricesOffset == -1)
                    return;

                var renderModelObjectInfo = renderModelObjectInfoData[renderNode.RenderObject.ObjectNode];

                var mappedCB = renderNode.Resources.ConstantBuffer.Data + blendMatricesOffset;
                var blendMatrix = (Matrix*)mappedCB;

                for (int i = 0; i < renderModelObjectInfo.NodeInfoCount; i++)
                {
                    int boneInfoIndex = renderModelObjectInfo.NodeInfoOffset + i;
                    Matrix.Multiply(ref nodeInfos[boneInfoIndex].LinkToMeshMatrix, ref nodeInfos[boneInfoIndex].NodeTransformation, out *blendMatrix++);
                }
            });
        }
    }
}