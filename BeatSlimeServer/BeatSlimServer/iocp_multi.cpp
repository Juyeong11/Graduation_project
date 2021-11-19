#include"stdfx.h"
#include"Network.h"
using namespace std;

Network network;

int main()
{
	wcout.imbue(locale("korean"));
	WSADATA WSAData;
	int ret = WSAStartup(MAKEWORD(2, 2), &WSAData);
	if (ret != 0) error_display(WSAGetLastError());

	{
		Network* net = Network::GetInstance();

		net->g_s_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

		SOCKADDR_IN server_addr;
		ZeroMemory(&server_addr, sizeof(server_addr));
		server_addr.sin_family = AF_INET;
		server_addr.sin_port = htons(SERVER_PORT);
		server_addr.sin_addr.s_addr = htonl(INADDR_ANY);
		bind(net->g_s_socket, reinterpret_cast<sockaddr*>(&server_addr), sizeof(server_addr));
		listen(net->g_s_socket, SOMAXCONN);

		net->g_h_iocp = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, NULL, 0);

		CreateIoCompletionPort(reinterpret_cast<HANDLE>(net->g_s_socket), net->g_h_iocp, 0, 0);

		net->start_accept();

		vector<thread> worker_threads;

		for (int i = 0; i < 6; ++i)
			worker_threads.emplace_back(&Network::worker, net);

		char buf[256];
		int bufStart = 0;

		while (true) {
			//game loop
			if (net->nPlayer != 3) {
				SleepEx(1000, true);
				continue;
			}
			memset(buf, 0, sizeof(buf));
			bufStart = 0;

			for (auto& En : net->Enemys) {
				En.update(buf, bufStart);
			}

			if (bufStart)
				for (auto& cl : net->clients) {
					cl.state_lock.lock();
					if (ST_INGAME != cl.state) {
						cl.state_lock.unlock();
						continue;
					}
					cl.state_lock.unlock();
					cl.do_send(bufStart, buf);
				}
			SleepEx(1000, true);
		}

		for (auto& th : worker_threads)
			th.join();


	}
	WSACleanup();
}