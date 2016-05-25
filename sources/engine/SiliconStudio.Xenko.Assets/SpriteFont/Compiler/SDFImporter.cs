// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using SharpDX.Direct2D1;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont.Compiler
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;
    using Factory = SharpDX.DirectWrite.Factory;

    // This code was originally taken from DirectXTk but rewritten with DirectWrite
    // for more accuracy in font rendering
    internal class SDFImporter : IFontImporter
    {
        // Properties hold the imported font data.
        public IEnumerable<Glyph> Glyphs { get; private set; }

        public float LineSpacing { get; private set; }

        public float BaseLine { get; private set; }


        private string fontSource = "";
        private string msdfgenExe = "C:\\Dev\\xenko\\deps\\msdfgen\\msdfgen.exe";
        private string tempDir    = "C:\\Temp\\";


        // Option 1: Import as static, then swap the Bitmap
        public void ImportOld(SpriteFontAsset options, List<char> characters)
        {
            var factory = new Factory();

            // TODO Get file's path
            // try to get the font face from the source file if not null
            FontFace fontFace = !string.IsNullOrEmpty(options.Source) ? GetFontFaceFromSource(factory, options) : GetFontFaceFromSystemFonts(factory, options);

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

            // TODO Use msdfgen HERE!!!
            // Rasterize each character in turn.
            foreach (var character in characters)
                glyphList.Add(ImportGlyph(factory, fontFace, character, fontMetrics, fontSize, options.AntiAlias));

            Glyphs = glyphList;

            factory.Dispose();

            // Swap the glyphs with MSDFGEN bitmaps
            if (string.IsNullOrEmpty(options.Source))
                return;

            fontSource = options.Source;

            foreach (var glyph in Glyphs)
            {
                SwapGlyphBitmap(glyph);
            }
        }

        /// <summary>
        /// Generates and load a SDF font glyph using the msdfgen.exe
        /// </summary>
        /// <param name="c"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="offsetx"></param>
        /// <param name="offsety"></param>
        /// <returns></returns>
        private Bitmap LoadSDFBitmap(char c, int width, int height, int offsetx, int offsety)
        {
            var characterCode = "0x" + Convert.ToUInt32(c).ToString("x4");
            var outputFile = tempDir + characterCode + ".bmp";
            var exportSize = " -size " + width + " " + height + " ";
            var translate = " -translate " + offsetx + " " + offsety + " ";

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = msdfgenExe;
            startInfo.Arguments = "msdf -font " + fontSource + " " + characterCode + " -o " + outputFile + exportSize + translate + " -autoframe";
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            var msdfgenProcess = Process.Start(startInfo);

            if (msdfgenProcess == null)
                return null;

            msdfgenProcess.WaitForExit();

            if (File.Exists(outputFile))
            {
                return (Bitmap)System.Drawing.Bitmap.FromFile(outputFile);
            }

            return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        }

        private void SwapGlyphBitmap(Glyph glyph)
        {
            var characterCode = Convert.ToUInt32(glyph.Character).ToString("x4");

            var exportSize = " -size " + glyph.Bitmap.Width + " " + glyph.Bitmap.Height;

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = msdfgenExe;
            startInfo.Arguments = "msdf -font " + fontSource + " 0x" + characterCode + " -o " + tempDir + characterCode + ".bmp " + exportSize + " -translate 2 2 -autoframe";
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            var msdfgenProcess = Process.Start(startInfo);

            if (msdfgenProcess == null)
                return;

            msdfgenProcess.WaitForExit();

//            var image = (Bitmap)System.Drawing.Bitmap.FromFile(tempDir + characterCode + ".bmp");

            // TODO Copy from file
//            glyph.Bitmap = image;
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
                    case Xenko.Graphics.Font.FontStyle.Regular:
                        fontSimulations = FontSimulations.None;
                        break;
                    case Xenko.Graphics.Font.FontStyle.Bold:
                        fontSimulations = FontSimulations.Bold;
                        break;
                    case Xenko.Graphics.Font.FontStyle.Italic:
                        fontSimulations = FontSimulations.Oblique;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RawBool isSupported;
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
                    var weight = options.Style.IsBold() ? FontWeight.Bold : FontWeight.Regular;
                    var style = options.Style.IsItalic() ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal;
                    font = fontFamily.GetFirstMatchingFont(weight, FontStretch.Normal, style);
                }
            }

            return new FontFace(font);
        }

        private Glyph ImportGlyph(FontFace fontFace, char character, FontMetrics fontMetrics)
        {
            // 
            return null;
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

            var matrix = new RawMatrix3x2
            {
                M11 = 1,
                M22 = 1,
                M31 = -(float)Math.Floor(xOffset) + 1,
                M32 = -(float)Math.Floor(yOffset) + 1
            };

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

                    var bounds = new RawRectangle(0, 0, pixelWidth, pixelHeight);
                    bitmap = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format32bppArgb);

                    if (renderingMode == RenderingMode.Aliased)
                    {

                        var texture = new byte[pixelWidth * pixelHeight];
                        runAnalysis.CreateAlphaTexture(TextureType.Aliased1x1, bounds, texture, texture.Length);
                        for (int y = 0; y < pixelHeight; y++)
                        {
                            for (int x = 0; x < pixelWidth; x++)
                            {
                                int pixelX = y * pixelWidth + x;
                                var grey = texture[pixelX];
                                var color = Color.FromArgb(grey, grey, grey);

                                bitmap.SetPixel(x, y, color);
                            }
                        }
                    }
                    else
                    {
                        var texture = new byte[pixelWidth * pixelHeight * 3];
                        runAnalysis.CreateAlphaTexture(TextureType.Cleartype3x1, bounds, texture, texture.Length);
                        for (int y = 0; y < pixelHeight; y++)
                        {
                            for (int x = 0; x < pixelWidth; x++)
                            {
                                int pixelX = (y * pixelWidth + x) * 3;
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
                XOffset = -matrix.M31,
                XAdvance = advanceWidth,
                YOffset = -matrix.M32,
            };
            return glyph;
        }

        // Option 1: Import as static, then swap the Bitmap
        public void Import(SpriteFontAsset options, List<char> characters)
        {
            // Swap the glyphs with MSDFGEN bitmaps
            if (string.IsNullOrEmpty(options.Source))
                return;

            fontSource = options.Source;

            var factory = new Factory();

            // TODO Get file's path
            // try to get the font face from the source file if not null
            FontFace fontFace = !string.IsNullOrEmpty(options.Source) ? GetFontFaceFromSource(factory, options) : GetFontFaceFromSystemFonts(factory, options);

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

            // Generate SDF bitmaps for each character in turn.
            foreach (var character in characters)
                glyphList.Add(ImportGlyphProper(fontFace, character, fontMetrics, fontSize));

            Glyphs = glyphList;

            factory.Dispose();            
        }


        private Glyph ImportGlyphProper(FontFace fontFace, char character, FontMetrics fontMetrics, float fontSize)
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


            var matrixM31 = -(float)Math.Floor(xOffset) + 1;
            var matrixM32 = -(float)Math.Floor(yOffset) + 1;

            Bitmap bitmap;
            if (char.IsWhiteSpace(character))
            {
                bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }
            else
            {
                bitmap = LoadSDFBitmap(character, pixelWidth, pixelHeight, 1, 1);
            }

            var glyph = new Glyph(character, bitmap)
            {
                XOffset = -matrixM31,
                XAdvance = advanceWidth,
                YOffset = -matrixM32,
            };
            return glyph;
        }

        private static byte LinearToGamma(byte color)
        {
            return (byte)(Math.Pow(color / 255.0f, 1 / 2.2f) * 255.0f);
        }
    }
}
