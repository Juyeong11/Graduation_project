#pragma once

#include "Network.h"
void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode);

struct MapTableRow {
	SQLINTEGER  p_x, p_y, p_z, p_w;
	SQLINTEGER p_color, p_type;
	SQLLEN cb_x = 0, cb_y = 0, cb_z = 0, cb_w = 0, cb_color = 0, cb_type = 0;
};
struct Map{
	int x, y, z, w;
	int color, type;

};
class DataBase
{
public:
	MapTableRow MapSchema;
	std::vector<Map> map_data;
	SQLHENV henv;
	SQLHDBC hdbc;
	SQLHSTMT hstmt = 0;

	DataBase();
	~DataBase();

	void read_map_data();
	
};

