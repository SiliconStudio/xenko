// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using System.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A class aiming at presenting another <see cref="UIElement"/>.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("ContentPresenter - Name={Name}")]
    public class ContentPresenter : UIElement
    {
        private static void ContentInvalidationCallback(object propertyOwner, PropertyKey<UIElement> propertyKey, UIElement oldContent)
        {
            var presenter = (ContentPresenter)propertyOwner;
            
            if(oldContent == presenter.Content)
                return;

            if (oldContent != null)
                SetVisualParent(oldContent, null);

            if (presenter.Content != null)
                SetVisualParent(presenter.Content, presenter);

            presenter.InvalidateMeasure();
        }

        /// <summary>
        /// The key to the Content dependency property.
        /// </summary>
        public readonly static PropertyKey<UIElement> ContentPropertyKey = new PropertyKey<UIElement>("ContentKey", typeof(ContentPresenter), DefaultValueMetadata.Static<UIElement>(null), ObjectInvalidationMetadata.New<UIElement>(ContentInvalidationCallback));

        private Matrix contentWorldMatrix;

        public ContentPresenter()
        {
            DepthAlignment = DepthAlignment.Stretch;
        }

        /// <summary>
        /// Gets or sets content of the presenter.
        /// </summary>
        [DataMemberIgnore]
        public UIElement Content
        {
            get { return DependencyProperties.Get(ContentPropertyKey); }
            set { DependencyProperties.Set(ContentPropertyKey, value); }
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            // measure size desired by the children
            var childDesiredSizeWithMargins = Vector3.Zero;
            if (Content != null)
            {
                Content.Measure(availableSizeWithoutMargins);
                childDesiredSizeWithMargins = Content.DesiredSizeWithMargins;
            }

            return childDesiredSizeWithMargins;
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            // arrange the content
            if (Content != null)
            {
                // arrange the child
                Content.Arrange(finalSizeWithoutMargins, IsCollapsed);
            }

            return finalSizeWithoutMargins;
        }

        protected override void UpdateWorldMatrix(ref Matrix parentWorldMatrix, bool parentWorldChanged)
        {
            var contentWorldMatrixChanged = parentWorldChanged || ArrangeChanged || LocalMatrixChanged;

            base.UpdateWorldMatrix(ref parentWorldMatrix, parentWorldChanged);

            if (Content != null)
            {
                if (contentWorldMatrixChanged)
                {
                    contentWorldMatrix = WorldMatrixInternal;
                    var contentMatrix = Matrix.Translation(-RenderSize / 2);
                    Matrix.Multiply(ref contentMatrix, ref WorldMatrixInternal, out contentWorldMatrix);
                }

                ((IUIElementUpdate)Content).UpdateWorldMatrix(ref contentWorldMatrix, contentWorldMatrixChanged);
            }
        }
    }
}
