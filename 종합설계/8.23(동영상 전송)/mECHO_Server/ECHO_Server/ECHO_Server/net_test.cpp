#include<iostream>

#include<WS2tcpip.h>	
#include<mfapi.h>

#pragma comment(lib,"WS2_32.LIB")	

#pragma comment(lib, "mfreadwrite")
#pragma comment(lib, "mfplat")
#pragma comment(lib, "mfuuid")						
using namespace std;

constexpr int BUF_SIZE = 1024;
constexpr short SERVER_PORT = 3333;

void display_error(int err_code)
{
	WCHAR* w_msg = nullptr;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, err_code, 
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		reinterpret_cast<LPWSTR>(&w_msg), 0, NULL);
	wcout << w_msg;
	LocalFree(w_msg);
}

const UINT32 VIDEO_WIDTH = 640;
const UINT32 VIDEO_HEIGHT = 480;
const UINT32 VIDEO_FPS = 30;
const UINT64 VIDEO_FRAME_DURATION = 10 * 1000 * 1000 / VIDEO_FPS;
const UINT32 VIDEO_BIT_RATE = 800000;
const GUID   VIDEO_ENCODING_FORMAT = MFVideoFormat_WMV3;
const GUID   VIDEO_INPUT_FORMAT = MFVideoFormat_RGB32;
const UINT32 VIDEO_PELS = VIDEO_WIDTH * VIDEO_HEIGHT;
const UINT32 VIDEO_FRAME_COUNT = 20 * VIDEO_FPS;

DWORD videoFrameBuffer[VIDEO_PELS];

int main()
{
	wcout.imbue(locale("korean"));
	WSAData WSAData;
	WSAStartup(MAKEWORD(2, 3), &WSAData);	
											

	SOCKET server_socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	SOCKADDR_IN server_addr;
	memset(&server_addr, sizeof(server_addr), 0);
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(SERVER_PORT);
	server_addr.sin_addr.s_addr = INADDR_ANY;
	//IP주소는 브로드캐스팅 주소로 해줘서 모두 다 받을 수 있도록 해줘야하고
	//서버주소는 포트번호를 위한 것
	bind(server_socket,reinterpret_cast<sockaddr*>(&server_addr),sizeof(server_addr));
	listen(server_socket, SOMAXCONN);
	int addr_size = sizeof(server_addr);
	SOCKET client_socket = accept(server_socket, reinterpret_cast<sockaddr*>(&server_addr), &addr_size);//서버는 사이즈를 받아야됨


	for (DWORD i = 0; i < VIDEO_PELS; ++i)
	{
	    videoFrameBuffer[i] = 0x0000FF00;
	}

	for (;;) {
		// 클라이언트에서 보내는 문자열을 수신

		char recv_buffer[BUF_SIZE];
		int recv_size = recv(client_socket, recv_buffer, BUF_SIZE, 0);
		recv_buffer[recv_size] = 0;
		cout << "Client Sent : [" << recv_buffer << "]" << endl;

		//클라이언트에 그대로 전송
		cout << sizeof(DWORD) * sizeof(videoFrameBuffer) << endl;
		int ret = send(client_socket, (char*)videoFrameBuffer, sizeof(videoFrameBuffer), 0);

		if (SOCKET_ERROR == ret) {
			int err_code = WSAGetLastError();
			cout << "Send Error : " << err_code << endl;
			display_error(err_code);
			exit(-1);
		}
		cout << "Server Sent : [" << recv_buffer << "]" << endl;
	}
	closesocket(client_socket);
	closesocket(server_socket);
	//mmo를 만들고 싶으면 게임서버  5대5 넷겜플
	WSACleanup();
}