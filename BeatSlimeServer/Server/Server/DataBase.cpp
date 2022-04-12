#include"stdfx.h"
#include"Client.h"
#include "DataBase.h"



void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode)
{

	SQLSMALLINT iRec = 0;
	SQLINTEGER iError;
	WCHAR wszMessage[1000];
	WCHAR wszState[SQL_SQLSTATE_SIZE + 1];
	if (RetCode == SQL_INVALID_HANDLE) {
		fwprintf(stderr, L"Invalid handle!\n");
		return;
	}
	while (SQLGetDiagRec(hType, hHandle, ++iRec, wszState, &iError, wszMessage,
		(SQLSMALLINT)(sizeof(wszMessage) / sizeof(WCHAR)), (SQLSMALLINT*)NULL) == SQL_SUCCESS) {
		// Hide data truncated..
		if (wcsncmp(wszState, L"01004", 5)) {
			fwprintf(stderr, L"[%5.5s] %s (%d)\n", wszState, wszMessage, iError);
		}
	}
}

DataBase::DataBase() {
	SQLRETURN ret = SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &henv);
	isConnect = false;
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "SQLHandle 생성 실패\n";
		return;
	}

	ret = SQLSetEnvAttr(henv, SQL_ATTR_ODBC_VERSION, (SQLPOINTER*)SQL_OV_ODBC3, 0);
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "ODBC버전 선택 실패\n";
		return;
	}
	ret = SQLAllocHandle(SQL_HANDLE_DBC, henv, &hdbc);

	SQLSetConnectAttr(hdbc, SQL_LOGIN_TIMEOUT, (SQLPOINTER)5, 0);
	ret = SQLConnect(hdbc, (SQLWCHAR*)L"BeatSlie", SQL_NTS, (SQLWCHAR*)NULL, 0, NULL, 0);
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "ODBC 연결 실패\n";

		return;
	}
	isConnect = true;
}

DataBase::~DataBase() {
	SQLDisconnect(hdbc);
	SQLFreeHandle(SQL_HANDLE_DBC, hdbc);
	SQLFreeHandle(SQL_HANDLE_ENV, henv);
}

bool DataBase::checkPlayer(PlayerData& data)
{
	if (false == isConnect) return true;

	SQLHSTMT hstmt = 0;
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring command = std::format(L"EXEC playerLogin '{}'", data.name);
	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), SQL_NTS);
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		//	// Bind columns 1, 2, and 3  
		//	// 미리 읽어둘 변수를 bind해준다.
		retcode = SQLBindCol(hstmt, 1, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &PlayerDataSchema.p_x, 4, &PlayerDataSchema.cb_x);
		retcode = SQLBindCol(hstmt, 3, SQL_C_LONG, &PlayerDataSchema.p_z, 4, &PlayerDataSchema.cb_z);
		retcode = SQLBindCol(hstmt, 4, SQL_C_LONG, &PlayerDataSchema.p_money, 4, &PlayerDataSchema.cb_money);
		retcode = SQLBindCol(hstmt, 5, SQL_C_TINYINT, &PlayerDataSchema.p_isUsing, 1, &PlayerDataSchema.cb_using);

		// Fetch and print each row of data. On an error, display a message and exit.  
		for (int i = 0; ; i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				data.name = PlayerDataSchema.p_name;

				data.x = PlayerDataSchema.p_x;
				data.z = PlayerDataSchema.p_z;
				data.money = PlayerDataSchema.p_money;

				int is_using = PlayerDataSchema.p_isUsing;
				SQLCancel(hstmt);
				SQLFreeHandle(SQL_HANDLE_STMT, hstmt);

				if (is_using)
					return false;
				return true;

			}
			else {
				break;
			}
		}
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);

		//data.name;
		data.money = 0;
		data.x = 13;
		data.z = -25;
		insertPlayer(data);
		return true;

	}
	else {

		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);

		return false;
	}



}

void DataBase::insertPlayer(PlayerData& name)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring command = std::format(L"EXEC insertPlayer '{}'", name.name);

	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);

	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::updatePlayer(const Client* const pl, bool isend)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);



	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);
	if (name == L"Happy") {
		return;
	}
	int x = pl->x;
	int z = pl->z;
	int hp = pl->hp;
	int skilltype = pl->skill->SkillType;
	int skilllevel = pl->skill->SkillLevel;
	int isEnd = isend;
	std::wstring command = std::format(L"EXEC updatePlayerData '{}',{},{},{},{},{},{},{}",
		name, x, z, hp,
		skilltype, skilllevel, 0, isEnd);


	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);

	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::readItems(std::array<Item*, 9>& items)
{
	if (false == isConnect) {

		items[0]->itemType = 0;
		items[0]->itemPrice = 50;
		items[1]->itemType = 1;
		items[1]->itemPrice = 150;
		items[2]->itemType = 2;
		items[2]->itemPrice = 500;
		items[3]->itemType = 3;
		items[3]->itemPrice = 50;
		items[4]->itemType = 4;
		items[4]->itemPrice = 150;
		items[5]->itemType = 5;
		items[5]->itemPrice = 500;
		items[6]->itemType = 6;
		items[6]->itemPrice = 50;
		items[7]->itemType = 7;
		items[7]->itemPrice = 200;
		items[8]->itemType = 8;
		items[8]->itemPrice = 800;
		return;
	}
	SQLHSTMT hstmt = 0;
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring command = std::format(L"EXEC readItems");
	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), SQL_NTS);
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		//	// Bind columns 1, 2, and 3  
		//	// 미리 읽어둘 변수를 bind해준다.
		retcode = SQLBindCol(hstmt, 1, SQL_C_WCHAR, &ItemDataSchema.p_name, MAX_NAME_SIZE, &ItemDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &ItemDataSchema.p_price, 4, &ItemDataSchema.cb_price);

		// Fetch and print each row of data. On an error, display a message and exit.  
		for (int i = 0; items.size(); i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				std::wstring itemType = ItemDataSchema.p_name;
				while(itemType.back() == ' ') itemType.pop_back();
				int item;
				if (itemType == L"ADLev1") {
					item = 0;
				}
				else if (itemType == L"ADLev2") {
					item = 1;

				}
				else if (itemType == L"ADLev3") {
					item = 2;

				}
				else if (itemType == L"TankLev1") {
					item = 3;

				}
				else if (itemType == L"TankLev2") {
					item = 4;

				}
				else if (itemType == L"TankLev3") {
					item = 5;

				}
				else if (itemType == L"SupLev1") {
					item = 6;

				}
				else if (itemType == L"SupLev2") {
					item = 7;

				}
				else if (itemType == L"SupLev3") {
					item = 8;

				}
				items[i]->itemType = item;
				items[i]->itemPrice = ItemDataSchema.p_price;
			}
			else {
				break;
			}
		}


		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);



	}
	else {

		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}
