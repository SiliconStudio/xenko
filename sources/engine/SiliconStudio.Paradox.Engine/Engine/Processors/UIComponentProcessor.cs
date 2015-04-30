// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Sprites;
using SiliconStudio.Paradox.UI;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class UIComponentProcessor : EntityProcessor<UIComponentProcessor.UIComponentState>
    {
        public List<UIComponentState> UIRoots { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteProcessor"/> class.
        /// </summary>
        public UIComponentProcessor()
            : base(new PropertyKey[] { UIComponent.Key, TransformComponent.Key })
        {
            UIRoots = new List<UIComponentState>();
        }
        
        protected override UIComponentState GenerateAssociatedData(Entity entity)
        {
            return new UIComponentState
            {
                UIComponent = entity.Get(UIComponent.Key),
                TransformComponent = entity.Get(TransformComponent.Key),
            };
        }

        public override void Draw(RenderContext gameTime)
        {
            UIRoots.Clear();
            foreach (var uiStateKeyPair in enabledEntities)
            {
                if (uiStateKeyPair.Value.UIComponent.Enabled)
                {
                    UIRoots.Add(uiStateKeyPair.Value);
                }
            }
        }

        public class UIComponentState
        {
            public UIComponent UIComponent;

            public TransformComponent TransformComponent;

            public UIElement LastOveredElement;

            public UIElement LastTouchedElement;

            public Vector3 LastIntersectionPoint;

            public Matrix LastRootMatrix;
        }
    }
}