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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace NShader
{
    public class NShaderLanguageService : LanguageService
    {
        private ColorableItem[] m_colorableItems;

        private LanguagePreferences m_preferences;

        public NShaderLanguageService()
        {
            m_colorableItems = new ColorableItem[]
                                   {
                                        /*1*/ new NShaderColorableItem("Paradox Shader Language - Keyword", "Paradox Shader Language - Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK),
                                        /*2*/ new NShaderColorableItem("Paradox Shader Language - Comment", "Paradox Shader Language - Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK),
                                        /*3*/ new NShaderColorableItem("Paradox Shader Language - Identifier", "Paradox Shader Language - Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK),
                                        /*4*/ new NShaderColorableItem("Paradox Shader Language - String", "Paradox Shader Language - String", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK),
                                        /*5*/ new NShaderColorableItem("Paradox Shader Language - Number", "Paradox Shader Language - Number", COLORINDEX.CI_DARKBLUE, COLORINDEX.CI_USERTEXT_BK),
                                        /*6*/ new NShaderColorableItem("Paradox Shader Language - Intrinsic", "Paradox Shader Language - Intrinsic", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK, FONTFLAGS.FF_BOLD),
                                        /*7*/ new NShaderColorableItem("Paradox Shader Language - Special", "Paradox Shader Language - Special", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK),
                                        /*8*/ new NShaderColorableItem("Paradox Shader Language - Preprocessor", "Paradox Shader Language - Preprocessor", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK),
                                   };
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