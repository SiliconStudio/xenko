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
using System.Drawing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace NShader
{
    public class NShaderColorableItem : ColorableItem
    {
        public NShaderColorableItem(string name, COLORINDEX foreColor, COLORINDEX backColor)
            : base(name, name, foreColor, backColor, Color.Empty, Color.Empty, FONTFLAGS.FF_DEFAULT)
        {
        }

        public NShaderColorableItem(string name, COLORINDEX foreColor, COLORINDEX backColor, FONTFLAGS fontFlags)
            : base(name, name, foreColor, backColor, Color.Empty, Color.Empty, fontFlags)
        {
        }

        public NShaderColorableItem(string name, string displayName, COLORINDEX foreColor, COLORINDEX backColor)
            : base(name, displayName, foreColor, backColor, Color.Empty, Color.Empty, FONTFLAGS.FF_DEFAULT)
        {
        }

        public NShaderColorableItem(string name, string displayName, COLORINDEX foreColor, COLORINDEX backColor, FONTFLAGS fontFlags)
            : base(name, displayName, foreColor, backColor, Color.Empty, Color.Empty, fontFlags)
        {
        }

        public NShaderColorableItem(string name, string displayName, COLORINDEX foreColor, COLORINDEX backColor, Color hiForeColor, Color hiBackColor, FONTFLAGS fontFlags)
            : base(name, displayName, foreColor, backColor, hiForeColor, hiBackColor, fontFlags)
        {
        }

        public override int GetMergingPriority(out int priority)
        {
           priority = 0x2000;
           return VSConstants.S_OK;
        }
    }
}