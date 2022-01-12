#pragma once



class MapInfo {
	int length;
	int halfLength;
	int size;
public:
	int* map;
	int bpm;

	void SetMap(std::string file_name);
	int GetTileType(int x, int z);

	~MapInfo()
	{
		delete[] map;
	}
	/*
	int half = 19 / 2;
	for (int x = -half; x <= half; ++x)
	{
		for (int z = -half; z <= half; ++z)
		{
			std::cout << maps[FIELD_MAP].GetTileType(x, z) << std::endl;
		}
	}*/
};

class Gameobject;
class GameRoom
{
public:
	int map_type;
	float bpm;
	std::chrono::system_clock::time_point start_time{};
	// atomic으로는 동시에 게임 시작하는걸 막을 수가 없을 듯
	std::mutex state_lock;
	bool isGaming;
	int player_ids[MAX_IN_GAME_PLAYER];
	std::mutex ready_lock;
	int ready_player_cnt;
	//std::mutex id_insert_lock;

	int boss_id;
	GameRoom();

	void GameRoomInit(int mapType, float BPM, int Boss, int* Players);
	bool FindPlayer(int id);
};

class Portal
{
public:
	int x, y, z;
	const int range = 1;
	MAP_TYPE map_type;
	std::unordered_set<int> player_ids;
	std::mutex id_lock;
	std::atomic_int ready_player_cnt;
	Portal(int _x, int _z) :x(_x), z(_z) {
		y = -z - x; map_type = WITCH_MAP;
		ready_player_cnt = 0;
	};

	bool isPortal(int _x, int _z)
	{
		if (range < abs(_x - x)) return false;
		if (range < abs(_z - z)) return false;

		return true;
	}

	bool findPlayer(int id) {
		id_lock.lock();
		if (player_ids.contains(id)) {
			id_lock.unlock();
			return true;
		}
		id_lock.unlock();
		return false;
	}
};