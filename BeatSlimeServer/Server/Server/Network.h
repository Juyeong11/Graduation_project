#pragma once

#include <WS2tcpip.h>
#include <MSWSock.h>
#include <windows.h> 
#include <sqlext.h>

#pragma comment (lib, "WS2_32.LIB")
#pragma comment (lib, "MSWSock.LIB")

#include"protocol.h"
#include"Client.h"



void error_display(const char* err_p,int err_no);

enum EVENT_TYPE { EVENT_ENEMY_MOVE, EVENT_ENEMY_ATTACK };
struct timer_event {
	int obj_id;
	int target_id;
	std::chrono::system_clock::time_point start_time;
	EVENT_TYPE ev;
	//int target_id;

	constexpr bool operator <(const timer_event& left)const
	{
		return (start_time > left.start_time);
	}
};


class DataBase;
class Network
{
private:
	static Network* instance;
public:
	static Network* GetInstance();
	HANDLE g_h_iocp;
	SOCKET g_s_socket;
	DataBase* DB;
	Network();
	~Network();


	void start_accept() {
		SOCKET c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

		*(reinterpret_cast<SOCKET*>(accept_ex._net_buf)) = c_socket;

		ZeroMemory(&accept_ex._wsa_over, sizeof(accept_ex._wsa_over));
		accept_ex._comp_op = OP_ACCEPT;

		AcceptEx(g_s_socket, c_socket, accept_ex._net_buf + sizeof(SOCKET), 0, sizeof(SOCKADDR_IN) + 16,
			sizeof(SOCKADDR_IN) + 16, NULL, &accept_ex._wsa_over);
	}

	void send_login_ok(int client_id);
	void send_move_object(int client_id, int mover_id);
	void send_attack_player(int client_id, int target_id);
	void send_put_object(int client_id, int target_id);
	void send_remove_object(int client_id, int victim_id);
	void send_map_data(int client_id,char* data, int nShell);
	void disconnect_client(int client_id);

	bool is_near(int a, int b)
	{
		if (VIEW_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
		if (VIEW_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

		return true;
	}
	bool can_attack(int a, int b)
	{
		if (ATTACK_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
		if (ATTACK_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

		return true;
	}
	bool is_npc(int id)
	{
		return (id >= NPC_ID_START) && (id <= NPC_ID_END);
	}
	bool is_player(int id)
	{
		return (id >= 0) && (id < MAX_USER);
	}
	int get_new_id();

	void Initialize_NPC() {
		for (int i = NPC_ID_START; i <= NPC_ID_END; ++i) {
			sprintf_s(clients[i]->name, "NPC%d", i);
			clients[i]->x = rand() % WORLD_WIDTH;
			clients[i]->z = rand() % WORLD_HEIGHT;
			clients[i]->id = i;
			clients[i]->state = ST_INGAME;
			clients[i]->type = ENEMY; // NPC
		}
	}
	void do_npc_move(int npc_id);
	void do_npc_attack(int npc_id,int target_id);

	void do_timer() {
		using namespace std;
		using namespace chrono;
		while (true) {

			timer_event ev;
			while (!timer_queue.empty()) {
				
				timer_queue.try_pop(ev);
					
				if (ev.start_time <= system_clock::now()) {
					//이벤트 시작
					EXP_OVER* ex_over;// = new EXP_OVER;
					//ex_over->_comp_op = OP_NPC_MOVE;
					while (!exp_over_pool.try_pop(ex_over));
					switch (ev.ev)
					{
					case EVENT_ENEMY_MOVE:
						ex_over->_comp_op = OP_ENEMY_MOVE;
						break;
					case EVENT_ENEMY_ATTACK:
						ex_over->_comp_op = OP_ENEMY_ATTACK;
						*reinterpret_cast<int*>(ex_over->_net_buf)= ev.target_id;
						break;
					default:
						break;
					}
					
					PostQueuedCompletionStatus(g_h_iocp, 1, ev.obj_id, &ex_over->_wsa_over);// 두번째 인자가 0이 되면 소캣 종료로 취급이 된다. 1로해주자
				}
				else {
					//기껏 뺐는데 다시 넣는거 좀 비효율 적이다.
					//다시 넣지 않는 방법으로 최적화 필요
					//모르겠다..
					timer_queue.push(ev);
					break;
				}
			}
			
			//큐가 비었거나
			this_thread::sleep_for(10ms);
		}
	}
	void process_packet(int client_id, unsigned char* p);

	void worker();
private:
	concurrency::concurrent_priority_queue<timer_event> timer_queue;
	concurrency::concurrent_queue<EXP_OVER*> exp_over_pool;
	std::array<Gameobject*, MAX_OBJECT> clients;//흠..
	EXP_OVER accept_ex;
};

