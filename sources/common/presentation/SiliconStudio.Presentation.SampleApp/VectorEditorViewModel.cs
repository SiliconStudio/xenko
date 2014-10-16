// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.SampleApp
{
    public class VectorEditorViewModel : ViewModelBase
    {
        private Vector2 vector2;
        private Vector3 vector3;
        private Vector4 vector4;
        private Matrix matrix;

        public Vector2 Vector2 { get { return vector2; } set { SetValue(ref vector2, value); } }

        public Vector3 Vector3 { get { return vector3; } set { SetValue(ref vector3, value); } }

        public Vector4 Vector4 { get { return vector4; } set { SetValue(ref vector4, value); } }

        public Matrix Matrix { get { return matrix; } set { SetValue(ref matrix, value); } }
    }
}
