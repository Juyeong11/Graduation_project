#pragma once

const short SERVER_PORT = 4000;

const int  WORLD_HEIGHT = 8;
const int  WORLD_WIDTH = 8;
const int  MAX_NAME_SIZE = 20;
const int  MAX_USER = 5000;
const int MAX_NPC = 4;
constexpr int NPC_ID_START = MAX_USER;
constexpr int NPC_ID_END = MAX_USER + MAX_NPC - 1;
constexpr int MAX_OBJECT = MAX_USER + MAX_NPC;

const char CS_PACKET_LOGIN = 1;
const char CS_PACKET_MOVE = 2;
const char CS_PACKET_READ_MAP = 3;

const char SC_PACKET_LOGIN_OK = 1;
const char SC_PACKET_MOVE = 2;
const char SC_PACKET_PUT_OBJECT = 3;
const char SC_PACKET_REMOVE_OBJECT = 4;
const char SC_PACKET_GAME_START = 5;
const char SC_PACKET_ATTACK = 6;
const char SC_PACKET_MAP_DATA = 7;

#pragma pack (push, 1)
//client -> server
struct cs_packet_login {
	unsigned char size;
	char	type;
	//char	name[MAX_NAME_SIZE];
};

struct cs_packet_move {
	unsigned char size;
	char	type;
	char	direction;			// 0 : up,  1: down, 2:left, 3:right
	//int		move_time;
};

struct cs_packet_read_map {
	unsigned char size;
	char	type;
};

//server->client
struct sc_packet_login_ok {
	unsigned char size;
	char type;
	int		id;
	short	x, y,z;
};

struct sc_packet_move {
	unsigned char size;
	char type;
	int		id;
	short  x, y,z;
	//int		move_time;
};

struct sc_packet_attack {
	unsigned char size;
	char type;
	int	id;
	int	target_id;
	char direction;
	//int		move_time;
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

struct sc_packet_game_start
{
	unsigned char size;
	char type;
};

struct sc_packet_map_data
{
	unsigned char size;
	char type;
	char buf[BUFSIZE/2];
};

#pragma pack(pop)
