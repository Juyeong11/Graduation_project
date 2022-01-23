#include"stdfx.h"
#include"Network.h"
#include"Map.h"

void MapInfo::SetMap(std::string map_name, std::string music_name)
{
	std::ifstream in{ map_name, std::ios::binary };
	if (!in) return;

	in.read(reinterpret_cast<char*>(&LengthX), sizeof(LengthX));
	in.read(reinterpret_cast<char*>(&LengthZ), sizeof(LengthZ));

	in.read(reinterpret_cast<char*>(&offsetX), sizeof(offsetX));
	in.read(reinterpret_cast<char*>(&offsetZ), sizeof(offsetZ));



	map = new int[(LengthX + 1) * LengthZ];

	in.read(reinterpret_cast<char*>(map), (LengthX + 1) * LengthZ * sizeof(int));

	/* // 맵 확인용
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

	std::ifstream in_m{ music_name, std::ios::binary };
	if (!in_m) return;
	char tmp;
	// BOM 건너뛰기
	in_m.read(&tmp, sizeof(tmp));
	in_m.read(&tmp, sizeof(tmp));
	in_m.read(&tmp, sizeof(tmp));


	std::string buf;

	// 메뉴 읽고
	std::getline(in_m, buf);
	std::stringstream ss(buf);
	while (std::getline(ss, buf, ',')) {
		if (!buf.empty() && buf.back() == '\r') {
			menu.push_back(std::string(buf.begin(), buf.end() - 1)); // \r을 제거한 문자 
			break;
		}
		menu.push_back(buf);
	}

	// 내용 읽고
	while (std::getline(in_m, buf)) {
		std::stringstream ss(buf);
		int i = 0;
		while (std::getline(ss, buf, ',')) {
			if (i == 0 && buf == "0") break;// 주석은 넘어가고
			if (!buf.empty() && buf.back() == '\r') {
				std::string tmp(buf.begin(), buf.end() - 1);// \r을 제거한 문자 
				if (tmp.empty())
					pattern[menu[i]].push_back("0");
				else
					pattern[menu[i]].push_back(tmp);
				break;
			}
			if (buf.empty())
				pattern[menu[i]].push_back("0");
			else
				pattern[menu[i]].push_back(buf);

			i++;
		}
	}


	barCounts = 4;
	bpm = 124;
	totalSongTime = 0; //(* 1000);
	nowSongTime = 0;
	timeByBeat = (int)(1000 * 60 / (float)bpm);
	timeByBar = timeByBeat * barCounts;
	timeBy24Beat = timeByBeat / 6;
	timeBy16Beat = timeByBeat / 4;

	num_totalPattern = pattern["noteType"].size();
	pattern_time.reserve(num_totalPattern);

	for (int i = 0; i < num_totalPattern; ++i) {
		int bar = stoi(pattern["bar"][i]);
		int addBeat = stoi(pattern["4th"][i]);
		int add16Beat = stoi(pattern["8th"][i]) * 2 + stoi(pattern["16th"][i]);
		int add24Beat =
			stoi(pattern["3rd"][i]) * 8 +
			stoi(pattern["6th"][i]) * 4 +
			stoi(pattern["12th"][i]) * 8 +
			stoi(pattern["24th"][i]);

		pattern_time.push_back(std::make_pair(stoi(pattern["noteType"][i]),
			bar * timeByBar
			+ addBeat * timeByBeat
			+ add16Beat * timeBy16Beat
			+ add24Beat * timeBy24Beat));
	}
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
	memcpy_s(player_ids, MAX_IN_GAME_PLAYER * sizeof(int), Players, MAX_IN_GAME_PLAYER * sizeof(int));
}
