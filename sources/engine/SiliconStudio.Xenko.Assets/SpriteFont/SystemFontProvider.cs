// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SharpDX.DirectWrite;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Assets.SpriteFont.Compiler;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("SystemFontProvider")]
    [Display("System Font")]
    public class SystemFontProvider : FontProviderBase
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SystemFontProvider");

        public const string DefaultFontName = "Arial";

        public SystemFontProvider()
        {
            FontName = DefaultFontName;
        }

        public SystemFontProvider(string fontName)
        {
            FontName = fontName;
        }

        /// <summary>
        /// Gets or sets the name of the font family to use when the <see cref="Source"/> is not specified.
        /// </summary>
        /// <userdoc>
        /// The name of the font family to use. Only the fonts installed on the system can be used here.
        /// </userdoc>
        [DataMember(20)]
        [Display("Font Name")]
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the style of the font. A combination of 'regular', 'bold', 'italic'. Default is 'regular'.
        /// </summary>
        /// <userdoc>
        /// The style of the font (regular / bold / italic). Note that this property is ignored is the desired style is not available in the font's source file.
        /// </userdoc>
        [DataMember(40)]
        [Display("Style")]
        public override SiliconStudio.Xenko.Graphics.Font.FontStyle Style { get; set; } = Graphics.Font.FontStyle.Regular;

        /// <inheritdoc/>
        public override FontFace GetFontFace()
        {
            var factory = new Factory();

            SharpDX.DirectWrite.Font font;
            using (var fontCollection = factory.GetSystemFontCollection(false))
            {
                int index;
                if (!fontCollection.FindFamilyName(FontName, out index))
                {
                    // Lets try to import System.Drawing for old system bitmap fonts (like MS Sans Serif)
                    throw new FontNotFoundException(FontName);
                }

                using (var fontFamily = fontCollection.GetFontFamily(index))
                {
                    var weight = Style.IsBold() ? FontWeight.Bold : FontWeight.Regular;
                    var style = Style.IsItalic() ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal;
                    font = fontFamily.GetFirstMatchingFont(weight, FontStretch.Normal, style);
                }
            }

            return new FontFace(font);
        }

        public override string GetFontPath(AssetCompilerResult result = null)
        {
            using (var factory = new Factory())
            {
                Font font;

                using (var fontCollection = factory.GetSystemFontCollection(false))
                {
                    int index;
                    if (!fontCollection.FindFamilyName(FontName, out index))
                    {
                        result?.Error("Cannot find system font '{0}'. Make sure it is installed on this machine.", FontName);
                        return null;
                    }

                    using (var fontFamily = fontCollection.GetFontFamily(index))
                    {
                        var weight = Style.IsBold() ? FontWeight.Bold : FontWeight.Regular;
                        var style = Style.IsItalic() ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal;
                        font = fontFamily.GetFirstMatchingFont(weight, FontStretch.Normal, style);
                        if (font == null)
                        {
                            result?.Error("Cannot find style '{0}' for font family {1}. Make sure it is installed on this machine.", Style, FontName);
                            return null;
                        }
                    }
                }

                var fontFace = new FontFace(font);

                // get the font path on the hard drive
                var file = fontFace.GetFiles().First();
                var referenceKey = file.GetReferenceKey();
                var originalLoader = (FontFileLoaderNative)file.Loader;
                var loader = originalLoader.QueryInterface<LocalFontFileLoader>();
                return loader.GetFilePath(referenceKey);
            }
        }

        /// <inheritdoc/>
        public override string GetFontName()
        {
            return FontName;
        }
    }
}
