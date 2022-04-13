#pragma once



class EXP_OVER {
public:
	WSAOVERLAPPED	_wsa_over;
	COMP_OP			_comp_op;
	WSABUF			_wsa_buf;
	unsigned char	_net_buf[BUFSIZE];
public:
	EXP_OVER(COMP_OP comp_op, unsigned char num_bytes, void* mess);

	EXP_OVER(COMP_OP comp_op);

	EXP_OVER();

	~EXP_OVER() = default;

	void set_exp(COMP_OP comp_op, unsigned char num_bytes, void* mess);
};


enum STATE { ST_FREE, ST_ACCEPT, ST_INGAME };
class GameObject {
public:
	char name[MAX_NAME_SIZE];
	int		id;
	short	x, y, z;
	short	pre_x, pre_y, pre_z;
	short	dest_x, dest_y, dest_z;
	int direction;
	std::atomic_int hp;
	GO_TYPE		type;
	//volatile해줘야 한다.
	volatile STATE state;
	std::mutex state_lock;
	int cur_room_num;


	int		last_packet_time;
	std::chrono::system_clock::time_point last_move_time;
	GameObject();
	virtual ~GameObject() {};
	virtual int GetMoney() { return 0; };
	virtual void SetMoney(int m) {  };
	virtual bool Hit(int damage);
};
class Npc : public GameObject {
public:
	std::atomic_bool is_active;

	Npc();
};

class SkillTrader : public Npc {
public:
	std::atomic_bool is_talk;

	SkillTrader();
};

class Curator : public Npc {
public:
	std::atomic_bool is_talk;

	Curator();
};

class Witch : public Npc {
public:
	std::atomic_bool is_attack;
	//공격을 몇 초마다 한다? -> 일단 이걸로 하자
	//파일에서 읽어와서 한다?

	Witch();
};
class Boss2 : public Npc {
public:
	std::atomic_bool is_attack;

	Boss2();
};

class Inventory {
public:
	//Item* items[MAX_IN_GAME_PLAYER];
	//int curPlayerNum;
	//Inventory();

};


struct Item {
	int itemType;
	int itemPrice;
};
class Skill
{
public:
	int Delay;
	int CoolTime;
	int Speed;
	int Damage;
	int SkillLevel;
	int SkillType;
	Skill(int sl, int st);
	Skill();

};
class Party {
public:
	int partyPlayer[MAX_IN_GAME_PLAYER];
	std::mutex partyLock;
	int curPlayerNum;
	Party();
	bool SetPartyPlayer(int id);
	bool DelPartyPlayer(int id);
	int DelParty();
};
const int VIEW_RANGE = 20;// test를 위한 거리
const int ATTACK_RANGE = 1;// test를 위한 거리

class PathFinder;
class MapInfo;
class Client : public GameObject
{
public:
	std::unordered_set<int> viewlist;
	std::mutex vl;

	EXP_OVER recv_over;
	SOCKET socket; // 재사용 하기 때문에 data race -> state로 보호

	Skill* skill;
	bool is_active;
	int		prev_recv_size;

	int		money;

	Party* party;

	PathFinder* Astar = nullptr;

	long long pre_parrying_pattern;
public:

	Client(Skill* sk, MapInfo* mapdata);
	~Client();

	void do_recv();
	void do_send(EXP_OVER* ex_over);
	virtual int GetMoney();
	virtual void SetMoney(int money);
};

