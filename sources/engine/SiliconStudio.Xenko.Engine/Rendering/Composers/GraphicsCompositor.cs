using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataSerializerGlobal(typeof(ReferenceSerializer<GraphicsCompositor>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<GraphicsCompositor>))]
    [DataContract]
    // Needed for indirect serialization of RenderSystem.RenderStages and RenderSystem.RenderFeatures
    // TODO: we would like an attribute to specify that serializing through the interface type is fine in this case (bypass type detection)
    [DataSerializerGlobal(null, typeof(FastTrackingCollection<RenderStage>))]
    [DataSerializerGlobal(null, typeof(FastTrackingCollection<RootRenderFeature>))]
    public class GraphicsCompositor : RendererBase
    {
        [Obsolete]
        public ISceneGraphicsCompositor Instance { get; set; }

        /// <summary>
        /// Gets the render system used with this graphics compositor.
        /// </summary>
        [DataMemberIgnore]
        public RenderSystem RenderSystem { get; } = new RenderSystem();

        /// <summary>
        /// Gets the cameras used by this composition.
        /// </summary>
        /// <value>The cameras.</value>
        /// <userdoc>The list of cameras used in the graphic pipeline</userdoc>
        [DataMember(10)]
        [Category]
        [MemberCollection(NotNullItems = true)]
        public SceneCameraSlotCollection Cameras { get; } = new SceneCameraSlotCollection();

        /// <summary>
        /// The list of render stages.
        /// </summary>
        [DataMember(20)]
        [Category]
        [MemberCollection(NotNullItems = true)]
        public IList<RenderStage> RenderStages => RenderSystem.RenderStages;

        /// <summary>
        /// The list of render features.
        /// </summary>
        [DataMember(30)]
        [Category]
        [MemberCollection(NotNullItems = true)]
        public IList<RootRenderFeature> RenderFeatures => RenderSystem.RenderFeatures;

        /// <summary>
        /// The code and values defined by this graphics compositor.
        /// </summary>
        public ISceneRenderer TopLevel { get; set; }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem.Initialize(Context);
        }

        /// <inheritdoc/>
        protected override void Destroy()
        {
            // Dispose renderers
            TopLevel?.Dispose();

            RenderSystem.Dispose();

            base.Destroy();
        }

        /// <inheritdoc/>
        protected override void DrawCore(RenderDrawContext context)
        {
            if (TopLevel != null)
            {
                // Get or create VisibilityGroup for this RenderSystem + SceneInstance
                var sceneInstance = SceneInstance.GetCurrent(context.RenderContext);
                var visibilityGroup = sceneInstance.GetOrCreateVisibilityGroup(RenderSystem);

                using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentVisibilityGroup, visibilityGroup))
                using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentRenderSystem, RenderSystem))
                using (context.RenderContext.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
                {
                    // Reset & cleanup
                    visibilityGroup.Reset();

                    // Clear views
                    foreach (var renderView in RenderSystem.Views)
                    {
                        renderView.RenderStages.Clear();
                    }
                    RenderSystem.Views.Clear();

                    // Set render system
                    context.RenderContext.RenderSystem = RenderSystem;
                    context.RenderContext.SceneInstance = sceneInstance;
                    context.RenderContext.VisibilityGroup = visibilityGroup;

                    // Set start states for viewports and output (it will be used during the Collect phase)
                    var renderOutputs = new RenderOutputDescription();
                    renderOutputs.CaptureState(context.CommandList);
                    context.RenderContext.RenderOutputs.Clear();
                    context.RenderContext.RenderOutputs.Push(renderOutputs);

                    var viewports = new ViewportState();
                    viewports.CaptureState(context.CommandList);
                    context.RenderContext.ViewportStates.Clear();
                    context.RenderContext.ViewportStates.Push(viewports);

                    try
                    {
                        // Collect in the game graphics compositor: Setup features/stages, enumerate views and populates VisibilityGroup
                        TopLevel.Collect(context.RenderContext);

                        // Collect in render features
                        RenderSystem.Collect(context.RenderContext);

                        // Collect visibile objects from each view (that were not properly collected previously)
                        foreach (var view in RenderSystem.Views)
                            visibilityGroup.TryCollect(view);

                        // Extract
                        RenderSystem.Extract(context.RenderContext);

                        // Prepare
                        RenderSystem.Prepare(context);

                        // Draw using the game graphics compositor
                        TopLevel.Draw(context);

                        // Flush
                        RenderSystem.Flush(context);
                    }
                    finally
                    {
                        // Reset render context data
                        RenderSystem.Reset();
                    }
                }
            }
            else
            {
                Instance?.Draw(context);
            }
        }

        // TODO GFXCOMP: Move that somewhere else; or even better: starts from user gfx compositor
        [Obsolete]
        internal static GraphicsCompositor CreateDefault(string modelEffectName, bool enablePostEffects, CameraComponent camera = null, Color4? clearColor = null)
        {
            var mainRenderStage = new RenderStage("Main", "Main") { SortMode = new StateChangeSortMode() };
            var transparentRenderStage = new RenderStage("Transparent", "Main") { SortMode = new BackToFrontSortMode() };
            var shadowCasterRenderStage = new RenderStage("ShadowMapCaster", "ShadowMapCaster") { SortMode = new FrontToBackSortMode() };

            return new GraphicsCompositor
            {
                Cameras =
                {
                    camera
                },
                RenderStages =
                {
                    mainRenderStage,
                    transparentRenderStage,
                    shadowCasterRenderStage,
                },
                RenderFeatures =
                {
                    new MeshRenderFeature
                    {
                        RenderFeatures =
                        {
                            new TransformRenderFeature(),
                            new SkinningRenderFeature(),
                            new MaterialRenderFeature(),
                            new ForwardLightingRenderFeature
                            {
                                ShadowMapRenderer = new ShadowMapRenderer
                                {
                                    Renderers =
                                    {
                                        new LightDirectionalShadowMapRenderer(),
                                        new LightSpotShadowMapRenderer(),
                                    },
                                    ShadowMapRenderStage = shadowCasterRenderStage,
                                },
                            }
                        },
                        RenderStageSelectors =
                        {
                            new MeshTransparentRenderStageSelector
                            {
                                EffectName = modelEffectName,
                                MainRenderStage = mainRenderStage,
                                TransparentRenderStage = transparentRenderStage,
                            },
                            new ShadowMapRenderStageSelector
                            {
                                EffectName = modelEffectName + ".ShadowMapCaster",
                                ShadowMapRenderStage = shadowCasterRenderStage,
                            },
                        },
                        PipelineProcessors =
                        {
                            new MeshPipelineProcessor(),
                            new ShadowMeshPipelineProcessor { ShadowMapRenderStage = shadowCasterRenderStage },
                        }
                    },
                    new SpriteRenderFeature
                    {
                        RenderStageSelectors =
                        {
                            new SpriteTransparentRenderStageSelector
                            {
                                EffectName = "Test",
                                MainRenderStage = mainRenderStage,
                                TransparentRenderStage = transparentRenderStage,
                            }
                        },
                    }
                },
                TopLevel = new CameraViewCompositor()
                {
                    Child = new TopLevelCompositor
                    {
                        ClearColor = clearColor ?? Color.CornflowerBlue,
                        UnitRenderer = new ForwardRenderer
                        {
                            MainRenderStage = mainRenderStage,
                            TransparentRenderStage = transparentRenderStage,
                            ShadowMapRenderStage = shadowCasterRenderStage,
                        },
                        PostEffects = enablePostEffects ? new PostProcessingEffects
                        {
                            ColorTransforms =
                            {
                                Transforms =
                                {
                                    new ToneMap()
                                },
                            },
                        } : null,
                    }
                },
            };
        }
    }
}