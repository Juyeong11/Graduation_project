#include"stdfx.h"
#include "Client.h"
#include "Network.h"
#include "Map.h"
#include "PathFinder.h"

int PathFinder::offsetX = 0;
int PathFinder::offsetZ = 0;
int PathFinder::LengthX = 0;
int PathFinder::LengthZ = 0;
int PathFinder::MAX_DEPTH = 1;
int PathFinder::MAX_LENGTH = 1;
int PathFinder::MAX_STEP = 1;

int PathFinder::cal_h(int end_x, int end_y, int next_x, int next_y) { //맨해튼 거리 사용

	float ex = end_x * 0.866f;
	float ez = end_x * 0.5f + end_y * 1.f;

	float nx = next_x * 0.866f;
	float nz = next_x * 0.5f + next_y * 1.f;
	float x = abs(ex - nx);
	float y = abs(ez - nz);
	return static_cast<int>((x + y) * 10);
}

void PathFinder::set_mapinfo(MapInfo* mapinfo) {
	offsetX = mapinfo->offsetX;
	offsetZ = mapinfo->offsetZ;
	LengthX = mapinfo->LengthX;
	LengthZ = mapinfo->LengthZ;

	MAX_DEPTH = ((LengthX < LengthZ) ? LengthZ : LengthX)/2;

	MAX_LENGTH  = MAX_DEPTH * 2;
	MAX_STEP  = MAX_LENGTH * MAX_LENGTH;
}

PathFinder::PathFinder(MapInfo* mapinfo) {
	if (offsetX == 0) {
		set_mapinfo(mapinfo);
	}
	p_map_data = mapinfo->map;
	nodes = new Node[MAX_LENGTH * MAX_LENGTH];

	Open.reserve(MAX_LENGTH * MAX_LENGTH);
}
PathFinder::~PathFinder() {
	delete[] nodes;
}
int PathFinder::find_path(GameObject* finder, const GameObject* const target) {
	int startX = finder->x;
	int startY = finder->y;
	int startZ = finder->z;
	int endX = finder->dest_x;
	int endY = finder->dest_y ;
	int endZ = finder->dest_z ;
	// 다른 스레드에서 좌표 값이 바뀌다 이상하게 읽혔으면 실행하지 말고 끝내자
	// endX가 -1이라면 종료하자 -> 플레이어가 어떤 이유에서 길찾기를 종료한 경우
	if ((startX + startY + startZ) != 0) return -1;
	if ((endX + endY + endZ) != 0) return -1;
	if (startX == endX && startZ == endZ) return -1;
	int cur_finder_x = startX - offsetX;
	int cur_finder_z = startZ - offsetZ;

	//memset(nodes, 0, sizeof(Node) * MAX_LENGTH * MAX_LENGTH);
	for (int i = -MAX_DEPTH; i < MAX_DEPTH; ++i) {
		for (int j = -MAX_DEPTH; j < MAX_DEPTH; ++j) {
			int x = cur_finder_x + i;
			int z = cur_finder_z + j;

			Node* n = &nodes[(j + MAX_DEPTH) * MAX_LENGTH + (i + MAX_DEPTH)];
			n->x = i + MAX_DEPTH;
			n->z = j + MAX_DEPTH;
			n->g = 1000;
			n->f = 0;
			if (x < 0 || z < 0) n->f = -1;
			else if (x > LengthX || z > LengthZ) n->f = -1;
			else if (p_map_data[z * LengthX + x] != 0)  n->f = -1;
		}
	}


	//가까운 오브젝트만 find_path를 호출함 따로 검사하지 않는다.

	int target_x = endX - offsetX -cur_finder_x;
	int target_z = endZ - offsetZ - cur_finder_z;
	

	Node end(MAX_DEPTH + target_z, MAX_DEPTH + target_x);

	//중점부터 탐색 시작
	Node* now = &nodes[MAX_LENGTH * MAX_DEPTH + MAX_DEPTH];
	Open.push_back(now);

	auto pre_time = std::chrono::system_clock::now();
	int step = 0;

	now->f = PathFinder::cal_h(end.x, end.z, MAX_DEPTH, MAX_DEPTH);// h+g -> g == 0
	now->g = 0;

	while (!Open.empty()) {
		auto cur = max_element(Open.cbegin(), Open.cend(), [&](Node* rhs, Node* lhs) {
			return rhs->f > lhs->f;
			});

		now = *cur;
		Open.erase(cur);
		if (*now == end) {
			//now = cur;
			break;
		}


		for (int i = 0; i < 6; ++i) {
			int nx = now->x + dx[i];
			int nz = now->z + dy[i];
			int new_g = now->g + PathFinder::cal_h(now->x, now->z, nx, nz);

			//막힌 길이면 pass
			if (nodes[MAX_LENGTH * nz + nx].f == -1) continue; 
			if (nodes[MAX_LENGTH * nz + nx].g < new_g) continue;
			//if (nodes[MAX_LENGTH * nz + nx].f == 1) continue;
			nodes[MAX_LENGTH * nz + nx].pre = now;
			nodes[MAX_LENGTH * nz + nx].f = new_g + PathFinder::cal_h(end.x,end.z,nx,nz);
			nodes[MAX_LENGTH * nz + nx].g = new_g; 
			//nodes[MAX_LENGTH * nz + nx].h = new_g;
			Open.push_back(&nodes[MAX_LENGTH * nz + nx]);
		}
	}

	Open.clear();
	if (now->pre == nullptr) return 1;
	if (step > MAX_STEP) {
		//reinterpret_cast<Npc*>(finder)->target_id = -1;
		std::cout << "can't find path" << std::endl;

		return 1;
	}
	std::cout << now->x << ", " << now->z << std::endl;
	while (now->pre->pre != nullptr) {
		now = now->pre;
		std::cout << now->x << ", " << now->z << std::endl;
	}
	finder->pre_x = finder->x;
	finder->pre_z = finder->z;
	finder->pre_y = finder->y;

	finder->x += now->x - MAX_DEPTH;
	finder->z += now->z - MAX_DEPTH;
	finder->y = -finder->x - finder->z;

	return 1;
}