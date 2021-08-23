//--------------------------------------------------------------------------------------
// File: ScreenGrab12.cpp
//
// Function for capturing a 2D texture and saving it to a file (aka a 'screenshot'
// when used on a Direct3D Render Target).
//
// Note these functions are useful as a light-weight runtime screen grabber. For
// full-featured texture capture, DDS writer, and texture processing pipeline,
// see the 'Texconv' sample and the 'DirectXTex' library.
//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
// http://go.microsoft.com/fwlink/?LinkID=615561
//--------------------------------------------------------------------------------------

// Does not capture 1D textures or 3D textures (volume maps)

// Does not capture mipmap chains, only the top-most texture level is saved

// For 2D array textures and cubemaps, it captures only the first image in the array
#include"stdafx.h"
#include "ScreenGrab12.h"

#include <cassert>
#include <cstddef>
#include <cstring>
#include <memory>
#include <new>
#include <vector>
#include <format>
//wav
#include <Windows.h>
#include <mfapi.h>
#include <mfidl.h>
#include <Mfreadwrite.h>
#include <mferror.h>

#pragma comment(lib, "mfreadwrite")
#pragma comment(lib, "mfplat")
#pragma comment(lib, "mfuuid")

#ifdef WIN32
#include <wincodec.h>
#include <wrl\client.h>
#else
#include <fstream>
#include <filesystem>
#include <thread>
#endif

#ifdef __clang__
#pragma clang diagnostic ignored "-Wtautological-type-limit-compare"
#pragma clang diagnostic ignored "-Wcovered-switch-default"
#pragma clang diagnostic ignored "-Wswitch"
#pragma clang diagnostic ignored "-Wswitch-enum"
#endif

#define D3DX12_NO_STATE_OBJECT_HELPERS
#ifdef WIN32
#include "d3dx12.h"
#else
#include "directx/d3dx12.h"
#endif

#include "XAudio2Versions.h"
#include "WAVFileReader.h"

using Microsoft::WRL::ComPtr;

//--------------------------------------------------------------------------------------
// Macros
//--------------------------------------------------------------------------------------
#ifndef MAKEFOURCC
#define MAKEFOURCC(ch0, ch1, ch2, ch3)                              \
                ((uint32_t)(uint8_t)(ch0) | ((uint32_t)(uint8_t)(ch1) << 8) |       \
                ((uint32_t)(uint8_t)(ch2) << 16) | ((uint32_t)(uint8_t)(ch3) << 24 ))
#endif /* defined(MAKEFOURCC) */

// HRESULT_FROM_WIN32(ERROR_ARITHMETIC_OVERFLOW)
#define HRESULT_E_ARITHMETIC_OVERFLOW static_cast<HRESULT>(0x80070216L)

// HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED)
#define HRESULT_E_NOT_SUPPORTED static_cast<HRESULT>(0x80070032L)

//--------------------------------------------------------------------------------------
// DDS file structure definitions
//
// See DDS.h in the 'Texconv' sample and the 'DirectXTex' library
//--------------------------------------------------------------------------------------
namespace
{
    #pragma pack(push,1)

    #define DDS_MAGIC 0x20534444 // "DDS "

    struct DDS_PIXELFORMAT
    {
        uint32_t    size;
        uint32_t    flags;
        uint32_t    fourCC;
        uint32_t    RGBBitCount;
        uint32_t    RBitMask;
        uint32_t    GBitMask;
        uint32_t    BBitMask;
        uint32_t    ABitMask;
    };

    #define DDS_FOURCC      0x00000004  // DDPF_FOURCC
    #define DDS_RGB         0x00000040  // DDPF_RGB
    #define DDS_RGBA        0x00000041  // DDPF_RGB | DDPF_ALPHAPIXELS
    #define DDS_LUMINANCE   0x00020000  // DDPF_LUMINANCE
    #define DDS_LUMINANCEA  0x00020001  // DDPF_LUMINANCE | DDPF_ALPHAPIXELS
    #define DDS_ALPHA       0x00000002  // DDPF_ALPHA
    #define DDS_BUMPDUDV    0x00080000  // DDPF_BUMPDUDV

    #define DDS_HEADER_FLAGS_TEXTURE        0x00001007  // DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT
    #define DDS_HEADER_FLAGS_MIPMAP         0x00020000  // DDSD_MIPMAPCOUNT
    #define DDS_HEADER_FLAGS_PITCH          0x00000008  // DDSD_PITCH
    #define DDS_HEADER_FLAGS_LINEARSIZE     0x00080000  // DDSD_LINEARSIZE

    #define DDS_SURFACE_FLAGS_TEXTURE 0x00001000 // DDSCAPS_TEXTURE

    struct DDS_HEADER
    {
        uint32_t        size;
        uint32_t        flags;
        uint32_t        height;
        uint32_t        width;
        uint32_t        pitchOrLinearSize;
        uint32_t        depth; // only if DDS_HEADER_FLAGS_VOLUME is set in flags
        uint32_t        mipMapCount;
        uint32_t        reserved1[11];
        DDS_PIXELFORMAT ddspf;
        uint32_t        caps;
        uint32_t        caps2;
        uint32_t        caps3;
        uint32_t        caps4;
        uint32_t        reserved2;
    };

    struct DDS_HEADER_DXT10
    {
        DXGI_FORMAT     dxgiFormat;
        uint32_t        resourceDimension;
        uint32_t        miscFlag; // see D3D11_RESOURCE_MISC_FLAG
        uint32_t        arraySize;
        uint32_t        reserved;
    };

    #pragma pack(pop)

    const DDS_PIXELFORMAT DDSPF_DXT1 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('D','X','T','1'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_DXT3 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('D','X','T','3'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_DXT5 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('D','X','T','5'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_BC4_UNORM =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('B','C','4','U'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_BC4_SNORM =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('B','C','4','S'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_BC5_UNORM =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('B','C','5','U'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_BC5_SNORM =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('B','C','5','S'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_R8G8_B8G8 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('R','G','B','G'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_G8R8_G8B8 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('G','R','G','B'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_YUY2 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('Y','U','Y','2'), 0, 0, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_A8R8G8B8 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGBA, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000 };

    const DDS_PIXELFORMAT DDSPF_X8R8G8B8 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGB,  0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0 };

    const DDS_PIXELFORMAT DDSPF_A8B8G8R8 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGBA, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000 };

    const DDS_PIXELFORMAT DDSPF_G16R16 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGB,  0, 32, 0x0000ffff, 0xffff0000, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_R5G6B5 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGB, 0, 16, 0xf800, 0x07e0, 0x001f, 0 };

    const DDS_PIXELFORMAT DDSPF_A1R5G5B5 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGBA, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x8000 };

    const DDS_PIXELFORMAT DDSPF_A4R4G4B4 =
    { sizeof(DDS_PIXELFORMAT), DDS_RGBA, 0, 16, 0x0f00, 0x00f0, 0x000f, 0xf000 };

    const DDS_PIXELFORMAT DDSPF_L8 =
    { sizeof(DDS_PIXELFORMAT), DDS_LUMINANCE, 0,  8, 0xff, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_L16 =
    { sizeof(DDS_PIXELFORMAT), DDS_LUMINANCE, 0, 16, 0xffff, 0, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_A8L8 =
    { sizeof(DDS_PIXELFORMAT), DDS_LUMINANCEA, 0, 16, 0x00ff, 0, 0, 0xff00 };

    const DDS_PIXELFORMAT DDSPF_A8 =
    { sizeof(DDS_PIXELFORMAT), DDS_ALPHA, 0, 8, 0, 0, 0, 0xff };

    const DDS_PIXELFORMAT DDSPF_V8U8 =
    { sizeof(DDS_PIXELFORMAT), DDS_BUMPDUDV, 0, 16, 0x00ff, 0xff00, 0, 0 };

    const DDS_PIXELFORMAT DDSPF_Q8W8V8U8 =
    { sizeof(DDS_PIXELFORMAT), DDS_BUMPDUDV, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000 };

    const DDS_PIXELFORMAT DDSPF_V16U16 =
    { sizeof(DDS_PIXELFORMAT), DDS_BUMPDUDV, 0, 32, 0x0000ffff, 0xffff0000, 0, 0 };

    // DXGI_FORMAT_R10G10B10A2_UNORM should be written using DX10 extension to avoid D3DX 10:10:10:2 reversal issue

    // This indicates the DDS_HEADER_DXT10 extension is present (the format is in dxgiFormat)
    const DDS_PIXELFORMAT DDSPF_DX10 =
    { sizeof(DDS_PIXELFORMAT), DDS_FOURCC, MAKEFOURCC('D','X','1','0'), 0, 0, 0, 0, 0 };

    //-----------------------------------------------------------------------------
#ifdef WIN32
    struct handle_closer { void operator()(HANDLE h) noexcept { if (h) CloseHandle(h); } };

    using ScopedHandle = std::unique_ptr<void, handle_closer>;

    inline HANDLE safe_handle(HANDLE h) noexcept { return (h == INVALID_HANDLE_VALUE) ? nullptr : h; }

    class auto_delete_file
    {
    public:
        auto_delete_file(HANDLE hFile) noexcept : m_handle(hFile) {}
        ~auto_delete_file()
        {
            if (m_handle)
            {
                FILE_DISPOSITION_INFO info = {};
                info.DeleteFile = TRUE;
                (void)SetFileInformationByHandle(m_handle, FileDispositionInfo, &info, sizeof(info));
            }
        }

        auto_delete_file(const auto_delete_file&) = delete;
        auto_delete_file& operator=(const auto_delete_file&) = delete;

        auto_delete_file(const auto_delete_file&&) = delete;
        auto_delete_file& operator=(const auto_delete_file&&) = delete;

        void clear() noexcept { m_handle = nullptr; }

    private:
        HANDLE m_handle;
    };

    class auto_delete_file_wic
    {
    public:
        auto_delete_file_wic(ComPtr<IWICStream>& hFile, const wchar_t* szFile) noexcept : m_filename(szFile), m_handle(hFile) {}
        ~auto_delete_file_wic()
        {
            if (m_filename)
            {
                m_handle.Reset();
                DeleteFileW(m_filename);
            }
        }

        auto_delete_file_wic(const auto_delete_file_wic&) = delete;
        auto_delete_file_wic& operator=(const auto_delete_file_wic&) = delete;

        auto_delete_file_wic(const auto_delete_file_wic&&) = delete;
        auto_delete_file_wic& operator=(const auto_delete_file_wic&&) = delete;

        void clear() noexcept { m_filename = nullptr; }

    private:
        const wchar_t* m_filename;
        ComPtr<IWICStream>& m_handle;
    };
#endif

    //--------------------------------------------------------------------------------------
    // Return the BPP for a particular format
    //--------------------------------------------------------------------------------------
    size_t BitsPerPixel(_In_ DXGI_FORMAT fmt) noexcept
    {
        switch (fmt)
        {
        case DXGI_FORMAT_R32G32B32A32_TYPELESS:
        case DXGI_FORMAT_R32G32B32A32_FLOAT:
        case DXGI_FORMAT_R32G32B32A32_UINT:
        case DXGI_FORMAT_R32G32B32A32_SINT:
            return 128;

        case DXGI_FORMAT_R32G32B32_TYPELESS:
        case DXGI_FORMAT_R32G32B32_FLOAT:
        case DXGI_FORMAT_R32G32B32_UINT:
        case DXGI_FORMAT_R32G32B32_SINT:
            return 96;

        case DXGI_FORMAT_R16G16B16A16_TYPELESS:
        case DXGI_FORMAT_R16G16B16A16_FLOAT:
        case DXGI_FORMAT_R16G16B16A16_UNORM:
        case DXGI_FORMAT_R16G16B16A16_UINT:
        case DXGI_FORMAT_R16G16B16A16_SNORM:
        case DXGI_FORMAT_R16G16B16A16_SINT:
        case DXGI_FORMAT_R32G32_TYPELESS:
        case DXGI_FORMAT_R32G32_FLOAT:
        case DXGI_FORMAT_R32G32_UINT:
        case DXGI_FORMAT_R32G32_SINT:
        case DXGI_FORMAT_R32G8X24_TYPELESS:
        case DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
        case DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
        case DXGI_FORMAT_X32_TYPELESS_G8X24_UINT:
        case DXGI_FORMAT_Y416:
        case DXGI_FORMAT_Y210:
        case DXGI_FORMAT_Y216:
            return 64;

        case DXGI_FORMAT_R10G10B10A2_TYPELESS:
        case DXGI_FORMAT_R10G10B10A2_UNORM:
        case DXGI_FORMAT_R10G10B10A2_UINT:
        case DXGI_FORMAT_R11G11B10_FLOAT:
        case DXGI_FORMAT_R8G8B8A8_TYPELESS:
        case DXGI_FORMAT_R8G8B8A8_UNORM:
        case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
        case DXGI_FORMAT_R8G8B8A8_UINT:
        case DXGI_FORMAT_R8G8B8A8_SNORM:
        case DXGI_FORMAT_R8G8B8A8_SINT:
        case DXGI_FORMAT_R16G16_TYPELESS:
        case DXGI_FORMAT_R16G16_FLOAT:
        case DXGI_FORMAT_R16G16_UNORM:
        case DXGI_FORMAT_R16G16_UINT:
        case DXGI_FORMAT_R16G16_SNORM:
        case DXGI_FORMAT_R16G16_SINT:
        case DXGI_FORMAT_R32_TYPELESS:
        case DXGI_FORMAT_D32_FLOAT:
        case DXGI_FORMAT_R32_FLOAT:
        case DXGI_FORMAT_R32_UINT:
        case DXGI_FORMAT_R32_SINT:
        case DXGI_FORMAT_R24G8_TYPELESS:
        case DXGI_FORMAT_D24_UNORM_S8_UINT:
        case DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
        case DXGI_FORMAT_X24_TYPELESS_G8_UINT:
        case DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
        case DXGI_FORMAT_R8G8_B8G8_UNORM:
        case DXGI_FORMAT_G8R8_G8B8_UNORM:
        case DXGI_FORMAT_B8G8R8A8_UNORM:
        case DXGI_FORMAT_B8G8R8X8_UNORM:
        case DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
        case DXGI_FORMAT_B8G8R8A8_TYPELESS:
        case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
        case DXGI_FORMAT_B8G8R8X8_TYPELESS:
        case DXGI_FORMAT_B8G8R8X8_UNORM_SRGB:
        case DXGI_FORMAT_AYUV:
        case DXGI_FORMAT_Y410:
        case DXGI_FORMAT_YUY2:
            return 32;

        case DXGI_FORMAT_P010:
        case DXGI_FORMAT_P016:
        case DXGI_FORMAT_V408:
            return 24;

        case DXGI_FORMAT_R8G8_TYPELESS:
        case DXGI_FORMAT_R8G8_UNORM:
        case DXGI_FORMAT_R8G8_UINT:
        case DXGI_FORMAT_R8G8_SNORM:
        case DXGI_FORMAT_R8G8_SINT:
        case DXGI_FORMAT_R16_TYPELESS:
        case DXGI_FORMAT_R16_FLOAT:
        case DXGI_FORMAT_D16_UNORM:
        case DXGI_FORMAT_R16_UNORM:
        case DXGI_FORMAT_R16_UINT:
        case DXGI_FORMAT_R16_SNORM:
        case DXGI_FORMAT_R16_SINT:
        case DXGI_FORMAT_B5G6R5_UNORM:
        case DXGI_FORMAT_B5G5R5A1_UNORM:
        case DXGI_FORMAT_A8P8:
        case DXGI_FORMAT_B4G4R4A4_UNORM:
        case DXGI_FORMAT_P208:
        case DXGI_FORMAT_V208:
            return 16;

        case DXGI_FORMAT_NV12:
        case DXGI_FORMAT_420_OPAQUE:
        case DXGI_FORMAT_NV11:
            return 12;

        case DXGI_FORMAT_R8_TYPELESS:
        case DXGI_FORMAT_R8_UNORM:
        case DXGI_FORMAT_R8_UINT:
        case DXGI_FORMAT_R8_SNORM:
        case DXGI_FORMAT_R8_SINT:
        case DXGI_FORMAT_A8_UNORM:
        case DXGI_FORMAT_BC2_TYPELESS:
        case DXGI_FORMAT_BC2_UNORM:
        case DXGI_FORMAT_BC2_UNORM_SRGB:
        case DXGI_FORMAT_BC3_TYPELESS:
        case DXGI_FORMAT_BC3_UNORM:
        case DXGI_FORMAT_BC3_UNORM_SRGB:
        case DXGI_FORMAT_BC5_TYPELESS:
        case DXGI_FORMAT_BC5_UNORM:
        case DXGI_FORMAT_BC5_SNORM:
        case DXGI_FORMAT_BC6H_TYPELESS:
        case DXGI_FORMAT_BC6H_UF16:
        case DXGI_FORMAT_BC6H_SF16:
        case DXGI_FORMAT_BC7_TYPELESS:
        case DXGI_FORMAT_BC7_UNORM:
        case DXGI_FORMAT_BC7_UNORM_SRGB:
        case DXGI_FORMAT_AI44:
        case DXGI_FORMAT_IA44:
        case DXGI_FORMAT_P8:
            return 8;

        case DXGI_FORMAT_R1_UNORM:
            return 1;

        case DXGI_FORMAT_BC1_TYPELESS:
        case DXGI_FORMAT_BC1_UNORM:
        case DXGI_FORMAT_BC1_UNORM_SRGB:
        case DXGI_FORMAT_BC4_TYPELESS:
        case DXGI_FORMAT_BC4_UNORM:
        case DXGI_FORMAT_BC4_SNORM:
            return 4;

        default:
            return 0;
        }
    }


    //--------------------------------------------------------------------------------------
    // Determines if the format is block compressed
    //--------------------------------------------------------------------------------------
    bool IsCompressed(_In_ DXGI_FORMAT fmt) noexcept
    {
        switch (fmt)
        {
        case DXGI_FORMAT_BC1_TYPELESS:
        case DXGI_FORMAT_BC1_UNORM:
        case DXGI_FORMAT_BC1_UNORM_SRGB:
        case DXGI_FORMAT_BC2_TYPELESS:
        case DXGI_FORMAT_BC2_UNORM:
        case DXGI_FORMAT_BC2_UNORM_SRGB:
        case DXGI_FORMAT_BC3_TYPELESS:
        case DXGI_FORMAT_BC3_UNORM:
        case DXGI_FORMAT_BC3_UNORM_SRGB:
        case DXGI_FORMAT_BC4_TYPELESS:
        case DXGI_FORMAT_BC4_UNORM:
        case DXGI_FORMAT_BC4_SNORM:
        case DXGI_FORMAT_BC5_TYPELESS:
        case DXGI_FORMAT_BC5_UNORM:
        case DXGI_FORMAT_BC5_SNORM:
        case DXGI_FORMAT_BC6H_TYPELESS:
        case DXGI_FORMAT_BC6H_UF16:
        case DXGI_FORMAT_BC6H_SF16:
        case DXGI_FORMAT_BC7_TYPELESS:
        case DXGI_FORMAT_BC7_UNORM:
        case DXGI_FORMAT_BC7_UNORM_SRGB:
            return true;

        default:
            return false;
        }
    }


    //--------------------------------------------------------------------------------------
    // Get surface information for a particular format
    //--------------------------------------------------------------------------------------
    HRESULT GetSurfaceInfo(
        _In_ size_t width,
        _In_ size_t height,
        _In_ DXGI_FORMAT fmt,
        _Out_opt_ size_t* outNumBytes,
        _Out_opt_ size_t* outRowBytes,
        _Out_opt_ size_t* outNumRows) noexcept
    {
        uint64_t numBytes = 0;
        uint64_t rowBytes = 0;
        uint64_t numRows = 0;

        bool bc = false;
        bool packed = false;
        bool planar = false;
        size_t bpe = 0;
        switch (fmt)
        {
        case DXGI_FORMAT_BC1_TYPELESS:
        case DXGI_FORMAT_BC1_UNORM:
        case DXGI_FORMAT_BC1_UNORM_SRGB:
        case DXGI_FORMAT_BC4_TYPELESS:
        case DXGI_FORMAT_BC4_UNORM:
        case DXGI_FORMAT_BC4_SNORM:
            bc = true;
            bpe = 8;
            break;

        case DXGI_FORMAT_BC2_TYPELESS:
        case DXGI_FORMAT_BC2_UNORM:
        case DXGI_FORMAT_BC2_UNORM_SRGB:
        case DXGI_FORMAT_BC3_TYPELESS:
        case DXGI_FORMAT_BC3_UNORM:
        case DXGI_FORMAT_BC3_UNORM_SRGB:
        case DXGI_FORMAT_BC5_TYPELESS:
        case DXGI_FORMAT_BC5_UNORM:
        case DXGI_FORMAT_BC5_SNORM:
        case DXGI_FORMAT_BC6H_TYPELESS:
        case DXGI_FORMAT_BC6H_UF16:
        case DXGI_FORMAT_BC6H_SF16:
        case DXGI_FORMAT_BC7_TYPELESS:
        case DXGI_FORMAT_BC7_UNORM:
        case DXGI_FORMAT_BC7_UNORM_SRGB:
            bc = true;
            bpe = 16;
            break;

        case DXGI_FORMAT_R8G8_B8G8_UNORM:
        case DXGI_FORMAT_G8R8_G8B8_UNORM:
        case DXGI_FORMAT_YUY2:
            packed = true;
            bpe = 4;
            break;

        case DXGI_FORMAT_Y210:
        case DXGI_FORMAT_Y216:
            packed = true;
            bpe = 8;
            break;

        case DXGI_FORMAT_NV12:
        case DXGI_FORMAT_420_OPAQUE:
        case DXGI_FORMAT_P208:
            planar = true;
            bpe = 2;
            break;

        case DXGI_FORMAT_P010:
        case DXGI_FORMAT_P016:
            planar = true;
            bpe = 4;
            break;

        default:
            break;
        }

        if (bc)
        {
            uint64_t numBlocksWide = 0;
            if (width > 0)
            {
                numBlocksWide = std::max<uint64_t>(1u, (uint64_t(width) + 3u) / 4u);
            }
            uint64_t numBlocksHigh = 0;
            if (height > 0)
            {
                numBlocksHigh = std::max<uint64_t>(1u, (uint64_t(height) + 3u) / 4u);
            }
            rowBytes = numBlocksWide * bpe;
            numRows = numBlocksHigh;
            numBytes = rowBytes * numBlocksHigh;
        }
        else if (packed)
        {
            rowBytes = ((uint64_t(width) + 1u) >> 1) * bpe;
            numRows = uint64_t(height);
            numBytes = rowBytes * height;
        }
        else if (fmt == DXGI_FORMAT_NV11)
        {
            rowBytes = ((uint64_t(width) + 3u) >> 2) * 4u;
            numRows = uint64_t(height) * 2u; // Direct3D makes this simplifying assumption, although it is larger than the 4:1:1 data
            numBytes = rowBytes * numRows;
        }
        else if (planar)
        {
            rowBytes = ((uint64_t(width) + 1u) >> 1) * bpe;
            numBytes = (rowBytes * uint64_t(height)) + ((rowBytes * uint64_t(height) + 1u) >> 1);
            numRows = height + ((uint64_t(height) + 1u) >> 1);
        }
        else
        {
            size_t bpp = BitsPerPixel(fmt);
            if (!bpp)
                return E_INVALIDARG;

            rowBytes = (uint64_t(width) * bpp + 7u) / 8u; // round up to nearest byte
            numRows = uint64_t(height);
            numBytes = rowBytes * height;
        }

#if defined(_M_IX86) || defined(_M_ARM) || defined(_M_HYBRID_X86_ARM64)
        static_assert(sizeof(size_t) == 4, "Not a 32-bit platform!");
        if (numBytes > UINT32_MAX || rowBytes > UINT32_MAX || numRows > UINT32_MAX)
            return HRESULT_E_ARITHMETIC_OVERFLOW;
#else
        static_assert(sizeof(size_t) == 8, "Not a 64-bit platform!");
#endif

        if (outNumBytes)
        {
            *outNumBytes = static_cast<size_t>(numBytes);
        }
        if (outRowBytes)
        {
            *outRowBytes = static_cast<size_t>(rowBytes);
        }
        if (outNumRows)
        {
            *outNumRows = static_cast<size_t>(numRows);
        }

        return S_OK;
    }


    //--------------------------------------------------------------------------------------
    DXGI_FORMAT EnsureNotTypeless(DXGI_FORMAT fmt) noexcept
    {
        // Assumes UNORM or FLOAT; doesn't use UINT or SINT
        switch (fmt)
        {
        case DXGI_FORMAT_R32G32B32A32_TYPELESS: return DXGI_FORMAT_R32G32B32A32_FLOAT;
        case DXGI_FORMAT_R32G32B32_TYPELESS:    return DXGI_FORMAT_R32G32B32_FLOAT;
        case DXGI_FORMAT_R16G16B16A16_TYPELESS: return DXGI_FORMAT_R16G16B16A16_UNORM;
        case DXGI_FORMAT_R32G32_TYPELESS:       return DXGI_FORMAT_R32G32_FLOAT;
        case DXGI_FORMAT_R10G10B10A2_TYPELESS:  return DXGI_FORMAT_R10G10B10A2_UNORM;
        case DXGI_FORMAT_R8G8B8A8_TYPELESS:     return DXGI_FORMAT_R8G8B8A8_UNORM;
        case DXGI_FORMAT_R16G16_TYPELESS:       return DXGI_FORMAT_R16G16_UNORM;
        case DXGI_FORMAT_R32_TYPELESS:          return DXGI_FORMAT_R32_FLOAT;
        case DXGI_FORMAT_R8G8_TYPELESS:         return DXGI_FORMAT_R8G8_UNORM;
        case DXGI_FORMAT_R16_TYPELESS:          return DXGI_FORMAT_R16_UNORM;
        case DXGI_FORMAT_R8_TYPELESS:           return DXGI_FORMAT_R8_UNORM;
        case DXGI_FORMAT_BC1_TYPELESS:          return DXGI_FORMAT_BC1_UNORM;
        case DXGI_FORMAT_BC2_TYPELESS:          return DXGI_FORMAT_BC2_UNORM;
        case DXGI_FORMAT_BC3_TYPELESS:          return DXGI_FORMAT_BC3_UNORM;
        case DXGI_FORMAT_BC4_TYPELESS:          return DXGI_FORMAT_BC4_UNORM;
        case DXGI_FORMAT_BC5_TYPELESS:          return DXGI_FORMAT_BC5_UNORM;
        case DXGI_FORMAT_B8G8R8A8_TYPELESS:     return DXGI_FORMAT_B8G8R8A8_UNORM;
        case DXGI_FORMAT_B8G8R8X8_TYPELESS:     return DXGI_FORMAT_B8G8R8X8_UNORM;
        case DXGI_FORMAT_BC7_TYPELESS:          return DXGI_FORMAT_BC7_UNORM;
        default:                                return fmt;
        }
    }


    //--------------------------------------------------------------------------------------
    inline void TransitionResource(
        _In_ ID3D12GraphicsCommandList* commandList,
        _In_ ID3D12Resource* resource,
        _In_ D3D12_RESOURCE_STATES stateBefore,
        _In_ D3D12_RESOURCE_STATES stateAfter) noexcept
    {
        assert(commandList != nullptr);
        assert(resource != nullptr);

        if (stateBefore == stateAfter)
            return;

        D3D12_RESOURCE_BARRIER desc = {};
        desc.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
        desc.Transition.pResource = resource;
        desc.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
        desc.Transition.StateBefore = stateBefore;
        desc.Transition.StateAfter = stateAfter;

        commandList->ResourceBarrier(1, &desc);
    }


    //--------------------------------------------------------------------------------------
    HRESULT CaptureTexture(_In_ ID3D12Device* device,
        _In_ ID3D12CommandQueue* pCommandQ,
        _In_ ID3D12Resource* pSource,
        UINT64 srcPitch,
        const D3D12_RESOURCE_DESC& desc,
        ComPtr<ID3D12Resource>& pStaging,
        D3D12_RESOURCE_STATES beforeState,
        D3D12_RESOURCE_STATES afterState) noexcept
    {
        if (!pCommandQ || !pSource)
            return E_INVALIDARG;

        if (desc.Dimension != D3D12_RESOURCE_DIMENSION_TEXTURE2D)
            return HRESULT_E_NOT_SUPPORTED;

        if (srcPitch > UINT32_MAX)
            return HRESULT_E_ARITHMETIC_OVERFLOW;

        UINT numberOfPlanes = D3D12GetFormatPlaneCount(device, desc.Format);
        if (numberOfPlanes != 1)
            return HRESULT_E_NOT_SUPPORTED;

        D3D12_HEAP_PROPERTIES sourceHeapProperties;
        HRESULT hr = pSource->GetHeapProperties(&sourceHeapProperties, nullptr);
        if (SUCCEEDED(hr) && sourceHeapProperties.Type == D3D12_HEAP_TYPE_READBACK)
        {
            // Handle case where the source is already a staging texture we can use directly
            pStaging = pSource;
            return S_OK;
        }

        // Create a command allocator
        ComPtr<ID3D12CommandAllocator> commandAlloc;
        hr = device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_ID3D12CommandAllocator, reinterpret_cast<void**>(commandAlloc.GetAddressOf()));
        if (FAILED(hr))
            return hr;

        // Spin up a new command list
        ComPtr<ID3D12GraphicsCommandList> commandList;
        hr = device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, commandAlloc.Get(), nullptr, IID_ID3D12GraphicsCommandList, reinterpret_cast<void**>(commandList.GetAddressOf()));
        if (FAILED(hr))
            return hr;

        // Create a fence
        ComPtr<ID3D12Fence> fence;
        hr = device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_ID3D12Fence, reinterpret_cast<void**>(fence.GetAddressOf()));
        if (FAILED(hr))
            return hr;

        assert((srcPitch & 0xFF) == 0);

        CD3DX12_HEAP_PROPERTIES defaultHeapProperties(D3D12_HEAP_TYPE_DEFAULT);
        CD3DX12_HEAP_PROPERTIES readBackHeapProperties(D3D12_HEAP_TYPE_READBACK);

        // Readback resources must be buffers
        D3D12_RESOURCE_DESC bufferDesc = {};
        bufferDesc.DepthOrArraySize = 1;
        bufferDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        bufferDesc.Flags = D3D12_RESOURCE_FLAG_NONE;
        bufferDesc.Format = DXGI_FORMAT_UNKNOWN;
        bufferDesc.Height = 1;
        bufferDesc.Width = srcPitch * desc.Height;
        bufferDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        bufferDesc.MipLevels = 1;
        bufferDesc.SampleDesc.Count = 1;

        ComPtr<ID3D12Resource> copySource(pSource);
        if (desc.SampleDesc.Count > 1)
        {
            // MSAA content must be resolved before being copied to a staging texture
            auto descCopy = desc;
            descCopy.SampleDesc.Count = 1;
            descCopy.SampleDesc.Quality = 0;
            descCopy.Alignment = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;

            ComPtr<ID3D12Resource> pTemp;
            hr = device->CreateCommittedResource(
                &defaultHeapProperties,
                D3D12_HEAP_FLAG_NONE,
                &descCopy,
                D3D12_RESOURCE_STATE_COPY_DEST,
                nullptr,
                IID_ID3D12Resource,
                reinterpret_cast<void**>(pTemp.GetAddressOf()));
            if (FAILED(hr))
                return hr;

            assert(pTemp);

            DXGI_FORMAT fmt = EnsureNotTypeless(desc.Format);

            D3D12_FEATURE_DATA_FORMAT_SUPPORT formatInfo = { fmt, D3D12_FORMAT_SUPPORT1_NONE, D3D12_FORMAT_SUPPORT2_NONE };
            hr = device->CheckFeatureSupport(D3D12_FEATURE_FORMAT_SUPPORT, &formatInfo, sizeof(formatInfo));
            if (FAILED(hr))
                return hr;

            if (!(formatInfo.Support1 & D3D12_FORMAT_SUPPORT1_TEXTURE2D))
                return E_FAIL;

            for (UINT item = 0; item < desc.DepthOrArraySize; ++item)
            {
                for (UINT level = 0; level < desc.MipLevels; ++level)
                {
                    UINT index = D3D12CalcSubresource(level, item, 0, desc.MipLevels, desc.DepthOrArraySize);
                    commandList->ResolveSubresource(pTemp.Get(), index, pSource, index, fmt);
                }
            }

            copySource = pTemp;
        }

        // Create a staging texture
        hr = device->CreateCommittedResource(
            &readBackHeapProperties,
            D3D12_HEAP_FLAG_NONE,
            &bufferDesc,
            D3D12_RESOURCE_STATE_COPY_DEST,
            nullptr,
            IID_ID3D12Resource,
            reinterpret_cast<void**>(pStaging.ReleaseAndGetAddressOf()));
        if (FAILED(hr))
            return hr;

        assert(pStaging);

        // Transition the resource if necessary
        TransitionResource(commandList.Get(), pSource, beforeState, D3D12_RESOURCE_STATE_COPY_SOURCE);

        // Get the copy target location
        D3D12_PLACED_SUBRESOURCE_FOOTPRINT bufferFootprint = {};
        bufferFootprint.Footprint.Width = static_cast<UINT>(desc.Width);
        bufferFootprint.Footprint.Height = desc.Height;
        bufferFootprint.Footprint.Depth = 1;
        bufferFootprint.Footprint.RowPitch = static_cast<UINT>(srcPitch);
        bufferFootprint.Footprint.Format = desc.Format;

        CD3DX12_TEXTURE_COPY_LOCATION copyDest(pStaging.Get(), bufferFootprint);
        CD3DX12_TEXTURE_COPY_LOCATION copySrc(copySource.Get(), 0);

        // Copy the texture
        commandList->CopyTextureRegion(&copyDest, 0, 0, 0, &copySrc, nullptr);

        // Transition the resource to the next state
        TransitionResource(commandList.Get(), pSource, D3D12_RESOURCE_STATE_COPY_SOURCE, afterState);

        hr = commandList->Close();
        if (FAILED(hr))
            return hr;

        // Execute the command list
        pCommandQ->ExecuteCommandLists(1, CommandListCast(commandList.GetAddressOf()));

        // Signal the fence
        hr = pCommandQ->Signal(fence.Get(), 1);
        if (FAILED(hr))
            return hr;

        // Block until the copy is complete
        while (fence->GetCompletedValue() < 1)
        {
#ifdef WIN32
            SwitchToThread();
#else
            std::this_thread::yield();
#endif
        }

        return S_OK;
    }

#ifdef WIN32
    BOOL WINAPI InitializeWICFactory(PINIT_ONCE, PVOID, PVOID* ifactory) noexcept
    {
        return SUCCEEDED(CoCreateInstance(
            CLSID_WICImagingFactory2,
            nullptr,
            CLSCTX_INPROC_SERVER,
            __uuidof(IWICImagingFactory2),
            ifactory)) ? TRUE : FALSE;
    }

    IWICImagingFactory2* _GetWIC() noexcept
    {
        static INIT_ONCE s_initOnce = INIT_ONCE_STATIC_INIT;

        IWICImagingFactory2* factory = nullptr;
        if (!InitOnceExecuteOnce(&s_initOnce,
            InitializeWICFactory,
            nullptr,
            reinterpret_cast<LPVOID*>(&factory)))
        {
            return nullptr;
        }

        return factory;
    }
#endif
} // anonymous namespace


UINT GetStride(
    const UINT width, // image width in pixels
    const UINT bitCount) { // bits per pixel
    assert(0 == bitCount % 8);
    const UINT byteCount = bitCount / 8;
    const UINT stride = (width * byteCount + 3) & ~3;
    assert(0 == stride % sizeof(DWORD));
    return stride;
}

template <class T> void SafeRelease(T** ppT)
{
    if (*ppT)
    {
        (*ppT)->Release();
        *ppT = NULL;
    }
}

using Microsoft::WRL::ComPtr;

//--------------------------------------------------------------------------------------
// Forward declaration
//--------------------------------------------------------------------------------------
HRESULT PlayWave(_In_ IXAudio2* pXaudio2, _In_z_ LPCWSTR szFilename);
HRESULT FindMediaFileCch(_Out_writes_(cchDest) WCHAR* strDestPath, _In_ int cchDest, _In_z_ LPCWSTR strFilename);


//--------------------------------------------------------------------------------------
// Entry point to the program
//--------------------------------------------------------------------------------------
void Play()
{
    //
    // Initialize XAudio2
    //
    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(hr))
    {
        wprintf(L"Failed to init COM: %#X\n", hr);
        return;
    }

#ifdef USING_XAUDIO2_7_DIRECTX
    // Workaround for XAudio 2.7 known issue
#ifdef _DEBUG
    HMODULE mXAudioDLL = LoadLibraryExW(L"XAudioD2_7.DLL", nullptr, 0x00000800 /* LOAD_LIBRARY_SEARCH_SYSTEM32 */);
#else
    HMODULE mXAudioDLL = LoadLibraryExW(L"XAudio2_7.DLL", nullptr, 0x00000800 /* LOAD_LIBRARY_SEARCH_SYSTEM32 */);
#endif
    if (!mXAudioDLL)
    {
        wprintf(L"Failed to find XAudio 2.7 DLL");
        CoUninitialize();
        return;
    }
#endif // USING_XAUDIO2_7_DIRECTX

    UINT32 flags = 0;
#if defined(USING_XAUDIO2_7_DIRECTX) && defined(_DEBUG)
    flags |= XAUDIO2_DEBUG_ENGINE;
#endif
    ComPtr<IXAudio2> pXAudio2;
    hr = XAudio2Create(pXAudio2.GetAddressOf(), flags);
    if (FAILED(hr))
    {
        wprintf(L"Failed to init XAudio2 engine: %#X\n", hr);
        CoUninitialize();
        return;
    }

#if !defined(USING_XAUDIO2_7_DIRECTX) && defined(_DEBUG)
    // To see the trace output, you need to view ETW logs for this application:
    //    Go to Control Panel, Administrative Tools, Event Viewer.
    //    View->Show Analytic and Debug Logs.
    //    Applications and Services Logs / Microsoft / Windows / XAudio2. 
    //    Right click on Microsoft Windows XAudio2 debug logging, Properties, then Enable Logging, and hit OK 
    XAUDIO2_DEBUG_CONFIGURATION debug = { 0 };
    debug.TraceMask = XAUDIO2_LOG_ERRORS | XAUDIO2_LOG_WARNINGS;
    debug.BreakMask = XAUDIO2_LOG_ERRORS;
    pXAudio2->SetDebugConfiguration(&debug, 0);
#endif

    //
    // Create a mastering voice
    //
    IXAudio2MasteringVoice* pMasteringVoice = nullptr;

    if (FAILED(hr = pXAudio2->CreateMasteringVoice(&pMasteringVoice)))
    {
        wprintf(L"Failed creating mastering voice: %#X\n", hr);
        pXAudio2.Reset();
        CoUninitialize();
        return;
    }

    //
    // Play a PCM wave file
    //
    wprintf(L"Playing mono WAV PCM file...");
    if (FAILED(hr = PlayWave(pXAudio2.Get(), L"BGM.wav")))
    {
        wprintf(L"Failed creating source voice: %#X\n", hr);
        pXAudio2.Reset();
        CoUninitialize();
        return;
    }

    //
    // Play an ADPCM wave file
    //
    wprintf(L"\nPlaying mono WAV ADPCM file (loops twice)...");
    if (FAILED(hr = PlayWave(pXAudio2.Get(), L"Media\\Wavs\\MusicMono_adpcm.wav")))
    {
        wprintf(L"Failed creating source voice: %#X\n", hr);
        pXAudio2.Reset();
        CoUninitialize();
        return;
    }

    //
    // Play a 5.1 PCM wave extensible file
    //
    wprintf(L"\nPlaying 5.1 WAV PCM file...");
    if (FAILED(hr = PlayWave(pXAudio2.Get(), L"Media\\Wavs\\MusicSurround.wav")))
    {
        wprintf(L"Failed creating source voice: %#X\n", hr);
        pXAudio2.Reset();
        CoUninitialize();
        return;
    }

#if defined(USING_XAUDIO2_7_DIRECTX) || defined(USING_XAUDIO2_9)

    //
    // Play a mono xWMA wave file
    //
    wprintf(L"\nPlaying mono xWMA file...");
    if (FAILED(hr = PlayWave(pXAudio2.Get(), L"Media\\Wavs\\MusicMono_xwma.wav")))
    {
        wprintf(L"Failed creating source voice: %#X\n", hr);
        pXAudio2.Reset();
        CoUninitialize();
        return;
    }

    //
    // Play a 5.1 xWMA wave file
    //
    wprintf(L"\nPlaying 5.1 xWMA file...");
    if (FAILED(hr = PlayWave(pXAudio2.Get(), L"Media\\Wavs\\MusicSurround_xwma.wav")))
    {
        wprintf(L"Failed creating source voice: %#X\n", hr);
        pXAudio2.Reset();
        CoUninitialize();
        return;
    }

#endif

    //
    // Cleanup XAudio2
    //
    wprintf(L"\nFinished playing\n");

    // All XAudio2 interfaces are released when the engine is destroyed, but being tidy
    pMasteringVoice->DestroyVoice();

    pXAudio2.Reset();

#ifdef USING_XAUDIO2_7_DIRECTX
    if (mXAudioDLL)
        FreeLibrary(mXAudioDLL);
#endif

    CoUninitialize();
}


//--------------------------------------------------------------------------------------
// Name: PlayWave
// Desc: Plays a wave and blocks until the wave finishes playing
//--------------------------------------------------------------------------------------
_Use_decl_annotations_
HRESULT PlayWave(IXAudio2* pXaudio2, LPCWSTR szFilename)
{
    //
    // Locate the wave file
    //
    WCHAR strFilePath[MAX_PATH];
    HRESULT hr = FindMediaFileCch(strFilePath, MAX_PATH, szFilename);
    if (FAILED(hr))
    {
        wprintf(L"Failed to find media file: %s\n", szFilename);
        return hr;
    }

    //
    // Read in the wave file
    //
    std::unique_ptr<uint8_t[]> waveFile;
    DirectX::WAVData waveData;
    if (FAILED(hr = DirectX::LoadWAVAudioFromFileEx(strFilePath, waveFile, waveData)))
    {
        wprintf(L"Failed reading WAV file: %#X (%s)\n", hr, strFilePath);
        return hr;
    }

    //
    // Play the wave using a XAudio2SourceVoice
    //

    // Create the source voice
    IXAudio2SourceVoice* pSourceVoice;
    if (FAILED(hr = pXaudio2->CreateSourceVoice(&pSourceVoice, waveData.wfx)))
    {
        wprintf(L"Error %#X creating source voice\n", hr);
        return hr;
    }

    // Submit the wave sample data using an XAUDIO2_BUFFER structure
    XAUDIO2_BUFFER buffer = { 0 };
    buffer.pAudioData = waveData.startAudio;
    buffer.Flags = XAUDIO2_END_OF_STREAM;  // tell the source voice not to expect any data after this buffer
    buffer.AudioBytes = waveData.audioBytes;

    if (waveData.loopLength > 0)
    {
        buffer.LoopBegin = waveData.loopStart;
        buffer.LoopLength = waveData.loopLength;
        buffer.LoopCount = 1; // We'll just assume we play the loop twice
    }

#if defined(USING_XAUDIO2_7_DIRECTX) || defined(USING_XAUDIO2_9)
    if (waveData.seek)
    {
        XAUDIO2_BUFFER_WMA xwmaBuffer = { 0 };
        xwmaBuffer.pDecodedPacketCumulativeBytes = waveData.seek;
        xwmaBuffer.PacketCount = waveData.seekCount;
        if (FAILED(hr = pSourceVoice->SubmitSourceBuffer(&buffer, &xwmaBuffer)))
        {
            wprintf(L"Error %#X submitting source buffer (xWMA)\n", hr);
            pSourceVoice->DestroyVoice();
            return hr;
        }
    }
#else
    if (waveData.seek)
    {
        wprintf(L"This platform does not support xWMA or XMA2\n");
        pSourceVoice->DestroyVoice();
        return hr;
    }
#endif
    else if (FAILED(hr = pSourceVoice->SubmitSourceBuffer(&buffer)))
    {
        wprintf(L"Error %#X submitting source buffer\n", hr);
        pSourceVoice->DestroyVoice();
        return hr;
    }

    hr = pSourceVoice->Start(0);

    // Let the sound play
    BOOL isRunning = TRUE;
    while (SUCCEEDED(hr) && isRunning)
    {
        XAUDIO2_VOICE_STATE state;
        pSourceVoice->GetState(&state);
        isRunning = (state.BuffersQueued > 0) != 0;

        // Wait till the escape key is pressed
        if (GetAsyncKeyState(VK_ESCAPE))
            break;

        Sleep(10);
    }

    // Wait till the escape key is released
    while (GetAsyncKeyState(VK_ESCAPE))
        Sleep(10);

    pSourceVoice->DestroyVoice();

    return hr;
}


//--------------------------------------------------------------------------------------
// Helper function to try to find the location of a media file
//--------------------------------------------------------------------------------------
_Use_decl_annotations_
HRESULT FindMediaFileCch(WCHAR* strDestPath, int cchDest, LPCWSTR strFilename)
{
    bool bFound = false;

    if (!strFilename || strFilename[0] == 0 || !strDestPath || cchDest < 10)
        return E_INVALIDARG;

    // Get the exe name, and exe path
    WCHAR strExePath[MAX_PATH] = { 0 };
    WCHAR strExeName[MAX_PATH] = { 0 };
    WCHAR* strLastSlash = nullptr;
    GetModuleFileName(nullptr, strExePath, MAX_PATH);
    strExePath[MAX_PATH - 1] = 0;
    strLastSlash = wcsrchr(strExePath, TEXT('\\'));
    if (strLastSlash)
    {
        wcscpy_s(strExeName, MAX_PATH, &strLastSlash[1]);

        // Chop the exe name from the exe path
        *strLastSlash = 0;

        // Chop the .exe from the exe name
        strLastSlash = wcsrchr(strExeName, TEXT('.'));
        if (strLastSlash)
            *strLastSlash = 0;
    }

    wcscpy_s(strDestPath, cchDest, strFilename);
    if (GetFileAttributes(strDestPath) != 0xFFFFFFFF)
        return S_OK;

    // Search all parent directories starting at .\ and using strFilename as the leaf name
    WCHAR strLeafName[MAX_PATH] = { 0 };
    wcscpy_s(strLeafName, MAX_PATH, strFilename);

    WCHAR strFullPath[MAX_PATH] = { 0 };
    WCHAR strFullFileName[MAX_PATH] = { 0 };
    WCHAR strSearch[MAX_PATH] = { 0 };
    WCHAR* strFilePart = nullptr;

    GetFullPathName(L".", MAX_PATH, strFullPath, &strFilePart);
    if (!strFilePart)
        return E_FAIL;

    while (strFilePart && *strFilePart != '\0')
    {
        swprintf_s(strFullFileName, MAX_PATH, L"%s\\%s", strFullPath, strLeafName);
        if (GetFileAttributes(strFullFileName) != 0xFFFFFFFF)
        {
            wcscpy_s(strDestPath, cchDest, strFullFileName);
            bFound = true;
            break;
        }

        swprintf_s(strFullFileName, MAX_PATH, L"%s\\%s\\%s", strFullPath, strExeName, strLeafName);
        if (GetFileAttributes(strFullFileName) != 0xFFFFFFFF)
        {
            wcscpy_s(strDestPath, cchDest, strFullFileName);
            bFound = true;
            break;
        }

        swprintf_s(strSearch, MAX_PATH, L"%s\\..", strFullPath);
        GetFullPathName(strSearch, MAX_PATH, strFullPath, &strFilePart);
    }
    if (bFound)
        return S_OK;

    // On failure, return the file as the path but also return an error code
    wcscpy_s(strDestPath, cchDest, strFilename);

    return HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
}

// Format constants
const UINT32 VIDEO_WIDTH = 640;
const UINT32 VIDEO_HEIGHT = 480;
const UINT32 VIDEO_FPS = 30;
const UINT64 VIDEO_FRAME_DURATION = 10 * 1000 * 1000 / VIDEO_FPS;
const UINT32 VIDEO_BIT_RATE = 800000;
const GUID   VIDEO_ENCODING_FORMAT = MFVideoFormat_WMV3;
const GUID   VIDEO_INPUT_FORMAT = MFVideoFormat_RGB32;
const UINT32 VIDEO_PELS = VIDEO_WIDTH * VIDEO_HEIGHT;
const UINT32 VIDEO_FRAME_COUNT = 20 * VIDEO_FPS;

// Buffer to hold the video frame data.
// 이게 사진 한 장이고
DWORD videoFrameBuffer[60][VIDEO_PELS];

const UINT32 SAMPLES_PER_SECOND = 44100;
const UINT32 AVG_BYTES_PER_SECOND = 6003;
const UINT32 NUM_CHANNELS = 1;
const UINT32 BITS_PER_SAMPLE = 16;
const UINT32 BLOCK_ALIGNMENT = 2230;
const UINT32 ONE_SECOND = 10;
const UINT32 BUFFER_LENGTH = BITS_PER_SAMPLE / 8 * NUM_CHANNELS * (SAMPLES_PER_SECOND / ONE_SECOND);
const LONGLONG SAMPLE_DURATION = 10000000 / (LONGLONG)ONE_SECOND;


HRESULT InitializeSinkWriter(IMFSinkWriter** ppWriter, DWORD* pStreamIndex)
{
    *ppWriter = NULL;
    *pStreamIndex = NULL;

    IMFSinkWriter* pSinkWriter = NULL;
    IMFMediaType* pVideoTypeOut = NULL;
    IMFMediaType* pVideoTypeIn = NULL;

    IMFMediaType* pAudioTypeOut = nullptr;
    IMFMediaType* pAudioTypeIn = nullptr;

    DWORD           streamIndex;

    static int i;
    i++;
    HRESULT hr = MFCreateSinkWriterFromURL(std::format(L"output{}.wmv", i).c_str(), NULL, NULL, &pSinkWriter);

    // Set the output media type.
    if (SUCCEEDED(hr))
    {
        hr = MFCreateMediaType(&pVideoTypeOut);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeOut->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeOut->SetGUID(MF_MT_SUBTYPE, VIDEO_ENCODING_FORMAT);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeOut->SetUINT32(MF_MT_AVG_BITRATE, VIDEO_BIT_RATE);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeOut->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFSetAttributeSize(pVideoTypeOut, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFSetAttributeRatio(pVideoTypeOut, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFSetAttributeRatio(pVideoTypeOut, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
    }
    if (SUCCEEDED(hr))
    {
        hr = pSinkWriter->AddStream(pVideoTypeOut, &streamIndex);
    }

    ComPtr<IMFCollection> availableTypes = nullptr;
    hr = MFTranscodeGetAudioOutputAvailableTypes(MFAudioFormat_WMAudioV9, MFT_ENUM_FLAG_ALL, NULL, availableTypes.GetAddressOf());

    DWORD count = 0;
    hr = availableTypes->GetElementCount(&count);  // Get the number of elements in the list.

    ComPtr<IUnknown>     pUnkAudioType = nullptr;
    ComPtr<IMFMediaType> audioOutputType = nullptr;
    for (DWORD i = 0; i < count; ++i)
    {
        hr = availableTypes->GetElement(i, pUnkAudioType.GetAddressOf());
        hr = pUnkAudioType.Get()->QueryInterface((void**)&pAudioTypeOut);

        // compare channels, sampleRate, and bitsPerSample to target numbers
        {
            // audioTypeOut is set!
            break;
        }

        audioOutputType.Reset();
    }
    availableTypes.Reset();

    hr = pSinkWriter->AddStream(pAudioTypeOut, &streamIndex);


    // Set the input media type.
    if (SUCCEEDED(hr))
    {
        hr = MFCreateMediaType(&pVideoTypeIn);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeIn->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeIn->SetGUID(MF_MT_SUBTYPE, VIDEO_INPUT_FORMAT);
    }
    if (SUCCEEDED(hr))
    {
        hr = pVideoTypeIn->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFSetAttributeSize(pVideoTypeIn, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFSetAttributeRatio(pVideoTypeIn, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFSetAttributeRatio(pVideoTypeIn, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
    }
    if (SUCCEEDED(hr))
    {
        hr = pSinkWriter->SetInputMediaType(streamIndex, pVideoTypeIn, NULL);
    }

    // NOTE: audioReader is an IMFMediaSource used to read the audio file
    //hr = audioReader->GetCurrentMediaType((DWORD)MF_SOURCE_READER_FIRST_AUDIO_STREAM, pAudioTypeIn);
    hr = pSinkWriter->SetInputMediaType(streamIndex, pAudioTypeIn, nullptr);
    

    // Tell the sink writer to start accepting data.
    if (SUCCEEDED(hr))
    {
        hr = pSinkWriter->BeginWriting();
    }

    // Return the pointer to the caller.
    if (SUCCEEDED(hr))
    {
        *ppWriter = pSinkWriter;
        (*ppWriter)->AddRef();
        *pStreamIndex = streamIndex;
    }

    SafeRelease(&pSinkWriter);
    SafeRelease(&pVideoTypeOut);
    SafeRelease(&pVideoTypeIn);

    SafeRelease(&pAudioTypeOut);
    SafeRelease(&pAudioTypeIn);
    return hr;
}

HRESULT WriteFrame(
    IMFSinkWriter* pWriter,
    DWORD streamIndex,
    const LONGLONG& rtStart,        // Time stamp.
    int index
)
{
    IMFSample* pSample = NULL;
    IMFMediaBuffer* pBuffer = NULL;

    const LONG cbWidth = 4 * VIDEO_WIDTH;
    const DWORD cbBuffer = cbWidth * VIDEO_HEIGHT;

    BYTE* pData = NULL;

    // Create a new memory buffer.
    HRESULT hr = MFCreateMemoryBuffer(cbBuffer, &pBuffer);

    // Lock the buffer and copy the video frame to the buffer.
    if (SUCCEEDED(hr))
    {
        hr = pBuffer->Lock(&pData, NULL, NULL);
    }
    if (SUCCEEDED(hr))
    {
        hr = MFCopyImage(
            pData,                      // Destination buffer.
            cbWidth,                    // Destination stride.
            (BYTE*)videoFrameBuffer[index],    // First row in source image.
            cbWidth,                    // Source stride.
            cbWidth,                    // Image width in bytes.
            VIDEO_HEIGHT                // Image height in pixels.
        );
    }
    if (pBuffer)
    {
        pBuffer->Unlock();
    }

    // Set the data length of the buffer.
    if (SUCCEEDED(hr))
    {
        hr = pBuffer->SetCurrentLength(cbBuffer);
    }

    // Create a media sample and add the buffer to the sample.
    if (SUCCEEDED(hr))
    {
        hr = MFCreateSample(&pSample);
    }
    if (SUCCEEDED(hr))
    {
        hr = pSample->AddBuffer(pBuffer);
    }

    // Set the time stamp and the duration.
    if (SUCCEEDED(hr))
    {
        hr = pSample->SetSampleTime(rtStart);
    }
    if (SUCCEEDED(hr))
    {
        hr = pSample->SetSampleDuration(VIDEO_FRAME_DURATION);
    }

    // Send the sample to the Sink Writer.
    if (SUCCEEDED(hr))
    {
        hr = pWriter->WriteSample(streamIndex, pSample);
    }

    SafeRelease(&pSample);
    SafeRelease(&pBuffer);
    return hr;
}

void DecodingWAV()
{
    // Set all pixels to green
    //for (DWORD i = 0; i < VIDEO_PELS; ++i)
    //{
    //    videoFrameBuffer[i] = 0x0000FF00;
    //}

    HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    if (SUCCEEDED(hr))
    {
        hr = MFStartup(MF_VERSION);
        if (SUCCEEDED(hr))
        {
            IMFSinkWriter* pSinkWriter = NULL;
            DWORD stream;

            hr = InitializeSinkWriter(&pSinkWriter, &stream);
            if (SUCCEEDED(hr))
            {
                // Send frames to the sink writer.
                LONGLONG rtStart = 0;


                for (DWORD i = 0; i < 60; ++i)
                {
                    hr = WriteFrame(pSinkWriter, stream, rtStart,i);
                    if (FAILED(hr))
                    {
                        break;
                    }
                    rtStart += VIDEO_FRAME_DURATION;
                }
            }
            if (SUCCEEDED(hr))
            {
                hr = pSinkWriter->Finalize();
            }
            SafeRelease(&pSinkWriter);
            MFShutdown();
        }
        CoUninitialize();
    }
}

//--------------------------------------------------------------------------------------
_Use_decl_annotations_
HRESULT DirectX::SaveDDSTextureToFile(
    ID3D12CommandQueue* pCommandQ,
    ID3D12Resource* pSource,
    const wchar_t* fileName,
    D3D12_RESOURCE_STATES beforeState,
    D3D12_RESOURCE_STATES afterState) noexcept
{
    if (!fileName)
        return E_INVALIDARG;

    ComPtr<ID3D12Device> device;
    pCommandQ->GetDevice(IID_ID3D12Device, reinterpret_cast<void**>(device.GetAddressOf()));

    // Get the size of the image
    const auto desc = pSource->GetDesc();

    if (desc.Width > UINT32_MAX)
        return E_INVALIDARG;

    UINT64 totalResourceSize = 0;
    UINT64 fpRowPitch = 0;
    UINT fpRowCount = 0;
    // Get the rowcount, pitch and size of the top mip
    device->GetCopyableFootprints(
        &desc,
        0,
        1,
        0,
        nullptr,
        &fpRowCount,
        &fpRowPitch,
        &totalResourceSize);

    // Round up the srcPitch to multiples of 256
    UINT64 dstRowPitch = (fpRowPitch + 255) & ~0xFFu;

    if (dstRowPitch > UINT32_MAX)
        return HRESULT_E_ARITHMETIC_OVERFLOW;

    ComPtr<ID3D12Resource> pStaging;
    HRESULT hr = CaptureTexture(device.Get(), pCommandQ, pSource, dstRowPitch, desc, pStaging, beforeState, afterState);
    if (FAILED(hr))
        return hr;

    // Create file
#ifdef WIN32
    ScopedHandle hFile(safe_handle(CreateFile2(fileName, GENERIC_WRITE, 0, CREATE_ALWAYS, nullptr)));
    if (!hFile)
        return HRESULT_FROM_WIN32(GetLastError());

    auto_delete_file delonfail(hFile.get());
#else
    std::ofstream outFile(std::filesystem::path(fileName), std::ios::out | std::ios::binary | std::ios::trunc);
    if (!outFile)
        return E_FAIL;
#endif

    // Setup header
    const size_t MAX_HEADER_SIZE = sizeof(uint32_t) + sizeof(DDS_HEADER) + sizeof(DDS_HEADER_DXT10);
    uint8_t fileHeader[MAX_HEADER_SIZE] = {};

    *reinterpret_cast<uint32_t*>(&fileHeader[0]) = DDS_MAGIC;

    auto header = reinterpret_cast<DDS_HEADER*>(&fileHeader[0] + sizeof(uint32_t));
    size_t headerSize = sizeof(uint32_t) + sizeof(DDS_HEADER);
    header->size = sizeof(DDS_HEADER);
    header->flags = DDS_HEADER_FLAGS_TEXTURE | DDS_HEADER_FLAGS_MIPMAP;
    header->height = desc.Height;
    header->width = static_cast<uint32_t>(desc.Width);
    header->mipMapCount = 1;
    header->caps = DDS_SURFACE_FLAGS_TEXTURE;

    // Try to use a legacy .DDS pixel format for better tools support, otherwise fallback to 'DX10' header extension
    DDS_HEADER_DXT10* extHeader = nullptr;
    switch (desc.Format)
    {
    case DXGI_FORMAT_R8G8B8A8_UNORM:        memcpy(&header->ddspf, &DDSPF_A8B8G8R8, sizeof(DDS_PIXELFORMAT));    break;
    case DXGI_FORMAT_R16G16_UNORM:          memcpy(&header->ddspf, &DDSPF_G16R16, sizeof(DDS_PIXELFORMAT));      break;
    case DXGI_FORMAT_R8G8_UNORM:            memcpy(&header->ddspf, &DDSPF_A8L8, sizeof(DDS_PIXELFORMAT));        break;
    case DXGI_FORMAT_R16_UNORM:             memcpy(&header->ddspf, &DDSPF_L16, sizeof(DDS_PIXELFORMAT));         break;
    case DXGI_FORMAT_R8_UNORM:              memcpy(&header->ddspf, &DDSPF_L8, sizeof(DDS_PIXELFORMAT));          break;
    case DXGI_FORMAT_A8_UNORM:              memcpy(&header->ddspf, &DDSPF_A8, sizeof(DDS_PIXELFORMAT));          break;
    case DXGI_FORMAT_R8G8_B8G8_UNORM:       memcpy(&header->ddspf, &DDSPF_R8G8_B8G8, sizeof(DDS_PIXELFORMAT));   break;
    case DXGI_FORMAT_G8R8_G8B8_UNORM:       memcpy(&header->ddspf, &DDSPF_G8R8_G8B8, sizeof(DDS_PIXELFORMAT));   break;
    case DXGI_FORMAT_BC1_UNORM:             memcpy(&header->ddspf, &DDSPF_DXT1, sizeof(DDS_PIXELFORMAT));        break;
    case DXGI_FORMAT_BC2_UNORM:             memcpy(&header->ddspf, &DDSPF_DXT3, sizeof(DDS_PIXELFORMAT));        break;
    case DXGI_FORMAT_BC3_UNORM:             memcpy(&header->ddspf, &DDSPF_DXT5, sizeof(DDS_PIXELFORMAT));        break;
    case DXGI_FORMAT_BC4_UNORM:             memcpy(&header->ddspf, &DDSPF_BC4_UNORM, sizeof(DDS_PIXELFORMAT));   break;
    case DXGI_FORMAT_BC4_SNORM:             memcpy(&header->ddspf, &DDSPF_BC4_SNORM, sizeof(DDS_PIXELFORMAT));   break;
    case DXGI_FORMAT_BC5_UNORM:             memcpy(&header->ddspf, &DDSPF_BC5_UNORM, sizeof(DDS_PIXELFORMAT));   break;
    case DXGI_FORMAT_BC5_SNORM:             memcpy(&header->ddspf, &DDSPF_BC5_SNORM, sizeof(DDS_PIXELFORMAT));   break;
    case DXGI_FORMAT_B5G6R5_UNORM:          memcpy(&header->ddspf, &DDSPF_R5G6B5, sizeof(DDS_PIXELFORMAT));      break;
    case DXGI_FORMAT_B5G5R5A1_UNORM:        memcpy(&header->ddspf, &DDSPF_A1R5G5B5, sizeof(DDS_PIXELFORMAT));    break;
    case DXGI_FORMAT_R8G8_SNORM:            memcpy(&header->ddspf, &DDSPF_V8U8, sizeof(DDS_PIXELFORMAT));        break;
    case DXGI_FORMAT_R8G8B8A8_SNORM:        memcpy(&header->ddspf, &DDSPF_Q8W8V8U8, sizeof(DDS_PIXELFORMAT));    break;
    case DXGI_FORMAT_R16G16_SNORM:          memcpy(&header->ddspf, &DDSPF_V16U16, sizeof(DDS_PIXELFORMAT));      break;
    case DXGI_FORMAT_B8G8R8A8_UNORM:        memcpy(&header->ddspf, &DDSPF_A8R8G8B8, sizeof(DDS_PIXELFORMAT));    break;
    case DXGI_FORMAT_B8G8R8X8_UNORM:        memcpy(&header->ddspf, &DDSPF_X8R8G8B8, sizeof(DDS_PIXELFORMAT));    break;
    case DXGI_FORMAT_YUY2:                  memcpy(&header->ddspf, &DDSPF_YUY2, sizeof(DDS_PIXELFORMAT));        break;
    case DXGI_FORMAT_B4G4R4A4_UNORM:        memcpy(&header->ddspf, &DDSPF_A4R4G4B4, sizeof(DDS_PIXELFORMAT));    break;

        // Legacy D3DX formats using D3DFMT enum value as FourCC
    case DXGI_FORMAT_R32G32B32A32_FLOAT:    header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 116; break; // D3DFMT_A32B32G32R32F
    case DXGI_FORMAT_R16G16B16A16_FLOAT:    header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 113; break; // D3DFMT_A16B16G16R16F
    case DXGI_FORMAT_R16G16B16A16_UNORM:    header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 36;  break; // D3DFMT_A16B16G16R16
    case DXGI_FORMAT_R16G16B16A16_SNORM:    header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 110; break; // D3DFMT_Q16W16V16U16
    case DXGI_FORMAT_R32G32_FLOAT:          header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 115; break; // D3DFMT_G32R32F
    case DXGI_FORMAT_R16G16_FLOAT:          header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 112; break; // D3DFMT_G16R16F
    case DXGI_FORMAT_R32_FLOAT:             header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 114; break; // D3DFMT_R32F
    case DXGI_FORMAT_R16_FLOAT:             header->ddspf.size = sizeof(DDS_PIXELFORMAT); header->ddspf.flags = DDS_FOURCC; header->ddspf.fourCC = 111; break; // D3DFMT_R16F

    case DXGI_FORMAT_AI44:
    case DXGI_FORMAT_IA44:
    case DXGI_FORMAT_P8:
    case DXGI_FORMAT_A8P8:
        return HRESULT_E_NOT_SUPPORTED;

    default:
        memcpy(&header->ddspf, &DDSPF_DX10, sizeof(DDS_PIXELFORMAT));

        headerSize += sizeof(DDS_HEADER_DXT10);
        extHeader = reinterpret_cast<DDS_HEADER_DXT10*>(fileHeader + sizeof(uint32_t) + sizeof(DDS_HEADER));
        extHeader->dxgiFormat = desc.Format;
        extHeader->resourceDimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
        extHeader->arraySize = 1;
        break;
    }

    size_t rowPitch, slicePitch, rowCount;
    hr = GetSurfaceInfo(static_cast<size_t>(desc.Width), desc.Height, desc.Format, &slicePitch, &rowPitch, &rowCount);
    if (FAILED(hr))
        return hr;

    if (rowPitch > UINT32_MAX || slicePitch > UINT32_MAX)
        return HRESULT_E_ARITHMETIC_OVERFLOW;

    if (IsCompressed(desc.Format))
    {
        header->flags |= DDS_HEADER_FLAGS_LINEARSIZE;
        header->pitchOrLinearSize = static_cast<uint32_t>(slicePitch);
    }
    else
    {
        header->flags |= DDS_HEADER_FLAGS_PITCH;
        header->pitchOrLinearSize = static_cast<uint32_t>(rowPitch);
    }

    // Setup pixels
    std::unique_ptr<uint8_t[]> pixels(new (std::nothrow) uint8_t[slicePitch]);
    if (!pixels)
        return E_OUTOFMEMORY;

    assert(fpRowCount == rowCount);
    assert(fpRowPitch == rowPitch);

    UINT64 imageSize = dstRowPitch * UINT64(rowCount);
    if (imageSize > UINT32_MAX)
        return HRESULT_E_ARITHMETIC_OVERFLOW;

    void* pMappedMemory = nullptr;
    D3D12_RANGE readRange = { 0, static_cast<SIZE_T>(imageSize) };
    D3D12_RANGE writeRange = { 0, 0 };
    hr = pStaging->Map(0, &readRange, &pMappedMemory);
    if (FAILED(hr))
        return hr;

    auto sptr = static_cast<const uint8_t*>(pMappedMemory);
    if (!sptr)
    {
        pStaging->Unmap(0, &writeRange);
        return E_POINTER;
    }

    uint8_t* dptr = pixels.get();

    size_t msize = std::min<size_t>(rowPitch, size_t(dstRowPitch));
    for (size_t h = 0; h < rowCount; ++h)
    {
        memcpy(dptr, sptr, msize);
        sptr += dstRowPitch;
        dptr += rowPitch;
    }

    pStaging->Unmap(0, &writeRange);

    // Write header & pixels
#ifdef WIN32
    DWORD bytesWritten;
    if (!WriteFile(hFile.get(), fileHeader, static_cast<DWORD>(headerSize), &bytesWritten, nullptr))
        return HRESULT_FROM_WIN32(GetLastError());

    if (bytesWritten != headerSize)
        return E_FAIL;

    if (!WriteFile(hFile.get(), pixels.get(), static_cast<DWORD>(slicePitch), &bytesWritten, nullptr))
        return HRESULT_FROM_WIN32(GetLastError());

    if (bytesWritten != slicePitch)
        return E_FAIL;

    delonfail.clear();
#else
    outFile.write(reinterpret_cast<char*>(fileHeader), static_cast<std::streamsize>(headerSize));
    if (!outFile)
        return E_FAIL;

    outFile.write(reinterpret_cast<char*>(pixels.get()), static_cast<std::streamsize>(slicePitch));
    if (!outFile)
        return E_FAIL;

    outFile.close();
#endif

    return S_OK;
}


//--------------------------------------------------------------------------------------
#ifdef WIN32
_Use_decl_annotations_
HRESULT DirectX::SaveWICTextureToFile(
    ID3D12CommandQueue* pCommandQ,
    ID3D12Resource* pSource,
    REFGUID guidContainerFormat,
    const wchar_t* fileName,
    D3D12_RESOURCE_STATES beforeState,
    D3D12_RESOURCE_STATES afterState,
    const GUID* targetFormat,
    std::function<void(IPropertyBag2*)> setCustomProps,
    bool forceSRGB)
{
    if (!fileName)
        return E_INVALIDARG;

    ComPtr<ID3D12Device> device;
    pCommandQ->GetDevice(IID_ID3D12Device, reinterpret_cast<void**>(device.GetAddressOf()));

    // Get the size of the image
    const auto desc = pSource->GetDesc();

    if (desc.Width > UINT32_MAX)
        return E_INVALIDARG;

    UINT64 totalResourceSize = 0;
    UINT64 fpRowPitch = 0;
    UINT fpRowCount = 0;
    // Get the rowcount, pitch and size of the top mip
    device->GetCopyableFootprints(
        &desc,
        0,
        1,
        0,
        nullptr,
        &fpRowCount,
        &fpRowPitch,
        &totalResourceSize);
    
    // Round up the srcPitch to multiples of 256
    UINT64 dstRowPitch = (fpRowPitch + 255) & ~0xFFu;

    if (dstRowPitch > UINT32_MAX)
        return HRESULT_E_ARITHMETIC_OVERFLOW;

    ComPtr<ID3D12Resource> pStaging;
    HRESULT hr = CaptureTexture(device.Get(), pCommandQ, pSource, dstRowPitch, desc, pStaging, beforeState, afterState);
    if (FAILED(hr))
        return hr;

    // Determine source format's WIC equivalent
    WICPixelFormatGUID pfGuid = {};
    bool sRGB = forceSRGB;
    switch (desc.Format)
    {
    case DXGI_FORMAT_R32G32B32A32_FLOAT:            pfGuid = GUID_WICPixelFormat128bppRGBAFloat; break;
    case DXGI_FORMAT_R16G16B16A16_FLOAT:            pfGuid = GUID_WICPixelFormat64bppRGBAHalf; break;
    case DXGI_FORMAT_R16G16B16A16_UNORM:            pfGuid = GUID_WICPixelFormat64bppRGBA; break;
    case DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:    pfGuid = GUID_WICPixelFormat32bppRGBA1010102XR; break;
    case DXGI_FORMAT_R10G10B10A2_UNORM:             pfGuid = GUID_WICPixelFormat32bppRGBA1010102; break;
    case DXGI_FORMAT_B5G5R5A1_UNORM:                pfGuid = GUID_WICPixelFormat16bppBGRA5551; break;
    case DXGI_FORMAT_B5G6R5_UNORM:                  pfGuid = GUID_WICPixelFormat16bppBGR565; break;
    case DXGI_FORMAT_R32_FLOAT:                     pfGuid = GUID_WICPixelFormat32bppGrayFloat; break;
    case DXGI_FORMAT_R16_FLOAT:                     pfGuid = GUID_WICPixelFormat16bppGrayHalf; break;
    case DXGI_FORMAT_R16_UNORM:                     pfGuid = GUID_WICPixelFormat16bppGray; break;
    case DXGI_FORMAT_R8_UNORM:                      pfGuid = GUID_WICPixelFormat8bppGray; break;
    case DXGI_FORMAT_A8_UNORM:                      pfGuid = GUID_WICPixelFormat8bppAlpha; break;

    case DXGI_FORMAT_R8G8B8A8_UNORM:
        pfGuid = GUID_WICPixelFormat32bppRGBA;
        break;

    case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
        pfGuid = GUID_WICPixelFormat32bppRGBA;
        sRGB = true;
        break;

    case DXGI_FORMAT_B8G8R8A8_UNORM:
        pfGuid = GUID_WICPixelFormat32bppBGRA;
        break;

    case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
        pfGuid = GUID_WICPixelFormat32bppBGRA;
        sRGB = true;
        break;

    case DXGI_FORMAT_B8G8R8X8_UNORM:
        pfGuid = GUID_WICPixelFormat32bppBGR;
        break;

    case DXGI_FORMAT_B8G8R8X8_UNORM_SRGB:
        pfGuid = GUID_WICPixelFormat32bppBGR;
        sRGB = true;
        break;

    default:
        return HRESULT_E_NOT_SUPPORTED;
    }

    auto pWIC = _GetWIC();
    if (!pWIC)
        return E_NOINTERFACE;

    ComPtr<IWICStream> stream;
    hr = pWIC->CreateStream(stream.GetAddressOf());
    if (FAILED(hr))
        return hr;

    hr = stream->InitializeFromFilename(fileName, GENERIC_WRITE);
    if (FAILED(hr))
        return hr;

    auto_delete_file_wic delonfail(stream, fileName);

    ComPtr<IWICBitmapEncoder> encoder;
    hr = pWIC->CreateEncoder(guidContainerFormat, nullptr, encoder.GetAddressOf());
    if (FAILED(hr))
        return hr;

    hr = encoder->Initialize(stream.Get(), WICBitmapEncoderNoCache);
    if (FAILED(hr))
        return hr;

    ComPtr<IWICBitmapFrameEncode> frame;
    ComPtr<IPropertyBag2> props;
    hr = encoder->CreateNewFrame(frame.GetAddressOf(), props.GetAddressOf());
    if (FAILED(hr))
        return hr;

    if (targetFormat && memcmp(&guidContainerFormat, &GUID_ContainerFormatBmp, sizeof(WICPixelFormatGUID)) == 0)
    {
        // Opt-in to the WIC2 support for writing 32-bit Windows BMP files with an alpha channel
        PROPBAG2 option = {};
        option.pstrName = const_cast<wchar_t*>(L"EnableV5Header32bppBGRA");

        VARIANT varValue;
        varValue.vt = VT_BOOL;
        varValue.boolVal = VARIANT_TRUE;
        (void)props->Write(1, &option, &varValue);
    }

    if (setCustomProps)
    {
        setCustomProps(props.Get());
    }

    hr = frame->Initialize(props.Get());
    if (FAILED(hr))
        return hr;

    hr = frame->SetSize(static_cast<UINT>(desc.Width), desc.Height);
    if (FAILED(hr))
        return hr;

    hr = frame->SetResolution(72, 72);
    if (FAILED(hr))
        return hr;

    // Pick a target format
    WICPixelFormatGUID targetGuid = {};
    if (targetFormat)
    {
        targetGuid = *targetFormat;
    }
    else
    {
        // Screenshots don't typically include the alpha channel of the render target
        switch (desc.Format)
        {
        case DXGI_FORMAT_R32G32B32A32_FLOAT:
        case DXGI_FORMAT_R16G16B16A16_FLOAT:
            targetGuid = GUID_WICPixelFormat96bppRGBFloat; // WIC 2
            break;

        case DXGI_FORMAT_R16G16B16A16_UNORM: targetGuid = GUID_WICPixelFormat48bppBGR; break;
        case DXGI_FORMAT_B5G5R5A1_UNORM:     targetGuid = GUID_WICPixelFormat16bppBGR555; break;
        case DXGI_FORMAT_B5G6R5_UNORM:       targetGuid = GUID_WICPixelFormat16bppBGR565; break;

        case DXGI_FORMAT_R32_FLOAT:
        case DXGI_FORMAT_R16_FLOAT:
        case DXGI_FORMAT_R16_UNORM:
        case DXGI_FORMAT_R8_UNORM:
        case DXGI_FORMAT_A8_UNORM:
            targetGuid = GUID_WICPixelFormat8bppGray;
            break;

        default:
            targetGuid = GUID_WICPixelFormat24bppBGR;
            break;
        }
    }

    hr = frame->SetPixelFormat(&targetGuid);
    if (FAILED(hr))
        return hr;
    
    if (targetFormat && memcmp(targetFormat, &targetGuid, sizeof(WICPixelFormatGUID)) != 0)
    {
        // Requested output pixel format is not supported by the WIC codec
        return E_FAIL;
    }

    // Encode WIC metadata
    ComPtr<IWICMetadataQueryWriter> metawriter;
    if (SUCCEEDED(frame->GetMetadataQueryWriter(metawriter.GetAddressOf())))
    {
        PROPVARIANT value;
        PropVariantInit(&value);

        value.vt = VT_LPSTR;
        value.pszVal = const_cast<char*>("DirectXTK");

        if (memcmp(&guidContainerFormat, &GUID_ContainerFormatPng, sizeof(GUID)) == 0)
        {
            // Set Software name
            (void)metawriter->SetMetadataByName(L"/tEXt/{str=Software}", &value);

            // Set sRGB chunk
            if (sRGB)
            {
                value.vt = VT_UI1;
                value.bVal = 0;
                (void)metawriter->SetMetadataByName(L"/sRGB/RenderingIntent", &value);
            }
            else
            {
                // add gAMA chunk with gamma 1.0
                value.vt = VT_UI4;
                value.uintVal = 100000; // gama value * 100,000 -- i.e. gamma 1.0
                (void)metawriter->SetMetadataByName(L"/gAMA/ImageGamma", &value);

                // remove sRGB chunk which is added by default.
                (void)metawriter->RemoveMetadataByName(L"/sRGB/RenderingIntent");
            }
        }
        else
        {
            // Set Software name
            (void)metawriter->SetMetadataByName(L"System.ApplicationName", &value);

            if (sRGB)
            {
                // Set EXIF Colorspace of sRGB
                value.vt = VT_UI2;
                value.uiVal = 1;
                (void)metawriter->SetMetadataByName(L"System.Image.ColorSpace", &value);
            }
        }
    }

    UINT64 imageSize = dstRowPitch * UINT64(desc.Height);
    if (imageSize > UINT32_MAX)
        return HRESULT_E_ARITHMETIC_OVERFLOW;

    void* pMappedMemory = nullptr;
    D3D12_RANGE readRange = { 0, static_cast<SIZE_T>(imageSize) };
    D3D12_RANGE writeRange = { 0, 0 };
    hr = pStaging->Map(0, &readRange, &pMappedMemory);
    if (FAILED(hr))
        return hr;

    if (memcmp(&targetGuid, &pfGuid, sizeof(WICPixelFormatGUID)) != 0)
    {
        // Conversion required to write
        ComPtr<IWICBitmap> source;
        hr = pWIC->CreateBitmapFromMemory(static_cast<UINT>(desc.Width), desc.Height,
            pfGuid,
            static_cast<UINT>(dstRowPitch), static_cast<UINT>(imageSize),
            static_cast<BYTE*>(pMappedMemory), source.GetAddressOf());


        const UINT stride = GetStride(640, 32);
        
        
        std::cout << stride * 480 <<std::endl;
        
        static int a = 0;

        source.Get()->CopyPixels(0,stride, 1228800,(BYTE*)&videoFrameBuffer[a][0]);
        a++;
        
        //1228800 
        if (a == 60) {
            a = 0;
            DecodingWAV();
        }


        if (FAILED(hr))
        {
            pStaging->Unmap(0, &writeRange);
            return hr;
        }
        /*
        ComPtr<IWICFormatConverter> FC;
        hr = pWIC->CreateFormatConverter(FC.GetAddressOf());
        if (FAILED(hr))
        {
            pStaging->Unmap(0, &writeRange);
            return hr;
        }

        BOOL canConvert = FALSE;
        hr = FC->CanConvert(pfGuid, targetGuid, &canConvert);
        if (FAILED(hr) || !canConvert)
        {
            pStaging->Unmap(0, &writeRange);
            return E_UNEXPECTED;
        }

        hr = FC->Initialize(source.Get(), targetGuid, WICBitmapDitherTypeNone, nullptr, 0, WICBitmapPaletteTypeMedianCut);
        if (FAILED(hr))
        {
            pStaging->Unmap(0, &writeRange);
            return hr;
        }

        WICRect rect = { 0, 0, static_cast<INT>(desc.Width), static_cast<INT>(desc.Height) };
        hr = frame->WriteSource(FC.Get(), &rect);*/
    }
    /*
    else
    {
        // No conversion required
        hr = frame->WritePixels(desc.Height, static_cast<UINT>(dstRowPitch), static_cast<UINT>(imageSize), static_cast<BYTE*>(pMappedMemory));
    }

    pStaging->Unmap(0, &writeRange);

    if (FAILED(hr))
        return hr;

    hr = frame->Commit();
    if (FAILED(hr))
        return hr;

    hr = encoder->Commit();
    if (FAILED(hr))
        return hr;
        
    delonfail.clear();
    */
    return S_OK;
}



#endif // WIN32



