#include "dxt_wrapper.h"

// Utilities functions
void dxtComputePitch( DXGI_FORMAT fmt, size_t width, size_t height, size_t& rowPitch, size_t& slicePitch, DWORD flags = DirectX::CP_FLAGS_NONE )
{
	return DirectX::ComputePitch(fmt, width, height, rowPitch, slicePitch, flags);
}

bool dxtIsCompressed(DXGI_FORMAT fmt) { return DirectX::IsCompressed(fmt); }

HRESULT dxtConvert( const DirectX::Image& srcImage, DXGI_FORMAT format, DWORD filter, float threshold, DirectX::ScratchImage& cImage )
{
	return DirectX::Convert(srcImage, format, filter, threshold, cImage);
}

HRESULT dxtConvertArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DXGI_FORMAT format, DWORD filter, float threshold, DirectX::ScratchImage& cImage )
{
	return DirectX::Convert(srcImages, nimages, metadata, format, filter, threshold, cImage);
}

HRESULT dxtCompress( const DirectX::Image& srcImage, DXGI_FORMAT format, DWORD compress, float alphaRef, DirectX::ScratchImage& cImage )
{
	return DirectX::Compress(srcImage, format, compress, alphaRef, cImage);
}

HRESULT dxtCompressArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DXGI_FORMAT format, DWORD compress, float alphaRef, DirectX::ScratchImage& cImages )
{
	return DirectX::Compress(srcImages, nimages, metadata, format, compress, alphaRef, cImages);
}

HRESULT dxtDecompress( const DirectX::Image& cImage, DXGI_FORMAT format, DirectX::ScratchImage& image )
{
	return DirectX::Decompress(cImage, format, image);
}

HRESULT dxtDecompressArray( const DirectX::Image* cImages, size_t nimages, const DirectX::TexMetadata& metadata, DXGI_FORMAT format, DirectX::ScratchImage& images )
{
	return DirectX::Decompress(cImages,  nimages, metadata, format, images);
}

HRESULT dxtGenerateMipMaps( const DirectX::Image& baseImage, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain, bool allow1D = false)
{
	return DirectX::GenerateMipMaps(baseImage, filter, levels, mipChain, allow1D);
}

HRESULT dxtGenerateMipMapsArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain )
{
	return DirectX::GenerateMipMaps(srcImages, nimages, metadata, filter, levels, mipChain);
}

HRESULT dxtGenerateMipMaps3D( const DirectX::Image* baseImages, size_t depth, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain )
{
	return DirectX::GenerateMipMaps3D(baseImages, depth, filter, levels, mipChain);
}

HRESULT dxtGenerateMipMaps3DArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain )
{
	return DirectX::GenerateMipMaps3D(srcImages, nimages, metadata, filter, levels, mipChain);
}

HRESULT dxtResize(const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, size_t width, size_t height, DWORD filter, DirectX::ScratchImage& result )
{
	return DirectX::Resize(srcImages, nimages, metadata, width, height, filter, result);
}

HRESULT dxtComputeNormalMap( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD flags, float amplitude, DXGI_FORMAT format, DirectX::ScratchImage& normalMaps )
{
	return DirectX::ComputeNormalMap(srcImages, nimages, metadata, flags, amplitude, format, normalMaps);
}

HRESULT dxtPremultiplyAlpha( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD flags, DirectX::ScratchImage& result )
{
	return DirectX::PremultiplyAlpha(srcImages, nimages, metadata, flags, result);
}


// I/O functions
HRESULT dxtLoadDDSFile(LPCWSTR szFile, DWORD flags, DirectX::TexMetadata* metadata, DirectX::ScratchImage& image)
{
	return DirectX::LoadFromDDSFile(szFile, flags, metadata, image);
}

HRESULT dxtSaveToDDSFile( const DirectX::Image& image, DWORD flags, LPCWSTR szFile )
{
	return DirectX::SaveToDDSFile(image, flags, szFile);
}

HRESULT dxtSaveToDDSFileArray( const DirectX::Image* images, size_t nimages, const DirectX::TexMetadata& metadata, DWORD flags, LPCWSTR szFile )
{
	return DirectX::SaveToDDSFile(images, nimages, metadata, flags, szFile);
}

// Scratch Image
DirectX::ScratchImage * dxtCreateScratchImage()
{
	return new DirectX::ScratchImage();
}

void dxtDeleteScratchImage(DirectX::ScratchImage * img) { delete img; }

HRESULT dxtInitialize(DirectX::ScratchImage * img, const DirectX::TexMetadata& mdata ) { return img->Initialize(mdata); }

HRESULT dxtInitialize1D(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t length,  size_t arraySize,  size_t mipLevels ) { return img->Initialize1D(fmt, length, arraySize, mipLevels); }
HRESULT dxtInitialize2D(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t width,  size_t height,  size_t arraySize,  size_t mipLevels ) { return img->Initialize2D(fmt, width, height, arraySize, mipLevels); }
HRESULT dxtInitialize3D(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t width,  size_t height,  size_t depth,  size_t mipLevels ) { return img->Initialize3D(fmt, width, height, depth, mipLevels); }
HRESULT dxtInitializeCube(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t width,  size_t height,  size_t nCubes,  size_t mipLevels ) { return img->InitializeCube(fmt, width, height, nCubes, mipLevels); }

HRESULT dxtInitializeFromImage(DirectX::ScratchImage * img, const DirectX::Image& srcImage, bool allow1D) { return img->InitializeFromImage(srcImage, allow1D); }
HRESULT dxtInitializeArrayFromImages(DirectX::ScratchImage * img, const DirectX::Image* images, size_t nImages, bool allow1D ) { return img->InitializeArrayFromImages(images, nImages, allow1D); }
HRESULT dxtInitializeCubeFromImages(DirectX::ScratchImage * img, const DirectX::Image* images,  size_t nImages ) { return img->InitializeCubeFromImages(images, nImages); }
HRESULT dxtInitialize3DFromImages(DirectX::ScratchImage * img, const DirectX::Image* images,  size_t depth ) { return img->Initialize3DFromImages(images, depth); }


void dxtRelease(DirectX::ScratchImage * img) { img->Release(); }

bool dxtOverrideFormat(DirectX::ScratchImage * img, DXGI_FORMAT f ) { return img->OverrideFormat(f); }

const DirectX::TexMetadata& dxtGetMetadata(const DirectX::ScratchImage * img) { return img->GetMetadata(); }
const DirectX::Image* dxtGetImage(const DirectX::ScratchImage * img, size_t mip,  size_t item,  size_t slice)  { return img->GetImage(mip, item, slice); }

const DirectX::Image* dxtGetImages(const DirectX::ScratchImage * img) { return img->GetImages(); }
size_t dxtGetImageCount(const DirectX::ScratchImage * img) { return img->GetImageCount(); }

uint8_t* dxtGetPixels(const DirectX::ScratchImage * img) { return img->GetPixels(); }
size_t dxtGetPixelsSize(const DirectX::ScratchImage * img) { return img->GetPixelsSize(); }