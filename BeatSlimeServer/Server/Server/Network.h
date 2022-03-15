#pragma once



#include"protocol.h"
#include"Client.h"


enum EVENT_TYPE {
	EVENT_BOSS_MOVE, EVENT_PLAYER_PARRYING,
	EVENT_BOSS_TILE_ATTACK,
	EVENT_GAME_END,
	EVENT_PLAYER_SKILL
};
enum PATTERN_TYPE { ONE_LINE, SIX_LINE, AROUND };
struct timer_event {
	int obj_id;
	int target_id;
	int game_room_id;
	int x, y, z;
	int type;
	int charging_time;
	int pivotType;
	std::chrono::system_clock::time_point start_time;
	EVENT_TYPE ev;
	//int target_id;

	constexpr bool operator <(const timer_event& left)const
	{
		return (start_time > left.start_time);
	}
};


class DataBase;
class GameRoom;
class MapInfo;
class Portal;
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
	void send_attack_player(int attacker, int target_id, int receiver);
	void send_put_object(int client_id, int target_id);
	void send_remove_object(int client_id, int victim_id);
	void send_map_data(int client_id, char* data, int nShell);
	void send_change_scene(int client_id, int map_type);
	void send_game_start(int client_id,int start_time);
	void send_game_init(int client_id, GameObject* ids[3], int boss_id);
	void send_game_end(int client_id,char end_type);
	void send_parrying(int client_id,int actor_id);
	void send_change_skill(int client_id,int actor_id);
	void send_ping(int client_id,int time);

	void send_effect(int client_id, int actor_id, int target_id, int effect_type, int charging_time,int dir, int x, int y, int z);
	void disconnect_client(int client_id);

	bool is_near(int a, int b)
	{
		if (VIEW_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
		if (VIEW_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

		return true;
	}
	bool is_attack(int a, int b)
	{
		if (ATTACK_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
		if (ATTACK_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

		return true;
	}
	bool is_attack(int a, int x, int z)
	{
		if (abs(clients[a]->x != x)) return false;
		if (abs(clients[a]->z != z)) return false;

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
	int get_npc_id(int monsterType);

	int get_game_room_id();
	void Initialize_NPC() {
		for (int i = NPC_ID_START; i < NPC_ID_END; ++i) {
			sprintf_s(clients[i]->name, "NPC%d", i);
			clients[i]->x = 0;
			clients[i]->z = 0;
			clients[i]->id = i;
			clients[i]->state = ST_ACCEPT;
			clients[i]->direction = DOWN;
			clients[i]->type = WITCH;

		}
	}
	void do_npc_move(int npc_id);
	void do_npc_attack(int npc_id, int target_id, int receiver);
	void do_npc_tile_attack(int game_room_id,int x,int y, int z);
	void do_player_skill(GameRoom* gr, Client* cl);
	void do_timer();
	void process_packet(int client_id, unsigned char* p);

	void worker();

	void game_start(int room_id);

	void set_next_pattern(int room_id);

	void check_game_over(int game_room_id, int dead_charactor_id) {
		// 게임 오버 패킷을 보내고

	}

	void check_game_clear(int hp) {
		// 게임  패킷을 보내고

	}
private:
	concurrency::concurrent_priority_queue<timer_event> timer_queue;
	concurrency::concurrent_queue<EXP_OVER*> exp_over_pool;
	std::array<GameObject*, MAX_OBJECT> clients;// 200, 200 맵을 존으로 나누어 뷰 리스트 제작할 것
	EXP_OVER accept_ex;

private:
	std::array<GameRoom*, MAX_GAME_ROOM_NUM> game_room;
	std::array<MapInfo*, MAP_NUM> maps;
	std::array<Portal*, PORTAL_NUM> portals;

	std::array<Skill*, SKILL_CNT> skills;
};

