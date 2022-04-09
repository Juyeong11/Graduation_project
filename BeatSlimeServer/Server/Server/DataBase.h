#pragma once

#include "Network.h"
void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode);

struct PlayerTableRow {
	SQLWCHAR p_name[MAX_NAME_SIZE];

	SQLINTEGER p_level, p_exp;
	SQLINTEGER p_x, p_y;
	SQLCHAR p_isUsing;


	SQLLEN cb_id = 0, cb_name = 0, cb_level = 0, cb_exp = 0;
	SQLLEN cb_x = 0, cb_y = 0, cb_using = 0;
};

class DataBase
{
public:
	PlayerTableRow PlayerSchema;

	SQLHENV henv;
	SQLHDBC hdbc;
	SQLHSTMT hstmt = 0;

	DataBase();
	~DataBase();

	void read_map_data();
	void checkPlayer(const char* name);
	void insertPlayer(const wchar_t* name);
	void update_map_data();
	
};

