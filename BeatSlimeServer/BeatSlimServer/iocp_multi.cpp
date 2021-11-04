#include <iostream>
#include <WS2tcpip.h>
#include <MSWSock.h>
#include <array>
#include <vector>
#include<thread>
#include<mutex>
#include"Enemy.h"

#include "protocol.h"
using namespace std;
#pragma comment (lib, "WS2_32.LIB")
#pragma comment (lib, "MSWSock.LIB")


const int BUFSIZE = 256;

bool g_shutdown = false;

void error_display(int err_num)
{
	WCHAR* lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, err_num,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf, 0, 0);
	wcout << lpMsgBuf << endl;
	//while (true);
	LocalFree(lpMsgBuf);
}

enum COMP_OP { OP_RECV, OP_SEND, OP_ACCEPT };

HANDLE g_h_iocp;
SOCKET g_s_socket;

class EXP_OVER {
public:
	WSAOVERLAPPED	_wsa_over;
	COMP_OP			_comp_op;// icop에서는 무엇이 완료되었는지 모르기 때문에 추가정보가 id에서 바뀜
	WSABUF			_wsa_buf;
	unsigned char			_net_buf[BUFSIZE];
public:
	EXP_OVER(COMP_OP comp_op, char num_bytes, void* mess) : _comp_op(comp_op)
	{
		ZeroMemory(&_wsa_over, sizeof(_wsa_over));
		_wsa_buf.buf = reinterpret_cast<char*>(_net_buf);
		_wsa_buf.len = num_bytes;
		memcpy(_net_buf, mess, num_bytes);
	}

	EXP_OVER(COMP_OP comp_op) : _comp_op(comp_op)
	{
		//ZeroMemory(&_wsa_over, sizeof(_wsa_over)); recv할때 따로 해주자
	}//recv용

	EXP_OVER()
	{
	}

	~EXP_OVER()
	{
	}
};


enum STATE { ST_FREE, ST_ACCEPT, ST_INGAME };

class CLIENT {
public:
	char name[MAX_NAME_SIZE];
	int	   _id;
	short  x, y, z;

	mutex state_lock;
	//volatile해줘야 한다.
	volatile STATE _state;

	EXP_OVER _recv_over;
	SOCKET _socket;
	int		_prev_size;
public:
	CLIENT()
	{
		_state = ST_FREE;

		x = 0;
		y = 0;
		z = 0;
		_prev_size = 0;
	}


	~CLIENT()
	{
		closesocket(_socket);
	}

	void do_recv()
	{
		DWORD recv_flag = 0;
		ZeroMemory(&_recv_over._wsa_over, sizeof(_recv_over._wsa_over));
		_recv_over._wsa_buf.buf = reinterpret_cast<char*>(_recv_over._net_buf + _prev_size);//
		_recv_over._wsa_buf.len = sizeof(_recv_over._net_buf) - _prev_size;
		int ret = WSARecv(_socket, &_recv_over._wsa_buf, 1, 0, &recv_flag, &_recv_over._wsa_over, NULL);
		if (ret == SOCKET_ERROR)
		{
			int error_num = WSAGetLastError();
			if (ERROR_IO_PENDING != error_num)
				error_display(error_num);
		}
	}

	void do_send(int num_bytes, void* mess)
	{
		EXP_OVER* ex_over = new EXP_OVER(OP_SEND, num_bytes, mess);
		WSASend(_socket, &ex_over->_wsa_buf, 1, 0, 0, &ex_over->_wsa_over, NULL);
	}
};

array <CLIENT, MAX_USER> clients;
vector <Enemy> Enemys;

int get_new_id()
{
	static int g_id = 0;

	for (int i = 0; i < MAX_USER; ++i) {
		clients[i].state_lock.lock();
		if (ST_FREE == clients[i]._state) {
			clients[i]._state = ST_ACCEPT;
			clients[i].state_lock.unlock();
			return i;
		}
		clients[i].state_lock.unlock();
	}
	cout << "Maximum Number of Clients Overflow!!\n";
	return -1;
}
void send_login_ok_packet(int c_id)
{
	sc_packet_login_ok packet;
	packet.id = c_id;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_LOGIN_OK;
	packet.x = clients[c_id].x;
	packet.y = clients[c_id].y;
	packet.z = clients[c_id].z;

	clients[c_id].do_send(sizeof(packet), &packet);
}
void send_move_packet(int c_id, int mover)
{
	sc_packet_move packet;
	packet.id = mover;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_MOVE;
	packet.x = clients[mover].x;
	packet.y = clients[mover].y;
	packet.z = clients[mover].z;

	clients[c_id].do_send(sizeof(packet), &packet);
}
void send_remove_object(int c_id, int victim)
{
	sc_packet_remove_object packet;
	packet.id = victim;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_REMOVE_OBJECT;

	clients[c_id].do_send(sizeof(packet), &packet);

}

void Disconnect(int c_id)
{
	clients[c_id].state_lock.lock();
	clients[c_id]._state = ST_FREE;
	closesocket(clients[c_id]._socket);

	clients[c_id].state_lock.unlock();

	for (auto& cl : clients) {
		//? 왜 락이 필요하지?
		cl.state_lock.lock();//
		if (ST_INGAME != cl._state) {
			cl.state_lock.unlock();
			continue;
		};
		cl.state_lock.unlock();
		send_remove_object(cl._id, c_id);
		clients[c_id].x = clients[c_id].y = clients[c_id].z = 0;
	}
}
void process_packet(int client_id, unsigned char* ps)
{
	unsigned char packet_type = ps[1];
	CLIENT& cl = clients[client_id];

	switch (packet_type)
	{
	case CS_PACKET_LOGIN:
	{
		cs_packet_login* packet = reinterpret_cast<cs_packet_login*>(ps);
		//strcpy_s(cl.name, packet->name);

		cout << "Client[" << cl._id << "]" << " login\n";
		send_login_ok_packet(client_id);

		for (auto& other : clients) {
			if (other._id == client_id)continue;

			other.state_lock.lock();
			if (ST_INGAME != other._state) {
				other.state_lock.unlock();
				continue;
			}
			other.state_lock.unlock();

			sc_packet_put_object packet;
			packet.id = client_id;
			//strcpy_s(packet.name, cl.name);
			packet.object_type = PLAPER;
			packet.size = sizeof(packet);
			packet.type = SC_PACKET_PUT_OBJECT;
			packet.x = cl.x;
			packet.y = cl.y;
			packet.z = cl.z;

			other.do_send(sizeof(packet), &packet);

		}

		for (auto& other : clients) {
			if (other._id == client_id)continue;

			other.state_lock.lock();
			if (ST_INGAME != other._state) {
				other.state_lock.unlock();
				continue;
			}
			other.state_lock.unlock();

			sc_packet_put_object packet;
			packet.id = other._id;
			//strcpy_s(packet.name, other.name);
			packet.object_type = PLAPER;
			packet.size = sizeof(packet);
			packet.type = SC_PACKET_PUT_OBJECT;
			packet.x = other.x;
			packet.y = other.y;
			packet.z = other.z;


			cl.do_send(sizeof(packet), &packet);
		}

		char buf[256];
		int bufStart = 0;


		for (auto& En : Enemys) {
			En.PutObject(buf, bufStart);
		}
		for (auto& cl : clients) {
			cl.do_send(bufStart, buf);
		}

	}
	break;
	case CS_PACKET_MOVE:
	{

		cs_packet_move* packet = reinterpret_cast<cs_packet_move*>(ps);
		short& x = cl.x;
		short& y = cl.y;
		short& z = cl.z;
		cout << "Client[" << cl._id << "]" << " move to " << x << ", " << y << ", " << z << endl;

		switch (packet->direction)
		{
		case DIR::LEFTUP:		if (true) { x--; z++; break; }
		case DIR::UP:			if (true) { y--; z++;  break; }
		case DIR::RIGHTUP:		if (true) { x++; y--; break; }
		case DIR::LEFTDOWN:		if (true) { x--; y++; break; }
		case DIR::DOWN:			if (true) { y++; z--; break; }
		case DIR::RIGHTDOWN:	if (true) { x++; z--; break; }

		default:
			cout << "Invalid move in client " << client_id << endl;
			exit(-1);
			break;
		}
		for (auto& cl : clients) {
			cl.state_lock.lock();
			if (ST_INGAME == cl._state) {
				cl.state_lock.unlock();

				send_move_packet(cl._id, client_id);
			}
			else cl.state_lock.unlock();
		}

	}
	break;
	default:
		break;
	}
}

void worker()
{
	for (;;) {
		DWORD num_byte;
		//PULONG_PTR iocp_key;
		LONG64 iocp_key;
		WSAOVERLAPPED* p_over;

		BOOL ret = GetQueuedCompletionStatus(g_h_iocp, &num_byte, (PULONG_PTR)&iocp_key, &p_over, INFINITE);
		//cout << "GQCS ACCEPT\n";


		//cout << "Entering Main Loop\n";
		int client_id = static_cast<int>(iocp_key);

		EXP_OVER* exp_over = reinterpret_cast<EXP_OVER*>(p_over);

		if (FALSE == ret) {
			int err_no = WSAGetLastError();
			cout << "GQCS err : ";
			error_display(err_no);
			cout << endl;
			Disconnect(client_id);
			if (exp_over->_comp_op == OP_SEND)
				delete exp_over;
			continue;
		}
		cout << exp_over->_comp_op << endl;
		switch (exp_over->_comp_op) {
		case OP_RECV:
		{
			if (num_byte == 0) {
				Disconnect(client_id);
				continue;

			}
			//packet 재조립
			CLIENT& cl = clients[client_id];
			int remain_data = num_byte + cl._prev_size;
			unsigned char* packet_start = exp_over->_net_buf;
			int packet_size = packet_start[0];

			while (packet_size <= remain_data)
			{
				process_packet(client_id, packet_start);
				remain_data -= packet_size;
				packet_start += packet_size;
				if (remain_data > 0) packet_size = packet_start[0];
				else break;
			}
			cl._prev_size = remain_data;
			if (0 < remain_data) {
				memcpy(exp_over->_net_buf, packet_start, remain_data);
			}
			cl.do_recv();
		}
		break;
		case OP_SEND:
		{
			if (num_byte != exp_over->_wsa_buf.len)
			{
				//DISCONNECT
				Disconnect(client_id);
			}
			delete exp_over;
		}
		break;
		case OP_ACCEPT:
		{
			cout << "Accept Completed.\n";
			SOCKET c_socket = *(reinterpret_cast<SOCKET*>(exp_over->_net_buf));
			int new_id = get_new_id();// x컨테이너의 빈 곳을 찾자
			CLIENT& cl = clients[new_id];

			cl.x = 0;
			cl.y = 0;
			cl._id = new_id;
			cl._prev_size = 0;
			cl._recv_over._comp_op = OP_RECV;
			cl._state = ST_INGAME;

			cl._recv_over._wsa_buf.buf = reinterpret_cast<char*>(cl._recv_over._net_buf);
			cl._recv_over._wsa_buf.len = sizeof(cl._recv_over._net_buf);
			ZeroMemory(&cl._recv_over._wsa_over, sizeof(cl._recv_over._wsa_over));


			cl._socket = c_socket; // acceptex


			CreateIoCompletionPort(reinterpret_cast<HANDLE>(c_socket), g_h_iocp, new_id, 0);
			cl.do_recv();

			//accept를 했으니 다시 accpet
			c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

			*(reinterpret_cast<SOCKET*>(exp_over->_net_buf)) = c_socket;

			ZeroMemory(&exp_over->_wsa_over, sizeof(exp_over->_wsa_over));


			AcceptEx(g_s_socket, c_socket, exp_over->_net_buf + sizeof(int*), 0, sizeof(SOCKADDR_IN) + 16,
				sizeof(SOCKADDR_IN) + 16, NULL, &exp_over->_wsa_over);

		}
		break;
		}
		//빠져나가는 조건 만들 것
	}
}

int main()
{
	wcout.imbue(locale("korean"));
	WSADATA WSAData;
	WSAStartup(MAKEWORD(2, 2), &WSAData);

	g_s_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);
	SOCKADDR_IN server_addr;
	::ZeroMemory(&server_addr, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(SERVER_PORT);
	server_addr.sin_addr.s_addr = htonl(INADDR_ANY);
	bind(g_s_socket, reinterpret_cast<sockaddr*>(&server_addr), sizeof(server_addr));
	listen(g_s_socket, SOMAXCONN);

	g_h_iocp = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, NULL, 0);
	CreateIoCompletionPort(reinterpret_cast<HANDLE>(g_s_socket), g_h_iocp, 0, 0);

	SOCKET c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);
	char	accept_buf[sizeof(SOCKADDR_IN) * 2 + 32 + 100];
	EXP_OVER	accept_ex;

	*(reinterpret_cast<SOCKET*>(&accept_ex._net_buf)) = c_socket;
	::ZeroMemory(&accept_ex._wsa_over, sizeof(accept_ex._wsa_over));
	accept_ex._comp_op = OP_ACCEPT;

	AcceptEx(g_s_socket, c_socket, accept_buf, 0, sizeof(SOCKADDR_IN) + 16,
		sizeof(SOCKADDR_IN) + 16, NULL, &accept_ex._wsa_over);

	//cout << "Accept Called\n";

	for (int i = 0; i < MAX_USER; ++i) {
		clients[i]._id = i;
	}


	Enemys.reserve(4);
	for (int i = 0; i < 4; ++i)
	{
		Enemys.emplace_back(i);
	}

	vector<thread> worker_threads;

	for (int i = 0; i < 6; ++i)
		worker_threads.emplace_back(worker);


	char buf[256];
	int bufStart = 0;

	while (true) {
		//game loop
		memset(buf, 0, sizeof(buf));
		bufStart = 0;

		for (auto& En : Enemys) {
			En.update(buf, bufStart);
		}

		if (bufStart)
			for (auto& cl : clients) {
				cl.do_send(bufStart, buf);
			}
		SleepEx(1000, true);
	}

	for (auto& th : worker_threads)
	{
		th.join();
	}


	for (auto& cl : clients) {
		if (ST_INGAME == cl._state)
			Disconnect(cl._id);
	}
	closesocket(g_s_socket);
	WSACleanup();
}


