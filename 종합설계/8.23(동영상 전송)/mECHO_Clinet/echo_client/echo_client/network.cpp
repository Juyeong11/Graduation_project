#include<format>
#include<iostream>
#include<string>
#include<WS2tcpip.h>	//Windos Socket Ver 2 TCP/IP API -> WS2tcpip
//버전1은 기본적인 것만 지원 2는 비동기 등 여러가지 기능이있음
//이건 c++언어가 아니라 비쥬얼의 메타언어
// 
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


#pragma comment(lib,"WS2_32.LIB")	// Windows Socket Ver2 for Win32 Library
									// WIN32는 WIN16 API의 반대 개념이다
using namespace std;

constexpr int BUF_SIZE = 1024;
constexpr const char* SERVER_IP = "127.0.0.1";
constexpr short SERVER_PORT = 3333;


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


void DecodingWAV();



int main()
{
	WSAData WSAData;
	WSAStartup(MAKEWORD(2, 3), &WSAData);	// 앞으로 시작되는 모든 통신은 소켓 api를 사용한다고 운영체제에 알려줌 -> 구석기 유물!!
											// 마소가 예전에 자신들의 네트워크를 만들겠다고 했던 때 생긴유물

	SOCKET server_socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	SOCKADDR_IN server_addr;
	memset(&server_addr, sizeof(server_addr), 0);
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(SERVER_PORT);	//네트워크 표준 엔디안? 으로 맞춰주어야 한다.
	inet_pton(AF_INET, SERVER_IP, &server_addr.sin_addr);//문자열을 넣으면 안되고 숫자로 바꿔서 ip주소를 넣어야죠

	connect(server_socket, reinterpret_cast<sockaddr*>(&server_addr), sizeof(server_addr));	// 운영체제에게 직접 보내주는 파라미터 ->
																// 운영체제는 server_addr의 주소를 받음 -> 운영체제는 이 주소에 마음대로 접근이 안된다?, 
																// 만약 8바이트 크기인데 4바이트만 보냈다면 운영체제에서 8바이트를 읽으면 읽지 않아야하는 부분을 읽을 수 있다

    char buffer[BUF_SIZE];
    cout << "Enter Text : ";
    cin.getline(buffer, BUF_SIZE);

    int data_size = strlen(buffer);// 널문자 안보낸다 받는 쪽 책임이니까 알아서 해라!
    send(server_socket, buffer, data_size, 0);
	for (;;) {

		//서버 전송
		//서버에서 보내는 것을 수신
        static int a = 0;

		
		int recv_size = recv(server_socket, (char*)videoFrameBuffer[a], 4915200, 0);

		//recv_buffer[recv_size] = 0;
		cout << "Server Sent : [" << videoFrameBuffer[a] << "]" << endl;
		

		a++;
		if (a == 60) {
			a = 0;
			DecodingWAV();
		}
	}
	WSACleanup();
}
template <class T> void SafeRelease(T** ppT)
{
    if (*ppT)
    {
        (*ppT)->Release();
        *ppT = NULL;
    }
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

    //ComPtr<IMFCollection> availableTypes = nullptr;
    //hr = MFTranscodeGetAudioOutputAvailableTypes(MFAudioFormat_WMAudioV9, MFT_ENUM_FLAG_ALL, NULL, availableTypes.GetAddressOf());

    //DWORD count = 0;
    //hr = availableTypes->GetElementCount(&count);  // Get the number of elements in the list.

    //ComPtr<IUnknown>     pUnkAudioType = nullptr;
    //ComPtr<IMFMediaType> audioOutputType = nullptr;
    //for (DWORD i = 0; i < count; ++i)
    //{
    //    hr = availableTypes->GetElement(i, pUnkAudioType.GetAddressOf());
    //    hr = pUnkAudioType.Get()->QueryInterface((void**)&pAudioTypeOut);

    //    // compare channels, sampleRate, and bitsPerSample to target numbers
    //    {
    //        // audioTypeOut is set!
    //        break;
    //    }

    //    audioOutputType.Reset();
    //}
    //availableTypes.Reset();

    //hr = pSinkWriter->AddStream(pAudioTypeOut, &streamIndex);


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
    //hr = pSinkWriter->SetInputMediaType(streamIndex, pAudioTypeIn, nullptr);


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
                    hr = WriteFrame(pSinkWriter, stream, rtStart, i);
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