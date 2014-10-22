// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Theme Coloring Source: https://github.com/fsprojects/VisualFSharpPowerTools
//
// Copyright 2014 F# Software Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace SiliconStudio.Paradox.VisualStudio.Classifiers
{
    internal class VisualStudioThemeEngine : IVsBroadcastMessageEvents, IDisposable
    {
        private const uint WM_SYSCOLORCHANGE = 0x0015;

        private readonly DTE2 dte;
        private readonly IVsShell shellService;
        private uint broadcastCookie;

        private readonly Dictionary<Guid, VisualStudioTheme> availableThemes = new Dictionary<Guid, VisualStudioTheme>
        {
            { new Guid("DE3DBBCD-F642-433C-8353-8F1DF4370ABA"), VisualStudioTheme.Light },
            { new Guid("A4D6A176-B948-4B29-8C66-53C97A1ED7D0"), VisualStudioTheme.Blue },
            { new Guid("1DED0138-47CE-435E-84EF-9EC1F439B749"), VisualStudioTheme.Dark }
        };

        public event EventHandler OnThemeChanged;

        public VisualStudioThemeEngine(IServiceProvider serviceProvider)
        {
            dte = (DTE2)serviceProvider.GetService(typeof(SDTE));

            // Register to Visual Studio theme change
            shellService = serviceProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (shellService != null)
                ErrorHandler.ThrowOnFailure(shellService.AdviseBroadcastMessages(this, out broadcastCookie));
        }

        public void Dispose()
        {
            if (shellService != null && broadcastCookie != 0)
            {
                shellService.UnadviseBroadcastMessages(broadcastCookie);
                broadcastCookie = 0;
            }
        }

        public VisualStudioTheme GetCurrentTheme()
        {
            var currentUser32 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
            using (var subkey = currentUser32.OpenSubKey(string.Format(@"{0}\General", dte.RegistryRoot)))
            {
                if (subkey == null)
                    return VisualStudioTheme.Unknown;

                var themeValue = (string)subkey.GetValue("CurrentTheme");
                if (themeValue == null)
                    return VisualStudioTheme.Unknown;

                Guid themeGuid;
                if (!Guid.TryParse(themeValue, out themeGuid))
                    return VisualStudioTheme.Unknown;

                VisualStudioTheme theme;
                availableThemes.TryGetValue(themeGuid, out theme);

                return theme;
            }
        }

        public int OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_SYSCOLORCHANGE)
            {
                if (OnThemeChanged != null)
                {
                    OnThemeChanged(this, EventArgs.Empty);
                }
            }
            return VSConstants.S_OK;
        }
    }
}