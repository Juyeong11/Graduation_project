#pragma once



class Enemy {
	short x, y, z;
	bool isActive;
public:
	int _id;
	Enemy(int id) :x(0), y(0), z(0), _id(id) {
		isActive = false;
	}
	void PutObject(char* buf, int& buf_start);
	void update(char* buf, int& buf_start);


};