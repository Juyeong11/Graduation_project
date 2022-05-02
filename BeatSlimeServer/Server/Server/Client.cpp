#include "stdfx.h"
#include "protocol.h"
#include "PathFinder.h"
#include "Map.h"
#include "Client.h"





EXP_OVER::EXP_OVER(COMP_OP comp_op, unsigned char num_bytes, void* mess) : _comp_op(comp_op)
{
	ZeroMemory(&_wsa_over, sizeof(_wsa_over));
	_wsa_buf.buf = reinterpret_cast<char*>(_net_buf);
	_wsa_buf.len = num_bytes;
	memcpy(_net_buf, mess, num_bytes);
}

EXP_OVER::EXP_OVER(COMP_OP comp_op) : _comp_op(comp_op) {}

EXP_OVER::EXP_OVER()
{
	_comp_op = OP_RECV;
}

void EXP_OVER::set_exp(COMP_OP comp_op, unsigned char num_bytes, void* mess)
{
	_comp_op = comp_op;
	ZeroMemory(&_wsa_over, sizeof(_wsa_over));
	_wsa_buf.buf = reinterpret_cast<char*>(_net_buf);
	_wsa_buf.len = num_bytes;
	memcpy(_net_buf, mess, num_bytes);
}
GameObject::GameObject() :state(ST_FREE) {
	pre_x = x = 0;
	pre_y = y = 0;
	pre_z = z = 0;
	dest_x = -1;
	dest_y = -1;
	dest_z = -1;
	last_move_time = std::chrono::system_clock::now();
	hp = 100;
}

bool GameObject::Hit(int damage)
{
	hp -= damage;
	if (hp <= 0)
	{
		hp = 0;
		return true;
	}

	return false;
}



Npc::Npc() : is_active(false) {

}

SkillTrader::SkillTrader() : is_talk(false) {

}
Curator::Curator() : is_talk(false) {

}
Witch::Witch() : is_attack(false) {

}

Boss2::Boss2() : is_attack(false) {

}

Skill::Skill(int sl, int st) : SkillLevel(sl), SkillType(st) {
	Damage = SkillLevel * SkillLevel;
	CoolTime = 10 / SkillLevel;
}
Skill::Skill() {

}

Client::Client(Skill* sk, MapInfo* mapdata) : skill(sk)
{
	type = PLAYER;
	is_active = true;
	prev_recv_size = 0;
	money = 0;
	cur_room_num = -1;
	party = nullptr;
	Astar = new PathFinder(mapdata);
}
Client::~Client()
{
	delete Astar;
	closesocket(socket);
}
void Client::do_recv()
{
	DWORD recv_flag = 0;
	ZeroMemory(&recv_over._wsa_over, sizeof(recv_over._wsa_over));
	recv_over._wsa_buf.buf = reinterpret_cast<char*>(recv_over._net_buf + prev_recv_size);
	recv_over._wsa_buf.len = sizeof(recv_over._net_buf) - prev_recv_size;
	int ret = WSARecv(socket, &recv_over._wsa_buf, 1, 0, &recv_flag, &recv_over._wsa_over, NULL);
	if (SOCKET_ERROR == ret) {
		int error_num = WSAGetLastError();
		if (ERROR_IO_PENDING != error_num)
			error_display("recv", error_num);
	}
}

void Client::do_send(EXP_OVER* ex_over)
{
	//EXP_OVER* ex_over = new EXP_OVER(OP_SEND, num_bytes, mess);
	int ret = WSASend(socket, &ex_over->_wsa_buf, 1, 0, 0, &ex_over->_wsa_over, NULL);
	if (SOCKET_ERROR == ret) {
		int error_num = WSAGetLastError();
		if (ERROR_IO_PENDING != error_num)
			error_display("send", error_num);
	}
}

int Client::GetMoney()
{
	return money;
}

void Client::SetMoney(int m)
{
	money += m;
}

Party::Party()
{
	for (auto& i : partyPlayer)
		i = -1;
	curPlayerNum = 0;
}

bool Party::SetPartyPlayer(int id)
{
	for (auto i : partyPlayer)
		if (i == id) return false;
	partyLock.lock();
	if (curPlayerNum >= MAX_IN_GAME_PLAYER) { partyLock.unlock(); return false; }
	partyPlayer[curPlayerNum++] = id;
	partyLock.unlock();
	return true;;
}

bool Party::DelPartyPlayer(int id)
{

	partyLock.lock();
	for (auto& i : partyPlayer)
	{
		if (i == id) {
			i = -1;
			curPlayerNum--;
			partyLock.unlock();
			return true;
		}
	}
	partyLock.unlock();
	return false;
}

int Party::DelParty()
{
	partyLock.lock();
	int ret = 0;
	for (auto& i : partyPlayer)
	{
		if (i != -1) {
			ret = i;
			i = -1;
			break;
		}
	}

	curPlayerNum = 0;
	partyLock.unlock();
	return ret;

}
