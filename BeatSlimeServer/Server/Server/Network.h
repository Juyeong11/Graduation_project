#pragma once





enum EVENT_TYPE {
	EVENT_BOSS_MOVE, EVENT_PLAYER_PARRYING,
	EVENT_BOSS_TILE_ATTACK,
	EVENT_GAME_END,
	EVENT_PLAYER_SKILL,
	EVENT_PALYER_MOVE
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
	char dir;
	std::chrono::system_clock::time_point start_time;
	EVENT_TYPE ev;
	//int target_id;

	constexpr bool operator <(const timer_event& left)const
	{
		return (start_time > left.start_time);
	}
};
enum DB_EVENT_TYPE {
	DB_PLAYER_LOGIN, DB_PLAYER_LOGOUT,
	DB_READ_INVENTORY, DB_READ_CLAER_MAP_INFO,
	DB_UPDATE_CLEAR_MAP,DB_UPDATE_PLAYER_DATA
};
struct db_event {
	int obj_id;
	DB_EVENT_TYPE ev;
};


class DataBase;
class GameRoom;
class MapInfo;
class Portal;
class GameObject;
class Client;
class EXP_OVER;
class Skill;
class Party;
struct Item;
class Inventory;
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


	void start_accept();

	void send_login_ok(int client_id);
	void send_login_fail(int client_id);
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
	void send_party_request(int reciver,int sender);
	void send_party_request_anwser(int reciver,int newPlayerid,Party* party,int type);
	void send_chat_packet(int user_id, int my_id, char* mess);
	void send_buy_result(int user_id, int itemType, char result);

	void send_effect(int client_id, int actor_id, int target_id, int effect_type, int charging_time,int dir, int x, int y, int z);
	void disconnect_client(int client_id);

	bool is_near(int a, int b);
	bool is_attack(int a, int b);
	bool is_attack(int a, int x, int z);
	bool is_npc(int id)
	{
		return (id >= NPC_ID_START) && (id <= NPC_ID_END);
	}
	bool is_player(int id)
	{
		return (id >= 0) && (id < MAX_USER);
	}
	bool is_item(int id)
	{
		return (id >= 0) && (id < 9);
	}
	int get_new_id();
	int get_npc_id(int monsterType);

	int get_game_room_id();
	int set_new_player_pos(int client_id);
	void Initialize_NPC();
	void do_npc_move(int npc_id);
	void do_npc_attack(int npc_id, int target_id, int receiver);
	void do_npc_tile_attack(int game_room_id,int x,int y, int z);
	void do_player_skill(GameRoom* gr, Client* cl);
	void do_timer();
	void do_DBevent();
	void process_packet(int client_id, unsigned char* p);

	void worker();

	void game_start(int room_id);

	void set_next_pattern(int room_id);

	void input_db_event(int c_id,DB_EVENT_TYPE type);
	//수정
	std::array<MapInfo*, MAP_NUM>& get_map() { return maps; }

private:
	concurrency::concurrent_priority_queue<timer_event> timer_queue;
	concurrency::concurrent_queue<db_event> db_event_queue;
	concurrency::concurrent_queue<EXP_OVER*> exp_over_pool;
	std::array<GameObject*, MAX_OBJECT> clients;// 200, 200 맵을 존으로 나누어 뷰 리스트 제작할 것
	EXP_OVER* accept_ex;

private:
	std::array<GameRoom*, MAX_GAME_ROOM_NUM> game_room;
	std::array<MapInfo*, MAP_NUM> maps;
	std::array<Portal*, PORTAL_NUM> portals;

	std::array<Skill*, SKILL_CNT> skills;
	std::array<Party*, MAX_USER/2> PartyPool;
	std::array<Inventory*, MAX_USER> inventorys;
};

