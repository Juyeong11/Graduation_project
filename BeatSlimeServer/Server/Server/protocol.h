#pragma once

const short SERVER_PORT = 4000;
const short CHAT_BUF_SIZE = 122;


const char CS_PACKET_LOGIN = 1;
const char CS_PACKET_MOVE = 2;
const char CS_PACKET_READ_MAP = 3;
const char CS_PACKET_WRITE_MAP = 4;
const char CS_PACKET_CHANGE_SCENE_READY = 5;
const char CS_PACKET_GAME_START_READY = 6;
const char CS_PACKET_PARRYING = 7;
const char CS_PACKET_CHANGE_SCENE_DONE = 8;
const char CS_PACKET_USE_SKILL = 9;
const char CS_PACKET_CHANGE_SKILL = 10;
const char CS_PACKET_PING_TEST = 11;
const char CS_PACKET_GET_SINGLE_PLAYER_LIST = 12;
const char CS_PACKET_PARTY_REQUEST = 13;
const char CS_PACKET_PARTY_REQUEST_ANWSER = 14;
const char CS_PACKET_CHAT = 15;


const char SC_PACKET_LOGIN_OK = 1;
const char SC_PACKET_MOVE = 2;
const char SC_PACKET_PUT_OBJECT = 3;
const char SC_PACKET_REMOVE_OBJECT = 4;
const char SC_PACKET_GAME_START = 5;
const char SC_PACKET_ATTACK = 6;
const char SC_PACKET_MAP_DATA = 7;
const char SC_PACKET_CHANGE_SCENE = 8;
const char SC_PACKET_EFFECT = 9;
const char SC_PACKET_GAME_END = 10;
const char SC_PACKET_PARRYING = 11;
const char SC_PACKET_GAME_INIT = 12;
const char SC_PACKET_CHANGE_SKILL = 13;
const char SC_PACKET_PING_TEST = 14;
const char SC_PACKET_PARTY_REQUEST = 15;
const char SC_PACKET_PARTY_REQUEST_ANWSER = 16;
const char SC_PACKET_CHAT = 17;

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
	int		move_time;
};

struct cs_packet_read_map {
	unsigned char size;
	char	type;
};

struct cs_packet_write_map {
	unsigned char size;
	char	type;
	int id;
	int x, y, z, w;
	int color, block_type;
};

struct cs_packet_change_scene_ready {
	unsigned char size;
	char	type;
	char is_ready;
};

struct cs_packet_change_scene_done {
	unsigned char size;
	char	type;
	char	scene_num;
};

struct cs_packet_game_start_ready {
	unsigned char size;
	char	type;
};

struct cs_packet_parrying {
	unsigned char size;
	char	type;
};

struct cs_packet_use_skill {
	unsigned char size;
	char	type;
};

struct cs_packet_change_skill {
	unsigned char size;
	char	type;
	char	skill_type;
};

struct cs_packet_ping_test {
	unsigned char size;
	char	type;
	int		ping_time; //
};

struct cs_packet_party_request {
	unsigned char size;
	char	type;
	int		id; //
};
struct cs_packet_party_request_anwser {
	unsigned char size;
	char	type;
	char	anwser; //
	int		requester; //
};

struct cs_packet_chat {
	unsigned char size;
	char	type;
	char	sendType;
	int		reciver;
	char	mess[CHAT_BUF_SIZE];
};

//server->client
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
	int dir;
	short  x, y, z;
	//int		move_time;
};

struct sc_packet_attack {
	unsigned char size;
	char type;
	int	id;
	int	target_id;
	char direction;
	int hp;
	//int		move_time;
};

struct sc_packet_put_object {
	unsigned char size;
	char type;
	int id;
	short x, y, z;
	char object_type;
	char direction;

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
	int game_start_time;

};
struct sc_packet_change_scene
{
	unsigned char size;
	char type;
	char scene_num;
};
struct sc_packet_game_init
{
	unsigned char size;
	char type;
	int player_id;
	int id1;
	int id2;
	int id3;
	int boss_id;
};

struct sc_packet_effect
{
	unsigned char size;
	char type;
	char effect_type;
	char dir;
	int id;
	int target_id;
	int charging_time;
	int x, y, z;
};
struct sc_packet_map_data
{
	unsigned char size;
	char type;
	char buf[256];
};

struct sc_packet_game_end
{
	unsigned char size;
	char type;
	char end_type;
};

struct sc_packet_parrying {
	unsigned char size;
	char	type;
	int id;
};

struct sc_packet_change_skill {
	unsigned char size;
	char	type;
	int		id;
	char	skill_type;
	char	skill_level;
};

struct sc_packet_ping_test {
	unsigned char size;
	char	type;
	int		ping_time; //
};

struct sc_packet_party_request {
	unsigned char size;
	char	type;
	int		requester_id; //
};
struct sc_packet_party_request_anwser {
	unsigned char size;
	char	type;
	char	anwser; //
	int		p_id; //

};

struct sc_packet_chat {
	unsigned char size;
	char	type;
	char	mess[CHAT_BUF_SIZE];
	int id;
};
#pragma pack(pop)
