// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.TextureConverter.Requests;


namespace SiliconStudio.TextureConverter.TexLibraries
{

    /// <summary>
    /// Class containing the needed native Data used by Paradox
    /// </summary>
    internal class ParadoxTextureLibraryData : ITextureLibraryData
    {
        /// <summary>
        /// The <see cref="Image"/> image
        /// </summary>
        public Image PdxImage;
    }


    /// <summary>
    /// Peforms requests from <see cref="TextureTool" /> using Paradox framework.
    /// </summary>
    internal class ParadoxTexLibrary : ITexLibrary
    {
        private static Logger Log = GlobalLogger.GetLogger("ParadoxTexLibrary");
        public static readonly string Extension = ".pdx";

        /// <summary>
        /// Initializes a new instance of the <see cref="ParadoxTexLibrary"/> class.
        /// </summary>
        public ParadoxTexLibrary(){}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. Nothing in this case
        /// </summary>
        public void Dispose(){}


        public void Dispose(TexImage image)
        {
            ParadoxTextureLibraryData libraryData = (ParadoxTextureLibraryData)image.LibraryData[this];
            if (libraryData.PdxImage != null) libraryData.PdxImage.Dispose();
        }

        public bool SupportBGRAOrder()
        {
            return true;
        }

        public void StartLibrary(TexImage image)
        {
            ParadoxTextureLibraryData libraryData = new ParadoxTextureLibraryData();
            image.LibraryData[this] = libraryData;
        }

        public void EndLibrary(TexImage image)
        {

        }

        public bool CanHandleRequest(TexImage image, IRequest request)
        {
            switch (request.Type)
            {
                case RequestType.Export:
                    {
                        string extension = Path.GetExtension(((ExportRequest)request).FilePath);
                        return extension.Equals(".dds") || extension.Equals(Extension);
                    }

                case RequestType.ExportToParadox:
                    return true;

                case RequestType.Loading: // Paradox can load dds file or his own format or a Paradox <see cref="Image"/> instance.
                    LoadingRequest load = (LoadingRequest)request;
                    if(load.Mode == LoadingRequest.LoadingMode.PdxImage) return true;
                    else if(load.Mode == LoadingRequest.LoadingMode.FilePath)
                    {
                        string extension = Path.GetExtension(load.FilePath);
                        return extension.Equals(".dds") || extension.Equals(Extension);
                    } else return false;

            }
            return false;
        }

        public void Execute(TexImage image, IRequest request)
        {
            ParadoxTextureLibraryData libraryData = image.LibraryData.ContainsKey(this) ? (ParadoxTextureLibraryData)image.LibraryData[this] : null;

            switch (request.Type)
            {
                case RequestType.Export:
                    Export(image, libraryData, (ExportRequest)request);
                    break;

                case RequestType.ExportToParadox:
                    ExportToParadox(image, libraryData, (ExportToParadoxRequest)request);
                    break;

                case RequestType.Loading:
                    Load(image, (LoadingRequest)request);
                    break;
            }
        }


        /// <summary>
        /// Exports the specified image into regular DDS file or a Paradox own file format.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="libraryData">The library data.</param>
        /// <param name="request">The request.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Image size different than expected.
        /// or
        /// Image could not be created.
        /// </exception>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="TexLibraryException">Unsupported file extension.</exception>
        private void Export(TexImage image, ParadoxTextureLibraryData libraryDataf, ExportRequest request)
        {
            Log.Info("Exporting to " + request.FilePath + " ...");

            Image pdxImage = null;

            if (request.MinimumMipMapSize > 1) // Check whether a minimum mipmap size was requested
            {
                if (image.Dimension == TexImage.TextureDimension.Texture3D)
                {

                    int newMipMapCount = 0; // the new mipmap count
                    int ct = 0; // ct will contain the number of SubImages per array element that we need to keep
                    int curDepth = image.Depth << 1;
                    for (int i = 0; i < image.MipmapCount; ++i)
                    {
                        curDepth = curDepth > 1 ? curDepth >>= 1 : curDepth;

                        if (image.SubImageArray[ct].Width <= request.MinimumMipMapSize || image.SubImageArray[ct].Height <= request.MinimumMipMapSize)
                        {
                            ct += curDepth;
                            ++newMipMapCount;
                            break;
                        }
                        ++newMipMapCount;
                        ct += curDepth;
                    }

                    int SubImagePerArrayElement = image.SubImageArray.Length / image.ArraySize; // number of SubImage in each texture array element.

                    // Initializing library native data according to the new mipmap level
                    pdxImage = Image.New3D(image.Width, image.Height, image.Depth, newMipMapCount, image.Format);

                    try
                    {
                        int ct2 = 0;
                        for (int i = 0; i < image.ArraySize; ++i)
                        {
                            for (int j = 0; j < ct; ++j)
                            {
                                Utilities.CopyMemory(pdxImage.PixelBuffer[ct2].DataPointer, pdxImage.PixelBuffer[j + i * SubImagePerArrayElement].DataPointer, pdxImage.PixelBuffer[j + i * SubImagePerArrayElement].BufferStride);
                                ++ct2;
                            }
                        }
                    }
                    catch (AccessViolationException e)
                    {
                        pdxImage.Dispose();
                        Log.Error("Failed to export texture with the mipmap minimum size request. ", e);
                        throw new TextureToolsException("Failed to export texture with the mipmap minimum size request. ", e);
                    }
                }
                else
                {

                    int newMipMapCount = image.MipmapCount;
                    int dataSize = image.DataSize;
                    for (int i = image.MipmapCount - 1; i > 0; --i)
                    {
                        if (image.SubImageArray[i].Width >= request.MinimumMipMapSize || image.SubImageArray[i].Height >= request.MinimumMipMapSize)
                        {
                            break;
                        }
                        dataSize -= image.SubImageArray[i].DataSize * image.ArraySize;
                        --newMipMapCount;
                    }

                    switch (image.Dimension)
                    {
                        case TexImage.TextureDimension.Texture1D:
                            pdxImage = Image.New1D(image.Width, image.MipmapCount, image.Format, image.ArraySize); break;
                        case TexImage.TextureDimension.Texture2D:
                            pdxImage = Image.New2D(image.Width, image.Height, newMipMapCount, image.Format, image.ArraySize); break;
                        case TexImage.TextureDimension.TextureCube:
                            pdxImage = Image.NewCube(image.Width, newMipMapCount, image.Format); break;
                    }
                    if (pdxImage == null)
                    {
                        Log.Error("Image could not be created.");
                        throw new InvalidOperationException("Image could not be created.");
                    }

                    if (pdxImage.TotalSizeInBytes != dataSize)
                    {
                        Log.Error("Image size different than expected.");
                        throw new InvalidOperationException("Image size different than expected.");
                    }

                    try
                    {
                        int gap = image.MipmapCount - newMipMapCount;
                        int j = 0;
                        for (int i = 0; i < image.ArraySize * newMipMapCount; ++i)
                        {
                            if (i == newMipMapCount || (i > newMipMapCount && (i % newMipMapCount == 0))) j += gap;
                            Utilities.CopyMemory(pdxImage.PixelBuffer[i].DataPointer, image.SubImageArray[j].Data, image.SubImageArray[j].DataSize);
                            ++j;
                        }
                    }
                    catch (AccessViolationException e)
                    {
                        pdxImage.Dispose();
                        Log.Error("Failed to export texture with the mipmap minimum size request. ", e);
                        throw new TextureToolsException("Failed to export texture with the mipmap minimum size request. ", e);
                    }
                }
            }
            else
            {
                switch (image.Dimension)
                {
                    case TexImage.TextureDimension.Texture1D:
                        pdxImage = Image.New1D(image.Width, image.MipmapCount, image.Format, image.ArraySize); break;
                    case TexImage.TextureDimension.Texture2D:
                        pdxImage = Image.New2D(image.Width, image.Height, image.MipmapCount, image.Format, image.ArraySize); break;
                    case TexImage.TextureDimension.Texture3D:
                        pdxImage = Image.New3D(image.Width, image.Height, image.Depth, image.MipmapCount, image.Format); break;
                    case TexImage.TextureDimension.TextureCube:
                        pdxImage = Image.NewCube(image.Width, image.MipmapCount, image.Format); break;
                }
                if (pdxImage == null)
                {
                    Log.Error("Image could not be created.");
                    throw new InvalidOperationException("Image could not be created.");
                }

                if (pdxImage.TotalSizeInBytes != image.DataSize)
                {
                    Log.Error("Image size different than expected.");
                    throw new InvalidOperationException("Image size different than expected.");
                }

                Utilities.CopyMemory(pdxImage.DataPointer, image.Data, image.DataSize);
            }

            using (var fileStream = new FileStream(request.FilePath, FileMode.Create, FileAccess.Write))
            {
                String extension = Path.GetExtension(request.FilePath);
                if(extension.Equals(Extension))
                    pdxImage.Save(fileStream, ImageFileType.Paradox);
                else if (extension.Equals(".dds"))
                    pdxImage.Save(fileStream, ImageFileType.Dds);
                else
                {
                    Log.Error("Unsupported file extension.");
                    throw new TextureToolsException("Unsupported file extension.");
                }
            }

            pdxImage.Dispose();
            image.Save(request.FilePath);
        }


        /// <summary>
        /// Exports to Paradox <see cref="Image"/>. An instance will be stored in the <see cref="ExportToParadoxRequest"/> instance.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="libraryData">The library data.</param>
        /// <param name="request">The request.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Image size different than expected.
        /// or
        /// Failed to convert texture into Paradox Image.
        /// </exception>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ExportToParadox(TexImage image, ParadoxTextureLibraryData libraryData, ExportToParadoxRequest request)
        {
            Log.Info("Exporting to Paradox Image ...");

            Image pdxImage = null;
            switch (image.Dimension)
            {
                case TexImage.TextureDimension.Texture1D:
                    pdxImage = Image.New1D(image.Width, image.MipmapCount, image.Format, image.ArraySize); break;
                case TexImage.TextureDimension.Texture2D:
                    pdxImage = Image.New2D(image.Width, image.Height, image.MipmapCount, image.Format, image.ArraySize); break;
                case TexImage.TextureDimension.Texture3D:
                    pdxImage = Image.New3D(image.Width, image.Height, image.Depth, image.MipmapCount, image.Format); break;
                case TexImage.TextureDimension.TextureCube:
                    pdxImage = Image.NewCube(image.Width, image.MipmapCount, image.Format); break;
            }
            if (pdxImage == null)
            {
                Log.Error("Image could not be created.");
                throw new InvalidOperationException("Image could not be created.");
            }

            if (pdxImage.TotalSizeInBytes != image.DataSize)
            {
                Log.Error("Image size different than expected.");
                throw new InvalidOperationException("Image size different than expected.");
            }

            Utilities.CopyMemory(pdxImage.DataPointer, image.Data, image.DataSize);

            request.PdxImage = pdxImage;
        }


        /// <summary>
        /// Loads the specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="request">The request.</param>
        private void Load(TexImage image, LoadingRequest request)
        {
            Log.Info("Loading Paradox Image ...");

            var libraryData = new ParadoxTextureLibraryData();
            image.LibraryData[this] = libraryData;

            Image inputImage;
            if (request.Mode == LoadingRequest.LoadingMode.PdxImage)
            {
                inputImage = request.PdxImage;
            }
            else if (request.Mode == LoadingRequest.LoadingMode.FilePath)
            {
                using (var fileStream = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read))
                    inputImage = Image.Load(fileStream);

                libraryData.PdxImage = inputImage; // the image need to be disposed by the paradox text library
            }
            else
            {
                throw new NotImplementedException();
            }

            var inputFormat = inputImage.Description.Format;
            image.Data = inputImage.DataPointer;
            image.DataSize = 0;
            image.Width = inputImage.Description.Width;
            image.Height = inputImage.Description.Height;
            image.Depth = inputImage.Description.Depth;
            image.Format = request.LoadAsSRgb ? inputFormat.ToSRgb() : inputFormat.ToNonSRgb();
            image.MipmapCount = request.KeepMipMap ? inputImage.Description.MipLevels : 1;
            image.ArraySize = inputImage.Description.ArraySize;

            int rowPitch, slicePitch;
            Tools.ComputePitch(image.Format, image.Width, image.Height, out rowPitch, out slicePitch);
            image.RowPitch = rowPitch;
            image.SlicePitch = slicePitch;

            var bufferStepFactor = request.KeepMipMap ? 1 : inputImage.Description.MipLevels;
            int imageCount = inputImage.PixelBuffer.Count / bufferStepFactor;
            image.SubImageArray = new TexImage.SubImage[imageCount];
            
            for (int i = 0; i < imageCount; ++i)
            { 
                image.SubImageArray[i] = new TexImage.SubImage();
                image.SubImageArray[i].Data = inputImage.PixelBuffer[i * bufferStepFactor].DataPointer;
                image.SubImageArray[i].DataSize = inputImage.PixelBuffer[i * bufferStepFactor].BufferStride;
                image.SubImageArray[i].Width = inputImage.PixelBuffer[i * bufferStepFactor].Width;
                image.SubImageArray[i].Height = inputImage.PixelBuffer[i * bufferStepFactor].Height;
                image.SubImageArray[i].RowPitch = inputImage.PixelBuffer[i * bufferStepFactor].RowStride;
                image.SubImageArray[i].SlicePitch = inputImage.PixelBuffer[i * bufferStepFactor].BufferStride;
                image.DataSize += image.SubImageArray[i].DataSize;
            }

            switch (inputImage.Description.Dimension)
            {
                case TextureDimension.Texture1D:
                    image.Dimension = TexImage.TextureDimension.Texture1D; break;
                case TextureDimension.Texture2D:
                    image.Dimension = TexImage.TextureDimension.Texture2D; break;
                case TextureDimension.Texture3D:
                    image.Dimension = TexImage.TextureDimension.Texture3D; break;
                case TextureDimension.TextureCube:
                    image.Dimension = TexImage.TextureDimension.TextureCube; break;
            }

            image.DisposingLibrary = this;
        }
    }
}
