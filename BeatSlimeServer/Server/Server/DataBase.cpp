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
	ret = SQLConnect(hdbc, (SQLWCHAR*)L"BeatSlime", SQL_NTS, (SQLWCHAR*)NULL, 0, NULL, 0);
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "ODBC 연결 실패\n";
		return;
	}
}

DataBase::~DataBase() {
	SQLDisconnect(hdbc);
	SQLFreeHandle(SQL_HANDLE_DBC, hdbc);
	SQLFreeHandle(SQL_HANDLE_ENV, henv);
}

bool DataBase::checkPlayer(PlayerData& data)
{
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
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);



	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);
	if (name == L"happy") {
		return;
	}
	int x = pl->x;
	int z = pl->z;
	int hp = pl->hp;
	int skilltype = pl->skill->SkillType;
	int skilllevel = pl->skill->SkillLevel;
	int isEnd = isend;
	std::wstring command = std::format(L"EXEC updatePlayerData '{}',{},{},{},{},{},{},{}",
		name, x,z,hp,
		skilltype,skilllevel,0,isEnd);


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