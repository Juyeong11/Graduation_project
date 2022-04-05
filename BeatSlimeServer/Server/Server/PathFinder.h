#pragma once

class Node {
public:
	int x, z;
	int f, g;

	Node* pre;
	Node() {
		g = f = 0;
		g = 1000;
		x = z = 0;
		pre = nullptr;
	}
	Node(int _z, int _x) {
		x = _x;
		z = _z;
		pre = nullptr;
	}

	bool operator==(const Node& rhs) {
		if (rhs.x != x) return false;
		if (rhs.z != z) return false;
		return true;
	}

	bool operator<(const Node& rhs) {
		
		return g<rhs.g;
	}

};
typedef std::pair<int, Node*> fweight;

const int dx[6] = { 1,-1, 0,0,-1,1};
const int dy[6] = { 0, 0,-1,1,1,-1 };



class GameObject;
class MapInfo;
class PathFinder
{
	Node* nodes = nullptr;
	static int offsetX;
	static int offsetZ;
	static int LengthX;
	static int LengthZ;

	static int MAX_DEPTH;
	static int MAX_LENGTH;
	static int MAX_STEP;

	std::vector<Node*> Open;
	int* p_map_data;
public:
	PathFinder(MapInfo* _p_map_data);
	~PathFinder();

	int find_path(GameObject* finder, const GameObject* const target);
	static int cal_h(int end_x, int end_y, int next_x, int next_y);
	static void set_mapinfo(MapInfo* mapinfo);
};

