#include"stdfx.h"
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

void DataBase::read_map_data()
{
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	db_map_data.clear();
	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)L"SELECT * FROM Map", SQL_NTS);
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		// Bind columns 1, 2, and 3  
		// 미리 읽어둘 변수를 bind해준다.
		retcode = SQLBindCol(hstmt, 1, SQL_C_LONG, &MapSchema.p_id, 2, &MapSchema.cb_id);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &MapSchema.p_x, 2, &MapSchema.cb_x);
		retcode = SQLBindCol(hstmt, 3, SQL_C_LONG, &MapSchema.p_y, 2, &MapSchema.cb_y);
		retcode = SQLBindCol(hstmt, 4, SQL_C_LONG, &MapSchema.p_z, 2, &MapSchema.cb_z);
		retcode = SQLBindCol(hstmt, 5, SQL_C_LONG, &MapSchema.p_w, 2, &MapSchema.cb_w);
		retcode = SQLBindCol(hstmt, 6, SQL_C_LONG, &MapSchema.p_color, 2, &MapSchema.cb_color);
		retcode = SQLBindCol(hstmt, 7, SQL_C_LONG, &MapSchema.p_type, 2, &MapSchema.cb_type);

		// Fetch and print each row of data. On an error, display a message and exit.  
		for (int i = 0; ; i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				//replace wprintf with printf
				//%S with %ls
				//warning C4477: 'wprintf' : format string '%S' requires an argument of type 'char *'
				//but variadic argument 2 has type 'SQLWCHAR *'
				//wprintf(L"%d: %S %S %S\n", i + 1, sCustID, szName, szPhone);  
				//printf("%d: %d \t %d \t  %d \t  %d \t  %d \t %d\n", i + 1, MapSchema.p_x, MapSchema.p_y, MapSchema.p_z, MapSchema.p_w, MapSchema.p_color, MapSchema.p_type);
				db_map_data.emplace_back(MapSchema.p_id, MapSchema.p_x, MapSchema.p_y, MapSchema.p_z,
					MapSchema.p_w, MapSchema.p_color, MapSchema.p_type);
			}
			else {
				break;
			}
		}
	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::insert_map_data(const Map& shell)
{
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	
	//std::wstring command= std::format(L"INSERT INTO Map (id, x, y, z, w, color, type) VALUES ({}, {}, {}, {}, {}, {}, {})", shell.id, shell.x, shell.y, shell.z, shell.w, shell.color, shell.type);

	//retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
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

void DataBase::update_map_data(const Map& shell)
{
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	//std::wstring command = std::format(L"UPDATE Map SET id = {}, x = {}, y = {}, z = {}, w = {}, color = {}, type = {} WHERE id = {}\n SELECT * FROM Map", shell.id, shell.x, shell.y, shell.z, shell.w, shell.color, shell.type, shell.id);


	//retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
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