#pragma once
#include "protocol.h"
void error_display(const char* err_p, int err_no);
class EXP_OVER {
public:
	WSAOVERLAPPED	_wsa_over;
	COMP_OP			_comp_op;
	WSABUF			_wsa_buf;
	unsigned char	_net_buf[BUFSIZE];
public:
	EXP_OVER(COMP_OP comp_op, char num_bytes, void* mess) : _comp_op(comp_op)
	{
		ZeroMemory(&_wsa_over, sizeof(_wsa_over));
		_wsa_buf.buf = reinterpret_cast<char*>(_net_buf);
		_wsa_buf.len = num_bytes;
		memcpy(_net_buf, mess, num_bytes);
	}

	EXP_OVER(COMP_OP comp_op) : _comp_op(comp_op) {}

	EXP_OVER()
	{
		_comp_op = OP_RECV;
	}

	~EXP_OVER()
	{
	}

	void set_exp(COMP_OP comp_op, char num_bytes, void* mess)
	{
		_comp_op = comp_op;
		ZeroMemory(&_wsa_over, sizeof(_wsa_over));
		_wsa_buf.buf = reinterpret_cast<char*>(_net_buf);
		_wsa_buf.len = num_bytes;
		memcpy(_net_buf, mess, num_bytes);
	}
};

enum DIR {
	LEFTUP, UP, RIGHTUP, LEFTDOWN, DOWN, RIGHTDOWN
};
enum STATE { ST_FREE, ST_ACCEPT, ST_INGAME };
class Gameobject {
public:
	char name[MAX_NAME_SIZE];
	int		id;
	short	x, y,z;
	int direction;
	std::atomic_int hp;
	GO_TYPE		type;
	//volatile해줘야 한다.
	volatile STATE state;
	std::mutex state_lock;


	int		last_move_time;
	Gameobject() :state(ST_FREE) {
		x = 0;
		y = 0;
		z = 0;
		hp = 100;
	}
};
class Npc: public Gameobject {
public:
	std::atomic_bool is_active;

	Npc() : is_active(false) {
		
	}
};

class SkillTrader : public Npc {
public:
	std::atomic_bool is_talk;

	SkillTrader() : is_talk (false) {

	}
};

class Curator : public Npc {
public:
	std::atomic_bool is_talk;

	Curator() : is_talk(false) {

	}
};

class Witch : public Npc {
public:
	std::atomic_bool is_attack;
	//공격을 몇 초마다 한다? -> 일단 이걸로 하자
	//파일에서 읽어와서 한다?

	Witch() : is_attack(false) {

	}
};
class Boss2 : public Npc {
public:
	std::atomic_bool is_attack;

	Boss2() : is_attack(false) {

	}
};

const int VIEW_RANGE = 5;// test를 위한 거리
const int ATTACK_RANGE = 1;// test를 위한 거리
class Client : public Gameobject
{
public:
	std::unordered_set<int> viewlist;
	std::mutex vl;

	EXP_OVER recv_over;
	SOCKET socket; // 재사용 하기 때문에 data race -> state로 보호

	bool is_active;
	int cur_map_type;
	int		prev_recv_size;
public:

	Client()
	{
		type = PLAYER;
		is_active = true;
		prev_recv_size = 0;
		cur_map_type = 0;
	}

	~Client()
	{
		closesocket(socket);
	}




	void do_recv();
	void do_send(EXP_OVER* ex_over);

};

