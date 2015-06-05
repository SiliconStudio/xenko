// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Settings;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Assets
{
    public static class GameUserSettings
    {
        public static class Effect
        {
            public static SettingsValueKey<EffectCompilationMode> EffectCompilationMode = new SettingsValueKey<EffectCompilationMode>("Package/Game/Effect/EffectCompilationMode", Engine.Design.EffectCompilationMode.Local);
            public static SettingsValueKey<bool> RecordEffectRequested = new SettingsValueKey<bool>("Package/Game/Effect/RecordEffectRequested", false);
        }
    }
}