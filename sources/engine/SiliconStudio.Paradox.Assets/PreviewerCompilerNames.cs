// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Assets
{
    internal static class PreviewerCompilerNames
    {
        // TODO: This will removed
        [Obsolete]
        public const string SharedAssemblyQualifiedName = "Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
        [Obsolete]
        public const string PreviewAssemblyName = "SiliconStudio.Paradox.GameStudio.Plugin";

        [Obsolete]
        public const string PreviewAssemblyQualifiedName = ", " + PreviewAssemblyName + ", " + SharedAssemblyQualifiedName;

        [Obsolete]
        public const string ThumbnailCompilersNamespace = "SiliconStudio.Paradox.GameStudio.Plugin.ThumbnailCompilers.";

        [Obsolete]
        public const string AnimationThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "AnimationThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SceneThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SceneThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string MaterialThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "MaterialThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string ModelThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "ModelThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SoundThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SoundThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string TextureThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "TextureThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string FontThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "FontThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string UIImageGroupThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "UIImageGroupThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SpriteGroupThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SpriteGroupThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string ProceduralModelThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "ProceduralModelThumbnailCompiler" + PreviewAssemblyQualifiedName;
    }
}
