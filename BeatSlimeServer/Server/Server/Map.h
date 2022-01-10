#pragma once



class MapInfo {
	int length;
	int halfLength;
	int size;
public:
	int* map;
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
	bool isGaming;

	Gameobject* boss;
	Gameobject** players;
	GameRoom();

	void GameStart(int mapType, float BPM, Gameobject* Boss, Gameobject** Players);
};

class Portal
{
public:
	int x, y, z;
	const int range = 1;
	int map_type;
	std::unordered_set<int> player_ids;
	std::mutex id_lock;
	Portal(int _x, int _z) :x(_x), z(_z) { y = -z - x; map_type = WITCH_MAP; };

	bool isPortal(int _x, int _z)
	{
		if (range < abs(_x - x)) return false;
		if (range < abs(_z - z)) return false;

		return true;
	}

	bool findPlayer(int id) {

	}
};