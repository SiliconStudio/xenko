#ifndef DXT_WRAPPER_H
#define DXT_WRAPPER_H

#define DXT_API __declspec(dllexport)

#include "DirectXTex.h"

extern "C" {

	// Utilities functions
	DXT_API void dxtComputePitch( DXGI_FORMAT fmt, size_t width, size_t height, size_t& rowPitch, size_t& slicePitch, DWORD flags );
	DXT_API bool dxtIsCompressed(DXGI_FORMAT fmt);
	DXT_API HRESULT dxtConvert( const DirectX::Image& srcImage, DXGI_FORMAT format, DWORD filter, float threshold, DirectX::ScratchImage& cImage );
	DXT_API HRESULT dxtConvertArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DXGI_FORMAT format, DWORD filter, float threshold, DirectX::ScratchImage& cImage );
	DXT_API HRESULT dxtCompress( const DirectX::Image& srcImage,  DXGI_FORMAT format,  DWORD compress,  float alphaRef, DirectX::ScratchImage& cImage );
    DXT_API HRESULT dxtCompressArray( const DirectX::Image* srcImages,  size_t nimages,  const DirectX::TexMetadata& metadata, DXGI_FORMAT format,  DWORD compress,  float alphaRef,  DirectX::ScratchImage& cImages );
    DXT_API HRESULT dxtDecompress(  const DirectX::Image& cImage,  DXGI_FORMAT format,  DirectX::ScratchImage& image );
    DXT_API HRESULT dxtDecompressArray( const DirectX::Image* cImages, size_t nimages, const DirectX::TexMetadata& metadata, DXGI_FORMAT format, DirectX::ScratchImage& images );
	DXT_API HRESULT dxtGenerateMipMaps( const DirectX::Image& baseImage, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain, bool allow1D);
    DXT_API HRESULT dxtGenerateMipMapsArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain );
    DXT_API HRESULT dxtGenerateMipMaps3D( const DirectX::Image* baseImages, size_t depth, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain );
    DXT_API HRESULT dxtGenerateMipMaps3DArray( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD filter, size_t levels, DirectX::ScratchImage& mipChain );
	DXT_API HRESULT dxtResize(const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, size_t width, size_t height, DWORD filter, DirectX::ScratchImage& result );
	DXT_API HRESULT dxtComputeNormalMap( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD flags, float amplitude, DXGI_FORMAT format, DirectX::ScratchImage& normalMaps );
	DXT_API HRESULT dxtPremultiplyAlpha( const DirectX::Image* srcImages, size_t nimages, const DirectX::TexMetadata& metadata, DWORD flags, DirectX::ScratchImage& result );

	// I/O functions
	DXT_API HRESULT dxtLoadDDSFile(LPCWSTR szFile, DWORD flags, DirectX::TexMetadata* metadata, DirectX::ScratchImage& image);
	DXT_API HRESULT dxtSaveToDDSFile( const DirectX::Image& image, DWORD flags, LPCWSTR szFile );
    DXT_API HRESULT dxtSaveToDDSFileArray( const DirectX::Image* images, size_t nimages, const DirectX::TexMetadata& metadata, DWORD flags, LPCWSTR szFile );

	// Scratch Image
	DXT_API DirectX::ScratchImage * dxtCreateScratchImage();
	DXT_API void dxtDeleteScratchImage(DirectX::ScratchImage * img);
	DXT_API HRESULT dxtInitialize(DirectX::ScratchImage * img, const DirectX::TexMetadata& mdata );

	DXT_API HRESULT dxtInitialize1D(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t length,  size_t arraySize,  size_t mipLevels );
	DXT_API HRESULT dxtInitialize2D(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t width,  size_t height,  size_t arraySize,  size_t mipLevels );
	DXT_API HRESULT dxtInitialize3D(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t width,  size_t height,  size_t depth,  size_t mipLevels );
	DXT_API HRESULT dxtInitializeCube(DirectX::ScratchImage * img, DXGI_FORMAT fmt,  size_t width,  size_t height,  size_t nCubes,  size_t mipLevels );

	DXT_API HRESULT dxtInitializeFromImage(DirectX::ScratchImage * img, const DirectX::Image& srcImage, bool allow1D);
	DXT_API HRESULT dxtInitializeArrayFromImages(DirectX::ScratchImage * img, const DirectX::Image* images, size_t nImages, bool allow1D );
	DXT_API HRESULT dxtInitializeCubeFromImages(DirectX::ScratchImage * img, const DirectX::Image* images,  size_t nImages );
    DXT_API HRESULT dxtInitialize3DFromImages(DirectX::ScratchImage * img, const DirectX::Image* images,  size_t depth );

	DXT_API void dxtRelease(DirectX::ScratchImage * img);

	DXT_API bool dxtOverrideFormat(DirectX::ScratchImage * img, DXGI_FORMAT f );

	DXT_API const DirectX::TexMetadata& dxtGetMetadata(const DirectX::ScratchImage * img);
	DXT_API const DirectX::Image* dxtGetImage(const DirectX::ScratchImage * img, size_t mip,  size_t item,  size_t slice);

	DXT_API const DirectX::Image* dxtGetImages(const DirectX::ScratchImage * img);
	DXT_API size_t dxtGetImageCount(const DirectX::ScratchImage * img);

	DXT_API uint8_t* dxtGetPixels(const DirectX::ScratchImage * img);
	DXT_API size_t dxtGetPixelsSize(const DirectX::ScratchImage * img);

} // extern "C"



#endif // DXT_WRAPPER_H