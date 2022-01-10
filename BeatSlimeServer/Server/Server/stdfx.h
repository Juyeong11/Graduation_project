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

enum MAP { FIELD_MAP, WITCH_MAP };
enum COMP_OP { OP_RECV, OP_SEND, OP_ACCEPT, OP_ENEMY_MOVE, OP_ENEMY_ATTACK };
const int BUFSIZE = 256;

