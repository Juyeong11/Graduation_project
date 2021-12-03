#pragma once

#include "Network.h"
void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode);

struct MapTableRow {
	SQLINTEGER  p_id;
	SQLINTEGER  p_x, p_y, p_z, p_w;
	SQLINTEGER p_color, p_type;
	SQLLEN cb_id = 0, cb_x = 0, cb_y = 0, cb_z = 0, cb_w = 0, cb_color = 0, cb_type = 0;
};
struct Map{
	int id;
	int x, y, z, w;
	int color, type;

	bool operator==(const Map& rhs) const {
		if (id != rhs.id) return false;
		if (x != rhs.x) return false;
		if (y != rhs.y) return false;
		if (z != rhs.z) return false;
		if (w != rhs.w) return false;
		if (color != rhs.color) return false;
		if (type != rhs.type) return false;
		return true;
	}
};
class DataBase
{
public:
	MapTableRow MapSchema;
	std::vector<Map> db_map_data;
	std::map<int,Map> client_map_data;
	SQLHENV henv;
	SQLHDBC hdbc;
	SQLHSTMT hstmt = 0;

	DataBase();
	~DataBase();

	void read_map_data();
	void insert_map_data(const Map& shell);
	void update_map_data(const Map& shell);
	
};

