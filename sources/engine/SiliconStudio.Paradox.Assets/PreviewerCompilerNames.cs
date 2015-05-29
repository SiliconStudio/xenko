// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Assets
{
    internal static class PreviewerCompilerNames
    {
        // TODO: This will removed and replaced by a the name of the plugin that matches the assets of the assembly.
        public const string SharedAssemblyQualifiedName = "Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
        public const string PreviewAssemblyName = "SiliconStudio.Paradox.GameStudio.Plugin";

        public const string PreviewAssemblyQualifiedName = ", " + PreviewAssemblyName + ", " + SharedAssemblyQualifiedName;

        public const string ThumbnailCompilersNamespace = "SiliconStudio.Paradox.GameStudio.Plugin.ThumbnailCompilers.";
        public const string PreviewBuildersNamespace = "SiliconStudio.Paradox.GameStudio.Plugin.PreviewBuilders.";

        public const string AnimationThumbnailCompilerQualifiedName       = ThumbnailCompilersNamespace + "AnimationThumbnailCompiler"       + PreviewAssemblyQualifiedName;
        public const string EntityThumbnailCompilerQualifiedName          = ThumbnailCompilersNamespace + "EntityThumbnailCompiler"          + PreviewAssemblyQualifiedName;
        public const string MaterialThumbnailCompilerQualifiedName        = ThumbnailCompilersNamespace + "MaterialThumbnailCompiler"        + PreviewAssemblyQualifiedName;
        public const string ModelThumbnailCompilerQualifiedName           = ThumbnailCompilersNamespace + "ModelThumbnailCompiler"           + PreviewAssemblyQualifiedName;
        public const string SoundThumbnailCompilerQualifiedName           = ThumbnailCompilersNamespace + "SoundThumbnailCompiler"           + PreviewAssemblyQualifiedName;
        public const string TextureThumbnailCompilerQualifiedName         = ThumbnailCompilersNamespace + "TextureThumbnailCompiler"         + PreviewAssemblyQualifiedName;
        public const string FontThumbnailCompilerQualifiedName            = ThumbnailCompilersNamespace + "FontThumbnailCompiler"            + PreviewAssemblyQualifiedName;
        public const string UIImageGroupThumbnailCompilerQualifiedName    = ThumbnailCompilersNamespace + "UIImageGroupThumbnailCompiler"    + PreviewAssemblyQualifiedName;
        public const string SpriteGroupThumbnailCompilerQualifiedName     = ThumbnailCompilersNamespace + "SpriteGroupThumbnailCompiler"     + PreviewAssemblyQualifiedName;
        public const string SceneThumbnailCompilerQualifiedName           = ThumbnailCompilersNamespace + "SceneThumbnailCompiler"           + PreviewAssemblyQualifiedName;
        public const string ProceduralModelThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "ProceduralModelThumbnailCompiler" + PreviewAssemblyQualifiedName;
    }
}
