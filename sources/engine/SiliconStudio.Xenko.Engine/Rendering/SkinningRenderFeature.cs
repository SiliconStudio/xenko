using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Computes and uploads skinning info.
    /// </summary>
    public class SkinningRenderFeature : SubRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private ObjectPropertyKey<RenderModelFrameInfo> renderModelObjectInfoKey;

        private ConstantBufferOffsetReference blendMatrices;

        private readonly FastList<NodeFrameInfo> nodeInfos = new FastList<NodeFrameInfo>();

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

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            renderModelObjectInfoKey = RootRenderFeature.RenderData.CreateObjectKey<RenderModelFrameInfo>();
            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;

            blendMatrices = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(TransformationSkinningKeys.BlendMatrixArray.Name);
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];
                    var renderMesh = (RenderMesh)renderObject;

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    if (renderMesh.Mesh.Skinning != null)
                    {
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasSkinningPosition, renderMesh.Mesh.Parameters.Get(MaterialKeys.HasSkinningPosition));
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasSkinningNormal, renderMesh.Mesh.Parameters.Get(MaterialKeys.HasSkinningNormal));
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasSkinningTangent, renderMesh.Mesh.Parameters.Get(MaterialKeys.HasSkinningTangent));

                        var skinningBones = Math.Max(MaxBones, renderMesh.Mesh.Skinning.Bones.Length);
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.SkinningMaxBones, skinningBones);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            var renderModelObjectInfo = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            nodeInfos.Clear();

            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;

                var skeleton = renderMesh.RenderModel.ModelComponent.Skeleton;
                var skinning = renderMesh.Mesh.Skinning;

                // Skip unskinned meshes
                if (skinning == null)
                {
                    renderModelObjectInfo[objectNodeReference] = new RenderModelFrameInfo();
                    continue;
                }

                var bones = skinning.Bones;
                var boneCount = bones.Length;

                // Reserve space in the node buffer
                renderModelObjectInfo[objectNodeReference] = new RenderModelFrameInfo
                {
                    NodeInfoOffset = nodeInfos.Count,
                    NodeInfoCount = boneCount
                };

                // Ensure buffer capacity
                nodeInfos.EnsureCapacity(nodeInfos.Count + boneCount);

                // Copy matrices
                for (int index = 0; index < boneCount; index++)
                {
                    var nodeIndex = bones[index].NodeIndex;

                    nodeInfos.Add(new NodeFrameInfo
                    {
                        LinkToMeshMatrix = bones[index].LinkToMeshMatrix,
                        NodeTransformation = skeleton.NodeTransformations[nodeIndex].WorldMatrix
                    });
                }
            }
        }

        /// <inheritdoc/>
        public override unsafe void Prepare(RenderDrawContext context)
        {
            var renderModelObjectInfoData = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).RenderNodes)
            {
                var perDrawLayout = renderNode.RenderEffect.Reflection.PerDrawLayout;
                if (perDrawLayout == null)
                    continue;

                var blendMatricesOffset = perDrawLayout.GetConstantBufferOffset(blendMatrices);
                if (blendMatricesOffset == -1)
                    continue;

                var renderModelObjectInfo = renderModelObjectInfoData[renderNode.RenderObject.ObjectNode];

                var mappedCB = renderNode.Resources.ConstantBuffer.Data + blendMatricesOffset;
                var blendMatrix = (Matrix*)mappedCB;

                for (int i = 0; i < renderModelObjectInfo.NodeInfoCount; i++)
                {
                    int boneInfoIndex = renderModelObjectInfo.NodeInfoOffset + i;
                    Matrix.Multiply(ref nodeInfos.Items[boneInfoIndex].LinkToMeshMatrix, ref nodeInfos.Items[boneInfoIndex].NodeTransformation, out *blendMatrix++);
                }
            }
        }
    }
}