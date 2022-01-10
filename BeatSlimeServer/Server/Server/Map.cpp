#include"stdfx.h"
#include"Client.h"
#include"Map.h"

void MapInfo::SetMap(std::string file_name)
{
	std::ifstream in{ file_name, std::ios::binary };
	if (!in) return;

	in.read(reinterpret_cast<char*>(&length), sizeof(length));

	size = length * length;
	map = new int[size];
	halfLength = length / 2;
	in.read(reinterpret_cast<char*>(map), size * sizeof(int));
}

int MapInfo::GetTileType(int x, int z)
{
	int _x = x + halfLength;
	int _z = z + halfLength;
	if (_x > length || _x < 0) return -1;
	if (_z > length || _z < 0) return -1;

	return map[_x * length + _z];
}

GameRoom::GameRoom() :bpm(0) {
	isGaming = false;
	map_type = -1;
}

void GameRoom::GameStart(int mapType, float BPM, Gameobject* Boss, Gameobject** Players)
{
	map_type = mapType;
	bpm = BPM;
	start_time = std::chrono::system_clock::now();
	isGaming = true;

	boss = Boss;
	players = Players;
}