#include"stdfx.h"
#include"Network.h"
#include"Map.h"

void MapInfo::SetMap(std::string file_name)
{
	std::ifstream in{ file_name, std::ios::binary };
	if (!in) return;

	in.read(reinterpret_cast<char*>(&LengthX), sizeof(LengthX));
	in.read(reinterpret_cast<char*>(&LengthZ), sizeof(LengthZ));

	in.read(reinterpret_cast<char*>(&offsetX), sizeof(offsetX));
	in.read(reinterpret_cast<char*>(&offsetZ), sizeof(offsetZ));



	map = new int[(LengthX+1) * LengthZ];

	in.read(reinterpret_cast<char*>(map), (LengthX + 1) * LengthZ * sizeof(int));

	/* // ¸Ê È®ÀÎ¿ë 
	std::cout << LengthX << std::endl;
	std::cout << LengthZ << std::endl;
	std::cout << offsetX << std::endl;
	std::cout << offsetZ << std::endl;
	for (int i = 0; i < LengthX* LengthZ; ++i) {

		std::cout<<std::setw(2) << map[i];
		if (i % LengthZ == 0) {
			std::cout << std::endl ;
			for (int j = 0; j < i / LengthZ; ++j)
				std::cout << " " ;
		}
	}
	*/
}

int MapInfo::GetTileType(int x, int z)
{
	int _x = x + offsetX;
	int _z = z + offsetZ;
	
	if (_x > LengthX || _x < 0) return -1;
	if (_z > LengthZ || _z < 0) return -1;

	return map[_x * LengthX + _z];
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
