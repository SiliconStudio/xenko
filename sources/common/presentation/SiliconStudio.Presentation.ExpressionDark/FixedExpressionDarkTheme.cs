// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Themes;

namespace SiliconStudio.Presentation.ExpressionDark
{
    public class FixedExpressionDarkTheme : Theme
    {
        public override Uri GetResourceUri()
        {
            return new Uri(
                "/SiliconStudio.Presentation.ExpressionDark;component/FixedExpressionDarkTheme.xaml",
                UriKind.Relative);
        }
    }
}
