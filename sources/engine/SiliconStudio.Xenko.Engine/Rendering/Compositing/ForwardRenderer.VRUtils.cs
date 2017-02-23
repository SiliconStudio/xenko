using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.VirtualReality;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public partial class ForwardRenderer
    {
        private static unsafe void ComputeCommonViewMatrices(RenderContext context, Matrix* viewMatrices, Matrix* projectionMatrices)
        {
            // there are some limitations to this technique:
            //  both eyes view matrices must be facing the same direction
            //  both eyes must have the same near plane
            //  both eyes must have equivalent left and right in absolute value

            var commonView = context.RenderView;
            commonView.View = viewMatrices[0];
            // near and far could be overriden by the VR device. let's take them as authority (assuming both eyes equal):
            commonView.NearClipPlane = projectionMatrices[0].M43 / projectionMatrices[0].M33;
            commonView.FarClipPlane  = commonView.NearClipPlane * (-projectionMatrices[0].M33 / (-projectionMatrices[0].M33 - 1));
            // We assume view matrices are similar except for a translation; we can take the average to have the "center eye" position
            commonView.View.TranslationVector = Vector3.Lerp(commonView.View.TranslationVector, viewMatrices[1].TranslationVector, 0.5f);

            // Also need to move it backward little bit
            // http://computergraphics.stackexchange.com/questions/1736/vr-and-frustum-culling

            // Projection: Need to extend size to cover equivalent of both eyes
            // So we cancel the left/right off-center and add it to the width to compensate
            commonView.Projection = projectionMatrices[0];
            // Compute left and right
            var left0  = commonView.NearClipPlane * (projectionMatrices[0].M31 - 1.0f) / projectionMatrices[0].M11;
            var right1 = commonView.NearClipPlane * (projectionMatrices[1].M31 + 1.0f) / projectionMatrices[1].M11;
            commonView.Projection.M11 = 2.0f * commonView.NearClipPlane / (right1 - left0);
            commonView.Projection.M31 = (right1 + left0) / (right1 - left0);
            // translate the view backwards:
            float ipd = (viewMatrices[0].TranslationVector - viewMatrices[1].TranslationVector).Length();
            // m11 stores 1 / tan(theta)
            var recessionFactor = (ipd / 2) * commonView.Projection.M11;
            commonView.View.M43 -= recessionFactor;

            // and now recompute the matrix entirely because we want to change the near and far planes:
            var bottom = commonView.NearClipPlane * (commonView.Projection.M32 - 1.0f) / commonView.Projection.M22;
            var top    = commonView.NearClipPlane * (commonView.Projection.M32 + 1.0f) / commonView.Projection.M22;
            // new near and far:
            var newNear = commonView.NearClipPlane + recessionFactor;
            var newFar  = commonView.FarClipPlane + recessionFactor;
            // adjust proportionally the parameters (l, r, u, b are defined at near, so we use nears ratio):
            var nearsRatio = newNear / commonView.NearClipPlane;
            // recreation from scratch:
            Matrix.PerspectiveOffCenterRH(left0 * nearsRatio, right1 * nearsRatio, bottom * nearsRatio, top * nearsRatio, newNear, newFar, out commonView.Projection);

            // update the view projection:
            Matrix.Multiply(ref commonView.View, ref commonView.Projection, out commonView.ViewProjection);
        }
    }
}
