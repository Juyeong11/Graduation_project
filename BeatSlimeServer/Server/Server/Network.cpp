#include "stdfx.h"

#include"protocol.h"
#include"Map.h"
#include "DataBase.h"
#include "Network.h"
#include"Client.h"
#include"PathFinder.h"


Network* Network::instance = nullptr;

Network* Network::GetInstance()
{
	return instance;
}


Network::Network() {
	//인스턴스는 한 개만!!
	assert(instance == nullptr);
	instance = this;

	accept_ex = new EXP_OVER();
	for (int i = 0; i < SKILL_CNT; ++i) {
		skills[i] = new Skill();
	}
	for (int i = 0; i < MAX_USER / 2; ++i) {
		PartyPool[i] = new Party();
	}

	for (int i = 0; i < MAP_NUM; ++i) {
		maps[i] = new MapInfo;
	}
	DB = new DataBase;

	DB->readSkills(skills);
	maps[FIELD_MAP]->SetMap("Map\\Forest1", "Music\\flower_load.csv");
	maps[WITCH_MAP]->SetMap("Map\\flower_load", "Music\\flower_load.csv");
	maps[WITCH_MAP_HARD]->SetMap("Map\\flower_load", "Music\\flower_load2.csv");
	maps[ROBOT_MAP]->SetMap("Map\\Robot1", "Music\\Aviform Skyliner.csv");
	maps[TUTORI_MAP]->SetMap("Map\\Tutorial", "Music\\Tutorial.csv");
	//수정
	//여기서 스킬을 초기화하지 말고 나중에 db연결되면 거기서 읽어오면서 스킬을 초기화하는 것으로 하자
	for (int i = 0; i < MAX_USER; ++i) {
		clients[i] = new Client(skills[0], maps[FIELD_MAP]);
	}
	for (int i = SKILL_TRADER_ID_START; i < SKILL_TRADER_ID_END; ++i) {
		clients[i] = new SkillTrader();
	}
	for (int i = CURATOR_ID_START; i < CURATOR_ID_END; ++i) {
		clients[i] = new Curator();
	}
	for (int i = WITCH_ID_START; i < WITCH_ID_END; ++i) {
		clients[i] = new Witch();
	}
	for (int i = BOSS2_ID_START; i < BOSS2_ID_END; ++i) {
		clients[i] = new Boss2();
	}
	for (int i = 0; i < MAX_OBJECT; ++i) {
		clients[i]->id = i;
	}
	for (int i = 0; i < MAX_OBJECT * 50; ++i) {
		exp_over_pool.push(new EXP_OVER);
	}
	Initialize_NPC();


	for (int i = 0; i < MAX_GAME_ROOM_NUM; ++i) {
		game_room[i] = new GameRoom(i);
	}


	// 포탈의 위치를 나타내는 자료필요

	portals[0] = new Portal(17, -21, WITCH_MAP);
	portals[1] = new Portal(14, 1, ROBOT_MAP);
	portals[2] = new Portal(19, -24, WITCH_MAP_HARD);


	cur_play_music = 99;

}
Network::~Network() {
	//스레드가 종료된 후 이기 때문에 락을 할 필요가 없다
//accpet상태일 때 문제가 생긴다
	delete accept_ex;
	for (int i = 0; i < MAX_USER; ++i)
		if (ST_INGAME == clients[i]->state)
			disconnect_client(clients[i]->id);

	for (int i = 0; i < MAX_OBJECT; ++i) {
		delete clients[i];
	}
	for (int i = 0; i < MAX_OBJECT; ++i) {
		EXP_OVER* ex;
		exp_over_pool.try_pop(ex);
		delete ex;
	}
	DB->endServer();
	delete DB;
	for (int i = 0; i < MAX_GAME_ROOM_NUM; ++i) {
		delete game_room[i];
	}
	for (int i = 0; i < MAP_NUM; ++i) {
		delete maps[i];
	}

	for (int i = 1; i <= SKILL_CNT; ++i) {
		delete skills[i];
	}

	for (int i = 0; i < MAX_USER / 2; ++i) {
		delete PartyPool[i];
	}
}

void Network::start_accept() {
	SOCKET c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

	*(reinterpret_cast<SOCKET*>(accept_ex->_net_buf)) = c_socket;

	ZeroMemory(&accept_ex->_wsa_over, sizeof(accept_ex->_wsa_over));
	accept_ex->_comp_op = OP_ACCEPT;

	AcceptEx(g_s_socket, c_socket, accept_ex->_net_buf + sizeof(SOCKET), 0, sizeof(SOCKADDR_IN) + 16,
		sizeof(SOCKADDR_IN) + 16, NULL, &accept_ex->_wsa_over);
}
void Network::Initialize_NPC() {
	for (int i = NPC_ID_START; i < NPC_ID_END; ++i) {
		sprintf_s(clients[i]->name, "NPC%d", i);
		clients[i]->x = 0;
		clients[i]->z = 0;
		clients[i]->id = i;
		clients[i]->state = ST_ACCEPT;
		clients[i]->direction = DOWN;
		clients[i]->type = WITCH;

	}
}
bool Network::is_attack(int a, int x, int z)
{
	if (abs(clients[a]->x != x)) return false;
	if (abs(clients[a]->z != z)) return false;

	return true;
}
bool Network::is_attack(int a, int b)
{
	if (ATTACK_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
	if (ATTACK_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

	return true;
}
bool Network::is_near(int a, int b)
{
	if (VIEW_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
	if (VIEW_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

	return true;
}
void Network::send_login_ok(int c_id, char inven[20])
{
	Client& cl = *reinterpret_cast<Client*>(clients[c_id]);

	sc_packet_login_ok packet;
	packet.id = c_id;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_LOGIN_OK;
	packet.x = clients[c_id]->x;
	packet.y = clients[c_id]->y;
	packet.z = clients[c_id]->z;
	packet.cur_skill_type = cl.skill->SkillType;
	packet.cur_skill_level = cl.skill->SkillLevel;
	packet.skill_progress[0] = cl.SkillAD;
	packet.skill_progress[1] = cl.SkillTa;
	packet.skill_progress[2] = cl.SkillHeal;
	packet.money = cl.money;
	strcpy_s(packet.name, clients[c_id]->name);
	memcpy_s(packet.inventory, 20, inven, 20);

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_login_fail(int client_id)
{
	sc_packet_login_fail packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_LOGIN_FAIL;
	packet.reason = 1;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[client_id])->do_send(ex_over);
}

void Network::send_change_scene(int c_id, int map_type)
{
	sc_packet_change_scene packet;
	packet.type = SC_PACKET_CHANGE_SCENE;
	packet.size = sizeof(packet);
	packet.scene_num = map_type;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);
}

void Network::send_game_start(int c_id, int start_time)
{
	sc_packet_game_start packet;
	packet.type = SC_PACKET_GAME_START;
	packet.size = sizeof(packet);
	packet.game_start_time = start_time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_game_init(int c_id, GameObject* ids[3], int boss_id)
{
	sc_packet_game_init packet;
	packet.type = SC_PACKET_GAME_INIT;
	packet.size = sizeof(packet);
	packet.player_id = c_id;
	packet.id1 = ids[0]->id;
	//
	//packet.id2 =-1;
	//packet.id3 =-1;
	if (ids[1] != nullptr)
		packet.id2 = ids[1]->id;
	else
		packet.id2 = -1;
	if (ids[2] != nullptr)
		packet.id3 = ids[2]->id;
	else
		packet.id3 = -1;

	packet.boss_id = boss_id;
	for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
		if (ids[i] == nullptr) continue;
		packet.coolTime[i] = skills[reinterpret_cast<Client*>(ids[i])->curSkill]->CoolTime;

	}
	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);

	for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
		if (ids[i] == nullptr) continue;
		send_put_object(c_id, ids[i]->id);
	}

}

void Network::send_effect(int client_id, int actor_id, int target_id, int effect_type, int charging_time, int dir, int x, int y, int z)
{
	sc_packet_effect packet;
	packet.type = SC_PACKET_EFFECT;
	packet.size = sizeof(packet);
	packet.effect_type = effect_type;
	packet.id = actor_id;
	packet.target_id = target_id;
	packet.dir = dir;
	packet.charging_time = charging_time;
	packet.x = x;
	packet.y = y;
	packet.z = z;
	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[client_id])->do_send(ex_over);
}

void Network::send_move_object(int c_id, int mover)
{
	sc_packet_move packet;
	packet.id = mover;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_MOVE;
	packet.x = clients[mover]->x;
	packet.y = clients[mover]->y;
	packet.z = clients[mover]->z;

	packet.dir = clients[mover]->direction;

	packet.move_time = clients[mover]->last_packet_time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_attack_player(int attacker, int target, int receiver)
{
	sc_packet_attack packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_ATTACK;
	packet.id = attacker;
	packet.target_id = target;
	//packet.direction = clients[attacker]->direction;
	packet.hp = clients[target]->hp;
	//packet.move_time = clients[mover]->last_move_time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[receiver])->do_send(ex_over);
}

void Network::send_change_skill(int c_id, int target) {
	sc_packet_change_skill packet;

	//strcpy_s(packet.name, clients[target]->name);
	packet.id = target;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_CHANGE_SKILL;
	packet.skill_type = reinterpret_cast<Client*>(clients[target])->skill->SkillType;
	packet.skill_level = reinterpret_cast<Client*>(clients[target])->skill->SkillLevel;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_put_object(int c_id, int target) {
	sc_packet_put_object packet;

	packet.id = target;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_PUT_OBJECT;
	packet.object_type = clients[target]->type;
	packet.x = clients[target]->x;
	packet.y = clients[target]->y;
	packet.z = clients[target]->z;

	packet.direction = clients[target]->direction;

	if (is_player(target)) {
		packet.skill_type = reinterpret_cast<Client*>(clients[target])->skill->SkillType;
		packet.skill_level = reinterpret_cast<Client*>(clients[target])->skill->SkillLevel;
		strcpy_s(packet.name, clients[target]->name);
	}

	EXP_OVER* ex_over;
	//std::cout << exp_over_pool.unsafe_size() << std::endl;

	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_remove_object(int c_id, int victim)
{
	sc_packet_remove_object packet;
	packet.id = victim;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_REMOVE_OBJECT;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);



	if (true == is_npc(victim)) {
		reinterpret_cast<Npc*>(clients[victim])->is_active = false;
	}
}

void Network::send_map_data(int c_id, char* data, int nShell)
{
	sc_packet_map_data packet;
	//packet.size = nShell * sizeof(Map) + 2;
	packet.type = SC_PACKET_MAP_DATA;
	//memcpy(packet.buf, reinterpret_cast<char*>(data), packet.size - 2);


	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, packet.size, &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);

}

void Network::send_game_end(int c_id, int score, int itemType, char money, char end_type)
{
	sc_packet_game_end packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_GAME_END;
	packet.end_type = end_type;
	packet.score = score;
	packet.item_type = itemType;
	packet.money = money;


	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_parrying(int c_id, int actor_id)
{
	sc_packet_parrying packet;
	packet.type = SC_PACKET_PARRYING;
	packet.size = sizeof(packet);
	packet.id = actor_id;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}

void Network::send_ping(int c_id, int time)
{
	sc_packet_ping_test packet;
	packet.type = SC_PACKET_PING_TEST;
	packet.size = sizeof(packet);
	packet.ping_time = time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[c_id])->do_send(ex_over);
}


void Network::send_party_request(int reciver, int sender)
{
	sc_packet_party_request packet;
	packet.type = SC_PACKET_PARTY_REQUEST;
	packet.size = sizeof(packet);
	packet.requester_id = sender;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[reciver])->do_send(ex_over);
}

void Network::send_party_request_anwser(int reciver, int newPlayerid, Party* party, int type)
{
	sc_packet_party_request_anwser packet;
	packet.type = SC_PACKET_PARTY_REQUEST_ANSWER;
	packet.size = sizeof(packet);
	packet.answer = type;
	packet.new_party_id = newPlayerid;
	if (party != nullptr) {

		party->partyLock.lock();
		memcpy_s(packet.p, sizeof(int) * 3, party->partyPlayer, sizeof(int) * 3);
		party->partyLock.unlock();
	}
	else {
		for (auto& p : packet.p)
			p = -1;

	}

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[reciver])->do_send(ex_over);
}
void Network::send_chat_packet(int user_id, int my_id, char* mess)
{
	sc_packet_chat packet;

	strcpy_s(packet.mess, mess);
	packet.id = my_id;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_CHAT;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[user_id])->do_send(ex_over);
}

void Network::send_buy_result(int user_id, int itemType, char result)
{
	sc_packet_buy_result packet;
	packet.result = result;
	packet.itemType = itemType;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_BUY_RESULT;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[user_id])->do_send(ex_over);
}

void Network::send_use_item(int user_id, int user, int itemType)
{
	sc_packet_use_item packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_USE_ITEM;
	packet.itemType = itemType;
	packet.user = user;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[user_id])->do_send(ex_over);

}

void Network::send_score(int user_id, int score, int receiver)
{
	int s = score;
	if (score < 0) {
		s = 0;
	}
	sc_packet_score packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_SEND_SCORE;
	packet.id = user_id;
	packet.score = s;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(clients[receiver])->do_send(ex_over);
}

void Network::disconnect_client(int c_id)
{
	if (c_id >= MAX_USER)
		std::cout << "disconnect_client : unexpected id range" << std::endl;
	Client& client = *reinterpret_cast<Client*>(clients[c_id]);
	client.dest_x = -1;
	client.dest_y = -1;
	client.dest_z = -1;

	int room_num = reinterpret_cast<Client*>(clients[c_id])->cur_room_num;

	maps[FIELD_MAP]->SetTileType(-1, -1, clients[c_id]->x, clients[c_id]->z);

	for (auto* p : portals) {
		if (false == p->findPlayer(c_id)) continue;
		p->id_lock.lock();
		p->player_ids.erase(c_id);
		p->id_lock.unlock();

		break;
	}

	client.vl.lock();
	std::unordered_set <int> my_vl = client.viewlist;
	client.vl.unlock();
	for (auto other : my_vl) {
		if (true == is_npc(other)) continue;

		Client& target = *reinterpret_cast<Client*>(clients[other]);
		if (ST_INGAME != target.state)
			continue;
		target.vl.lock();
		if (0 != target.viewlist.count(c_id)) {
			target.viewlist.erase(c_id);
			target.vl.unlock();
			send_remove_object(other, c_id);
		}
		else target.vl.unlock();
	}

	//여기서 end 패킷 보내고 종료 처리를 하자
	input_db_event(c_id, DB_PLAYER_LOGOUT);

	if (client.cur_room_num != -1) {
		for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
			//send_remove_object(game_room[client.cur_room_num]->player_ids[i]->id, c_id);

			if (clients[c_id] == game_room[client.cur_room_num]->player_ids[i]) {
				game_room[client.cur_room_num]->player_ids[i] = nullptr;

				// 예상되는 버그
				// GameRoom::gmae_end를 호출하기 전 한 명이 더 나가서 이벤트 등록을 한번 더한다면?
				timer_event tev;
				tev.ev = EVENT_GAME_END;
				tev.game_room_id = client.cur_room_num;
				//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
				tev.start_time = std::chrono::system_clock::now();

				timer_queue.push(tev);
				client.cur_room_num = -1;
				break;
				//game_room[client.cur_room_num]->pattern_progress = -1;
				//reinterpret_cast<Client*>(clients[c_id])->cur_room_num = -1;

			}
		}
		client.cur_room_num = -1;
	}

	if (client.party != nullptr) {

		client.party->DelPartyPlayer(c_id);

		for (auto p : client.party->partyPlayer) {
			if (p == -1)continue;
			send_party_request_anwser(p, c_id, client.party, 3);
		}

		if (client.party->curPlayerNum == 1) {
			int last_id = client.party->DelParty();
			Client* lastPartyPlayer = reinterpret_cast<Client*>(clients[last_id]);

			lastPartyPlayer->party = nullptr;
		}
		client.party = nullptr;
	}


	clients[c_id]->state_lock.lock();
	closesocket(reinterpret_cast<Client*>(clients[c_id])->socket);

	clients[c_id]->state_lock.unlock();
}

int Network::get_new_id()
{
	for (int i = 0; i < MAX_USER; ++i) {
		clients[i]->state_lock.lock();
		if (ST_FREE == clients[i]->state) {
			clients[i]->state = ST_ACCEPT;
			clients[i]->state_lock.unlock();
			return i;
		}
		clients[i]->state_lock.unlock();
	}
	std::cout << "1 : Maximum Number of Clients Overflow!!\n";
	return -1;
}
int Network::get_npc_id(int monsterType) {
	switch (monsterType)
	{
	case WITCH:
		for (int i = WITCH_ID_START; i < WITCH_ID_END; ++i) {
			clients[i]->state_lock.lock();
			if (ST_ACCEPT == clients[i]->state) {
				clients[i]->state = ST_INGAME;
				clients[i]->state_lock.unlock();
				return i;
			}
			clients[i]->state_lock.unlock();
		}
		std::cout << "2-0 : Maximum Number of Monster Overflow!!\n";
		return -1;

		break;
	case BOSS2:
		for (int i = BOSS2_ID_START; i < BOSS2_ID_END; ++i) {
			clients[i]->state_lock.lock();
			if (ST_ACCEPT == clients[i]->state) {
				clients[i]->state = ST_INGAME;
				clients[i]->state_lock.unlock();
				return i;
			}
			clients[i]->state_lock.unlock();
		}
		std::cout << "2-1 : Maximum Number of Monster Overflow!!\n";
		break;
	case SKILL_TRADER:
		break;
	case CURATOR:
		break;
	default:
		std::cout << "wrong npc type\n";
		return -1;
		break;
	}
	return -1;
}
int Network::get_game_room_id()
{
	for (int i = 0; i < MAX_GAME_ROOM_NUM; ++i) {
		game_room[i]->state_lock.lock();
		if (false == game_room[i]->isGaming) {
			game_room[i]->isGaming = true;
			game_room[i]->state_lock.unlock();
			return i;
		}
		game_room[i]->state_lock.unlock();
	}
	std::cout << "3 : Maximum Number of Game Room Overflow!!\n";
	return -1;
}

int Network::set_new_player_pos(int client_id)
{
	short orgin_x = clients[client_id]->x;
	short orgin_z = clients[client_id]->z;


	short& _x = clients[client_id]->x;
	short& _y = clients[client_id]->y;
	short& _z = clients[client_id]->z;

	int i = 0;
	int step = 1;
	while (maps[FIELD_MAP]->GetTileType(_x, _z) != 0) {
		_x = PatternInfo::HexCellAround[i][0] * step + orgin_x;
		_z = PatternInfo::HexCellAround[i][2] * step + orgin_z;
		_y = -_x - _z;
		i++;
		if (i == 6) {
			step++;
			i = 0;
		}
		if (step > 20) {
			std::cout << "Can not find empty pos\n";
			return -1;
		}
	}

	maps[FIELD_MAP]->SetTileType(_x, _z, _x, _z);
	return 1;
}

void Network::do_npc_move(int npc_id) {


}

void Network::do_npc_attack(int npc_id, int target_id, int reciver) {

}

void Network::do_npc_tile_attack(int game_room_id, int x, int y, int z)
{
	int damage = 20 * damamge_bug;
	for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {

		GameObject* pl = game_room[game_room_id]->player_ids[i];
		if (pl == nullptr) continue;
		if (false == is_attack(pl->id, x, z)) continue;
		pl->hp -= std::clamp((damage - pl->armour), 0, 20);
		//std::cout << "id : " << i << " damaged " << damage << std::endl;
		if (game_room[game_room_id]->boss_id == nullptr) return;
		game_room[game_room_id]->Score[i] -= 20 * std::clamp((damage - pl->armour), 0, 20);
		for (const auto& p : game_room[game_room_id]->player_ids) {
			if (p == nullptr) continue;
			send_score(pl->id, game_room[game_room_id]->Score[i], p->id);
			send_attack_player(game_room[game_room_id]->boss_id->id, pl->id, p->id);
		}


		if (pl->hp < 0) {

			// 한 명이라도 죽으면 게임 끝
			timer_event tev;
			tev.ev = EVENT_GAME_END;
			tev.game_room_id = game_room_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
			tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

			timer_queue.push(tev);
			for (const auto& p : game_room[game_room_id]->player_ids) {
				if (p == nullptr) continue;
				reinterpret_cast<Client*>(p)->is_active = false;
			}
		}
	}


}

void Network::do_player_skill(GameRoom* gr, Client* cl) {


	bool attack_flag = false;
	int damage = cl->skill->Damage * cl->power;
	//damage = 10;
	int cur_skill_type = cl->skill->SkillType - 1;
	switch (cur_skill_type)
	{
	case QUAKE:
		if (false == is_attack(gr->boss_id->id, cl->id))
			break;
		__fallthrough;
	case WATERGUN:
		if (gr->boss_id->Hit(damage)) {
			timer_event tev;
			tev.ev = EVENT_GAME_END;
			tev.game_room_id = gr->game_room_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
			tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

			timer_queue.push(tev);
		}
		gr->Score[gr->FindPlayerID_by_GameRoom(cl->id)] += 100 * damage;
		std::cout << gr->boss_id->hp<<std::endl;
		std::cout << gr->Score[gr->FindPlayerID_by_GameRoom(cl->id)] <<std::endl;
		for (const auto pl : gr->player_ids) {
			if (pl == nullptr) continue;
			send_score(cl->id, gr->Score[gr->FindPlayerID_by_GameRoom(cl->id)], pl ->id);

			send_attack_player(cl->id, gr->boss_id->id, pl->id);
		}
		break;
	case HEAL:
		for (auto pl : gr->player_ids) {
			if (pl == nullptr) continue;
			pl->Hit(-damage);
			if (pl->hp > 100)
				pl->hp = 100;
			send_score(cl->id, gr->Score[gr->FindPlayerID_by_GameRoom(cl->id)], pl->id);


			send_attack_player(cl->id, pl->id, pl->id);
		}
		gr->Score[gr->FindPlayerID_by_GameRoom(cl->id)] += 100 * damage;

		break;
	default:
		std::cout << "wrong skill type\n";
		break;
	}

}

void Network::process_packet(int client_id, unsigned char* p)
{
	unsigned char packet_type = p[1];
	Client& cl = *reinterpret_cast<Client*>(clients[client_id]);

	switch (packet_type)
	{
	case CS_PACKET_LOGIN:
	{
		cs_packet_login* packet = reinterpret_cast<cs_packet_login*>(p);
		//strcpy_s(cl.name, packet->name);
		//로그인 요청이 오면 디비 스레드에 해당 아이디에 대한 정보를 알려달라고 요청하고 종료한다.
		//그럼 디비 스레드에서 정보를 알려주고 
		std::string id = packet->name;
		std::regex re("[^a-zA-Z0-9]");
		//std::cout << packet->name << std::endl;
		if (true == std::regex_match(id, re)) {
			send_login_fail(client_id);
			disconnect_client(client_id);
			break;
		}
		strcpy_s(cl.name, packet->name);

		// 아래부터는 디비 스레드에서 pqcs하면 다시 하기 시작
		input_db_event(client_id, DB_PLAYER_LOGIN);
		// 로그인 씬 완성되면 옮겨야됨
		if (cur_play_music != 99)
			send_use_item(client_id, 0, cur_play_music);
		if (DB->isConnect == false) {
			char inven[20];
			for (int i = 0; i < 20; ++i) {
				inven[i] = 10;
			}
			send_login_ok(client_id, inven);

			cl.state_lock.lock();
			cl.state = ST_INGAME;
			cl.state_lock.unlock();

			cl.skill = skills[0];

			cl.curSkill = 0;

			cl.SkillAD = 0;
			cl.SkillTa = 0;
			cl.SkillHeal = 0;

			cl.MMR = 100;
			cl.money = 0;

			//cl.inventory = inventorys[client_id];
		}

	}
	break;
	case CS_PACKET_MOVE:
	{
		if (false == cl.is_active) break;

		cs_packet_move* packet = reinterpret_cast<cs_packet_move*>(p);
		cl.last_packet_time = packet->move_time;
		if (cl.last_move_time + std::chrono::milliseconds(200) > std::chrono::system_clock::now()) break;
		cl.last_move_time = std::chrono::system_clock::now();

		cl.pre_x = cl.x;
		cl.pre_y = cl.y;
		cl.pre_z = cl.z;
		cl.dest_x = -1;
		cl.dest_y = -1;
		cl.dest_z = -1;
		short& x = cl.x;
		short& y = cl.y;
		short& z = cl.z;
		cl.direction = packet->direction;
		int cur_map = 0;
		if (cl.cur_room_num != -1) {
			cur_map = game_room[cl.cur_room_num]->map_type;

		}


		switch (packet->direction) {
		case DIR::LEFTUP:
			if (maps[cur_map]->GetTileType(x - 1, z + 1) < 0) {
				break;
			}
			x--; z++;
			break;
		case DIR::UP:
			if (maps[cur_map]->GetTileType(x, z + 1) < 0) {
				break;
			}
			y--; z++;
			break;
		case DIR::RIGHTUP:
			if (maps[cur_map]->GetTileType(x + 1, z) < 0) {
				break;
			}
			x++; y--;
			break;
		case DIR::LEFTDOWN:
			if (maps[cur_map]->GetTileType(x - 1, z) < 0) {
				break;
			}
			x--; y++;
			break;
		case DIR::DOWN:
			if (maps[cur_map]->GetTileType(x, z - 1) < 0) {
				break;
			}
			y++; z--;
			break;
		case DIR::RIGHTDOWN:
			if (maps[cur_map]->GetTileType(x + 1, z - 1) < 0) {
				break;
			}
			x++; z--;
			break;
		default:
			std::cout << "Invalid move in client " << client_id << std::endl;
			exit(-1);
		}

		//std::cout << "x : " << x << "y : " << y << "z : " << z << std::endl;

		if (cur_map != 0) {

			for (auto pl : game_room[cl.cur_room_num]->player_ids) {
				if (pl == nullptr) continue;
				send_move_object(pl->id, cl.id);
			}
			break;
		}


		maps[cur_map]->SetTileType(x, z, cl.pre_x, cl.pre_z);
		// 이동한 클라이언트에 대한 nearlist 생성
		// 꼭 unordered_set이여야 할까?
		// 얼마나 추가될지 모르고, 데이터는 id이기 때문에 중복없음이 보장되있다. id로 구분안하는 경우가 있나?
		// 섹터를 나누어 근처에 있는지 검색해 속도를 높이자
		std::unordered_set<int> nearlist;
		for (auto* other : clients) {
			if (other->id == client_id)
				continue;
			if (other->cur_room_num != -1)
				continue;
			if (ST_INGAME != other->state)
				continue;
			if (false == is_near(client_id, other->id))
				continue;

			nearlist.insert(other->id);
		}

		send_move_object(cl.id, cl.id);

		//lock시간을 줄이기 위해 자료를 복사해서 사용
		cl.vl.lock();
		std::unordered_set<int> my_vl{ cl.viewlist };
		cl.vl.unlock();


		// 움직임으로써 시야에 들어온 플레이어 확인 및 추가
		for (int other : nearlist) {
			// cl의 뷰리스트에 없으면
			if (0 == my_vl.count(other)) {
				// cl의 뷰리스트에 추가하고
				cl.vl.lock();
				cl.viewlist.insert(other);
				cl.vl.unlock();
				// 보였으니 그리라고 패킷을 보낸다.
				send_put_object(cl.id, other);

				//npc는 send를 안한다.
				//npc는 뷰리스트가 없고 자신을 볼 수 있는 플레이어가 있다면 isActive변수를 통해 움직인다.
				//플레이어에게 보인 NPC의 움직임 이벤트를 시작한다.
				if (true == is_npc(other)) {
					//lock이 있어야 하나? atomic으로하자
					//reinterpret_cast<Npc*>(clients[other])->is_active = true;
					//timer_event t;
					//t.ev = EVENT_NPC_MOVE;
					//t.obj_id = other;
					//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);
					//timer_queue.push(t);
					continue;
				}

				Client* otherPlayer = reinterpret_cast<Client*>(clients[other]);
				// 나한테 보이면 상대에게도 보인다는 뜻이니
				// 상대 뷰리스트도 확인한다.
				otherPlayer->vl.lock();

				// 상대 뷰리스트에 없으면
				if (0 == otherPlayer->viewlist.count(cl.id)) {
					// 뷰리스트에 추가하고 cl을 그리라고 전송
					otherPlayer->viewlist.insert(cl.id);
					otherPlayer->vl.unlock();
					send_put_object(other, cl.id);
				}
				// 상대 뷰리스트에 있으면 이동 패킷 전송
				else {
					otherPlayer->vl.unlock();
					send_move_object(other, cl.id);
				}

			}
			//계속 시야에 존재하는 플레이어 처리
			else {

				if (true == is_npc(other)) continue;
				Client* otherPlayer = reinterpret_cast<Client*>(clients[other]);
				otherPlayer->vl.lock();
				//상대방에 뷰리스트에 내가 있는지 확인
				if (0 != otherPlayer->viewlist.count(cl.id))
				{
					otherPlayer->vl.unlock();

					send_move_object(other, cl.id);
				}
				else {
					otherPlayer->viewlist.insert(cl.id);
					otherPlayer->vl.unlock();

					send_put_object(other, cl.id);
				}
			}
		}


		// 움직임으로써 시야에서 빠진 플레이어 확인 및 제거
		for (int other : my_vl) {
			// nearlist에 없으면
			if (0 == nearlist.count(other)) {
				// 나한테서 지우고
				cl.vl.lock();
				cl.viewlist.erase(other);
				cl.vl.unlock();

				//npc는 view리스트를 가지고 있지 않다.
				if (true == is_npc(other)) {
					//reinterpret_cast<Npc*>(clients[other])->is_active = false;
					continue;
				}
				send_remove_object(cl.id, other);
				Client* otherPlayer = reinterpret_cast<Client*>(clients[other]);

				// 상대방도 나를 지운다.
				otherPlayer->vl.lock();
				//있다면 지움
				if (0 != otherPlayer->viewlist.count(cl.id)) {

					otherPlayer->viewlist.erase(cl.id);
					otherPlayer->vl.unlock();

					send_remove_object(other, cl.id);
				}
				else otherPlayer->vl.unlock();
			}

		}
	}
	break;

	// change 씬이 되면 씬이 바뀐걸 서버에 알려주고 
	// 그러면 서버에서 플레이어 아이디를 보내주자
	case CS_PACKET_CHANGE_SCENE_READY:
	{
		// 올바른 위치에서 ready했는지 확인
		cs_packet_change_scene_ready* packet = reinterpret_cast<cs_packet_change_scene_ready*>(p);


		if (packet->is_ready) {
			for (auto* p : portals) {
				if (false == p->isPortal(cl.x, cl.z)) continue;
				// 포탈에 들어오거나 나가서 plaer_ids를 수정해야하는 경우는 해당 패킷이 왔을 때 딱 한번 발생한다. -> lock을 한 번만 하면됨
				//그래서 player_ids를 복사해 수정한 후 복사하는 방법을 lock횟수가 같기 때문에 그냥 lock을 건다.

				p->id_lock.lock();
				p->player_ids.insert(cl.id);

				// 준비 이펙트 전송
				GameObject* players[MAX_IN_GAME_PLAYER];
				if (p->player_ids.size() >= MAX_IN_GAME_PLAYER) {
					// 씬 전환

					int i = 0;
					for (int id : p->player_ids) {
						players[i] = clients[id];
						i++;
						maps[FIELD_MAP]->SetTileType(-1, -1, clients[id]->x, clients[id]->z);

						if (i > MAX_IN_GAME_PLAYER) break;
					}
					//std::cout << "시작" << std::endl;
					int room_id = get_game_room_id();
					int boss_id = get_npc_id(1);
					if (boss_id != -1)
						game_room[room_id]->GameRoomInit(p->map_type, maps[p->map_type]->bpm, clients[boss_id], players, p);
					p->ready_player_cnt = 0;

					for (int id : p->player_ids) {

						send_change_scene(id, p->map_type + 2);

					}


					p->player_ids.clear();
					// 포탈에서 GameRoom으로 이동

				}
				p->id_lock.unlock();
				break;
			}
		}
		else {
			for (auto* p : portals) {
				if (false == p->findPlayer(client_id)) continue;
				p->id_lock.lock();
				p->player_ids.erase(cl.id);
				p->id_lock.unlock();

				break;
			}
		}
	}
	break;
	case CS_PACKET_CHANGE_SCENE_DONE:
	{
		cs_packet_change_scene_done* packet = reinterpret_cast<cs_packet_change_scene_done*>(p);
		switch (packet->scene_num)
		{
		case FIELD_MAP:
		{
			cl.vl.lock();
			cl.viewlist.clear();
			cl.vl.unlock();
			if (cl.cur_room_num != -1) {
				if (game_room[cl.cur_room_num]->portal != nullptr) {
					cl.x = game_room[cl.cur_room_num]->portal->x + 1;
					cl.z = game_room[cl.cur_room_num]->portal->z + 1;
					cl.y = -cl.x - cl.z;
					if (set_new_player_pos(client_id) == -1) {
						cl.x = game_room[cl.cur_room_num]->portal->x;
						cl.z = game_room[cl.cur_room_num]->portal->z;
						cl.y = -cl.x - cl.z;
						// 빈자리가 없어서 다시 인게임으로 들어감
					}
				}
				else {
					cl.x = 13;
					cl.z = -25;
					cl.y = -cl.x - cl.z;
					set_new_player_pos(client_id);
				}
			}


			cl.cur_room_num = -1;

			// login OK 에서 했던 로직을 가져오자
			//다른 클라이언트에게 새로운 클라이언트가 들어옴을 알림
			for (int i = 0; i < MAX_USER; ++i)
			{
				Client* other = reinterpret_cast<Client*>(clients[i]);
				if (i == client_id) continue;
				other->state_lock.lock();
				if (ST_INGAME != other->state) {
					other->state_lock.unlock();
					continue;
				}
				other->state_lock.unlock();
				if (other->cur_room_num != -1) continue;
				if (false == is_near(other->id, client_id))
					continue;

				// 새로 들어온 클라이언트가 가까이 있다면 뷰 리스트에 넣고 put packet을 보낸다.
				other->vl.lock();
				other->viewlist.insert(client_id);
				other->vl.unlock();

				send_put_object(other->id, client_id);
			}

			//새로 접속한 클라이언트에게 현재 객체들의 현황을 알려줌
			for (auto* other : clients) {
				//여기서 NPC도 알려줘야지

				if (other->id == client_id) continue;
				other->state_lock.lock();
				if (ST_INGAME != other->state) {
					other->state_lock.unlock();
					continue;
				}
				other->state_lock.unlock();
				if (other->cur_room_num != -1) continue;
				if (false == is_near(other->id, client_id))
					continue;

				// 기존에 있던 클라이언트가 가까이 있다면 뷰 리스트에 넣고 put packet을 보낸다.
				cl.vl.lock();
				cl.viewlist.insert(other->id);
				cl.vl.unlock();

				send_put_object(client_id, other->id);
			}

			send_move_object(client_id, client_id);
			if (cur_play_music != 99)
				send_use_item(client_id, 0, cur_play_music);
		}
		break;
		case TUTORI_MAP:
		{
			GameObject* players[MAX_IN_GAME_PLAYER];

			maps[FIELD_MAP]->SetTileType(-1, -1, cl.x, cl.z);
			players[0] = clients[client_id];
			players[1] = nullptr;
			players[2] = nullptr;

			std::cout << "set room\n";
			int room_id = get_game_room_id();
			int boss_id = get_npc_id(1);
			if (boss_id != -1)
				game_room[room_id]->GameRoomInit(TUTORI_MAP, maps[TUTORI_MAP]->bpm, clients[boss_id], players, nullptr);

			//game init
			if (cl.cur_room_num == -1) break;
			GameRoom* gr = game_room[cl.cur_room_num];

			if (-1 == gr->FindPlayer(client_id)) break;

			cl.vl.lock();
			std::unordered_set <int> my_vl = cl.viewlist;
			cl.viewlist.clear();
			for (auto p : gr->player_ids) {
				if (p == nullptr) continue;

				cl.viewlist.insert(p->id);
			}

			cl.vl.unlock();

			for (auto other : my_vl) {
				if (false == is_player(other)) continue;

				Client& target = *reinterpret_cast<Client*>(clients[other]);
				if (-1 != target.cur_room_num) continue;
				if (ST_INGAME != target.state)
					continue;
				target.vl.lock();
				if (0 != target.viewlist.count(client_id)) {
					target.viewlist.erase(client_id);
					target.vl.unlock();
					send_remove_object(other, cl.id);

				}
				else target.vl.unlock();
			}
			send_game_init(client_id, gr->player_ids, gr->boss_id->id);


			//game start
			gr->start_time = std::chrono::system_clock::now();
			int game_start_time = static_cast<int>(std::chrono::time_point_cast<std::chrono::milliseconds>(gr->start_time).time_since_epoch().count());
			game_start(gr->game_room_id);

			send_game_start(client_id, game_start_time);
			Client* p = reinterpret_cast<Client*>(players[0]);
		}
		break;
		case 2:// 1 == in game map num
		{
			if (cl.cur_room_num == -1) break;
			GameRoom* gr = game_room[cl.cur_room_num];


			if (-1 == gr->FindPlayer(client_id)) break;

			cl.vl.lock();
			std::unordered_set <int> my_vl = cl.viewlist;
			cl.viewlist.clear();
			for (auto p : gr->player_ids) {
				if (p == nullptr) continue;

				cl.viewlist.insert(p->id);
			}

			cl.vl.unlock();



			for (auto other : my_vl) {
				if (false == is_player(other)) continue;

				Client& target = *reinterpret_cast<Client*>(clients[other]);
				if (-1 != target.cur_room_num) continue;
				if (ST_INGAME != target.state)
					continue;
				target.vl.lock();
				if (0 != target.viewlist.count(client_id)) {
					target.viewlist.erase(client_id);
					target.vl.unlock();
					send_remove_object(other, cl.id);

				}
				else target.vl.unlock();
			}
			send_game_init(client_id, gr->player_ids, gr->boss_id->id);


		}
		break;

		default:
			break;
		}


	}
	break;
	case CS_PACKET_GAME_START_READY:
	{
		cs_packet_game_start_ready* packet = reinterpret_cast<cs_packet_game_start_ready*>(p);

		for (auto* gr : game_room) {
			if (false == gr->isGaming) continue;
			gr->ready_lock.lock();
			if (-1 == gr->FindPlayer(client_id)) { gr->ready_lock.unlock(); continue; }
			gr->ready_player_cnt++;
			if (gr->ready_player_cnt >= MAX_IN_GAME_PLAYER) {
				//static_cast<int>(time_point_cast<milliseconds>(ev.start_time).time_since_epoch().count());
				gr->start_time = std::chrono::system_clock::now();
				int game_start_time = static_cast<int>(std::chrono::time_point_cast<std::chrono::milliseconds>(gr->start_time).time_since_epoch().count());
				game_start(gr->game_room_id);

				for (const auto pl : gr->player_ids) {
					send_game_start(pl->id, game_start_time);
					Client* p = reinterpret_cast<Client*>(pl);
					p->cur_room_num = gr->game_room_id;
				}



				std::cout << "Game Start\n";

			}
			//gr->ready_player_cnt = 0;
			gr->ready_lock.unlock();
			break;
		}

	}
	break;
	case CS_PACKET_PARRYING:
	{
		//이 맵의 모든 패턴 중에서 running_time주변 시간에 패링 공격이 있는지 확인하고
		//있다면 패링 성공 패킷을 보낸다.
		cs_packet_parrying* packet = reinterpret_cast<cs_packet_parrying*>(p);
#ifdef DEBUG
		//printf("%d : I want parrying!!\n", client_id);
#endif
		GameRoom* gr = game_room[reinterpret_cast<Client*>(clients[client_id])->cur_room_num];

		// 해당 플레이어의 게임 방을 찾고
		if (-1 == gr->FindPlayer(client_id)) { std::cout << "Can not find player game room\n"; }
		int id = gr->FindPlayerID_by_GameRoom(client_id);
		//게임시간이 얼마나 지났는지 확인하고
		int running_time = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - gr->start_time).count();
		// 현재시간에 패링패턴이 있었는지 확인
		const std::vector<PatternInfo>& pt = maps[gr->map_type]->GetPatternTime();


		for (auto& pattern : pt) {
			// 패링 노트인지 채크
			if (10 != pattern.type) {
				continue;
			}
			// 타겟이 된 플레이어인지 체크
			if (id != (pattern.pivotType - 4)) {
				continue;
			}

			//100ms보다 작으면
			if (abs(running_time - pattern.time) < 600) {
				//패링 성공
				// 패링 성공 패킷을 클라이언트로 보냄
#ifdef DEBUG
				//printf("%d parry successed : %d in %d\n", id, running_time, pattern.time);
#endif
				reinterpret_cast<Client*>(clients[client_id])->pre_parrying_pattern
					= std::chrono::time_point_cast<std::chrono::milliseconds>(std::chrono::system_clock::now()).time_since_epoch().count();
				
				gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)] += 14 * cl.power;
				//수정
				gr->boss_id->hp -= 14 *cl.power;
				if (gr->boss_id->hp < 0) {
					timer_event tev;
					tev.ev = EVENT_GAME_END;
					tev.game_room_id = gr->game_room_id;
					//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
					tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

					timer_queue.push(tev);
				}
				for (const auto pl : gr->player_ids) {
					if (pl == nullptr) continue;
					send_parrying(pl->id, client_id);
					send_score(cl.id, gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)], pl->id);

					send_attack_player(client_id, gr->boss_id->id, pl->id);
				}
			}
		}
		//						//패링 성공
		//				// 패링 성공 패킷을 클라이언트로 보냄
		//#ifdef DEBUG
		//		printf("%d parry successed", id);
		//#endif
		//		reinterpret_cast<Client*>(clients[client_id])->pre_parrying_pattern
		//			= std::chrono::time_point_cast<std::chrono::milliseconds>(std::chrono::system_clock::now()).time_since_epoch().count();
		//
		//		//수정
		//		gr->boss_id->hp -= 14;
		//		if (gr->boss_id->hp < 0) {
		//			timer_event tev;
		//			tev.ev = EVENT_GAME_END;
		//			tev.game_room_id = gr->game_room_id;
		//			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
		//			tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);
		//
		//			timer_queue.push(tev);
		//		}
		//		for (const auto pl : gr->player_ids) {
		//			if (pl == nullptr) continue;
		//			send_parrying(pl->id, client_id);
		//			send_attack_player(client_id, gr->boss_id->id, pl->id);
		//		}
	}
	break;
	case CS_PACKET_USE_SKILL:
	{
		/*
		* 스킬 구현에 필요한 것
		*	- 클라이언트에서 서버로 보내는 패킷
		*		- 패킷, 클라이언트 작업..
		*			- 클라 패킷에는 사용한 스킬 종류를 보낼 필요는 없고 그냥 사용했다..라고만 보내면 알아서 함
		*	- 성장 가능한 스킬
		*		- 플레이어마다 db에 저장해두고 불러올 예정
		*	- 스킬에 따른 알맞은 알고리즘
		*		- class를 이용해 해결
		*	시작
		*/

		/*
		* 플레이어는 스킬을 가지고 사용한다.
		* 자신의 게임방에 있는 친구들에게 자신의 스킬 사용 유무를 알려줘야 한다.
		*/
		cs_packet_use_skill* packet = reinterpret_cast<cs_packet_use_skill*>(p);
		Client* cl = reinterpret_cast<Client*>(clients[client_id]);
		GameRoom* gr = game_room[cl->cur_room_num];


		if ((cl->last_skill_time + std::chrono::milliseconds(maps[gr->map_type]->timeByBeat * cl->skill->CoolTime)) > std::chrono::system_clock::now()) break;
		cl->last_skill_time = std::chrono::system_clock::now();
		//std::cout << maps[gr->map_type]->timeByBeat * cl->skill->CoolTime << std::endl;


		/*
		* 같은 방에 있는 친구들을 찾고
		*
		* 해당 친구들에게 스킬 이펙트를 보내고
		*
		* 딜레이 시간만큼 뒤에 해당 블럭에 있는 플레이어나 보스와 상호 작용
		* -> 이렇게 하려면 이벤트를 등록해야되는데 그냥 그래 그러자
		*
		* 타겟팅인 경우 그냥 끝
		*/

		/*
		* 나의 레벨에 맞는 이펙트와 스킬을 보내야함
		* 이펙트 딜레이를 계산해야 되는데 그러면 내가 플레이 중인 맵에 노래에 맞춰서 딜레이를 보여야 함
		*/
		for (const auto& pl : gr->player_ids) {
			if (pl == nullptr) continue;
			const Skill* plskill = reinterpret_cast<Client*>(pl)->skill;
			send_effect(pl->id, client_id, gr->boss_id->id, 55, 1000, cl->direction, cl->skill->SkillLevel, cl->skill->SkillType, -1);
		}

		//플레이어 스킬 이벤트 등록

		//timer_event tev;
		//tev.game_room_id = gr->game_room_id;
		//tev.charging_time = cl->skill.Delay;
		//tev.ev = EVENT_PLAYER_SKILL;
		//tev.x = cl->skill.SkillLevel;
		//tev.y = cl->skill.SkillType;
		//tev.start_time = 
		//timer_queue.push(tev);

		// 그냥 if문으로 구분하자... -> 클라이언트의 상호작용을 효율적으로 설계할 수 없을까?
		//플레이어가 가지고 있는 스킬을 타입과 레벨을 확인해 그에 맞는 걸 하자
		do_player_skill(gr, cl);
	}
	break;
	case CS_PACKET_CHANGE_SKILL:
	{
		cs_packet_change_skill* packet = reinterpret_cast<cs_packet_change_skill*>(p);

		/*
		* 스킬이 풀려있는지 보고 바꿔준다. -> db연결하면 하자
		* 스킬특성이 있는 파일이 있어야 함 -> 파일 입출력을 해서 서버 시작할 때 읽어 오도록 -> 달라지는게 쿨타임, 데미지 뿐인데 그냥 level에 비례하게 하면 될거 같음
		*
		* 일단 패킷이 오면 해당 타입의 스킬 1레벨로 바꾸는 걸로 하자
		* 나중에는 db에 접근해 스킬이 풀려있는지 확인하고 해당 레벨의 능력치로 맞춰서 알려주자
		*/
		Client* c = reinterpret_cast<Client*>(clients[client_id]);
		int skill_index = ((packet->skill_type - 1) * 4 + packet->skill_level);
		if (skill_index >= SKILL_CNT) { std::cout << "change skill : wrong skillType or skillLevel\n"; break; }
		//std::cout << (int)packet->skill_type<<", "<< (int)packet->skill_level << std::endl;
		//std::cout << (int)skills[skill_index]->SkillType<<", "<< (int)skills[skill_index]->SkillLevel << std::endl;
		if (packet->skill_type == 1)
			if (c->SkillAD < packet->skill_level) break;
			else if (packet->skill_type == 2)
				if (c->SkillAD < packet->skill_level) break;
				else if (packet->skill_type == 3)
					if (c->SkillAD < packet->skill_level) break;

		c->skill = skills[skill_index];
		c->vl.lock();
		std::unordered_set<int> my_vl{ c->viewlist };
		c->vl.unlock();
		for (int i : my_vl) {
			if (false == is_player(i)) continue;
			send_change_skill(i, client_id);
		}
		send_change_skill(client_id, client_id);

	}
	break;
	case CS_PACKET_PING_TEST:
	{
		cs_packet_ping_test* packet = reinterpret_cast<cs_packet_ping_test*>(p);

		send_ping(client_id, packet->ping_time);
	}
	break;
	case CS_PACKET_PARTY_REQUEST:
	{
		cs_packet_party_request* packet = reinterpret_cast<cs_packet_party_request*>(p);

		if (false == is_player(packet->id) || packet->id == client_id) break;
		// 상대가 파티가 있는 경우
		Client* accepter = reinterpret_cast<Client*>(clients[packet->id]);
		Party* accepterParty = accepter->party;
		if (accepterParty != nullptr) {
			send_party_request_anwser(client_id, -1, nullptr, 4);

			break;
		}


		send_party_request(packet->id, client_id);


	}
	break;
	case CS_PACKET_PARTY_REQUEST_ANSWER:
	{
		cs_packet_party_request_anwser* packet = reinterpret_cast<cs_packet_party_request_anwser*>(p);
		if (false == is_player(packet->requester)) break;
		Client* accepter = reinterpret_cast<Client*>(clients[client_id]);


		// 이 경우는 파티를 탈퇴할 때
		if (client_id == packet->requester) {
			if (accepter->party == nullptr) { std::cout << "can't empty party exit\n"; break; }

			for (auto p : accepter->party->partyPlayer) {
				if (p == -1)continue;
				send_party_request_anwser(p, client_id, accepter->party, 3);
			}

			accepter->party->DelPartyPlayer(client_id);


			if (accepter->party->curPlayerNum == 1) {
				int last_id = accepter->party->DelParty();
				Client* lastPartyPlayer = reinterpret_cast<Client*>(clients[last_id]);

				lastPartyPlayer->party = nullptr;
			}

			accepter->party = nullptr;
			break;
		}

		Client* requester = reinterpret_cast<Client*>(clients[packet->requester]);

		Party* requesterParty = requester->party;
		if (packet->answer == 1)
		{

			//기존 파티가 없는 상태에서 파티 신청을 한거면 새로운 파티를 지정해줌
			if (requesterParty == nullptr) {
				for (auto p : PartyPool) {
					p->partyLock.lock();
					if (p->curPlayerNum == 0) {
						requesterParty = requester->party = p;
						accepter->party = p;
						p->partyPlayer[p->curPlayerNum++] = requester->id;
						p->partyPlayer[p->curPlayerNum++] = accepter->id;
						p->partyLock.unlock();
						break;
					}
					p->partyLock.unlock();
				}
			}
			else {
				// 파티 요청한 사람이 파티가 있다면 그 파티에 들어감

				if (false == requesterParty->SetPartyPlayer(client_id)) {
					//파티 요청한 사람의 파티가 가득 찬경우
					//수락한 사람한테 다시 거절 메세지를 보냄
					send_party_request_anwser(client_id, -1, nullptr, 2);


					break;

				}
				accepter->party = requester->party;

			}
			for (auto p : requesterParty->partyPlayer) {
				if (p != -1)
					send_party_request_anwser(p, client_id, requesterParty, 1);
			}

		}
		else if (packet->answer == 0)
		{
			if (requesterParty == nullptr) {
				send_party_request_anwser(packet->requester, -1, nullptr, 0);
			}
			else {
				for (auto p : requesterParty->partyPlayer) {
					if (p != -1)
						send_party_request_anwser(p, -1, requesterParty, 0);
				}
			}
		}

	}
	break;
	case CS_PACKET_CHAT:
	{
		cs_packet_chat* packet = reinterpret_cast<cs_packet_chat*>(p);
		//std::cout << packet->mess << std::endl;


		if (packet->sendType == 0)
		{

			for (int i = 0; i < MAX_USER; ++i) {
				if (ST_INGAME != clients[i]->state) continue;
				send_chat_packet(i, client_id, packet->mess);
			}

		}
		else if (packet->sendType == 1) {
			Client* client = reinterpret_cast<Client*>(clients[client_id]);
			if (client->party == nullptr) break;
			for (int id : client->party->partyPlayer) {
				if (id == -1) continue;
				send_chat_packet(id, client_id, packet->mess);

			}
			//send_chat_packet(client_id, client_id, packet->mess);

		}
		else if (packet->sendType == 2) {

			for (int i = 0; i < MAX_USER; ++i) {
				if (ST_INGAME != clients[i]->state) continue;
				if (i == packet->reciver) {
					send_chat_packet(i, client_id, packet->mess);
					break;
				}
			}
			send_chat_packet(client_id, client_id, packet->mess);
		}

	}
	break;
	case CS_PACKET_SET_PATH:
	{
		cs_packet_set_path* packet = reinterpret_cast<cs_packet_set_path*>(p);
		if (clients[client_id]->dest_x == -1) {
			clients[client_id]->dest_x = packet->x;
			clients[client_id]->dest_z = packet->z;
			clients[client_id]->dest_y = -packet->x - packet->z;

			timer_event tev;
			tev.ev = EVENT_PALYER_MOVE;
			tev.obj_id = client_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
			tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

			timer_queue.push(tev);
		}
		else {
			clients[client_id]->dest_x = packet->x;
			clients[client_id]->dest_z = packet->z;
			clients[client_id]->dest_y = -packet->x - packet->z;
		}

	}
	break;
	case CS_PACKET_TELEPORT:
	{
		cs_packet_teleport* packet = reinterpret_cast<cs_packet_teleport*>(p);

		if (packet->pos == 0) {
			/*
			17   4 - 21
			16   4 - 20
			17   3 - 20
			*/
			int x[3] = { 17,16,17 };
			int y[3] = { 4,4,3 };
			int z[3] = { -21,-20,-20 };
			cl.pre_x = cl.x;
			cl.pre_y = cl.y;
			cl.pre_z = cl.z;
			for (int i = 0; i < 3; ++i) {
				if (maps[FIELD_MAP]->GetTileType(x[i], z[i]) >= 0) {
					cl.x = x[i];
					cl.y = y[i];
					cl.z = z[i];
					maps[FIELD_MAP]->SetTileType(x[i], z[i], cl.pre_x, cl.pre_z);
					break;
				}
			}


		}
		else if (packet->pos == 5) {
			//14 - 15	1
			//15 - 15	0
			//15	-16	1

			int x[3] = { 14,15,15 };
			int y[3] = { -15,-15,-16 };
			int z[3] = { 1,0,1 };
			cl.pre_x = cl.x;
			cl.pre_y = cl.y;
			cl.pre_z = cl.z;
			for (int i = 0; i < 3; ++i) {
				if (maps[FIELD_MAP]->GetTileType(x[i], z[i]) >= 0) {
					cl.x = x[i];
					cl.y = y[i];
					cl.z = z[i];
					maps[FIELD_MAP]->SetTileType(x[i], z[i], cl.pre_x, cl.pre_z);
					break;
				}
			}
		}
		else if (packet->pos == 1) {
			//  임시로 재화를 획득하는 패킷으로 변경한다.
			cl.SetMoney(100);
			std::cout << "id :" << client_id << " get 100 money \ntotal : " << cl.GetMoney() << std::endl;
		}
		else if (packet->pos == 2) {
			int room_id = reinterpret_cast<Client*>(clients[client_id])->cur_room_num;
			if (room_id == -1) break;
			GameRoom* gr = game_room[room_id];

			//보스의 체력을 깍는다.
			if (gr->boss_id->Hit(500)) {
				timer_event tev;
				tev.ev = EVENT_GAME_END;
				tev.game_room_id = gr->game_room_id;
				//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
				tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

				timer_queue.push(std::move(tev));
			}
			for (const auto pl : gr->player_ids) {
				if (pl == nullptr) continue;
				send_attack_player(client_id, gr->boss_id->id, pl->id);
			}
		}
		else if (packet->pos == 3) {
			input_db_event(client_id, DB_GET_SCROLL, 1);
			break;
		}
		else if (packet->pos == 4) { // 데미지 치트
			std::cout << "damage cheat\n";
			if (damamge_bug >= 0.9f) {

				damamge_bug = 0.1f;
			}
			else
				damamge_bug = 1.0f;

		}
		else {
			cl.pre_x = cl.x;
			cl.pre_y = cl.y;
			cl.pre_z = cl.z;
			do
			{
				cl.x = rand() % 20;
				cl.z = rand() % 20;
				cl.y = -cl.x - cl.z;
			} while (maps[FIELD_MAP]->GetTileType(cl.x, cl.z) != 0);

		}
		maps[FIELD_MAP]->SetTileType(cl.x, cl.z, cl.pre_x, cl.pre_z);
		cl.vl.lock();
		std::unordered_set<int> my_vl{ cl.viewlist };
		cl.vl.unlock();

		for (int id : my_vl) {
			if (false == is_player(id)) continue;
			send_move_object(id, client_id);
		}
		send_move_object(client_id, client_id);

	}
	break;
	case CS_PACKET_BUY:
	{
		cs_packet_buy* packet = reinterpret_cast<cs_packet_buy*>(p);
		if (false == is_item(packet->itemType))break;
		//std::cout << (int)packet->itemType << std::endl;
		if (cl.money < skills[packet->itemType]->SkillPrice) {
			send_buy_result(client_id, packet->itemType, 0); break;
		}
		switch (packet->itemType / 4)
		{
		case 0:
			cl.SkillAD = max(cl.SkillAD, packet->itemType % 4);
			break;
		case 1:
			cl.SkillTa = max(cl.SkillTa, packet->itemType % 4);
			break;
		case 2:
			cl.SkillHeal = max(cl.SkillHeal, packet->itemType % 4);
			break;
		default:
			break;
		}


		cl.money -= skills[packet->itemType]->SkillPrice;
		send_buy_result(client_id, packet->itemType, 1);
	}
	break;
	case CS_PACKET_USE_ITEM:
		//DB로 아이템을 사용했다고 이벤트를 보낸다.
	{
		//오르골 플레이 중에 플레이어가 들어오면 들어온 플레이어도 음악을 들을 수 있어야 한다.
		cs_packet_buy* packet = reinterpret_cast<cs_packet_buy*>(p);
		if (packet->itemType == 99) {
			for (int i = 0; i < MAX_USER; ++i)
			{
				Client* other = reinterpret_cast<Client*>(clients[i]);
				//if (i == client_id) continue;
				other->state_lock.lock();
				if (ST_INGAME != other->state) {
					other->state_lock.unlock();
					continue;
				}
				other->state_lock.unlock();

				send_use_item(i, client_id, 99);
			}
			cur_play_music = packet->itemType;
			break;
		}
		input_db_event(client_id, DB_USE_SCROLL, packet->itemType);
		if (DB->isConnect == false) {
			for (int i = 0; i < MAX_USER; ++i)
			{
				Client* other = reinterpret_cast<Client*>(clients[i]);
				//if (i == client_id) continue;
				other->state_lock.lock();
				if (ST_INGAME != other->state) {
					other->state_lock.unlock();
					continue;
				}
				other->state_lock.unlock();

				send_use_item(i, client_id, packet->itemType);
			}
		}

	}
	break;

	case CS_PACKET_GET_ITEM:
	{
		cs_packet_get_item* packet = reinterpret_cast<cs_packet_get_item*>(p);

		if (cl.cur_room_num == -1) break;
		for (auto p : game_room[cl.cur_room_num]->player_ids) {
			if (p == nullptr) continue;
			send_use_item(p->id, cl.id, packet->itemType);
		}
		GameRoom* gr = game_room[cl.cur_room_num];
		cl.power = 1;
		cl.armour = 0;
		switch (packet->itemType)
		{
		case DIR::UP: // 점수

			gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)] += 500;

			for (const auto pl : gr->player_ids) {
				if (pl == nullptr) continue;
				send_score(cl.id, gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)], pl->id);
				
			}
			

			break;
		case DIR::DOWN: // 트루뎀
			if (gr->boss_id->Hit(100)) {
				timer_event tev;
				tev.ev = EVENT_GAME_END;
				tev.game_room_id = gr->game_room_id;
				//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
				tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

				timer_queue.push(tev);
			}
			gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)] += 100;
			for (const auto pl : gr->player_ids) {
				if (pl == nullptr) continue;
				send_score(cl.id, gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)], pl->id);
				send_attack_player(cl.id, gr->boss_id->id, pl->id);

			}
			
			break;
		case DIR::LEFTDOWN: // 공증 -> 게임 끝날 때 까지 지속
			cl.power = 3;
			break;
		case DIR::RIGHTDOWN: // 방증
			cl.armour = 10;
			break;
		case DIR::LEFTUP: // 체력회복
			for (auto pl : game_room[cl.cur_room_num]->player_ids) {
				if (pl == nullptr) continue;
				pl->Hit(-50);
				if (pl->hp > 100)
					pl->hp = 100;
				send_score(cl.id, gr->Score[gr->FindPlayerID_by_GameRoom(cl.id)], pl->id);

				send_attack_player(cl.id, cl.id, pl->id);
			}
			break;
		default:
			break;
		}
	}
	break;
	default:
		std::cout << "wrong packet\n";
		break;
	}
}

void Network::worker()
{
	while (true) {
		DWORD num_byte;
		LONG64 iocp_key;
		WSAOVERLAPPED* p_over;
		BOOL ret = GetQueuedCompletionStatus(g_h_iocp, &num_byte, (PULONG_PTR)&iocp_key, &p_over, INFINITE);

		int client_id = static_cast<int>(iocp_key);
		EXP_OVER* exp_over = reinterpret_cast<EXP_OVER*>(p_over);

		if (FALSE == ret) {

			error_display("GQCS", WSAGetLastError());

			disconnect_client(client_id);
			if (exp_over->_comp_op == OP_SEND ||
				exp_over->_comp_op == OP_BOSS_MOVE ||
				exp_over->_comp_op == OP_PLAYER_PARRYING ||
				exp_over->_comp_op == OP_BOSS_TILE_ATTACK_START ||
				exp_over->_comp_op == OP_BOSS_TILE_ATTACK ||
				exp_over->_comp_op == OP_GAME_END ||
				exp_over->_comp_op == OP_DB_PLAYER_LOGIN ||
				exp_over->_comp_op == OP_DB_USE_ITEM
				)
				exp_over_pool.push(exp_over);
			continue;
		}

		switch (exp_over->_comp_op)
		{
		case OP_RECV:
		{
			if (num_byte == 0) {
				disconnect_client(client_id);
				continue;
			}
			//하나의 소켓에 대해 Recv호출은 언제나 하나 -> EXP_OVER(버퍼, WSAOVERLAPPED) 재사용 가능
			//패킷이 중간에 잘려진 채로 도착할 수 있다. -> 버퍼에 놔두었다가 다음에 온 데이터와 결합 -> 이전에 받은 크기를 기억해 그 위치부터 받기 시작하자
			//패킷이 여러 개 한번에 도착할 수 있다.	 -> 첫 번째가 사이즈이니 잘라서 처리하자
			Client& cl = *reinterpret_cast<Client*>(clients[client_id]);

			int remain_data = cl.prev_recv_size + num_byte;
			unsigned char* packet_start = exp_over->_net_buf;
			int packet_size = packet_start[0];

			while (packet_size <= remain_data) {
				process_packet(client_id, packet_start);
				remain_data -= packet_size;
				packet_start += packet_size;
				if (remain_data > 0)
					packet_size = packet_start[0];
			}


			cl.prev_recv_size = remain_data;
			if (remain_data) {
				memcpy_s(&exp_over->_net_buf, remain_data, packet_start, remain_data);
			}

			cl.do_recv();
		}
		break;
		case OP_SEND:
		{
			if (num_byte != exp_over->_wsa_buf.len) {
				std::cout << num_byte << " 송신버퍼 가득 참\n";
				std::cout << "클라이언트 연결 끊음\n";
				disconnect_client(client_id);
			}

			exp_over_pool.push(exp_over);
		}
		break;
		case OP_ACCEPT:
		{
			std::cout << "Accept Completed.\n";
			SOCKET c_socket = *(reinterpret_cast<SOCKET*>(exp_over->_net_buf)); // 확장 overlapped구조체에 넣어 두었던 소캣을 꺼낸다
			int new_id = get_new_id();
			if (-1 == new_id) { std::cout << "need more ID\n"; continue; }

			Client& cl = *(reinterpret_cast<Client*>(clients[new_id]));

			//13, 12, -25
			//4, 0, -4

			cl.id = new_id;
			cl.prev_recv_size = 0;
			cl.recv_over._comp_op = OP_RECV;
			//cl._state = ST_INGAME;
			cl.recv_over._wsa_buf.buf = reinterpret_cast<char*>(cl.recv_over._net_buf);
			cl.recv_over._wsa_buf.len = sizeof(cl.recv_over._net_buf);
			ZeroMemory(&cl.recv_over._wsa_over, sizeof(cl.recv_over._wsa_over));
			cl.socket = c_socket;

			CreateIoCompletionPort(reinterpret_cast<HANDLE>(c_socket), g_h_iocp, new_id, 0);

			cl.do_recv();

			// exp_over 재활용
			ZeroMemory(&exp_over->_wsa_over, sizeof(exp_over->_wsa_over));
			c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

			*(reinterpret_cast<SOCKET*>(exp_over->_net_buf)) = c_socket;

			AcceptEx(g_s_socket, c_socket, exp_over->_net_buf + sizeof(SOCKET), 0, sizeof(SOCKADDR_IN) + 16,
				sizeof(SOCKADDR_IN) + 16, NULL, &exp_over->_wsa_over);
		}
		break;
		case OP_BOSS_MOVE:
		{
			int x = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int y = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));
			int z = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 3));
			int pivotType = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 4));

			int target_id = game_room[game_room_id]->find_online_player();

			int pivot_x, pivot_y, pivot_z;

			if (target_id == -1) { game_room[game_room_id]->pattern_progress = -1; break; }
			// 보스가 무엇을 기준으로 움직이는지
			switch (pivotType)
			{
			case PlayerM:
				target_id = game_room[game_room_id]->find_max_hp_player();
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case Playerm:
				target_id = game_room[game_room_id]->find_min_hp_player();
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case World:
				pivot_x = 0;
				pivot_y = 0;
				pivot_z = 0;
				break;
			case Boss:
				target_id = client_id;
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case Player1:
				//case Player2:
				//case Player3:
				if (game_room[game_room_id]->player_ids[0] != nullptr)
					target_id = game_room[game_room_id]->player_ids[0]->id;
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case Player2:
				if (game_room[game_room_id]->player_ids[1] != nullptr)
					target_id = game_room[game_room_id]->player_ids[1]->id;
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case Player3:
				if (game_room[game_room_id]->player_ids[2] != nullptr)
					target_id = game_room[game_room_id]->player_ids[2]->id;
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			default:
				std::cout << "wrong pivotType" << std::endl;
				pivot_x = 0;
				pivot_y = 0;
				pivot_z = 0;
				break;
			}

			Client& cl = *reinterpret_cast<Client*>(clients[client_id]);
			cl.x = x + pivot_x;
			cl.y = y + pivot_y;
			cl.z = z + pivot_z;
			cl.direction = rand() % 6;
			for (const auto pl : game_room[game_room_id]->player_ids) {
				if (pl == nullptr) continue;
				send_move_object(pl->id, client_id);
			}
			set_next_pattern(game_room_id);

			exp_over_pool.push(exp_over);
		}
		break;
		case OP_PLAYER_PARRYING:
		{
			int target_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));
			int pivotType = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			long long parrying_end_time = *(reinterpret_cast<long long*>(exp_over->_net_buf + sizeof(int) * 3));//std::chrono::system_clock::now();//

			target_id = game_room[game_room_id]->find_online_player();
			if (target_id == -1) { game_room[game_room_id]->pattern_progress = -1; break; }

			switch (pivotType)
			{
			case PlayerM:
				target_id = game_room[game_room_id]->find_max_hp_player();
				break;
			case Playerm:
				target_id = game_room[game_room_id]->find_min_hp_player();
				break;

			case Boss:
				//target_id = game_room[game_room_id]->boss_id->id;
				std::cout << "Boss can not Parrying self\n";
				break;
			case Player1:
				//case Player2:
				//case Player3:
				if (game_room[game_room_id]->player_ids[0] != nullptr)
					target_id = game_room[game_room_id]->player_ids[0]->id;
				break;
			case Player2:
				if (game_room[game_room_id]->player_ids[1] != nullptr)
					target_id = game_room[game_room_id]->player_ids[1]->id;

				break;
			case Player3:
				if (game_room[game_room_id]->player_ids[2] != nullptr)
					target_id = game_room[game_room_id]->player_ids[2]->id;

				break;
			}

			//패링을 했는지 안했는지 확인
			auto player_parring_time = reinterpret_cast<Client*>(clients[target_id])->pre_parrying_pattern;

			if (parrying_end_time - player_parring_time < 600) {
				std::cout << parrying_end_time << " " << player_parring_time << " player already parrying\n";
				//패링 했으니 넘어가자
				//패링 한 뒤 동작은 이미 worker thread에서 수행된 상태이다
				game_room[game_room_id]->Money[game_room[game_room_id]->FindPlayerID_by_GameRoom(target_id)] += 1;
			}
			else {
				//std::cout << parrying_end_time << " " << player_parring_time << " player parrying failed\n";
				//패링 못했으니 플레이어를 때리자
				clients[target_id]->hp -= (10 * damamge_bug - clients[target_id]->armour);
				for (const auto& p : game_room[game_room_id]->player_ids) {
					if (p == nullptr) continue;
					if (game_room[game_room_id]->boss_id == nullptr) continue;
					send_attack_player(game_room[game_room_id]->boss_id->id, target_id, p->id);
				}
				if (clients[target_id]->hp < 0) {
					timer_event tev;
					tev.ev = EVENT_GAME_END;
					tev.game_room_id = game_room_id;
					//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
					tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

					timer_queue.push(tev);
					for (const auto& p : game_room[game_room_id]->player_ids) {
						if (p == nullptr) continue;
						reinterpret_cast<Client*>(p)->is_active = false;
					}
				}

			}
			set_next_pattern(game_room_id);
			exp_over_pool.push(exp_over);

		}
		break;
		case OP_BOSS_TILE_ATTACK_START:
		{
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int pattern_type = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));
			int pivotType = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			int pivot_x = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 3));
			int pivot_y = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 4));
			int pivot_z = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 5));
			int dir = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 6));
			std::chrono::system_clock::time_point charging_time = *(reinterpret_cast<std::chrono::system_clock::time_point*>(exp_over->_net_buf + sizeof(int) * 7));


			int target_id = game_room[game_room_id]->find_online_player();

			if (game_room[game_room_id]->isGaming == false) break;
			if (target_id == -1) {
				std::cout << "tile attack : wrong target id\n";
				game_room[game_room_id]->pattern_progress = -1;
				break;
			}

			int pos_x, pos_z, pos_y;

			//printf("%d %d %d\n", pivot_x, pivot_y, pivot_z);
			switch (pivotType)
			{
			case PlayerM:
				target_id = game_room[game_room_id]->find_max_hp_player();

				break;
			case Playerm:
				target_id = game_room[game_room_id]->find_min_hp_player();

				break;
			case World:

				break;
			case Boss:
				target_id = game_room[game_room_id]->boss_id->id;

				break;
			case Player1:
				//case Player2:
				//case Player3:
				if (game_room[game_room_id]->player_ids[0] != nullptr)
					target_id = game_room[game_room_id]->player_ids[0]->id;

				break;
			case Player2:
				if (game_room[game_room_id]->player_ids[1] != nullptr)
					target_id = game_room[game_room_id]->player_ids[1]->id;

				break;
			case Player3:
				if (game_room[game_room_id]->player_ids[2] != nullptr)
					target_id = game_room[game_room_id]->player_ids[2]->id;

				break;
			case PlayerF:
				target_id = game_room[game_room_id]->find_max_distance_player();

				break;
			case PlayerN:
				target_id = game_room[game_room_id]->find_min_distance_player();
				break;
			}

			if (pivotType == World) {
				pos_x = pivot_x;
				pos_y = pivot_y;
				pos_z = pivot_z;
			}
			else {

				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;



				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;

				pos_y = -pos_x - pos_z;
			}
			//std::cout << "Target Pos -> " << "x : " << pos_x << "y : " << pos_y << "z : " << pos_z << std::endl;

			set_next_pattern(game_room_id);

			//이벤트 등록
			timer_event tev;
			tev.ev = EVENT_BOSS_TILE_ATTACK;

			tev.type = pattern_type;
			tev.x = pos_x;
			tev.y = pos_y;
			tev.z = pos_z;
			tev.game_room_id = game_room_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);

			//tev.start_time = std::chrono::time_point<std::chrono::system_clock>(std::chrono::milliseconds(start_time + charging_time));
			tev.start_time = charging_time;
			//std::cout << "go " << start_time + charging_time << std::endl;
			//static int i = 1;
			//game_room[game_room_id]->ready_lock.lock();
			//std::cout << i++<< " go " << tev.start_time << std::endl;
			//game_room[game_room_id]->ready_lock.unlock();
			//tev.charging_time = charging_time;
			tev.dir = dir;
			timer_queue.push(tev);

			exp_over_pool.push(exp_over);
		}
		break;

		case OP_BOSS_TILE_ATTACK:
		{
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int pattern_type = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));

			int pos_x = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			int pos_y = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 3));
			int pos_z = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 4));
			int dir = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 5));
			//int start_time = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 6));
			//std::cout << "to " << start_time << std::endl;
			//static int i = 1;
			//game_room[game_room_id]->ready_lock.lock();
			//std::cout << i++ <<" real " << std::chrono::system_clock::now() << std::endl;
			//game_room[game_room_id]->ready_lock.unlock();


			switch (pattern_type)
			{
			case 3:
			{
				for (int i = 0; i < 10; ++i) {
					do_npc_tile_attack(game_room_id,
						PatternInfo::HexPattern3[i][0] + pos_x,
						PatternInfo::HexPattern3[i][1] + pos_y,
						PatternInfo::HexPattern3[i][2] + pos_z);

				}
			}
			break;
			case 4:
			{
				for (int i = 0; i < 8; ++i) {
					do_npc_tile_attack(game_room_id,
						PatternInfo::HexPattern4[i][0] + pos_x,
						PatternInfo::HexPattern4[i][1] + pos_y,
						PatternInfo::HexPattern4[i][2] + pos_z);
				}
			}
			break;
			case 99:
			{
				do_npc_tile_attack(game_room_id,
					pos_x,
					pos_y,
					pos_z);
			}
			break;
			case 5:
			{
				do_npc_tile_attack(game_room_id,
					pos_x,
					pos_y,
					pos_z);
			}
			break;
			case 6:// 패턴파일에 적힌 방향으로 지진
			{
				int up = (int)(dir - 1 < DIR::LEFTUP ? DIR::RIGHTDOWN : dir - 1);
				int mid = (int)dir;
				int down = (int)(dir + 1 > DIR::RIGHTDOWN ? DIR::LEFTUP : dir + 1);
				//std::cout << dir << std::endl;
				for (int i = 0; i < 5; ++i) {
					do_npc_tile_attack(game_room_id,
						PatternInfo::HexCellAround[up][0] * i + pos_x,
						PatternInfo::HexCellAround[up][1] * i + pos_y,
						PatternInfo::HexCellAround[up][2] * i + pos_z);
					do_npc_tile_attack(game_room_id,
						PatternInfo::HexCellAround[mid][0] * i + pos_x,
						PatternInfo::HexCellAround[mid][1] * i + pos_y,
						PatternInfo::HexCellAround[mid][2] * i + pos_z);
					do_npc_tile_attack(game_room_id,
						PatternInfo::HexCellAround[down][0] * i + pos_x,
						PatternInfo::HexCellAround[down][1] * i + pos_y,
						PatternInfo::HexCellAround[down][2] * i + pos_z);
				}
			}
			break;
			case 7:
				for (int i = 0; i < 6; ++i) {
					do_npc_tile_attack(game_room_id,
						PatternInfo::HexCellAround[i][0] + pos_x,
						PatternInfo::HexCellAround[i][1] + pos_y,
						PatternInfo::HexCellAround[i][2] + pos_z);
				}
				break;
			case 8:
				do_npc_tile_attack(game_room_id,
					pos_x,
					pos_y,
					pos_z);
				break;
			default:
				std::cout << "wrong pattern type\n";
				break;
			}


			exp_over_pool.push(exp_over);
		}
		break;

		case OP_GAME_END:
		{
			//이건 무조건 한 번만 하도록 보장되야함
			//
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			game_room[game_room_id]->state_lock.lock();
			//보스 체력 확인하고
			if (game_room[game_room_id]->isGaming == false) {
				game_room[game_room_id]->state_lock.unlock();
				break;// GameEnd는 한 번만 들어와야됨
			}
			int boss_id = game_room[game_room_id]->boss_id->id;

			//if(clients[boss_id]->hp<10;
			//체력에 따라 클리어 유무
			std::cout << boss_id << " -> Game Over\n";
			//std::cout << game_room_id << " -> Game Over\n";
			bool isabnormal = false;
			bool isDie = false;
			for (const auto p : game_room[game_room_id]->player_ids) {
				if (p == nullptr) { isabnormal = true; break; }
				if (p->hp < 0) isDie = true;
			}
			if (isabnormal) {
				int i = 0;
				for (const auto p : game_room[game_room_id]->player_ids) {
					if (p == nullptr) continue;

					send_game_end(p->id, 0, -1, game_room[game_room_id]->Money[i++], GAME_OVER);
					//돈만 추가
				}
				game_room[game_room_id]->game_end();

				//std::cout << "game end\n";
			}
			else if (isDie) {
				int i = 0;
				for (const auto p : game_room[game_room_id]->player_ids) {
					if (p == nullptr) continue;

					send_game_end(p->id, 0, -1, game_room[game_room_id]->Money[i++], GAME_OVER);
					//돈만 추가
				}
				game_room[game_room_id]->game_end();

			}
			else {
				for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
					GameObject* p = game_room[game_room_id]->player_ids[i];
					if (p == nullptr) continue;
					if (game_room[game_room_id]->Score[i] < 0)game_room[game_room_id]->Score[i] = 0;
					std::cout << "clear score : " << game_room[game_room_id]->Score[i] << std::endl;
					std::cout << "clear money : " << game_room[game_room_id]->Money[i] << std::endl;
					if (game_room[game_room_id]->Score[i] < 0) game_room[game_room_id]->Score[i] = 0;

					int get_item_type = game_room[game_room_id]->get_item_result();
					send_game_end(p->id, game_room[game_room_id]->Score[i], get_item_type, game_room[game_room_id]->Money[i], GAME_CLEAR);
					p->SetScore(game_room[game_room_id]->map_type - 1, game_room[game_room_id]->Score[i]);
					p->SetMoney(game_room[game_room_id]->Money[i]);
					input_db_event(p->id, DB_GET_SCROLL, get_item_type);
					input_db_event(p->id, DB_UPDATE_CLEAR_MAP);
				}
				game_room[game_room_id]->game_end();
			}
			game_room[game_room_id]->state_lock.unlock();


			exp_over_pool.push(exp_over);
		}
		break;
		case OP_PLAYER_MOVE:
		{
			int mover_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			Client* cl = reinterpret_cast<Client*>(clients[mover_id]);
			int isMove = cl->Astar->find_path(clients[mover_id], clients[mover_id]);
			//std::cout << dir << std::endl;

			if (isMove == -1)
			{
				cl->dest_x = -1;
				cl->dest_y = -1;
				cl->dest_z = -1;
				//길찾기 종료
			}
			else if (isMove == 1)
			{
				maps[FIELD_MAP]->SetTileType(cl->x, cl->z, cl->pre_x, cl->pre_z);

				cl->vl.lock();
				std::unordered_set<int> my_vl{ cl->viewlist };
				cl->vl.unlock();
				for (auto i : my_vl) {
					if (false == is_player(i)) continue;
					send_move_object(i, cl->id);
				}
				send_move_object(cl->id, cl->id);

				timer_event tev;
				tev.ev = EVENT_PALYER_MOVE;
				tev.obj_id = client_id;
				//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
				tev.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);

				timer_queue.push(tev);
			}



			exp_over_pool.push(exp_over);
		}
		break;
		case OP_DB_PLAYER_LOGIN:
		{
			Client& cl = *reinterpret_cast<Client*>(clients[client_id]);
			char item[20];
			memcpy_s(item, 20, reinterpret_cast<char*>(exp_over->_net_buf), 20);
			//set_new_player_pos(client_id);


			cl.state_lock.lock();
			cl.state = ST_INGAME;
			cl.state_lock.unlock();
			send_login_ok(client_id, item);


			exp_over_pool.push(exp_over);
		}
		break;
		case OP_DB_USE_ITEM:
		{
			char isUsed = *(reinterpret_cast<int*>(exp_over->_net_buf));
			char itemType = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(char)));
			if (isUsed == 1)
			{
				for (int i = 0; i < MAX_USER; ++i)
				{
					Client* other = reinterpret_cast<Client*>(clients[i]);
					//if (i == client_id) continue;
					other->state_lock.lock();
					if (ST_INGAME != other->state) {
						other->state_lock.unlock();
						continue;
					}
					other->state_lock.unlock();

					send_use_item(i, client_id, itemType);
				}
				cur_play_music = itemType;
			}
			exp_over_pool.push(exp_over);
		}
		break;
		default:
			std::cout << "Undefine work type\n";
			break;
		}
	}
}

void Network::do_timer() {
	using namespace std;
	using namespace chrono;
	while (true) {

		timer_event ev;
		while (!timer_queue.empty()) {

			if (timer_queue.try_pop(ev) == false) break;

			if (ev.start_time <= system_clock::now()) {
				//이벤트 시작
				EXP_OVER* ex_over;// = new EXP_OVER;
				//ex_over->_comp_op = OP_NPC_MOVE;
				while (!exp_over_pool.try_pop(ex_over));
				switch (ev.ev)
				{
				case EVENT_BOSS_MOVE:
					ex_over->_comp_op = OP_BOSS_MOVE;
					*reinterpret_cast<int*>(ex_over->_net_buf) = ev.x;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int)) = ev.y;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 2) = ev.z;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 3) = ev.game_room_id;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 4) = ev.pivotType;// 타겟 id가 패턴 종류 구분

					break;
				case EVENT_BOSS_TILE_ATTACK_START:
					ex_over->_comp_op = OP_BOSS_TILE_ATTACK_START;
					*reinterpret_cast<int*>(ex_over->_net_buf) = ev.game_room_id;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int)) = ev.type;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 2) = ev.pivotType;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 3) = ev.x;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 4) = ev.y;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 5) = ev.z;
					//*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 6) = time_point_cast<milliseconds>(ev.start_time).time_since_epoch().count();
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 6) = ev.dir;
					*reinterpret_cast<system_clock::time_point*>(ex_over->_net_buf + sizeof(int) * 7) = ev.start_time + milliseconds(ev.charging_time + 100);
					break;
				case EVENT_BOSS_TILE_ATTACK:

					ex_over->_comp_op = OP_BOSS_TILE_ATTACK;
					*reinterpret_cast<int*>(ex_over->_net_buf) = ev.game_room_id;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int)) = ev.type;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 2) = ev.x;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 3) = ev.y;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 4) = ev.z;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 5) = ev.dir;
					//*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 6) = time_point_cast<milliseconds>(ev.start_time).time_since_epoch().count();
					break;

				case EVENT_PLAYER_PARRYING:
					ex_over->_comp_op = OP_PLAYER_PARRYING;
					*reinterpret_cast<int*>(ex_over->_net_buf) = ev.target_id;
					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int)) = ev.game_room_id;

					*reinterpret_cast<int*>(ex_over->_net_buf + sizeof(int) * 2) = ev.pivotType;
					*reinterpret_cast<long long*>(ex_over->_net_buf + sizeof(int) * 3) =
						time_point_cast<milliseconds>(ev.start_time).time_since_epoch().count();
					break;
				case EVENT_GAME_END:
					ex_over->_comp_op = OP_GAME_END;
					*reinterpret_cast<int*>(ex_over->_net_buf) = ev.game_room_id;
					break;
				case EVENT_PALYER_MOVE:
				{
					ex_over->_comp_op = OP_PLAYER_MOVE;
					*reinterpret_cast<int*>(ex_over->_net_buf) = ev.obj_id;

				}
				break;
				default:
					break;
				}
				if (ev.ev != EVENT_PALYER_MOVE && game_room[ev.game_room_id]->isGaming == false) break;
				PostQueuedCompletionStatus(g_h_iocp, 1, ev.obj_id, &ex_over->_wsa_over);// 두번째 인자가 0이 되면 소캣 종료로 취급이 된다. 1로해주자
			}
			else {
				//기껏 뺐는데 다시 넣는거 좀 비효율 적이다.
				//다시 넣지 않는 방법으로 최적화 필요
				timer_queue.push(ev);
				break;
			}
		}

		//큐가 비었거나
		this_thread::sleep_for(10ms);
	}
}

void Network::do_DBevent()
{
	using namespace std;
	using namespace chrono;
	while (true) {
		//worker 스레드에서 디비 이벤트를 큐에 담으면
		//여기서 빼서 디비 저장프로시저를 사용하고
		//결과를 worker 스레드에 알려준다.
		db_event ev;
		while (!db_event_queue.empty()) {

			db_event_queue.try_pop(ev);



			Client& cl = *reinterpret_cast<Client*>(clients[ev.obj_id]);
			PlayerData player_data;
			switch (ev.ev)
			{
			case DB_PLAYER_LOGIN:
			{
				EXP_OVER* ex_over;
				while (!exp_over_pool.try_pop(ex_over));
				ex_over->_comp_op = OP_DB_PLAYER_LOGIN;
				player_data.name = std::wstring(cl.name, &cl.name[strnlen_s(cl.name, MAX_NAME_SIZE)]);
				//std::cout << cl.name << std::endl;
				if (DB->checkPlayer(player_data)) {

					cl.x = player_data.x;
					cl.z = player_data.z;
					cl.y = -cl.x - cl.z;
					if (set_new_player_pos(cl.id) == -1) {
						cl.x = 17;
						cl.z = -21;
						cl.y = -cl.x - cl.z;
					}
					cl.curSkill = player_data.curSkill;

					cl.SkillAD = player_data.SkillAD;
					cl.SkillTa = player_data.SkillTa;
					cl.SkillHeal = player_data.SkillHeal;

					cl.MMR = player_data.MMR;
					cl.money = player_data.money;
					memset(ex_over->_net_buf, 0, 20);
					DB->readInventory(&cl, reinterpret_cast<char*>(ex_over->_net_buf));
					DB->readClearMap(&cl);
				}
				else {
					//사용 중인 아이디이므로 worker까지 갈 것도 없이 그냥 로그인 실패 패킷을 보낸다.
					send_login_fail(ev.obj_id);
					disconnect_client(ev.obj_id);
					exp_over_pool.push(ex_over);
					cl.ClearMap[0] = 0;
					cl.ClearMap[1] = 0;
					break;
				}

				//db처리가 완료됐다고 알려주는거임 -> 데이터를 읽어오는 경우만 알려주면 될 것 같다.
				PostQueuedCompletionStatus(g_h_iocp, 1, ev.obj_id, &ex_over->_wsa_over);
			}
			break;
			case DB_PLAYER_LOGOUT: {
				// player data update
				DB->updatePlayer(&cl, true);
				cl.money = 0;
				cl.state_lock.lock();
				cl.state = ST_FREE;
				cl.state_lock.unlock();
			}
								 break;
			case DB_READ_INVENTORY:
				// pqcs inventory info

				//ex_over->_comp_op = OP_DB_INVENTORY;
				break;
			case DB_READ_CLAER_MAP_INFO:
				// pqcs clear map info -> 이건 사용하는 방향으로 해보자

				break;
			case DB_USE_SCROLL:
				// pqcs result info
			{
				EXP_OVER* ex_over;
				while (!exp_over_pool.try_pop(ex_over));
				ex_over->_comp_op = OP_DB_USE_ITEM;
				*reinterpret_cast<char*>(ex_over->_net_buf) = DB->updateInventory(&cl, ev.data, -1);;
				*reinterpret_cast<char*>(ex_over->_net_buf + sizeof(char)) = ev.data;

				PostQueuedCompletionStatus(g_h_iocp, 1, ev.obj_id, &ex_over->_wsa_over);
			}
			break;
			case DB_GET_SCROLL:
				DB->updateInventory(&cl, ev.data, 1);
				break;
			case DB_UPDATE_CLEAR_MAP:
				DB->updateClearInfo(&cl);

				break;
			case DB_UPDATE_PLAYER_DATA:
				// 게임 중간중간 저장할 때 사용
				DB->updatePlayer(&cl, false);
				break;
			default:
				break;
			}


		}


		this_thread::sleep_for(1000ms);
	}
}
void Network::set_next_pattern(int room_id)
{
	// GetPatternTime을 하던 중 pattern_progress가 -1이 된다면? 안쓰이는 패턴이 등록된거고 만약 게임 방의 player들이 이미 null로 정의된 상태면 프로그램이 터지게됨
	//const PattenrInfo... 바로 전에 한다해도 중간 시간을 줄이는 거지 원인을 제거한게 아니잖아
	if (game_room[room_id]->pattern_progress == -1) return;
	const std::vector<PatternInfo>& pt = maps[game_room[room_id]->map_type]->GetPatternTime();
	if (pt.size() <= game_room[room_id]->pattern_progress) return;
	const PatternInfo& t = pt[game_room[room_id]->pattern_progress++];

	int boss_id = game_room[room_id]->boss_id->id;

	timer_event tev;


	switch (t.type)
	{
	case -1:

		tev.ev = EVENT_BOSS_MOVE;
		tev.obj_id = boss_id;
		tev.game_room_id = room_id;
		tev.x = t.x;
		tev.y = t.y;
		tev.z = t.z;
		//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
		tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time);
		//tev.charging_time = t.speed;
		tev.pivotType = t.pivotType;
		timer_queue.push(tev);
		break;
	case 3:// 패턴 3번
	case 4:// 패턴 4번
	case 99:// 단일 장판 공격
	case 5: // 가장 멀리있는 적에게 물줄기 발사
	case 6: // 보스가 보는 방향으로 지진
	case 7:
	case 8:

		tev.ev = EVENT_BOSS_TILE_ATTACK_START;
		tev.obj_id = boss_id;
		tev.type = t.type;
		tev.x = t.x;
		tev.y = t.y;
		tev.z = t.z;
		tev.game_room_id = room_id;
		//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
		tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time - t.speed - 100);
		tev.charging_time = t.speed;
		tev.pivotType = t.pivotType;
		tev.dir = t.dir;
		timer_queue.push(tev);


		break;

	case 10: // 유도 공격 -> 패링할 수 있음


		tev.ev = EVENT_PLAYER_PARRYING;
		tev.obj_id = boss_id;
		tev.type = t.type;
		tev.x = t.x;
		tev.y = t.y;
		tev.z = t.z;
		tev.game_room_id = room_id;
		//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
		tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time + 300);
		//tev.charging_time = t.speed;
		tev.pivotType = t.pivotType;
		timer_queue.push(tev);
		break;
	case -600:
		tev.ev = EVENT_GAME_END;
		tev.game_room_id = room_id;
		//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
		tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time);

		timer_queue.push(tev);
		break;
	default:

		std::cout << t.type << " : 잘못된 패턴 타입\n";
		set_next_pattern(room_id);
		break;
	}
}

void Network::input_db_event(int c_id, DB_EVENT_TYPE type, int data)
{
#ifdef DEBUG
	if (DB->isConnect == false) return;
#endif // DEBUG

	db_event dev;
	dev.obj_id = c_id;
	dev.ev = type;
	dev.data = data;
	db_event_queue.push(dev);
}


void Network::game_start(int room_id)
{
	int boss_id = game_room[room_id]->boss_id->id;
	int map_type = game_room[room_id]->map_type;
	game_room[room_id]->boss_id->cur_room_num = map_type;

	const std::vector<PatternInfo>& pt = maps[map_type]->GetPatternTime();


	Witch* boss = reinterpret_cast<Witch*>(clients[boss_id]);
	//수정

	boss->hp = maps[map_type]->bossHP;

	int i = 0;
	for (auto p : game_room[room_id]->player_ids) {
		if (p == nullptr)continue;
		p->hp = 100;
		Client* cl = reinterpret_cast<Client*>(p);

		cl->pre_parrying_pattern = -1;
		maps[FIELD_MAP]->SetTileType(-1, -1, p->x, p->z);
		p->dest_x = -1;
		p->dest_y = -1;
		p->dest_z = -1;

		p->power = 1;
		p->armour = 0;

		p->x = maps[game_room[room_id]->map_type]->startX[i];
		p->z = maps[game_room[room_id]->map_type]->startZ[i];

		p->y = -p->x - p->z;
		i++;
		p->pre_x = p->x;
		p->pre_y = p->y;
		p->pre_z = p->z;


		for (auto id : game_room[room_id]->player_ids) {
			if (id == nullptr)continue;

			send_move_object(id->id, p->id);
		}
		//send_move_object(p->id, p->id);
	}


	set_next_pattern(room_id);

	if (room_id == -1) {

	}
}