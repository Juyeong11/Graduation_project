#pragma once
#include <iostream>
#include<assert.h>
#include<algorithm>
#include<array>
#include<vector>
#include<map>
#include<format>
#include<string>
#include<string_view>
#include<fstream>

#include<thread>
#include<mutex>
#include<chrono>
#include<unordered_set>
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

enum MAP_TYPE { FIELD_MAP, WITCH_MAP };
enum COMP_OP { OP_RECV, OP_SEND, OP_ACCEPT, OP_ENEMY_MOVE, OP_ENEMY_ATTACK };
enum GO_TYPE {PLAYER, WITCH, BOSS2, SKILL_TRADER, CURATOR};
const int BUFSIZE = 256;

const int MAX_IN_GAME_PLAYER = 1;

const int WORLD_HEIGHT = 8;
const int WORLD_WIDTH = 8;
const int MAX_NAME_SIZE = 20;
const int MAX_USER = 5000;
const int MAX_SKILL_TRADER = 0;
const int MAX_CURATOR = 0;
const int MAX_WITCH = 5;
const int MAX_BOSS2 = 5;
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

const int MAP_NUM = 2;
const int PORTAL_NUM = 1;
const int MAX_GAME_ROOM_NUM = 5;