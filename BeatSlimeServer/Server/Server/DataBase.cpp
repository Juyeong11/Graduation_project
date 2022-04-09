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

void DataBase::checkPlayer(const char* name)
{
	

}

void DataBase::insertPlayer(const wchar_t* name)
{
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	
	std::wstring command = std::format(L"EXEC insertPlayer '{}'", name);

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

void DataBase::update_map_data()
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