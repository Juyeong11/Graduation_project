//Including of headers from SDK 7.1
#include "wmcodecdsp.h"
#include "mfapi.h"
#include "mfidl.h"
#include "Mfreadwrite.h"
#include "mferror.h"


//Including of libraries from SDK 7.1
#pragma comment(lib, "mf")
#pragma comment(lib, "mfreadwrite")
#pragma comment(lib, "mfplat")
#pragma comment(lib, "mfuuid")


//Video constants
const GUID	    VIDEO_MAJOR_TYPE = MFMediaType_Video;			//for video treatment
const GUID 	VIDEO_ENCODING_FORMAT = MFVideoFormat_H264;	//encoding format (output)
const GUID 	VIDEO_INPUT_FORMAT = MFVideoFormat_RGB32;		//input format
const UINT32	VIDEO_WIDTH = 720;							//picture size : width
const UINT32	VIDEO_HEIGHT = 576;							//picture size : height
const UINT32	VIDEO_FPS = 25;							//frames per second
const UINT32	VIDEO_BIT_RATE = 2000000;					//encoding bitrate (in bps)
const UINT32	VIDEO_MODE = MFVideoInterlace_Progressive;		//progressive or interlaced pictures
const UINT32	VIDEO_PELS = VIDEO_WIDTH * VIDEO_HEIGHT;		//resolution (width * height)
const UINT32	VIDEO_FRAME_COUNT = NUMBER_OF_SECONDS_FOR_FILE * VIDEO_FPS;	//frame counter (duration of the video)

//Audio constants
const GUID 	AUDIO_MAJOR_TYPE = MFMediaType_Audio;			//for audio treatment
const GUID	    AUDIO_ENCODING_FORMAT = MFAudioFormat_AAC;		//encoding format (output)
const GUID	    AUDIO_INPUT_FORMAT = MFAudioFormat_PCM;		//input format
const UINT32	AUDIO_SAMPLES_PER_SECOND = 44100;			//samples per second
const UINT32	AUDIO_AVG_BYTES_PER_SECOND = 16000;			//average bytes per second
const UINT32	AUDIO_NUM_CHANNELS = 1;						//MONO or STEREO
const UINT32	AUDIO_BITS_PER_SAMPLE = 16;					//bits per sample
const UINT32	AUDIO_ONE_SECOND = 10;						//quantity of buffers per second
const UINT32	AUDIO_BUFFER_LENGTH = AUDIO_BITS_PER_SAMPLE / 8 * AUDIO_NUM_CHANNELS * AUDIO_SAMPLES_PER_SECOND; <br / >                                                     //max. buffer size
const LONGLONG	AUDIO_SAMPLE_DURATION = 10000000;			//sample duration


//Buffer for one video frame
DWORD videoFrameBuffer[VIDEO_PELS];


//Creation of a template to release pointers
template <class T> void SafeRelease(T** ppT)
{
	if (*ppT)
	{
		(*ppT)->Release();
		*ppT = NULL;
	}
}


//Creation of the Byte Stream
IMFByteStream* CreateFileByteStream(LPCWSTR FileName)
{
	//create file byte stream
	IMFByteStream* pByteStream = NULL;

	HRESULT hr = MFCreateFile(MF_ACCESSMODE_WRITE, MF_OPENMODE_DELETE_IF_EXIST, MF_FILEFLAGS_NONE, FileName, &pByteStream);

	if (FAILED(hr))
		pByteStream = NULL;

	return pByteStream;
}


//Creation of the Video profile (H264)
IMFMediaType* CreateVideoProfile()
{
	IMFMediaType* pMediaType = NULL;

	HRESULT hr = MFCreateMediaType(&pMediaType);
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetGUID(MF_MT_MAJOR_TYPE, VIDEO_MAJOR_TYPE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetGUID(MF_MT_SUBTYPE, VIDEO_ENCODING_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetUINT32(MF_MT_AVG_BITRATE, VIDEO_BIT_RATE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetUINT32(MF_MT_INTERLACE_MODE, VIDEO_MODE);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeSize(pMediaType, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaType, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaType, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
	}

	if (FAILED(hr))
		pMediaType = NULL;

	return pMediaType;
}


//Creation of the Audio profile (AAC)
IMFMediaType* CreateAudioProfile()
{
	IMFMediaType* pMediaType = NULL;

	HRESULT hr = MFCreateMediaType(&pMediaType);
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetGUID(MF_MT_MAJOR_TYPE, AUDIO_MAJOR_TYPE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetGUID(MF_MT_SUBTYPE, AUDIO_ENCODING_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetUINT32(MF_MT_AUDIO_BITS_PER_SAMPLE, AUDIO_BITS_PER_SAMPLE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetUINT32(MF_MT_AUDIO_SAMPLES_PER_SECOND, AUDIO_SAMPLES_PER_SECOND);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetUINT32(MF_MT_AUDIO_NUM_CHANNELS, AUDIO_NUM_CHANNELS);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaType->SetUINT32(MF_MT_AUDIO_AVG_BYTES_PER_SECOND, AUDIO_AVG_BYTES_PER_SECOND);
	}

	if (FAILED(hr))
		pMediaType = NULL;

	return pMediaType;
}


//Create an aggregate source (both audio and video)
IMFMediaSource* CreateAggregatedSource(IMFMediaSource* pSource1, IMFMediaSource* pSource2, IMFMediaSource* pAggSource)
{
	pAggSource = NULL;
	IMFCollection* pCollection = NULL;

	HRESULT hr = MFCreateCollection(&pCollection);
	if (SUCCEEDED(hr))
	{
		hr = pCollection->AddElement(pSource1);
	}
	if (SUCCEEDED(hr))
	{
		hr = pCollection->AddElement(pSource2);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFCreateAggregateSource(pCollection, &pAggSource);
	}

	SafeRelease(&pCollection);

	if (FAILED(hr))
		pAggSource = NULL;

	return pAggSource;
}


//Creation of the MPEG-4 MediaSink
IMFMediaSink* CreateMediaSink(IMFByteStream* pByteStream, IMFMediaType* pVideoMediaType, IMFMediaType* pAudioMediaType)
{
	IMFMediaSink* pMediaSink = NULL;
	DWORD pdwCharac = NULL;
	DWORD pdwStreamCount = NULL;

	HRESULT hr = MFCreateMPEG4MediaSink(pByteStream, pVideoMediaType, pAudioMediaType, &pMediaSink);

	//// DEBUG ////
	pMediaSink->GetCharacteristics(&pdwCharac);
	pMediaSink->GetStreamSinkCount(&pdwStreamCount);

	if (FAILED(hr))
		pMediaSink = NULL;

	return pMediaSink;
}


IMFAttributes* CreateAttributesForSinkWriter()
{
	IMFAttributes* pMFAttributes = NULL;

	HRESULT hr = MFCreateAttributes(&pMFAttributes, 100);

	if (SUCCEEDED(hr))
	{
		hr = pMFAttributes->SetGUID(MF_TRANSCODE_CONTAINERTYPE, MFTranscodeContainerType_MPEG4);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMFAttributes->SetUINT32(MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, FALSE); //no hardware encoding
	}
	if (SUCCEEDED(hr))
	{
		hr = pMFAttributes->SetUINT32(MF_READWRITE_DISABLE_CONVERTERS, FALSE); //enable converting formats
	}

	if (FAILED(hr))
		pMFAttributes = NULL;

	return pMFAttributes;
}


//Initialization of the Video SinkWriter...
HRESULT InitializeSinkWriterVideo(IMFSinkWriter** ppWriter, DWORD* pStreamIndex, IMFMediaSink* pMediaSink)
{
	*ppWriter = NULL;
	*pStreamIndex = NULL;

	IMFSinkWriter* pSinkWriter = NULL;
	IMFMediaType* pMediaTypeOut = NULL;
	IMFMediaType* pMediaTypeIn = NULL;
	IMFAttributes* pAttrib = NULL;
	DWORD     streamIndexV = 0;
	DWORD		 streamIndexA = 1;

	HRESULT hr = MFCreateSinkWriterFromMediaSink(pMediaSink, NULL, &pSinkWriter);

	//input : video
	if (SUCCEEDED(hr))
	{
		hr = MFCreateMediaType(&pMediaTypeIn);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_MAJOR_TYPE, VIDEO_MAJOR_TYPE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_SUBTYPE, VIDEO_INPUT_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetUINT32(MF_MT_INTERLACE_MODE, VIDEO_MODE);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeSize(pMediaTypeIn, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->SetInputMediaType(streamIndexV, pMediaTypeIn, NULL);
	}

	//Tell to the Video SinkWriter to begin data treatment
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->BeginWriting();
	}

	//Possible error codes
	if (FAILED(hr))
	{
		UINT32 uiShutDown = 9999;

		if (hr == MF_E_INVALIDMEDIATYPE)
			uiShutDown = 0;

		if (hr == MF_E_INVALIDSTREAMNUMBER)
			uiShutDown = 1;

		if (hr == MF_E_TOPO_CODEC_NOT_FOUND)
			uiShutDown = 2;

		if (hr == E_INVALIDARG)
			uiShutDown = 3;
	}

	//Returns the pointer of the caller
	if (SUCCEEDED(hr))
	{
		*ppWriter = pSinkWriter;
		(*ppWriter)->AddRef();
		*pStreamIndex = streamIndexV;
	}

	//Release pointers
	SafeRelease(&pSinkWriter);
	SafeRelease(&pMediaTypeOut);
	SafeRelease(&pMediaTypeIn);
	SafeRelease(&pAttrib);

	return hr;
}


//Initialization of the Audio SinkWriter...
HRESULT InitializeSinkWriterAudio(IMFSinkWriter** ppWriter, DWORD* pStreamIndex, IMFMediaSink* pMediaSink)
{
	*ppWriter = NULL;
	*pStreamIndex = NULL;

	IMFSinkWriter* pSinkWriter = NULL;
	IMFMediaType* pMediaTypeOut = NULL;
	IMFMediaType* pMediaTypeIn = NULL;
	IMFAttributes* pAttrib = NULL;
	DWORD      streamIndex = 1;

	HRESULT hr = MFCreateSinkWriterFromMediaSink(pMediaSink, NULL, &pSinkWriter);

	//input : audio
	if (SUCCEEDED(hr))
	{
		hr = MFCreateMediaType(&pMediaTypeIn);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_MAJOR_TYPE, AUDIO_MAJOR_TYPE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_SUBTYPE, AUDIO_INPUT_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetUINT32(MF_MT_AUDIO_BITS_PER_SAMPLE, AUDIO_BITS_PER_SAMPLE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetUINT32(MF_MT_AUDIO_SAMPLES_PER_SECOND, AUDIO_SAMPLES_PER_SECOND);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetUINT32(MF_MT_AUDIO_NUM_CHANNELS, AUDIO_NUM_CHANNELS);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->SetInputMediaType(streamIndex, pMediaTypeIn, NULL);
	}

	//Tell the Audio SinkWriter to begin data treatment
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->BeginWriting();
	}

	//Possible error codes
	if (FAILED(hr))
	{
		if (hr == MF_E_INVALIDMEDIATYPE)
			UINT32 uiShutDown = 0;

		if (hr == MF_E_INVALIDSTREAMNUMBER)
			UINT32 uiShutDown = 1;

		if (hr == MF_E_TOPO_CODEC_NOT_FOUND)
			UINT32 uiShutDown = 2;
	}

	//Returns the pointer of the caller
	if (SUCCEEDED(hr))
	{
		*ppWriter = pSinkWriter;
		(*ppWriter)->AddRef();
		*pStreamIndex = streamIndex;
	}

	//Release pointers
	SafeRelease(&pSinkWriter);
	SafeRelease(&pMediaTypeOut);
	SafeRelease(&pMediaTypeIn);
	SafeRelease(&pAttrib);

	return hr;
}


//Write a video frame
HRESULT WriteVideoFrame(IMFSinkWriter* pWriter, DWORD streamIndex, const LONGLONG& rtStart, const LONGLONG& rtDuration)
{
	/*
	* rtStart : time stamp
	* rtDuration : frame duration
	*/

	IMFSample* pSample = NULL;
	IMFMediaBuffer* pBuffer = NULL;

	const LONG cbWidth = 4 * VIDEO_WIDTH;
	const DWORD cbBuffer = 4 * VIDEO_PELS;

	BYTE* pData = NULL;

	//Create a new memory buffer, whose max. size is cbBuffer (H*L*4 for one picture)
	HRESULT hr = MFCreateMemoryBuffer(cbBuffer, &pBuffer);

	//Lock the buffer and copy the video frame to the buffer
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->Lock(&pData, NULL, NULL);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFCopyImage(pData, cbWidth, (BYTE*)videoFrameBuffer, cbWidth, cbWidth, VIDEO_HEIGHT);

		/*
		* pData : Destination buffer
		* cbWidth : Destination stride
		* (BYTE*)videoFrameBuffer : First row in source image
		* cbWidth : Source stride
		* cbWidth : Image width in bytes
		* VIDEO_HEIGHT: Image height in pixels
		*/

	}
	if (pBuffer)
	{
		pBuffer->Unlock();
	}

	//Set the data length of the buffer
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->SetCurrentLength(cbBuffer);
	}

	//Create a media sample and add the buffer to the sample
	if (SUCCEEDED(hr))
	{
		hr = MFCreateSample(&pSample);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->AddBuffer(pBuffer);
	}

	//Set the time stamp and the duration
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleTime(rtStart);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleDuration(rtDuration);
	}

	//Send the sample to the Sink Writer
	if (SUCCEEDED(hr))
	{
		hr = pWriter->WriteSample(streamIndex, pSample);
	}

	//Release pointers
	SafeRelease(&pSample);
	SafeRelease(&pBuffer);

	return hr;
}


//Write an audio packet
HRESULT WriteAudioPacket(IMFSinkWriter* pWriter, DWORD streamIndex, const LONGLONG& rtStart, const LONGLONG& rtDuration, UINT32 Quantity)
{
	IMFSample* pSample = NULL;
	IMFMediaBuffer* pBuffer = NULL;

	const DWORD cbBuffer = Quantity * 2;

	BYTE* pData = NULL;


	//Create a new memory buffer, whose max. size is cbBuffer (QuantityOfSamplesPerVideoFrame * 2 Bytes)
	HRESULT hr = MFCreateMemoryBuffer(cbBuffer, &pBuffer);

	//Lock the buffer and copy the audio packet to the buffer
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->Lock(&pData, NULL, NULL);
	}
	if (SUCCEEDED(hr))
	{
		for (DWORD n = 0; n < cbBuffer; n++)
		{
			pData[n] = (BYTE)(rand() & 0xFF);	//generation of random noise
		}
	}
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->Unlock();
	}

	// Set the data length of the buffer
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->SetCurrentLength(cbBuffer);
	}

	//Create a media sample and add the buffer to the sample
	if (SUCCEEDED(hr))
	{
		hr = MFCreateSample(&pSample);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->AddBuffer(pBuffer);
	}

	//Set the time stamp and the duration
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleTime(rtStart);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleDuration(rtDuration);
	}

	//Send the sample to the Sink Writer
	if (SUCCEEDED(hr))
	{
		hr = pWriter->WriteSample(streamIndex, pSample);
	}

	//Release pointers
	SafeRelease(&pSample);
	SafeRelease(&pBuffer);

	return hr;
}


// MAIN FUNCTION
void CMediaSinkDlg::OnBnClickedOk()
{
	HRESULT hr = S_OK;
	IMFByteStream* spByteStream = NULL;
	IMFMediaSink* pMediaSink = NULL;
	IMFSinkWriter* spSinkWriterVid = NULL;
	IMFSinkWriter* spSinkWriterAud = NULL;
	IMFMediaType* spVideo = NULL;
	IMFMediaType* spAudio = NULL;
	//IMFMediaEventGenerator	*spMFMediaEvtGene = NULL;
	//IMFMediaEvent			*spMFMediaEvent = NULL;
	IMFAttributes* spAttrib = NULL;

	DWORD					sindexVid = 0, sindexAud = 0, j = 0;

	LPCWSTR	str = L"outputfile.mp4";

	hr = CoInitialize(NULL);
	if (SUCCEEDED(hr))
	{
		hr = MFStartup(MF_VERSION);
		if (SUCCEEDED(hr))
		{
			spByteStream = CreateFileByteStream(str);
			if (spByteStream != NULL)
			{
				spVideo = CreateVideoProfile();
			}
			if (spVideo != NULL)
			{
				spAudio = CreateAudioProfile();
			}
			if (spAudio != NULL)
			{
				pMediaSink = CreateMediaSink(spByteStream, spVideo, spAudio);
			}

			if (pMediaSink != NULL)
			{
				hr = InitializeSinkWriterVideo(&spSinkWriterVid, &sindexVid, pMediaSink);
				if (SUCCEEDED(hr))
				{
					LONGLONG rtStartVid = 0;
					UINT64 rtDurationVid;

					/********************************************************
					*						VIDEO PART				 *
					********************************************************/

					//Calculate the average time per frame, for video<br/>                    MFFrameRateToAverageTimePerFrame(VIDEO_FPS, 1, &rtDurationVid);

					//loop to treat all the pictures
					for (DWORD i = 0; i < VIDEO_FRAME_COUNT; ++i, ++j)
					{
						//Picture pixels
						for (DWORD k = 0; k < VIDEO_PELS; k++)
						{
							if (j > 255)
								j = 0;

							videoFrameBuffer[k] = ((j << 16) & 0x00FF0000) | ((j << 8) & 0x0000FF00) | (j & 0x000000FF);
						}

						hr = WriteVideoFrame(spSinkWriterVid, sindexVid, rtStartVid, rtDurationVid);
						if (FAILED(hr))
						{
							break;
						}

						//Update the time stamp value
						rtStartVid += rtDurationVid;
					}

					//Finalization of writing with the Video SinkWriter
					if (SUCCEEDED(hr))
					{
						hr = spSinkWriterVid->Finalize();
					}
				}
			}

			SafeRelease(&spVideo);
			SafeRelease(&spSinkWriterVid);

			if (SUCCEEDED(hr))
			{
				hr = InitializeSinkWriterAudio(&spSinkWriterAud, &sindexAud, pMediaSink);
				if (SUCCEEDED(hr))
				{
					LONGLONG rtStartAud = 0;
					UINT64 rtDurationAud;
					double QtyAudioSamplesPerVideoFrame = 0;

					//Calculate the approximate quantity of samples, according to a video frame duration
					//44100 Hz -> 1 s
					//????? Hz -> 0.04 s (= 40 ms = one video frame duration)
					if (VIDEO_FPS != 0)
						QtyAudioSamplesPerVideoFrame = ((double)AUDIO_SAMPLES_PER_SECOND / (double)VIDEO_FPS);
					else
						QtyAudioSamplesPerVideoFrame = 0;

					MFFrameRateToAverageTimePerFrame(VIDEO_FPS, 1, &rtDurationAud);	//we treat the same duration as the video
					//it means that we will treat N audio packets for the last of one picture (=40 ms)

					//loop to treat all the audio packets
					if (rtDurationAud != 0)
					{
						for (DWORD i = 0; i < VIDEO_FRAME_COUNT; ++i)
						{
							//Audio packets
							hr = WriteAudioPacket(spSinkWriterAud, sindexAud, rtStartAud, rtDurationAud, (UINT32)QtyAudioSamplesPerVideoFrame);
							if (FAILED(hr))
							{
								break;
							}

							//Update the time stamp value
							rtStartAud += rtDurationAud;
						}

						//Finalization of writing with the Audio SinkWriter
						if (SUCCEEDED(hr))
						{
							hr = spSinkWriterAud->Finalize();
						}
					}
				}
			}

			//Release pointers
			SafeRelease(&spByteStream);
			SafeRelease(&spAudio);
			SafeRelease(&spSinkWriterAud);
			SafeRelease(&spAttrib);

			//Shutdown the MediaSink (not done by the SinkWriter)
			pMediaSink->Shutdown();
			SafeRelease(&pMediaSink);
		}

		//Shutdown MediaFoundation
		MFShutdown();
		CoUninitialize();
	}

	CDialog::OnOK();
}