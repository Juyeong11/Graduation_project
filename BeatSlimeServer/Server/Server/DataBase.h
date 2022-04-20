#pragma once


void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode);

struct PlayerTableRow {
	SQLWCHAR p_name[MAX_NAME_SIZE];

	SQLINTEGER p_x, p_z;
	SQLINTEGER p_UsingSkill;
	SQLCHAR p_isUsing; // tiny int

	SQLCHAR p_SkillAD;
	SQLCHAR p_SkillTa;
	SQLCHAR p_SkillHeal; // tiny int

	SQLINTEGER p_MapName;
	SQLINTEGER p_ClearScore;

	SQLINTEGER p_MMR;

	SQLINTEGER p_Money;
	SQLINTEGER p_MusicScroll;
	SQLINTEGER p_MusicScrollCount;


	SQLLEN cb_name = 0, cb_x = 0, cb_z = 0, cb_Money = 0, cb_UsingSkill = 0, cb_isUsing;
	SQLLEN cb_SkillAD = 0, cb_SKillTa = 0, cb_SkillHeal = 0;
	SQLLEN cb_MapName = 0, cb_ClearScore = 0;
	SQLLEN cb_MMR = 0;
	SQLLEN  cb_MusicScroll,cb_MusicScrollCount;
};

struct ItemTableRow {
	SQLWCHAR p_name[MAX_NAME_SIZE];

	SQLINTEGER p_price;
	SQLINTEGER p_damage;
	SQLINTEGER p_CoolTime;

	SQLLEN cb_price = 0, cb_name = 0, cb_damage, cb_coolTime;
};
struct Item;
class Client;
class Skill;
class DataBase
{
public:
	PlayerTableRow PlayerDataSchema;
	ItemTableRow ItemDataSchema;

	SQLHENV henv;
	SQLHDBC hdbc;
	SQLHSTMT hstmt = 0;

	bool isConnect;
	DataBase();
	~DataBase();

	void read_map_data();
	bool checkPlayer(PlayerData& name);
	void insertPlayer(PlayerData& name);
	void updatePlayer(const Client* const player,bool isend);
	void updateClearInfo(const Client* const player);
	void updateInventory(const Client* const player);
	void readSkills(std::array<Skill*, SKILL_CNT>& items);
	void readInventory(Client* player);
	void readClearMap(Client* player);
};

