#include"stdfx.h"
#include"Client.h"	
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



	map = new int[(LengthX ) * LengthZ];

	in.read(reinterpret_cast<char*>(map), (LengthX ) * LengthZ * sizeof(int));
	
	for (int i = 0; i < LengthX * LengthZ; ++i) {
		if (map[i] == 1) {
			int _z = i / LengthX + offsetZ;
			int _x = i % LengthX + offsetX;
			startX[0] = _x;
			startZ[0] = _z;
		}
		else if (map[i] == 2) {
			int _z = i / LengthX + offsetZ;
			int _x = i % LengthX + offsetX;
			startX[1] = _x;
			startZ[1] = _z;
		}
		else if (map[i] == 3) {
			int _z = i / LengthX + offsetZ;
			int _x = i % LengthX + offsetX;
			startX[2] = _x;
			startZ[2] = _z;
		}
		else if (map[i] == 5 || map[i] == 6)
			map[i] = 0;
	}
	
	// 맵 확인용
	/*
	std::cout << LengthX << std::endl;
	std::cout << LengthZ << std::endl;
	std::cout << offsetX << std::endl;
	std::cout << offsetZ << std::endl;
	std::cout << " ";
	for (int i = 1; i <= LengthX* LengthZ ; ++i) {

		std::cout<<std::setw(2) << map[i-1];
		if (i % (LengthX) == 0) {
			std::cout << std::endl ;
			for (int j = 0; j <= i / (LengthX); ++j)
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
	num_totalPattern = pattern["noteType"].size();


	barCounts = 4;
	for (int i = 0; i < num_totalPattern; ++i) {
		if (999 == stoi(pattern["noteType"][i])) {
			bpm = stoi(pattern["pivotType"][i]);
			bossHP = stoi(pattern["direction"][i]);
			break;
		}

	}
	if (bpm == 0) {
		std::cout << "wrong bpm check pattern csv file \n";

	}
	if (bossHP == 0)
		bossHP = 1000;
	
	totalSongTime = 0; //(* 1000);
	nowSongTime = 0;
	timeByBeat = (int)(1000 * 60 / (float)bpm);
	timeByBar = timeByBeat * barCounts;
	timeBy24Beat = timeByBeat / 6;
	timeBy16Beat = timeByBeat / 4;

	pattern_time.reserve(num_totalPattern);
	int num_parrying=0;
	for (int i = 0; i < num_totalPattern; ++i) {
		int bar = stoi(pattern["bar"][i]);
		int addBeat = stoi(pattern["4th"][i]);
		int add16Beat = stoi(pattern["8th"][i]) * 2 + stoi(pattern["16th"][i]);
		int add24Beat =
			stoi(pattern["3rd"][i]) * 8 +
			stoi(pattern["6th"][i]) * 4 +
			stoi(pattern["12th"][i]) * 8 +
			stoi(pattern["24th"][i]);

		int noteType = stoi(pattern["noteType"][i]);
		if (noteType > 500 || noteType == 0 || noteType == 100 || noteType == 101 || noteType == 200) continue;
		
		int speed = 0;
		if (pattern["speed"][i] == "0") // 0
			speed = 0;
		else if (pattern["speed"][i] == "1") // 8beat
			speed = 0.5f * timeByBeat;
		else if (pattern["speed"][i] == "2") // 4beat
			speed = timeByBeat;
		else if (pattern["speed"][i] == "3") // 2beat
			speed = 2 * timeByBeat;
		else if (pattern["speed"][i] == "4") // 1bar
			speed = timeByBar;
		else {
			std::cout << "Load : wrong speed\n";
			continue;
		}
		// speed에 맞춰서 빼고
		// 
		int dir = 0;
		if (pattern["direction"][i] == "LU")
			dir = DIR::LEFTUP;
		else if (pattern["direction"][i] == "U")
			dir = DIR::UP;
		else if (pattern["direction"][i] == "RU")
			dir = DIR::RIGHTUP;
		else if (pattern["direction"][i] == "LD")
			dir = DIR::LEFTDOWN;
		else if (pattern["direction"][i] == "D")
			dir = DIR::DOWN;
		else if (pattern["direction"][i] == "RD")
			dir = DIR::RIGHTDOWN;
		else {
			//std::cout << "Load : wrong dir : "<< pattern["direction"][i]<< '\n';
			continue;
		}


		int pivotType = 0;
		if (pattern["pivotType"][i] == "PlayerM")
			pivotType = 0;
		else if (pattern["pivotType"][i] == "Playerm")
			pivotType = 1;
		else if (pattern["pivotType"][i] == "World")
			pivotType = 2;
		else if (pattern["pivotType"][i] == "Boss")
			pivotType = 3;
		else if (pattern["pivotType"][i] == "Player1")
			pivotType = 4;
		else if (pattern["pivotType"][i] == "Player2")
			pivotType = 5;
		else if (pattern["pivotType"][i] == "Player3")
			pivotType = 6;
		else if (pattern["pivotType"][i] == "PlayerF")
			pivotType = 7;
		else if (pattern["pivotType"][i] == "PlayerN")
			pivotType = 8;
		else if (pattern["pivotType"][i] == "PlayerA")
			pivotType = 9;
		else if (pattern["pivotType"][i] == "END")
			std::cout << std::endl << music_name + " - pattern csv load done.\n";
		else {
			std::cout << "Load : wrong pivotType\n";
			continue;
		}

		int px = stoi(pattern["pivotX"][i]);
		int py = stoi(pattern["pivotY"][i]);
		int pz = stoi(pattern["pivotZ"][i]);

		if (pivotType == 9) {
			for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
				pattern_time.emplace_back(
					noteType,
					4 + i,
					bar * timeByBar
					+ addBeat * timeByBeat
					+ add16Beat * timeBy16Beat
					+ add24Beat * timeBy24Beat + 70,
					dir,
					speed,
					px, py, pz);
			}
		}
		else
		{
			pattern_time.emplace_back(
				noteType,
				pivotType,
				bar * timeByBar
				+ addBeat * timeByBeat
				+ add16Beat * timeBy16Beat
				+ add24Beat * timeBy24Beat + 70,
				dir,
				speed,
				px, py, pz);
		}

		
			////패링공격이면 저장
			//if (stoi(pattern["noteType"][i]) == 10)
			//{
			//	int starttime = pattern_time.back().time;
			//	//int delay = pattern_time.back().speed;
			//	parrying_pattern.emplace(starttime, num_parrying++);
			//}
	}

	std::sort(pattern_time.begin(), pattern_time.end());
}

int MapInfo::GetTileType(int x, int z)
{
	int _x = x - offsetX;
	int _z = z - offsetZ;

	//std::cout << "x : " << _x << " z : " << _z << std::endl;
	if (_x > LengthX || _x < 0) return -1;
	if (_z > LengthZ || _z < 0) return -1;

	//10 -> 상점
	return map[_z * LengthX + _x];
}

void MapInfo::SetTileType(int x, int z, int pre_x, int pre_z)
{
	int p_x = pre_x - offsetX;
	int p_z = pre_z - offsetZ;
	// 이전 좌표는 범위 밖에 있을 수 없다.
	if (p_x > LengthX || p_x < 0) return;
	if (p_z > LengthZ || p_z < 0) return;

	map[p_z * LengthX + p_x] = 0;

	int _x = x - offsetX;
	int _z = z - offsetZ;

	//std::cout << "x : " << _x << " z : " << _z << std::endl;
	if (_x > LengthX || _x < 0) return ;
	if (_z > LengthZ || _z < 0) return ;

	map[_z * LengthX + _x] = -2;
}

GameRoom::GameRoom() :bpm(0) {
	isGaming = false;
	map_type = 0;
	ready_player_cnt = 0;
	game_room_id = -1;
	pattern_progress = -1;
}
GameRoom::GameRoom(int id) : game_room_id(id) {
	isGaming = false;
	map_type = 0;
	ready_player_cnt = 0;
	pattern_progress = -1;

}

int GameRoom::FindPlayer(int id) const
{
	int room_id=0;
	for (const auto p : player_ids) {
		if (p == nullptr) continue;
		if (p->id == id) return room_id;

	}
	return -1;
}
int GameRoom::FindPlayerID_by_GameRoom(int id) const
{
	int room_id = 0;
	for (int i = 0; i < MAX_IN_GAME_PLAYER;++i) {
		if (player_ids[i] == nullptr) continue;

		if (player_ids[i]->id == id) return i;

	}
	return -1;
}

void GameRoom::GameRoomInit(int mapType, float BPM, GameObject* Boss, GameObject* Players[MAX_IN_GAME_PLAYER],Portal* p)
{
	map_type = mapType;
	bpm = BPM;

	isGaming = true;

	boss_id = Boss;
	pattern_progress = 0;

	//memcpy_s(player_ids, MAX_IN_GAME_PLAYER * sizeof(int), Players, MAX_IN_GAME_PLAYER * sizeof(int));
	for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
		player_ids[i] = Players[i];
		if (player_ids[i] == nullptr) continue;

		player_ids[i]->cur_room_num = game_room_id;

	}
	portal = p;
}

int GameRoom::find_max_hp_player() const
{
	int maxhp = 0;
	int ret = 0;
	for (const auto pl : player_ids) {
		if (pl == nullptr) continue;
		if (pl->hp > maxhp) {
			maxhp = pl->hp;
			ret = pl->id;
		};
	}
	return ret;
}
int GameRoom::find_online_player() const
{
	for (const auto pl : player_ids) {
		if (pl == nullptr) continue;
		return pl->id;
	}
	return -1;
}

int GameRoom::find_min_hp_player() const {
	int minhp = 0xfffff;
	int ret = 0;
	for (const auto pl : player_ids) {
		if (pl == nullptr) continue;

		if (pl->hp < minhp) {
			minhp = pl->hp;
			ret = pl->id;
		};
	}
	return ret;
}
int GameRoom::find_max_distance_player() const {
	int max_distance = 0;
	int distance = 0;
	int ret_id;
	
	for (const auto pl : player_ids) {
		if (pl == nullptr) continue;

		distance = abs(boss_id->x - pl->x) + abs(boss_id->z - pl->z);
		if (distance > max_distance) {
			max_distance = distance;
			ret_id = pl->id;
		};
	}
	return ret_id;
}
int GameRoom::find_min_distance_player() const {
	int min_distance = 0xffff;
	int distance = 0;
	int ret_id;

	for (const auto pl : player_ids) {
		if (pl == nullptr) continue;

		distance = abs(boss_id->x - pl->x) + abs(boss_id->z - pl->z);
		if (distance < min_distance) {
			min_distance = distance;
			ret_id = pl->id;
		};
	}
	return ret_id;
}

int GameRoom::get_item_result() const
{
	
	return 10 - boss_id->hp / 100;
}
void GameRoom::set_player_portal_pos(int c_id) 
{

}
void GameRoom::game_end()
{
	isGaming = false;
	map_type = 0;
	ready_player_cnt = 0;
	//game_room_id = -1;

	boss_id->cur_room_num = -1;
	boss_id->state = ST_ACCEPT;
	boss_id = nullptr;
	pattern_progress = -1;
	//memcpy_s(player_ids, MAX_IN_GAME_PLAYER * sizeof(int), Players, MAX_IN_GAME_PLAYER * sizeof(int));
	
	for (auto p : player_ids) {
		if (p == nullptr) continue;

		p->power = 0;
		p->armour = 0;
		//reinterpret_cast<Client*>(p)->cur_room_num = -1;
		reinterpret_cast<Client*>(p)->is_active = true;
		p = nullptr;
	}
	//portal = nullptr;

}