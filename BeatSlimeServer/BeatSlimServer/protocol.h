#pragma once

const int WORLD_HEIGHT = 8;
const int WORLD_WIDTH = 8;
const int  MAX_NAME_SIZE = 20;
const int  MAX_USER = 10;

const char CS_PACKET_LOGIN = 1;
const char CS_PACKET_MOVE = 2;
const char CS_PACKET_TIMER = 3;

const char SC_PACKET_LOGIN_OK = 1;
const char SC_PACKET_MOVE = 2;
const char SC_PACKET_PUT_OBJECT = 3;
const char SC_PACKET_REMOVE_OBJECT = 4;
const char SC_PACKET_MOVE_OBJECT = 5;
const char SC_PACKET_TIMER = 6;

const short SERVER_PORT = 4000;

enum DIR {
	UP, DOWN, LEFTUP, RIGHTUP, LEFTDOWN, RIGHTDOWN
};

enum OBJECT_TYPE
{
	PLAPER, ENEMY
};
#pragma pack (push, 1)
// Client -> Server
struct cs_packet_login {
	unsigned char size;
	char	type;
	//char	name[MAX_NAME_SIZE];
};

struct cs_packet_move {
	unsigned char size;
	char	type;
	char	direction;
};

struct cs_packet_timer
{
	unsigned char size;
	char type;
	double timestamp;
};
// Server -> Client
struct sc_packet_login_ok {
	unsigned char size;
	char type;
	int		id;
	short	x, y, z;
};

struct sc_packet_move {
	unsigned char size;
	char type;
	int		id;
	short  x, y, z;
};
struct sc_packet_move_object {
	unsigned char size;
	char type;
	int		id;
	short  x, y, z;
};

struct sc_packet_put_object {
	unsigned char size;
	char type;
	int id;
	short x, y,z;
	char object_type;
	//char	name[MAX_NAME_SIZE];
};

struct sc_packet_remove_object {
	unsigned char size;
	char type;
	int id;
};

struct sc_packet_timer
{
	unsigned char size;
	char type;
	double timestamp;
};
#pragma pack(pop)
