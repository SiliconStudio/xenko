// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using System.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A class aiming at presenting another <see cref="UIElement"/>.
    /// </summary>
    [DataContract(nameof(ContentPresenter))]
    [DebuggerDisplay("ContentPresenter - Name={Name}")]
    public class ContentPresenter : UIElement
    {
        private Matrix contentWorldMatrix;
        private UIElement content;

        public ContentPresenter()
        {
            DepthAlignment = DepthAlignment.Stretch;
        }

        /// <summary>
        /// Gets or sets content of the presenter.
        /// </summary>
        [DataMember]
        [DefaultValue(null)]
        public UIElement Content
        {
            get { return content; }
            set
            {
                if (content == value)
                    return;

                if (content != null)
                    SetVisualParent(content, null);
                
                content = value;

                if (content != null)
                    SetVisualParent(content, this);

                content = value;
                InvalidateMeasure();
            }
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
            // arrange child elements
            Content?.Arrange(finalSizeWithoutMargins, IsCollapsed);

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
