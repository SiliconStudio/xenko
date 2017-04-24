// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Shaders;
using Keys = SiliconStudio.Xenko.Input.Keys;

namespace ScriptTest
{
    public class PickingSystem : INotifyPropertyChanged
    {
        private class SelectedEntity
        {
            public Entity Entity;
            public Vector3 PickingObjectOrigin;

            public SelectedEntity(Entity entity)
            {
                Entity = entity;
            }
        }

        private Entity translationGizmo;
        private Entity rotationGizmo;
        private EntitySystem editorEntitySystem;
        private SelectedEntity[] selectedEntities;
        private GizmoAction currentActiveGizmoActionMode;
        private Entity currentActiveGizmoEntity;

        private static readonly PropertyKey<GizmoAction> GizmoActionKey = new PropertyKey<GizmoAction>("GizmoAction", typeof(ScriptDebug));
        private static readonly PropertyKey<Color> GizmoColorKey = new PropertyKey<Color>("GizmoColor", typeof(ScriptDebug));

        public PickingSystem()
        {
            ActiveGizmoActionMode = GizmoAction.Translation;
            SelectedEntities = new Entity[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Gets or sets currently active picking mode (valid choices: None for selection, Translation, Rotation).</summary>
        public GizmoAction ActiveGizmoActionMode { get; set; }

        /// <summary>Gets or sets currently selected entity.</summary>
        public Entity[] SelectedEntities { get; set; }

        public Entity GenerateRotationGizmo()
        {
            var entity = new Entity("RotationGizmo");
            entity.Set(TransformationComponent.Key, new TransformationComponent());

            // TODO: Factorize some of this code with GenerateTranslationGizmo?
            var gizmoActions = new[] { GizmoAction.RotationX, GizmoAction.RotationY, GizmoAction.RotationZ };
            var orientationMatrices = new[] { Matrix.Identity, Matrix.RotationZ((float)Math.PI * 0.5f), Matrix.RotationY(-(float)Math.PI * 0.5f) };
            var colors = new[] { Color.Green, Color.Blue, Color.Red };

            var albedoMaterial = new ShaderMixinSource()
                {
                    "AlbedoDiffuseBase",
                    "AlbedoSpecularBase",
                    new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorFixed", MaterialKeys.DiffuseColor)),
                    new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColor")), // TODO: Default values!
                };

            for (int axis = 0; axis < 3; ++axis)
            {
                // Rendering
                var circleEffectMeshData = new EffectMeshData();
                circleEffectMeshData.Parameters = new ParameterCollection();
                circleEffectMeshData.MeshData = MeshDataHelper.CreateCircle(20.0f, 32, colors[axis]);
                circleEffectMeshData.EffectData = new EffectData("Gizmo") { AlbedoMaterial = albedoMaterial };

                var circleEntity = new Entity("ArrowCone");
                circleEntity.GetOrCreate(ModelComponent.Key).SubMeshes.Add(circleEffectMeshData);
                circleEntity.Set(TransformationComponent.Key, TransformationMatrix.CreateComponent(orientationMatrices[axis]));

                circleEntity.Set(GizmoColorKey, colors[axis]);
                circleEntity.Set(GizmoActionKey, gizmoActions[axis]);

                entity.GetOrCreate(TransformationComponent.Key).Children.Add(circleEntity.GetOrCreate(TransformationComponent.Key));
            }

            return entity;
        }

        public Entity GenerateTranslationGizmo()
        {
            var entity = new Entity("TranslationGizmo");
            entity.Set(TransformationComponent.Key, new TransformationComponent());

            var gizmoActions = new[] { GizmoAction.TranslationX, GizmoAction.TranslationY, GizmoAction.TranslationZ };
            var colors = new[] { Color.Green, Color.Blue, Color.Red };
            var orientationMatrices = new[] { Matrix.Identity, Matrix.RotationZ((float)Math.PI * 0.5f), Matrix.RotationY(-(float)Math.PI * 0.5f) };

            var albedoMaterial = new ShaderMixinSource()
                {
                    "AlbedoDiffuseBase",
                    "AlbedoSpecularBase",
                    new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorFixed", MaterialKeys.DiffuseColor)),
                    new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColor")), // TODO: Default values!
                };

            for (int axis = 0; axis < 3; ++axis)
            {
                // Rendering
                var lineEffectMeshData = new EffectMeshData();
                lineEffectMeshData.Parameters = new ParameterCollection();
                lineEffectMeshData.MeshData = MeshDataHelper.CreateLine(20.0f, colors[axis]);
                lineEffectMeshData.EffectData = new EffectData("Gizmo") { AlbedoMaterial = albedoMaterial };

                var lineEntity = new Entity("ArrowCone");
                lineEntity.GetOrCreate(ModelComponent.Key).SubMeshes.Add(lineEffectMeshData);
                lineEntity.Set(TransformationComponent.Key, TransformationMatrix.CreateComponent(orientationMatrices[axis]));

                var coneEffectMeshData = new EffectMeshData();
                coneEffectMeshData.Parameters = new ParameterCollection();
                coneEffectMeshData.MeshData = MeshDataHelper.CreateCone(2.5f, 10.0f, 10, colors[axis]);
                coneEffectMeshData.EffectData = new EffectData("Gizmo") { AlbedoMaterial = albedoMaterial };

                var coneEntity = new Entity("ArrowBody");
                coneEntity.GetOrCreate(ModelComponent.Key).SubMeshes.Add(coneEffectMeshData);
                coneEntity.Set(TransformationComponent.Key, TransformationMatrix.CreateComponent(Matrix.Translation(20.0f, 0.0f, 0.0f) * orientationMatrices[axis]));

                lineEntity.Set(GizmoColorKey, colors[axis]);
                coneEntity.Set(GizmoColorKey, colors[axis]);

                lineEntity.Set(GizmoActionKey, gizmoActions[axis]);
                coneEntity.Set(GizmoActionKey, gizmoActions[axis]);

                entity.Transformation.Children.Add(lineEntity.Transformation);
                entity.Transformation.Children.Add(coneEntity.Transformation);
            }

            return entity;
        }

        private void RefreshGizmos(GizmoAction currentGizmoAction)
        {
            var renderingSetup = RenderingSetup.Singleton;

            // Delay gizmo change until current action is finished
            if (currentActiveGizmoActionMode != ActiveGizmoActionMode
                && currentGizmoAction == GizmoAction.None)
            {
                if (currentActiveGizmoEntity != null)
                    editorEntitySystem.Entities.Remove(currentActiveGizmoEntity);

                if (ActiveGizmoActionMode == GizmoAction.Translation)
                {
                    currentActiveGizmoEntity = translationGizmo;
                }
                else if (ActiveGizmoActionMode == GizmoAction.Rotation)
                {
                    currentActiveGizmoEntity = rotationGizmo;
                }
                else
                {
                    currentActiveGizmoEntity = null;
                }
                currentActiveGizmoActionMode = ActiveGizmoActionMode;
            }

            if (currentActiveGizmoEntity == null)
                return;

            TransformationComponent[] selectedTransformations;

            if (selectedEntities != null && (selectedTransformations = selectedEntities.Select(x => x.Entity.Transformation).Where(x => x != null && x.Value is TransformationTRS).ToArray()).Length > 0)
            {
                if (!editorEntitySystem.Entities.Contains(currentActiveGizmoEntity))
                    editorEntitySystem.Add(currentActiveGizmoEntity);

                // TODO: Only act on "current" gizmo?
                var gizmoTransformation = (TransformationTRS)currentActiveGizmoEntity.Transformation.Value;
                var translationSum = Vector3.Zero;

                // Make average of selected objects origin to position gizmo
                foreach (var selectedTransformation in selectedTransformations)
                {
                    // Compensate scaling
                    // TODO: Negative/zero scaling?
                    var parentMatrix = selectedTransformation.WorldMatrix;
                    translationSum += parentMatrix.TranslationVector;
                }

                gizmoTransformation.Translation = translationSum / selectedTransformations.Length;

                // Make gizmo size constant (screen-space) -- only if not currently being used
                // TODO: Currently FOV-dependent
                if (currentGizmoAction == GizmoAction.None)
                {
                    var view = renderingSetup.MainPlugin.ViewParameters.Get(TransformationKeys.View);
                    var worldView = view;
                    var position = Vector3.TransformCoordinate(gizmoTransformation.Translation, worldView);
                    gizmoTransformation.Scaling = new Vector3(position.Z * 0.01f);
                }
            }
            else
            {
                if (editorEntitySystem.Entities.Contains(currentActiveGizmoEntity))
                    editorEntitySystem.Entities.Remove(currentActiveGizmoEntity);
            }
        }

        private void UpdateSelectedEntities(EngineContext engineContext, Entity nextSelectedEntity)
        {
            if (engineContext.InputManager.IsKeyDown(Keys.LeftShift))
            {
                // Shift pressed: Add or remove entity to current selection
                if (!selectedEntities.Any(x => x.Entity == nextSelectedEntity))
                    selectedEntities = selectedEntities.Concat(new[] { new SelectedEntity(nextSelectedEntity) }).ToArray();
                else
                    selectedEntities = selectedEntities.Where(x => x.Entity != nextSelectedEntity).ToArray();
            }
            else
            {
                // Shift not pressed: Replace selection
                selectedEntities = nextSelectedEntity != null ? new[] { new SelectedEntity(nextSelectedEntity) } : new SelectedEntity[0];
            }

            SelectedEntities = selectedEntities.Select(x => x.Entity).ToArray();
        }

        public async Task ProcessGizmoAndPicking(EngineContext engineContext)
        {
            var gizmoTargetPlugin = (RenderTargetsPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("GizmoTargetPlugin");
            if (gizmoTargetPlugin == null)
                return;

            var pickingResults = new Queue<PickingAction>();

            var effectGizmo = engineContext.RenderContext.BuildEffect("Gizmo")
                .Using(
                    new BasicShaderPlugin(
                        new ShaderMixinSource()
                            {
                                "ShaderBase",
                                "TransformationWVP",
                                "AlbedoFlatShading",
                            }) { RenderPassPlugin = gizmoTargetPlugin })
                .Using(new MaterialShaderPlugin() { RenderPassPlugin = gizmoTargetPlugin })
                //.Using(new LightingShaderPlugin() { RenderPassPlugin = renderingSetup.LightingPlugin })
                ;

            effectGizmo.PickingPassMainPlugin = gizmoTargetPlugin;

            //effectGizmo.Permutations.Set(LightingPermutation.Key, new LightingPermutation { Lights = new Light[] { new DirectionalLight { LightColor = new Color3(1.0f), LightDirection = new R32G32B32_Float(-1.0f, -1.0f, 1.0f) } } });

            engineContext.RenderContext.Effects.Add(effectGizmo);

            editorEntitySystem = new EntitySystem();
            editorEntitySystem.Processors.Add(new MeshProcessor(engineContext.RenderContext, engineContext.AssetManager));
            editorEntitySystem.Processors.Add(new HierarchicalProcessor());
            editorEntitySystem.Processors.Add(new TransformationProcessor());
            editorEntitySystem.Processors.Add(new TransformationUpdateProcessor());

            // Prepare gizmo entities
            translationGizmo = GenerateTranslationGizmo();
            rotationGizmo = GenerateRotationGizmo();
            UpdateGizmoHighlighting(GizmoAction.None);

            RenderPassPlugin renderPassPlugin;
            engineContext.DataContext.RenderPassPlugins.TryGetValue("PickingPlugin", out renderPassPlugin);
            var pickingPlugin = renderPassPlugin as PickingPlugin;
            engineContext.DataContext.RenderPassPlugins.TryGetValue("MouseOverPickingPlugin", out renderPassPlugin);
            var mouseOverPickingPlugin = renderPassPlugin as PickingPlugin;

            // States
            var pickingGizmoOrigin = new Vector3();
            var previousMouseLocation = new Vector2();
            var currentGizmoAction = GizmoAction.None;
            var nextGizmoAction = GizmoAction.None;
            Entity nextSelectedEntity = null;

            var previousMousePosition = Vector2.Zero;

            while (true)
            {
                await engineContext.Scheduler.NextFrame();

                lock (pickingResults)
                {
                    var mousePosition = engineContext.InputManager.MousePosition;
                    if (engineContext.InputManager.IsMouseButtonPressed(MouseButton.Left))
                    {
                        pickingResults.Enqueue(new PickingAction
                        {
                            Type = PickingActionType.MouseDown,
                            MouseLocation = mousePosition,
                            PickingResult = pickingPlugin.Pick(engineContext.InputManager.MousePosition),
                        });
                    }
                    else if (engineContext.InputManager.IsMouseButtonReleased(MouseButton.Left))
                    {
                        pickingResults.Enqueue(new PickingAction
                        {
                            Type = PickingActionType.MouseUp,
                            MouseLocation = mousePosition,
                        });
                    }

                    if (engineContext.InputManager.IsMouseButtonDown(MouseButton.Left) && previousMousePosition != mousePosition)
                    {
                        pickingResults.Enqueue(new PickingAction
                        {
                            Type = PickingActionType.MouseMove,
                            MouseLocation = mousePosition,
                        });
                    }

                    pickingResults.Enqueue(new PickingAction
                        {
                            Type = PickingActionType.MouseOver,
                            MouseLocation = mousePosition,
                            PickingResult = mouseOverPickingPlugin.Pick(mousePosition)
                        });

                    previousMousePosition = mousePosition;

                    while (pickingResults.Count > 0 && (pickingResults.Peek().PickingResult == null || pickingResults.Peek().PickingResult.IsCompleted))
                    {
                        // State machine handling mouse down/move/over. Everything work in async deferred (picking results only comes after a few frames due to GPU asynchronism).
                        // Possible improvements:
                        // - If user do a mouse down and no gizmo action active, it should either be gizmo action if selected item didn't change, or instant picking (no wait for mouse up) if picking changed.
                        //   Gizmo action could probably start on new entity if MouseMove happens during the same sequence.
                        var pickingAction = pickingResults.Dequeue();
                        var pickedEntity = pickingAction.PickingResult != null ? GetPickedEntity(engineContext, pickingAction.PickingResult.Result.EffectMesh) : null;
                        switch (pickingAction.Type)
                        {
                            case PickingActionType.MouseOver:
                                if (currentGizmoAction == GizmoAction.None)
                                {
                                    // Mouse over or click on gizmo: highlight the appropriate parts (yellow)
                                    nextGizmoAction = pickedEntity != null ? pickedEntity.Get(GizmoActionKey) : GizmoAction.None;
                                    UpdateGizmoHighlighting(nextGizmoAction);
                                    if (pickedEntity != null)
                                        pickingGizmoOrigin = pickingAction.PickingResult.Result.Position;
                                }
                                break;
                            case PickingActionType.MouseDown:
                                nextSelectedEntity = pickedEntity;
                                previousMouseLocation = pickingAction.MouseLocation;
                                
                                // User isn't around a "mouse over gizmo" so it is a picking selection for sure, doesn't wait for mouse move or mouse up
                                if (nextGizmoAction == GizmoAction.None)
                                {
                                    UpdateSelectedEntities(engineContext, nextSelectedEntity);

                                    // Force gizmo refresh (otherwise it won't happen as we enforce gizmo action right away, however we don't want it to be highlighted until used)
                                    RefreshGizmos(GizmoAction.None);
                                    UpdateGizmoHighlighting(currentGizmoAction);

                                    // Engage default action
                                    if (currentActiveGizmoActionMode == GizmoAction.Translation)
                                    {
                                        currentGizmoAction = GizmoAction.TranslationXY;
                                        if (pickedEntity != null)
                                            pickingGizmoOrigin = pickingAction.PickingResult.Result.Position;
                                    }
                                    else if (currentGizmoAction == GizmoAction.Rotation)
                                    {
                                        currentGizmoAction = GizmoAction.RotationZ;
                                    }
                                }

                                if (selectedEntities != null && selectedEntities.Length > 0)
                                {
                                    // Save aside picking object origin in case it turns out to be a translation
                                    foreach (var selectedEntity in selectedEntities)
                                    {
                                        var transformationComponent = selectedEntity.Entity.Transformation.Value as TransformationTRS;
                                        selectedEntity.PickingObjectOrigin = transformationComponent != null ? transformationComponent.Translation : Vector3.Zero;
                                    }
                                }
                                break;
                            case PickingActionType.MouseMove:
                                // Gizmo action just started?
                                if (currentGizmoAction == GizmoAction.None)
                                    currentGizmoAction = nextGizmoAction;
                                UpdateGizmoHighlighting(currentGizmoAction);
                                if (selectedEntities != null && selectedEntities.Length > 0)
                                {
                                    // Performs translation
                                    if ((currentGizmoAction & GizmoAction.Translation) != GizmoAction.None)
                                    {
                                        // Translation is computed from origin position during mouse down => mouse delta is not reset
                                        foreach (var selectedEntity in selectedEntities)
                                        {
                                            MoveEntity(engineContext, selectedEntity.Entity, currentGizmoAction, pickingGizmoOrigin, selectedEntity.PickingObjectOrigin, pickingAction.MouseLocation - previousMouseLocation);
                                        }
                                    }
                                    else if ((currentGizmoAction & GizmoAction.Rotation) != GizmoAction.None)
                                    {
                                        foreach (var selectedEntity in selectedEntities)
                                        {
                                            RotateEntity(engineContext, selectedEntity.Entity, currentGizmoAction, pickingAction.MouseLocation - previousMouseLocation);
                                        }
                                        // Rotation is using incremental => reset delta
                                        previousMouseLocation = pickingAction.MouseLocation;
                                    }
                                }
                                break;
                            case PickingActionType.MouseUp:
                                if (currentGizmoAction == GizmoAction.None)
                                {
                                    // Selection
                                    UpdateSelectedEntities(engineContext, nextSelectedEntity);
                                }

                                // Reset states
                                currentGizmoAction = GizmoAction.None;
                                nextGizmoAction = GizmoAction.None;
                                UpdateGizmoHighlighting(nextGizmoAction);
                                nextSelectedEntity = null;
                                break;
                        }
                    }
                }

                if (currentGizmoAction == GizmoAction.None)
                {
                    if (!EnumerableExtensions.Equals(SelectedEntities, selectedEntities != null ? selectedEntities.Select(x => x.Entity) : null))
                        selectedEntities = SelectedEntities.Select(x => new SelectedEntity(x)).ToArray();
                }

                RefreshGizmos(currentGizmoAction);
                editorEntitySystem.Update();
            }
        }

        private void MoveEntity(EngineContext engineContext, Entity entity, GizmoAction currentGizmoAction, Vector3 pickingGizmoOrigin, Vector3 pickingObjectOrigin, Vector2 delta)
        {
            var renderingSetup = RenderingSetup.Singleton;

            // Get current transformation component
            var transformationComponent = entity != null ? entity.Transformation : null;
            if (delta == Vector2.Zero || transformationComponent == null)
                return;

            var transformationComponentValues = transformationComponent.Value as TransformationTRS;
            if (transformationComponentValues == null)
                return;

            var viewParameters = renderingSetup.MainPlugin.ViewParameters;

            transformationComponentValues.Translation = pickingObjectOrigin;

            if ((currentGizmoAction & GizmoAction.Translation) != GizmoAction.None)
            {
                var axes = new Vector3[3];
                int axisCount = 0;
                if ((currentGizmoAction & GizmoAction.TranslationX) != GizmoAction.None)
                    axes[axisCount++] = Vector3.UnitX;
                if ((currentGizmoAction & GizmoAction.TranslationY) != GizmoAction.None)
                    axes[axisCount++] = Vector3.UnitY;
                if ((currentGizmoAction & GizmoAction.TranslationZ) != GizmoAction.None)
                    axes[axisCount++] = Vector3.UnitZ;

                if (axisCount == 3)
                    throw new NotImplementedException("Translation should only act on two axes.");

                var viewProj = viewParameters.Get(TransformationKeys.View) * viewParameters.Get(TransformationKeys.Projection);

                // Only one axis, we should find the best second "helper" axis to build our plan.
                // Currently, it looks for another unit axis to build that plan (the one that is the most "perpendicular" to current axis in screenspace).
                if (axisCount == 1)
                {
                    Vector3 projectedAxis;
                    Vector3.TransformNormal(ref axes[0], ref viewProj, out projectedAxis);
                    var unitX = Vector3.TransformNormal(Vector3.UnitX, viewProj);
                    var unitY = Vector3.TransformNormal(Vector3.UnitY, viewProj);
                    var unitZ = Vector3.TransformNormal(Vector3.UnitZ, viewProj);

                    // Ignore Z axis (depth)
                    projectedAxis.Z = 0.0f;
                    unitX.Z = 0.0f;
                    unitY.Z = 0.0f;
                    unitZ.Z = 0.0f;

                    // Normalize
                    projectedAxis.Normalize();
                    unitX.Normalize();
                    unitY.Normalize();
                    unitZ.Normalize();

                    var dotX = Math.Abs(Vector3.Dot(unitX, projectedAxis));
                    var dotY = Math.Abs(Vector3.Dot(unitY, projectedAxis));
                    var dotZ = Math.Abs(Vector3.Dot(unitZ, projectedAxis));

                    if (dotX < dotY && dotX < dotZ)
                        axes[1] = Vector3.UnitX;
                    else if (dotY < dotZ)
                        axes[1] = Vector3.UnitY;
                    else
                        axes[1] = Vector3.UnitZ;
                }

                var parentMatrix = transformationComponent.Parent != null ? transformationComponent.Parent.WorldMatrix : Matrix.Identity;
                parentMatrix.Invert();

                transformationComponentValues.Translation += Vector3.TransformNormal(ComputePickingDelta(engineContext, axes[0], axes[1], ref viewProj, pickingGizmoOrigin, pickingObjectOrigin, delta), parentMatrix);
                if (axisCount == 2)
                    transformationComponentValues.Translation += Vector3.TransformNormal(ComputePickingDelta(engineContext, axes[1], axes[0], ref viewProj, pickingGizmoOrigin, pickingObjectOrigin, delta), parentMatrix);
            }
        }

        private void RotateEntity(EngineContext engineContext, Entity entity, GizmoAction currentGizmoAction, Vector2 delta)
        {
            // Get current transformation component
            var transformationComponent = entity != null ? entity.Transformation : null;

            //transformationComponent.Rotation = pickingObjectRotation;

            if (delta == Vector2.Zero || transformationComponent == null)
                return;

            var transformationComponentValues = transformationComponent.Value as TransformationTRS;
            if (transformationComponentValues == null)
                return;

            Matrix currentRotationMatrix;

            Vector3 pickingGizmoRotationLocal;
            transformationComponentValues.LocalMatrix.DecomposeXYZ(out pickingGizmoRotationLocal);

            var currentRotationMatrixLocal = Matrix.RotationX(pickingGizmoRotationLocal.X)
                                           * Matrix.RotationY(pickingGizmoRotationLocal.Y)
                                           * Matrix.RotationZ(pickingGizmoRotationLocal.Z);

            var parentMatrix = transformationComponent.Parent != null ? transformationComponent.Parent.WorldMatrix : Matrix.Identity;
            var parentMatrixInverse = Matrix.Invert(parentMatrix);

            float deltaRotation = (delta.X + delta.Y) * 0.01f;

            Vector3 rotationBefore;
            currentRotationMatrixLocal.DecomposeXYZ(out rotationBefore);
           
            // Apply the rotation in parent local space
            if (currentGizmoAction == GizmoAction.RotationX)
            {
                currentRotationMatrix = currentRotationMatrixLocal * parentMatrixInverse * Matrix.RotationX(deltaRotation) * parentMatrix;
            }
            else if (currentGizmoAction == GizmoAction.RotationY)
            {
                currentRotationMatrix = currentRotationMatrixLocal * parentMatrixInverse * Matrix.RotationY(deltaRotation) * parentMatrix;
            }
            else if (currentGizmoAction == GizmoAction.RotationZ)
            {
                currentRotationMatrix = currentRotationMatrixLocal * parentMatrixInverse * Matrix.RotationZ(deltaRotation) * parentMatrix;
            }
            else
            {
                throw new NotImplementedException();
            }

            Vector3 rotationAfter;
            currentRotationMatrix.DecomposeXYZ(out rotationAfter);

            transformationComponentValues.RotationEuler += rotationAfter - rotationBefore;
        }

        private enum PickingActionType
        {
            MouseOver = 0,
            MouseDown = 1,
            MouseMove = 2,
            MouseUp = 3,
        }

        private struct PickingAction
        {
            public PickingActionType Type;
            public Vector2 MouseLocation;
            public Task<PickingPlugin.Result> PickingResult;
        }

        [Flags]
        public enum GizmoAction
        {
            None = 0,
            TranslationX = 1,
            TranslationY = 2,
            TranslationZ = 4,
            TranslationXY = TranslationX | TranslationY,
            TranslationXZ = TranslationX | TranslationZ,
            TranslationYZ = TranslationY | TranslationZ,
            Translation = TranslationX | TranslationY | TranslationZ,
            RotationX = 8,
            RotationY = 16,
            RotationZ = 32,
            Rotation = RotationX | RotationY | RotationZ,
        }

        private Entity GetPickedEntity(EngineContext engineContext, EffectMesh pickedEffectMesh)
        {
            if (pickedEffectMesh == null)
                return null;

            // Find Entity from EffectMesh
            var pickedEntity = pickedEffectMesh.Get(PickingPlugin.AssociatedEntity);
            if (pickedEntity == null)
            {
                // Iterate over both scene entities and editor entities (i.e. gizmo)
                var entities = engineContext.EntityManager.Entities.AsEnumerable();
                if (editorEntitySystem != null)
                    entities = entities.Concat(editorEntitySystem.Entities);
                foreach (var entity in entities)
                {
                    var meshComponent = entity.Get(ModelComponent.Key);
                    if (meshComponent == null)
                        continue;

                    if (!meshComponent.InstantiatedSubMeshes.Any(x => x.Value == pickedEffectMesh))
                        continue;

                    pickedEntity = entity;
                    break;
                }
            }
            return pickedEntity;
        }

        private void UpdateGizmoHighlighting(GizmoAction gizmoAction)
        {
            if (currentActiveGizmoEntity == null)
                return;

            var gizmoTranslationHierarchicalParent = currentActiveGizmoEntity.Transformation;

            foreach (var child in gizmoTranslationHierarchicalParent.Children)
            {
                var childGizmoAction = child.Entity.Get(GizmoActionKey);
                bool isActiveGizmo = ((childGizmoAction & gizmoAction) == childGizmoAction);
                child.Entity.Get(ModelComponent.Key).MeshParameters.Set(MaterialKeys.DiffuseColor, isActiveGizmo ? Color.Yellow : child.Entity.Get(GizmoColorKey));
            }
        }

        private Vector3 ComputePickingDelta(EngineContext engineContext, Vector3 axis1, Vector3 axis2, ref Matrix viewProj, Vector3 pickingGizmoOrigin, Vector3 pickingObjectOrigin, Vector2 delta)
        {
            // Build plane along which object will move
            var plane = new Plane(pickingGizmoOrigin, pickingGizmoOrigin + axis1, pickingGizmoOrigin + axis2);
            plane.Normalize();

            // Position difference in screen space to move one unit
            var pickingStartTranslationScreenSpace = Vector3.TransformCoordinate(pickingGizmoOrigin, viewProj);

            // Build mouse picking ray
            var mouseDelta = new Vector3(delta.X / engineContext.RenderContext.Width * 2.0f,
                                         -delta.Y / engineContext.RenderContext.Height * 2.0f,
                                         0.0f);
            var mousePickingPosition = pickingStartTranslationScreenSpace + mouseDelta;
            var invViewProj = Matrix.Invert(viewProj);
            mousePickingPosition.Z = 0.0f;
            var picking1 = Vector3.TransformCoordinate(mousePickingPosition, invViewProj);
            mousePickingPosition.Z = 0.1f;
            var picking2 = Vector3.TransformCoordinate(mousePickingPosition, invViewProj);

            // Intersect moving plane with mouse picking ray
            var ray = new Ray(picking1, Vector3.Normalize(picking2 - picking1));
            Vector3 pickingDelta;
            plane.Intersects(ref ray, out pickingDelta);

            // Project result along movement axis
            pickingDelta = Vector3.Dot(axis1, pickingDelta - pickingGizmoOrigin) * axis1;

            return pickingDelta;
        }
    }
}
