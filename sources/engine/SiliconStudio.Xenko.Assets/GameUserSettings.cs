// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Settings;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Assets
{
    public static class GameUserSettings
    {
        public static class Effect
        {
            public static SettingsKey<EffectCompilationMode> EffectCompilation = new SettingsKey<EffectCompilationMode>("Package/Game/Effect/EffectCompilation", PackageUserSettings.SettingsContainer, EffectCompilationMode.LocalOrRemote)
            {
                DisplayName = "Effect Compiler"
            };
            public static SettingsKey<bool> RecordUsedEffects = new SettingsKey<bool>("Package/Game/Effect/RecordUsedEffects", PackageUserSettings.SettingsContainer, true)
            {
                DisplayName = "Record used effects"
            };
        }

        public static class Editor
        {
            public static SettingsKey<RenderingMode> EditorRenderingMode = new SettingsKey<RenderingMode>("Package/Game/Editor/RenderingMode", PackageUserSettings.SettingsContainer, RenderingMode.HDR)
            {
                DisplayName = "Editor Rendering Mode"
            };

            public static T GetValueForPackage<T>(SettingsKey<T> settingsKey, Package package)
            {
                return settingsKey.GetValue(package.UserSettings.Profile, true);
            }
        }
    }
}