// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

using SiliconStudio.Paradox.Graphics.Font;

using Factory = SharpDX.DirectWrite.Factory;

namespace SiliconStudio.Paradox.Assets.SpriteFont.Compiler
{
    using System.Drawing;
    using System.Drawing.Imaging;

    // This code was originally taken from DirectXTk but rewritten with DirectWrite
    // for more accuracy in font rendering
    internal class TrueTypeImporter : IFontImporter
    {
        // Properties hold the imported font data.
        public IEnumerable<Glyph> Glyphs { get; private set; }

        public float LineSpacing { get; private set; }

        public float BaseLine { get; private set; }

        public void Import(SpriteFontAsset options, List<char> characters)
        {
            var factory = new Factory();

            // try to get the font face from the source file if not null
            FontFace fontFace = options.Source != null ? GetFontFaceFromSource(factory, options) : GetFontFaceFromSystemFonts(factory, options);
            
            var fontMetrics = fontFace.Metrics;

            // Create a bunch of GDI+ objects.
            var fontSize = FontHelper.PointsToPixels(options.Size);

            var glyphList = new List<Glyph>();

            // Remap the LineMap coming from the font with a user defined remapping
            // Note:
            // We are remapping the lineMap to allow to shrink the LineGap and to reposition it at the top and/or bottom of the 
            // font instead of using only the top
            // According to http://stackoverflow.com/questions/13939264/how-to-determine-baseline-position-using-directwrite#comment27947684_14061348
            // (The response is from a MSFT employee), the BaseLine should be = LineGap + Ascent but this is not what
            // we are experiencing when comparing with MSWord (LineGap + Ascent seems to offset too much.)
            //
            // So we are first applying a factor to the line gap:
            //     NewLineGap = LineGap * LineGapFactor
            var lineGap = fontMetrics.LineGap * options.LineGapFactor;

            // Store the font height.
            LineSpacing = (float)(lineGap + fontMetrics.Ascent + fontMetrics.Descent) / fontMetrics.DesignUnitsPerEm * fontSize;

            // And then the baseline is also changed in order to allow the linegap to be distributed between the top and the 
            // bottom of the font:
            //     BaseLine = NewLineGap * LineGapBaseLineFactor
            BaseLine = (float)(lineGap * options.LineGapBaseLineFactor + fontMetrics.Ascent) / fontMetrics.DesignUnitsPerEm * fontSize;

            // Rasterize each character in turn.
            foreach (var character in characters)
                glyphList.Add(ImportGlyph(factory, fontFace, character, fontMetrics, fontSize, options.AntiAlias));

            Glyphs = glyphList;

            factory.Dispose();
        }
        
        private FontFace GetFontFaceFromSource(Factory factory, SpriteFontAsset options)
        {
            if (!File.Exists(options.Source))
            {
                // Font does not exist
                throw new FontNotFoundException(options.Source);
            }

            using (var fontFile = new FontFile(factory, options.Source))
            {
                FontSimulations fontSimulations;
                switch (options.Style)
                {
                    case Paradox.Graphics.Font.FontStyle.Regular:
                        fontSimulations = FontSimulations.None;
                        break;
                    case Paradox.Graphics.Font.FontStyle.Bold:
                        fontSimulations = FontSimulations.Bold;
                        break;
                    case Paradox.Graphics.Font.FontStyle.Italic:
                        fontSimulations = FontSimulations.Oblique;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Bool isSupported;
                FontFileType fontType;
                FontFaceType faceType;
                int numberFaces;

                fontFile.Analyze(out isSupported, out fontType, out faceType, out numberFaces);

                return new FontFace(factory, faceType, new[] { fontFile }, 0, fontSimulations);
            }
        }

        private FontFace GetFontFaceFromSystemFonts(Factory factory, SpriteFontAsset options)
        {
            SharpDX.DirectWrite.Font font;
            using (var fontCollection = factory.GetSystemFontCollection(false))
            {
                int index;
                if (!fontCollection.FindFamilyName(options.FontName, out index))
                {
                    // Lets try to import System.Drawing for old system bitmap fonts (like MS Sans Serif)
                    throw new FontNotFoundException(options.FontName);
                }

                using (var fontFamily = fontCollection.GetFontFamily(index))
                {
                    var weight = options.Style.IsBold()? FontWeight.Bold: FontWeight.Regular;
                    var style = options.Style.IsItalic() ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal;
                    font = fontFamily.GetFirstMatchingFont(weight, FontStretch.Normal, style);
                }
            }

            return new FontFace(font);
        }

        private Glyph ImportGlyph(Factory factory, FontFace fontFace, char character, FontMetrics fontMetrics, float fontSize, FontAntiAliasMode antiAliasMode)
        {
            var indices = fontFace.GetGlyphIndices(new int[] { character });

            var metrics = fontFace.GetDesignGlyphMetrics(indices, false);
            var metric = metrics[0];

            var width = (float)(metric.AdvanceWidth - metric.LeftSideBearing - metric.RightSideBearing) / fontMetrics.DesignUnitsPerEm * fontSize;
            var height = (float)(metric.AdvanceHeight - metric.TopSideBearing - metric.BottomSideBearing) / fontMetrics.DesignUnitsPerEm * fontSize;

            var xOffset = (float)metric.LeftSideBearing / fontMetrics.DesignUnitsPerEm * fontSize;
            var yOffset = (float)(metric.TopSideBearing - metric.VerticalOriginY) / fontMetrics.DesignUnitsPerEm * fontSize;

            var advanceWidth = (float)metric.AdvanceWidth / fontMetrics.DesignUnitsPerEm * fontSize;
            //var advanceHeight = (float)metric.AdvanceHeight / fontMetrics.DesignUnitsPerEm * fontSize;

            var pixelWidth = (int)Math.Ceiling(width + 4);
            var pixelHeight = (int)Math.Ceiling(height + 4);

            var matrix = Matrix.Identity;
            matrix.M41 = -(float)Math.Floor(xOffset) + 1;
            matrix.M42 = -(float)Math.Floor(yOffset) + 1;

            Bitmap bitmap;
            if (char.IsWhiteSpace(character))
            {
                bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }
            else
            {
                var glyphRun = new GlyphRun
                {
                    FontFace = fontFace,
                    Advances = new[] { (float)Math.Ceiling(advanceWidth) },
                    FontSize = fontSize,
                    BidiLevel = 0,
                    Indices = indices,
                    IsSideways = false,
                    Offsets = new[] { new GlyphOffset() }
                };


                RenderingMode renderingMode;
                if (antiAliasMode != FontAntiAliasMode.Aliased)
                {
                    var rtParams = new RenderingParams(factory);
                    renderingMode = fontFace.GetRecommendedRenderingMode(fontSize, 1.0f, MeasuringMode.Natural, rtParams);
                    rtParams.Dispose();
                }
                else
                {
                    renderingMode = RenderingMode.Aliased;
                }

                using (var runAnalysis = new GlyphRunAnalysis(factory,
                    glyphRun,
                    1.0f,
                    matrix,
                    renderingMode,
                    MeasuringMode.Natural,
                    0.0f,
                    0.0f))
                {

                    var bounds = new SharpDX.Rectangle(0, 0, pixelWidth, pixelHeight);
                    bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

                    if (renderingMode == RenderingMode.Aliased)
                    {

                        var texture = new byte[bounds.Width * bounds.Height];
                        runAnalysis.CreateAlphaTexture(TextureType.Aliased1x1, bounds, texture, texture.Length);
                        for (int y = 0; y < bounds.Height; y++)
                        {
                            for (int x = 0; x < bounds.Width; x++)
                            {
                                int pixelX = y * bounds.Width + x;
                                var grey = texture[pixelX];
                                var color = Color.FromArgb(grey, grey, grey);

                                bitmap.SetPixel(x, y, color);
                            }
                        }
                    }
                    else
                    {
                        var texture = new byte[bounds.Width * bounds.Height * 3];
                        runAnalysis.CreateAlphaTexture(TextureType.Cleartype3x1, bounds, texture, texture.Length);
                        for (int y = 0; y < bounds.Height; y++)
                        {
                            for (int x = 0; x < bounds.Width; x++)
                            {
                                int pixelX = (y * bounds.Width + x) * 3;
                                var red = LinearToGamma(texture[pixelX]);
                                var green = LinearToGamma(texture[pixelX + 1]);
                                var blue = LinearToGamma(texture[pixelX + 2]);
                                var color = Color.FromArgb(red, green, blue);

                                bitmap.SetPixel(x, y, color);
                            }
                        }
                    }
                }
            }

            var glyph = new Glyph(character, bitmap)
            {
                XOffset = -matrix.M41,
                XAdvance = advanceWidth,
                YOffset = -matrix.M42,
            };
            return glyph;
        }

        private static byte LinearToGamma(byte color)
        {
            return (byte)(Math.Pow(color / 255.0f, 1 / 2.2f) * 255.0f);
        }
    }
}
