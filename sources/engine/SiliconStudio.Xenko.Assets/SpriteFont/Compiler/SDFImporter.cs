// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Text;
using Microsoft.Win32;
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


        private string fontSource;
        private string msdfgenExe;
        private string tempDir;

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
                return (Bitmap)Bitmap.FromFile(outputFile);
            }

            return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
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

        private string GetFontSource(SpriteFontAsset options)
        {
            if (!string.IsNullOrEmpty(options.Source))
                return options.Source;

            // TODO Cache this dictionary
            Dictionary<string, string> foundFonts = new Dictionary<string, string>();

            string fontsfolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts);

            if (!Directory.Exists(fontsfolder)) throw new Exception("directory doesnt exist");

            foreach (FileInfo fi in new DirectoryInfo(fontsfolder).GetFiles("*.ttf"))
            {
                PrivateFontCollection fileFonts = new PrivateFontCollection();
                fileFonts.AddFontFile(fi.FullName);
                {
                    var fontKey = fileFonts.Families[0].Name;

                    // Regular
                    if (fileFonts.Families[0].IsStyleAvailable(System.Drawing.FontStyle.Regular))
                    {
                        if (!foundFonts.ContainsKey(fontKey))
                        {
                            foundFonts.Add(fontKey, fi.FullName);
                        }
                    }

                    // Bold
                    if (fileFonts.Families[0].IsStyleAvailable(System.Drawing.FontStyle.Bold))
                    {
                        if (!foundFonts.ContainsKey(fontKey + " Bold"))
                        {
                            foundFonts.Add(fontKey + " Bold", fi.FullName);
                        }
                    }

                    // Italic
                    if (fileFonts.Families[0].IsStyleAvailable(System.Drawing.FontStyle.Italic))
                    {
                        if (!foundFonts.ContainsKey(fontKey + " Italic"))
                        {
                            foundFonts.Add(fontKey + " Italic", fi.FullName);
                        }
                    }

                    // Bold Italic
                    if (fileFonts.Families[0].IsStyleAvailable(System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))
                    {
                        if (!foundFonts.ContainsKey(fontKey + " Bold Italic"))
                        {
                            foundFonts.Add(fontKey + " Bold Italic", fi.FullName);
                        }
                    }
                }
            }

            string outSource;
            if (options.Style.IsBold() && options.Style.IsItalic())
            {
                if (foundFonts.TryGetValue(options.FontName + " Bold Italic", out outSource))
                    return outSource;
            }

            if (options.Style.IsBold())
            {
                if (foundFonts.TryGetValue(options.FontName + " Bold", out outSource))
                    return outSource;
            }

            if (options.Style.IsItalic())
            {
                if (foundFonts.TryGetValue(options.FontName + " Italic", out outSource))
                    return outSource;
            }

            if (foundFonts.TryGetValue(options.FontName, out outSource))
                return outSource;
            
            return null;
        }

        /// <inheritdoc/>
        public void Import(SpriteFontAsset options, List<char> characters)
        {
            fontSource = GetFontSource(options);
            if (string.IsNullOrEmpty(fontSource))
              return;

            msdfgenExe = $"{Environment.GetEnvironmentVariable("SiliconStudioXenkoDir")}\\deps\\msdfgen\\msdfgen.exe";
            tempDir = $"{Environment.GetEnvironmentVariable("TEMP")}\\";

            var factory = new Factory();

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
                glyphList.Add(ImportGlyph(fontFace, character, fontMetrics, fontSize));

            Glyphs = glyphList;

            factory.Dispose();            
        }

        private Glyph ImportGlyph(FontFace fontFace, char character, FontMetrics fontMetrics, float fontSize)
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
