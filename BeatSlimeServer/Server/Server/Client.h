#pragma once


enum COMP_OP { OP_RECV, OP_SEND, OP_ACCEPT, OP_ENEMY_MOVE, OP_ENEMY_ATTACK };

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
enum TYPE {
	PLAYER, ENEMY
};
enum DIR {
	UP, DOWN, LEFTUP, RIGHTUP, LEFTDOWN, RIGHTDOWN
};
enum STATE { ST_FREE, ST_ACCEPT, ST_INGAME };
class Gameobject {
public:
	char name[MAX_NAME_SIZE];
	int		id;
	short	x, y,z;
	int direction;
	char		type;
	//volatile해줘야 한다.
	volatile STATE state;
	std::mutex state_lock;


	int		last_move_time;
	Gameobject() :state(ST_FREE) {
		x = 0;
		y = 0;
		z = 0;
	}
};
class Npc: public Gameobject {
public:
	std::atomic_bool is_active;

	Npc() : is_active(false) {
		type = ENEMY;
	}
};


const int VIEW_RANGE = 5;// test를 위한 거리
const int ATTACK_RANGE = 2;// test를 위한 거리
class Client : public Gameobject
{
public:
	std::unordered_set<int> viewlist;
	std::mutex vl;

	EXP_OVER recv_over;
	SOCKET socket; // 재사용 하기 때문에 data race -> state로 보호

	int		prev_recv_size;
public:

	Client()
	{
		type = PLAYER;

		prev_recv_size = 0;
	}

	~Client()
	{
		closesocket(socket);
	}




	void do_recv();
	void do_send(EXP_OVER* ex_over);

};

