#include"stdfx.h"
#include"Network.h"
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
	ready_player_cnt = 0;
	game_room_id = -1;
}
GameRoom::GameRoom(int id) : game_room_id(id) {
	isGaming = false;
	map_type = -1;
	ready_player_cnt = 0;
}

bool GameRoom::FindPlayer(int id)
{
	for (int p : player_ids) {
		if (p == id) return true;
	}
	return false;
}

void GameRoom::GameRoomInit(int mapType, float BPM, int Boss, int* Players)
{
	map_type = mapType;
	bpm = BPM;
	
	isGaming = true;

	boss_id = Boss;
	memcpy_s(player_ids,MAX_IN_GAME_PLAYER*sizeof(int), Players, MAX_IN_GAME_PLAYER * sizeof(int));
}
