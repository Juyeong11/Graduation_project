#pragma once
#include <iostream>
#include<assert.h>
#include<algorithm>
#include<array>
#include<vector>
#include<map>
#include<queue>
#include<format>
#include<string>
#include<string_view>
#include<fstream>
#include <regex>
#include<thread>
#include<mutex>
#include<chrono>
#include<unordered_set>
#include<unordered_map>
#include<concurrent_unordered_set.h>
#include<atomic>
#include<concurrent_priority_queue.h> 
#include<concurrent_queue.h>


#include <WS2tcpip.h>
#include <MSWSock.h>
#include <windows.h> 
#include <sqlext.h>


#pragma comment (lib, "WS2_32.LIB")
#pragma comment (lib, "MSWSock.LIB")

enum MAP_TYPE { FIELD_MAP,TUTORI_MAP,WITCH_MAP, WITCH_MAP_HARD, ROBOT_MAP };

enum COMP_OP {
	OP_RECV, OP_SEND, OP_ACCEPT, OP_BOSS_MOVE, OP_PLAYER_PARRYING,
	OP_BOSS_TILE_ATTACK,
	OP_BOSS_TILE_ATTACK_START,
	OP_GAME_END,
	OP_GAME_ROOM_READY,
	OP_PLAYER_SKILL,
	OP_PLAYER_MOVE,
	OP_DB_PLAYER_LOGIN,
	OP_DB_INVENTORY,
	OP_DB_USE_ITEM
};
enum GO_TYPE {PLAYER, WITCH, BOSS2, SKILL_TRADER, CURATOR};

enum PIVOTTYPE { PlayerM, Playerm, World, Boss, Player1, Player2, Player3,PlayerF,PlayerN};

enum DIR {
	LEFTUP, UP, RIGHTUP, LEFTDOWN, DOWN, RIGHTDOWN
};
extern void error_display(const char* err_p, int err_no);
enum SKILL_TYPE { WATERGUN, QUAKE, HEAL };
#define DEBUG

const int BUFSIZE = 256;



const int WORLD_HEIGHT = 8;
const int WORLD_WIDTH = 8;

const int MAX_USER = 5000;
const int MAX_SKILL_TRADER = 0;
const int MAX_CURATOR = 0;
const int MAX_WITCH = 5000;
const int MAX_BOSS2 = 1000;
const int MAX_NPC = MAX_SKILL_TRADER + MAX_CURATOR + MAX_WITCH + MAX_BOSS2;

constexpr int NPC_ID_START = MAX_USER;
constexpr int NPC_ID_END = MAX_USER + MAX_NPC;
constexpr int MAX_OBJECT = MAX_USER + MAX_NPC;

constexpr int SKILL_TRADER_ID_START = MAX_USER;
constexpr int SKILL_TRADER_ID_END = SKILL_TRADER_ID_START + MAX_SKILL_TRADER;

constexpr int CURATOR_ID_START = SKILL_TRADER_ID_END;
constexpr int CURATOR_ID_END = CURATOR_ID_START + MAX_CURATOR;

constexpr int WITCH_ID_START = CURATOR_ID_END;
constexpr int WITCH_ID_END = WITCH_ID_START + MAX_WITCH;

constexpr int BOSS2_ID_START = WITCH_ID_END;
constexpr int BOSS2_ID_END = BOSS2_ID_START + MAX_BOSS2;

const int MAP_NUM = 5;
const int SKILL_CNT = 4*3;
const int PORTAL_NUM = 3;
const int MAX_GAME_ROOM_NUM = 5000;

enum GAME_END_TYPE{GAME_OVER,GAME_CLEAR};

struct PlayerData {
	std::wstring name;
	int x, z;
	int curSkill;


	char SkillAD;
	char SkillTa;
	char SkillHeal;

	char ClearMap[2];

	int MMR;

	int money;
	int MusicScroll;
	int MusicScrollCount;
};

/*
* -600 -> 게임 끝
* 99 -> 단일 타일공격
* -1 -> 움직임
*/