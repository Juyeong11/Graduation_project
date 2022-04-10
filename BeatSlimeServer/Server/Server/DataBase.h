#pragma once

#include "Network.h"
void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode);

struct PlayerTableRow {
	SQLWCHAR p_name[MAX_NAME_SIZE];

	SQLINTEGER p_x, p_z;
	SQLINTEGER p_money;
	SQLCHAR p_isUsing;


	SQLLEN cb_id = 0, cb_name = 0, cb_money = 0, cb_exp = 0;
	SQLLEN cb_x = 0, cb_z = 0, cb_using = 0;
};
struct PlayerData {
	std::wstring name;
	int x, z;
	int money;

};

class DataBase
{
public:
	PlayerTableRow PlayerDataSchema;

	SQLHENV henv;
	SQLHDBC hdbc;
	SQLHSTMT hstmt = 0;

	DataBase();
	~DataBase();

	void read_map_data();
	bool checkPlayer(PlayerData& name);
	void insertPlayer(PlayerData& name);
	void updatePlayer(const Client* const player,bool isend);
	
};

