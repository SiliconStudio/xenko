// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class UIComponentProcessor : EntityProcessor<UIComponent, UIComponentProcessor.UIComponentState>
    {
        public List<UIComponentState> UIRoots { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteProcessor"/> class.
        /// </summary>
        public UIComponentProcessor()
            : base(typeof(TransformComponent))
        {
            UIRoots = new List<UIComponentState>();
        }
        
        protected override UIComponentState GenerateComponentData(Entity entity, UIComponent component)
        {
            return new UIComponentState(component, entity.Transform);
        }

        protected override bool IsAssociatedDataValid(Entity entity, UIComponent component, UIComponentState associatedData)
        {
            return
                entity.Get<UIComponent>() == component &&
                entity.Transform == associatedData.TransformComponent;
        }

        public override void Draw(RenderContext gameTime)
        {
            UIRoots.Clear();
            foreach (var uiStateKeyPair in ComponentDatas)
            {
                if (uiStateKeyPair.Value.UIComponent.Enabled)
                {
                    UIRoots.Add(uiStateKeyPair.Value);
                }
            }
        }

        public class UIComponentState
        {
            public UIComponentState(UIComponent uiComponent, TransformComponent transformComponent)
            {
                UIComponent = uiComponent;
                TransformComponent = transformComponent;
            }

            public readonly UIComponent UIComponent;

            public readonly TransformComponent TransformComponent;

            public UIElement LastOveredElement;

            public UIElement LastTouchedElement;

            public Vector3 LastIntersectionPoint;

            public Matrix LastRootMatrix;
        }
    }
}