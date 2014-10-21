#region Header Licence
//  ---------------------------------------------------------------------
// 
//  Copyright (c) 2009 Alexandre Mutel and Microsoft Corporation.  
//  All rights reserved.
// 
//  This code module is part of NShader, a plugin for visual studio
//  to provide syntax highlighting for shader languages (hlsl, glsl, cg)
// 
//  ------------------------------------------------------------------
// 
//  This code is licensed under the Microsoft Public License. 
//  See the file License.txt for the license details.
//  More info on: http://nshader.codeplex.com
// 
//  ------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using SiliconStudio.Paradox.VisualStudio.Classifiers;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace NShader
{
    public class NShaderLanguageService : LanguageService
    {
        private VisualStudioThemeEngine themeEngine;
        private NShaderColorableItem[] m_colorableItems;

        private LanguagePreferences m_preferences;

        public override void Initialize()
        {
            base.Initialize();

            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            // Check if already initialized
            if (m_colorableItems != null)
                return;

            // Initialize theme engine
            themeEngine = new VisualStudioThemeEngine(Site);
            themeEngine.OnThemeChanged += themeEngine_OnThemeChanged;

            var currentTheme = themeEngine.GetCurrentTheme();

            m_colorableItems = new NShaderColorableItem[]
                                   {
                                        /*1*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Keyword", "Paradox Shader Language - Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(86, 156, 214), Color.Empty, FONTFLAGS.FF_DEFAULT),
                                        /*2*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Comment", "Paradox Shader Language - Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_GREEN, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(87, 166, 74), Color.Empty, FONTFLAGS.FF_DEFAULT),
                                        /*3*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Identifier", "Paradox Shader Language - Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK, FONTFLAGS.FF_DEFAULT),
                                        /*4*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - String", "Paradox Shader Language - String", COLORINDEX.CI_RED, COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(214, 157, 133), Color.Empty, FONTFLAGS.FF_DEFAULT),
                                        /*5*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Number", "Paradox Shader Language - Number", COLORINDEX.CI_DARKBLUE, COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(181, 206, 168), Color.Empty, FONTFLAGS.FF_DEFAULT),
                                        /*6*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Intrinsic", "Paradox Shader Language - Intrinsic", COLORINDEX.CI_MAROON, COLORINDEX.CI_CYAN, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(239, 242, 132), Color.Empty, FONTFLAGS.FF_BOLD),
                                        /*7*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Special", "Paradox Shader Language - Special", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(78, 201, 176), Color.Empty, FONTFLAGS.FF_DEFAULT),
                                        /*8*/ new NShaderColorableItem(currentTheme, "Paradox Shader Language - Preprocessor", "Paradox Shader Language - Preprocessor", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_LIGHTGRAY, COLORINDEX.CI_USERTEXT_BK, Color.Empty, Color.FromArgb(155, 155, 155), Color.Empty, FONTFLAGS.FF_DEFAULT),
                                   };
        }

        public override void Dispose()
        {
            themeEngine.OnThemeChanged -= themeEngine_OnThemeChanged;
            themeEngine.Dispose();

            base.Dispose();
        }

        void themeEngine_OnThemeChanged(object sender, EventArgs e)
        {
            var colorUtilities = Site.GetService(typeof(SVsFontAndColorStorage)) as IVsFontAndColorUtilities;
            var currentTheme = themeEngine.GetCurrentTheme();

            var store = Package.GetGlobalService(typeof(SVsFontAndColorStorage)) as IVsFontAndColorStorage;
            store.OpenCategory(DefGuidList.guidTextEditorFontCategory, (uint)(__FCSTORAGEFLAGS.FCSF_LOADDEFAULTS | __FCSTORAGEFLAGS.FCSF_NOAUTOCOLORS | __FCSTORAGEFLAGS.FCSF_PROPAGATECHANGES));

            // Update each colorable item
            foreach (var colorableItem in m_colorableItems)
            {
                // Get display name of setting
                string displayName;
                colorableItem.GetDisplayName(out displayName);
                
                // Get new color
                var hiColor = currentTheme == VisualStudioTheme.Dark ? colorableItem.HiForeColorDark : colorableItem.HiForeColorLight;
                var colorIndex = currentTheme == VisualStudioTheme.Dark ? colorableItem.ForeColorDark : colorableItem.ForeColorLight;
                
                uint color;
                if (hiColor != Color.Empty)
                    color = hiColor.R | ((uint)hiColor.G << 8) | ((uint)hiColor.B << 16);
                else
                    colorUtilities.EncodeIndexedColor(colorIndex, out color);

                // Update color in settings
                store.SetItem(displayName, new[] { new ColorableItemInfo { bForegroundValid = 1, crForeground = color } });
            }
        }

        public override int GetItemCount(out int count)
        {
            count = m_colorableItems.Length;
            return VSConstants.S_OK;
        }

        public override int GetColorableItem(int index, out IVsColorableItem item)
        {
            if (index < 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            item = m_colorableItems[index-1];
            return VSConstants.S_OK;
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (m_preferences == null)
            {
                m_preferences = new LanguagePreferences(this.Site,
                                                        typeof(NShaderLanguageService).GUID,
                                                        this.Name);
                m_preferences.Init();
            }
            return m_preferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            string filePath = FilePathUtilities.GetFilePath(buffer);
            // Return dynamic scanner based on file extension
            return NShaderScannerFactory.GetShaderScanner(filePath);
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            return new NShaderSource(this, buffer, GetColorizer(buffer));
        }

        public override Colorizer GetColorizer(IVsTextLines buffer)
        {
            EnsureInitialized();

            // Clear font cache
            // http://social.msdn.microsoft.com/Forums/office/en-US/54064c52-727d-4015-af70-c72e44d116a7/vs2012-fontandcolors-text-editor-category-for-language-service-colors?forum=vsx
            IVsFontAndColorStorage storage;
            Guid textMgrIID = new Guid(
//#if VISUALSTUDIO_11_0
		            "{E0187991-B458-4F7E-8CA9-42C9A573B56C}" /* 'Text Editor Language Services Items' category discovered in the registry. Resetting TextEditor has no effect. */
//#else
//		            FontsAndColorsCategory.TextEditor
//#endif
	        );
	        if (null != (storage = GetService(typeof(IVsFontAndColorStorage)) as IVsFontAndColorStorage) &&
		        VSConstants.S_OK == storage.OpenCategory(ref textMgrIID, (uint)(__FCSTORAGEFLAGS.FCSF_READONLY | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS)))
	        {
		        bool missingColor = false;
		        try
		        {
			        ColorableItemInfo[] info = new ColorableItemInfo[1];
                    for (int i = 0; i < m_colorableItems.Length; ++i)
                    {
                        string colorName;
                        m_colorableItems[i].GetDisplayName(out colorName);
				        if (ErrorHandler.Failed(storage.GetItem(colorName, info)))
				        {
					        missingColor = true;
					        break;
				        }
			        }
		        }
		        finally
		        {
			        storage.CloseCategory();
		        }
		        if (missingColor)
		        {
			        IOleServiceProvider oleProvider;
			        // The service and interface guids are different, so we need to go to the OLE layer to get the service
			        Guid iid = typeof(IVsFontAndColorCacheManager).GUID;
			        Guid sid = typeof(SVsFontAndColorCacheManager).GUID;
			        IntPtr pCacheManager;
			        if (null != (oleProvider = GetService(typeof(IOleServiceProvider)) as IOleServiceProvider) &&
				        VSConstants.S_OK == oleProvider.QueryService(ref sid, ref iid, out pCacheManager) &&
				        pCacheManager != IntPtr.Zero)
			        {
				        try
				        {
					        IVsFontAndColorCacheManager cacheManager = (IVsFontAndColorCacheManager)Marshal.GetObjectForIUnknown(pCacheManager);
					        cacheManager.ClearCache(ref textMgrIID);
				        }
				        finally
				        {
					        Marshal.Release(pCacheManager);
				        }
			        }
		        }
            }

            return base.GetColorizer(buffer);
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            // req.FileName
            return new TestAuthoringScope();
        }

        public override string GetFormatFilterList()
        {
            return "";
        }

        public override string Name
        {
            get { return "Paradox Shader Language"; }
        }
        
        internal class TestAuthoringScope : AuthoringScope
        {
            public override string GetDataTipText(int line, int col, out TextSpan span)
            {
                span = new TextSpan();
                return null;
            }

            public override Declarations GetDeclarations(IVsTextView view,
                                                         int line,
                                                         int col,
                                                         TokenInfo info,
                                                         ParseReason reason)
            {
                return null;
            }

            public override Methods GetMethods(int line, int col, string name)
            {
                return null;
            }

            public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
            {
                span = new TextSpan();
                return null;
            }
        }

    }
}