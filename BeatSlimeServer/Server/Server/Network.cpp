#include "stdfx.h"

#include"Map.h"
#include "DataBase.h"
#include "Network.h"

Network* Network::instance = nullptr;

Network* Network::GetInstance()
{
	return instance;
}


Network::Network() {
	//인스턴스는 한 개만!!
	assert(instance == nullptr);
	instance = this;

	for (int i = 0; i < MAX_USER; ++i) {
		clients[i] = new Client;
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
	for (int i = 0; i < MAX_OBJECT; ++i) {
		exp_over_pool.push(new EXP_OVER);
	}
	Initialize_NPC();
	DB = new DataBase;

	for (int i = 0; i < MAX_GAME_ROOM_NUM; ++i) {
		game_room[i] = new GameRoom(i);
	}
	for (int i = 0; i < MAP_NUM; ++i) {
		maps[i] = new MapInfo;
	}

	maps[FIELD_MAP]->SetMap("Map\\Field_Map", "Music\\Test.csv");
	maps[WITCH_MAP]->SetMap("Map\\Forest1", "Music\\Test.csv");

	// 포탈의 위치를 나타내는 자료필요
	for (int i = 0; i < PORTAL_NUM; ++i) {
		portals[i] = new Portal(2, -2);
	}
}
Network::~Network() {
	//스레드가 종료된 후 이기 때문에 락을 할 필요가 없다
//accpet상태일 때 문제가 생긴다
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
}
void Network::send_login_ok(int c_id)
{
	sc_packet_login_ok packet;
	packet.id = c_id;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_LOGIN_OK;
	packet.x = clients[c_id]->x;
	packet.y = clients[c_id]->y;
	packet.z = clients[c_id]->z;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);
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
	reinterpret_cast<Client*>(ex_over, clients[c_id])->cur_map_type = map_type;
}

void Network::send_game_start(int c_id, GameObject* ids[3], int boss_id)
{
	sc_packet_game_start packet;
	packet.type = SC_PACKET_GAME_START;
	packet.size = sizeof(packet);
	packet.player_id = c_id;
	packet.id1 = ids[0]->id;
	packet.id2 = ids[1]->id;
	packet.id3 = ids[2]->id;
	packet.boss_id = boss_id;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);

	for (int i = 0; i < MAX_IN_GAME_PLAYER; ++i) {
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
	reinterpret_cast<Client*>(ex_over, clients[client_id])->do_send(ex_over);
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

	//packet.move_time = clients[mover]->last_move_time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);
}
void Network::send_attack_player(int attacker, int target, int receiver)
{
	sc_packet_attack packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_ATTACK;
	packet.id = attacker;
	packet.target_id = target;
	packet.direction = clients[attacker]->direction;
	packet.hp = clients[target]->hp;
	//packet.move_time = clients[mover]->last_move_time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[receiver])->do_send(ex_over);
}

void Network::send_put_object(int c_id, int target) {
	sc_packet_put_object packet;

	//strcpy_s(packet.name, clients[target]->name);
	packet.id = target;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_PUT_OBJECT;
	packet.object_type = clients[target]->type;
	packet.x = clients[target]->x;
	packet.y = clients[target]->y;
	packet.z = clients[target]->z;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);
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
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);

	if (true == is_npc(victim)) {
		reinterpret_cast<Npc*>(clients[victim])->is_active = false;
	}
}

void Network::send_map_data(int c_id, char* data, int nShell)
{
	sc_packet_map_data packet;
	packet.size = nShell * sizeof(Map) + 2;
	packet.type = SC_PACKET_MAP_DATA;
	memcpy(packet.buf, reinterpret_cast<char*>(data), packet.size - 2);


	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, packet.size, &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);

}

void Network::send_game_end(int c_id, char end_type)
{
	sc_packet_game_end packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_GAME_END;
	packet.end_type = end_type;


	//packet.move_time = clients[mover]->last_move_time;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);
}

void Network::send_parrying(int c_id,int actor_id)
{
	sc_packet_parrying packet;
	packet.type = SC_PACKET_PARRYING;
	packet.size = sizeof(packet);
	packet.id = actor_id;

	EXP_OVER* ex_over;
	while (!exp_over_pool.try_pop(ex_over));
	ex_over->set_exp(OP_SEND, sizeof(packet), &packet);
	reinterpret_cast<Client*>(ex_over, clients[c_id])->do_send(ex_over);
}

void Network::disconnect_client(int c_id)
{
	if (c_id >= MAX_USER)
		std::cout << "disconnect_client : unexpected id range" << std::endl;
	Client& client = *reinterpret_cast<Client*>(clients[c_id]);

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
	clients[c_id]->state_lock.lock();
	closesocket(reinterpret_cast<Client*>(clients[c_id])->socket);
	clients[c_id]->state = ST_FREE;
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
		std::cout << "2 : Maximum Number of Monster Overflow!!\n";
		return -1;

		break;
	case BOSS2:
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

void Network::do_npc_move(int npc_id) {


}

void Network::do_npc_attack(int npc_id, int target_id, int reciver) {

}

void Network::do_npc_tile_attack(int game_room_id, int x, int y, int z)
{
	const int damage = 5;
	for (const auto& pl : game_room[game_room_id]->player_ids) {
		if (false == is_attack(pl->id, x, z)) continue;
		pl->hp -= damage;
		for (const auto& p : game_room[game_room_id]->player_ids)
			send_attack_player(game_room[game_room_id]->boss_id->id, pl->id, p->id);

		if (pl->hp < 0) {
			// 게임 끝
			reinterpret_cast<Client*>(pl)->is_active = false;
			bool is_game_end = true;
			for (const auto& p : game_room[game_room_id]->player_ids) {
				if (p->hp > 0) is_game_end = false;
			}

			if (is_game_end) {
				//게임룸 돌리고
				//씬 변경도 해야됨
				//게임이 끝난 게임룸의 이벤트는 모두 제거해야됨
				//이미 들어간건 찾을 수 없는데 
				//한번에 다 넣지 말고 한 패턴 끝나면 넣고 이런식으로 해야되나 -> 고려해볼만 하구만
				for (const auto& p : game_room[game_room_id]->player_ids)
					send_game_end(p->id, GAME_OVER);
			}
		}

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
		send_login_ok(client_id);

		cl.state_lock.lock();
		cl.state = ST_INGAME;
		cl.state_lock.unlock();


		//cl.x = maps[FIELD_MAP]->LengthX / 2;
		//cl.z = maps[FIELD_MAP]->LengthZ / 2;
		//cl.y = -cl.z - cl.x;


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

			if (false == is_near(other->id, client_id))
				continue;

			// 기존에 있던 클라이언트가 가까이 있다면 뷰 리스트에 넣고 put packet을 보낸다.
			cl.vl.lock();
			cl.viewlist.insert(other->id);
			cl.vl.unlock();

			send_put_object(client_id, other->id);
		}
	}
	break;
	case CS_PACKET_MOVE:
	{
		if (false == cl.is_active) break;
		std::cout << "player move\n";

		cs_packet_move* packet = reinterpret_cast<cs_packet_move*>(p);
		//cl.last_move_time = packet->move_time;
		short& x = cl.x;
		short& y = cl.y;
		short& z = cl.z;
		cl.direction = packet->direction;
		int cur_map = cl.cur_map_type;

		switch (packet->direction) {
		case DIR::LEFTUP:
			if (maps[cur_map]->GetTileType(x - 1, z + 1) != 0) {
				x--; z++;
			}
			break;
		case DIR::UP:
			if (maps[cur_map]->GetTileType(x, z + 1) != 0) {
				y--; z++;
			}
			break;
		case DIR::RIGHTUP:
			if (maps[cur_map]->GetTileType(x + 1, z) != 0) {
				x++; y--;
			}
			break;
		case DIR::LEFTDOWN:
			if (maps[cur_map]->GetTileType(x - 1, z) != 0) {
				x--; y++;
			}
			break;
		case DIR::DOWN:
			if (maps[cur_map]->GetTileType(x, z - 1) != 0) {
				y++; z--;
			}
			break;
		case DIR::RIGHTDOWN:
			if (maps[cur_map]->GetTileType(x + 1, z - 1) != 0) {
				x++; z--;
			}
			break;
		default:
			std::cout << "Invalid move in client " << client_id << std::endl;
			exit(-1);
		}

		// 이동한 클라이언트에 대한 nearlist 생성
		// 꼭 unordered_set이여야 할까?
		// 얼마나 추가될지 모르고, 데이터는 id이기 때문에 중복없음이 보장되있다. id로 구분안하는 경우가 있나?
		// 섹터를 나누어 근처에 있는지 검색해 속도를 높이자
		std::unordered_set<int> nearlist;
		for (auto* other : clients) {
			if (other->id == client_id)
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
				send_remove_object(cl.id, other);

				//npc는 view리스트를 가지고 있지 않다.
				if (true == is_npc(other)) {
					//reinterpret_cast<Npc*>(clients[other])->is_active = false;
					continue;
				}
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
	case CS_PACKET_READ_MAP:
	{
		DB->read_map_data();
		int MAX_SEND_NUM = 3;
		int size = DB->db_map_data.size();

		int n = size / MAX_SEND_NUM;
		int m = size % MAX_SEND_NUM;
		for (int i = 0; i < n; i++)
			send_map_data(client_id, reinterpret_cast<char*>(&(DB->db_map_data[i * MAX_SEND_NUM])), MAX_SEND_NUM);

		send_map_data(client_id, reinterpret_cast<char*>(&(DB->db_map_data[n * MAX_SEND_NUM])), m);
	}
	break;
	case CS_PACKET_WRITE_MAP:
	{
		cs_packet_write_map* pk = reinterpret_cast<cs_packet_write_map*>(p);
		if (pk->id != -1)
			DB->client_map_data[pk->id] = Map{ pk->id, pk->x, pk->y,pk->z,pk->w,pk->color,pk->block_type };
		else {
			std::cout << "맵 수신 완료 update 및 insert 시작\n";
			DB->read_map_data();

			//DB->db_map_data; -> 여기서 찾았는데 해당 id의 값이 없으면 삽입

			for (const std::pair<int, Map>& m : DB->client_map_data) {
				auto re = std::find_if(DB->db_map_data.cbegin(), DB->db_map_data.cend(), [&](const Map& a) {
					return a.id == m.first;
					});
				if (re == DB->db_map_data.cend()) {
					//삽입해야할 데이터
					DB->insert_map_data(m.second);
				}
				else {
					if (*re == m.second) {
						//유지된 데이터
					}
					else {
						//업데이트해야할 데이터
						DB->update_map_data(m.second);

					}
				}
			}

			// 삽입해야할 것

			// 수정해야할 것
		}
	}
	break;
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
					for (int id : p->player_ids) { // 이미 change 씬
						players[i] = clients[id];
						i++;
						if (i > MAX_IN_GAME_PLAYER) break;
					}

					for (int id : p->player_ids) {
						send_change_scene(id, p->map_type);

					}
					// 포탈에서 GameRoom으로 이동
					int room_id = get_game_room_id();
					int boss_id = get_npc_id(p->map_type);
					game_room[room_id]->GameRoomInit(p->map_type, maps[p->map_type]->bpm, clients[boss_id], players);
					//std::cout << "시작" << std::endl;
					p->player_ids.clear();
					p->ready_player_cnt = 0;
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
	case CS_PACKET_GAME_START_READY:
	{
		cs_packet_game_start_ready* packet = reinterpret_cast<cs_packet_game_start_ready*>(p);

		for (auto* gr : game_room) {
			gr->ready_lock.lock();
			if (false == gr->FindPlayer(client_id)) { gr->ready_lock.unlock(); continue; }
			gr->ready_player_cnt++;
			if (gr->ready_player_cnt >= MAX_IN_GAME_PLAYER) {

				for (const auto pl : gr->player_ids) {
					send_game_start(pl->id, gr->player_ids, gr->boss_id->id);
				}
				gr->start_time = std::chrono::system_clock::now();
				game_start(gr->game_room_id);
				std::cout << "게임 시작\n";

			}
			gr->ready_lock.unlock();
			break;
		}

	}
	break;
	case CS_PACKET_PARRYING:
	{
		cs_packet_parrying* packet = reinterpret_cast<cs_packet_parrying*>(p);
		/*
		*	- 서버에 전송된 패킷을 보고 서버에서
		*		- 클라이언트가 플레이중인 게임 방을 찾음
		*		- 해당 게임 방의 게임 플레이 시간과 패킷이 온시간을 계산
		*		- 계산된 시간대에 패링가능한 공격이 있었는지 확인한 후 만약 있었다면
		*			- 패링 성공 패킷 클라이언트로 전송 -> 이 때 타겟인 플레이어가 패링 패킷을 보낸경우에만 패링가능 -> 패킷에 자신의 번호가 들어있어야함
		*			- 없으면 실패
		*/

		
		for (auto* gr : game_room) {
			if (false == gr->isGaming) continue;
			// 해당 플레이어의 게임 방을 찾고
			if (false == gr->FindPlayer(client_id)) continue;
			int id = gr->FindPlayer(client_id); 
			// 게임시간이 얼마나 지났는지 확인하고
			int running_time = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - gr->start_time).count();
			// 현재시간에 패링패턴이 있었는지 확인
			const std::vector<PatternInfo>& pt = maps[gr->map_type]->GetPatternTime();
			for (auto& pattern : pt) {
				if (10 != pattern.type) continue;// 패링 노트인지 채크
				if (id != (pattern.pivotType - 4)) continue; // 타겟이 된 플레이어인지 체크
				//100ms보다 작으면
				if (abs(running_time - pattern.time) < 100) {
					//패링 성공
					// 패링 성공 패킷을 클라이언트로 보냄
					for (const auto pl : gr->player_ids) {
						send_parrying(pl->id, client_id);
					}
				}
				else {
					//패링 실패
				}
				
			}
		}
		std::cout << "Can not find player game room\n";
	}
	break;
	default:
		std::cout << "이상한 패킷 수신\n";
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
			if (exp_over->_comp_op == OP_SEND)
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
			if (-1 == new_id) continue;

			Client& cl = *(reinterpret_cast<Client*>(clients[new_id]));
			cl.x = 0;
			cl.y = 0;
			cl.z = 0;
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
			//char* 버퍼를 socket*로 바꿔 소켓을 가르킬 수 있도록 한다. 소켓도 포인터인디?
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

			int target_id = -1;
			int pivot_x, pivot_y, pivot_z;

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
				target_id = game_room[game_room_id]->player_ids[0]->id;
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case Player2:
				target_id = game_room[game_room_id]->player_ids[1]->id;
				pivot_x = clients[target_id]->x;
				pivot_z = clients[target_id]->z;
				pivot_y = -pivot_x - pivot_z;
				break;
			case Player3:
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

				send_move_object(pl->id, client_id);
			}
			exp_over_pool.push(exp_over);
		}
		break;
		case OP_PLAYER_PARRYING:
		{
			int target_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));
			int charging_time = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			int pivotType = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 3));

			switch (pivotType)
			{
			case PlayerM:
				target_id = game_room[game_room_id]->find_max_hp_player();
				break;
			case Playerm:
				target_id = game_room[game_room_id]->find_min_hp_player();
				break;

			case Boss:
				target_id = game_room[game_room_id]->boss_id->id;

				break;
			case Player1:
				target_id = game_room[game_room_id]->player_ids[0]->id;

				break;
			case Player2:
				target_id = game_room[game_room_id]->player_ids[1]->id;

				break;
			case Player3:
				target_id = game_room[game_room_id]->player_ids[2]->id;

				break;
			}
			for (const auto pl : game_room[game_room_id]->player_ids) {
				send_effect(pl->id, client_id, target_id, 10, charging_time, 0, 0, 0, 0);
			}
			exp_over_pool.push(exp_over);
		}
		break;
		case OP_BOSS_TILE_ATTACK_START:
		{
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int pattern_type = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));
			int charging_time = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			int pivotType = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 3));
			int pivot_x = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 4));
			int pivot_y = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 5));
			int pivot_z = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 6));


			// 시작 위치를 중심으로 패턴 공격
			// 공격 이펙트 보내고
			// 서버에서 공격 처리할 이벤트 추가
			int target_id = -1;

			int pos_x, pos_z, pos_y, dir = 0;
			//printf("%d %d %d\n", pivot_x, pivot_y, pivot_z);
			switch (pivotType)
			{
			case PlayerM:
				target_id = game_room[game_room_id]->find_max_hp_player();
				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;
				pos_y = -pos_x - pos_z;
				break;
			case Playerm:
				target_id = game_room[game_room_id]->find_min_hp_player();
				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;
				pos_y = -pos_x - pos_z;
				break;
			case World:
				pos_x = pivot_x;
				pos_y = pivot_y;
				pos_z = pivot_z;
				break;
			case Boss:
				target_id = game_room[game_room_id]->boss_id->id;
				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;
				pos_y = -pos_x - pos_z;
				break;
			case Player1:
				target_id = game_room[game_room_id]->player_ids[0]->id;
				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;
				pos_y = -pos_x - pos_z;
				break;
			case Player2:
				target_id = game_room[game_room_id]->player_ids[1]->id;
				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;
				pos_y = -pos_x - pos_z;
				break;
			case Player3:
				target_id = game_room[game_room_id]->player_ids[2]->id;
				pos_x = clients[target_id]->x + pivot_x;
				pos_z = clients[target_id]->z + pivot_z;
				pos_y = -pos_x - pos_z;
				break;
			}

			switch (pattern_type)
			{
			case 3:
			{
				for (int i = 0; i < 10; ++i) {
					// 다 똑같은 시간을 가지고 있다
					// 즉 이벤트를 나눌 필요가 굳이 있을까 싶다
					// 이걸 합치는 방법을 생각해봐야겠다.

					timer_event t;
					t.ev = EVENT_BOSS_TILE_ATTACK;
					t.obj_id = client_id;
					t.target_id = target_id;
					t.game_room_id = game_room_id;
					t.x = PatternInfo::HexPattern3[i][0] + pos_x;
					t.y = PatternInfo::HexPattern3[i][1] + pos_y;
					t.z = PatternInfo::HexPattern3[i][2] + pos_z;
					t.start_time = std::chrono::system_clock::now() + std::chrono::milliseconds(charging_time);
					timer_queue.push(t);
				}
			}
			break;
			case 4:
			{
				for (int i = 0; i < 8; ++i) {
					timer_event t;
					t.ev = EVENT_BOSS_TILE_ATTACK;
					t.obj_id = client_id;
					t.target_id = target_id;
					t.game_room_id = game_room_id;
					t.x = PatternInfo::HexPattern4[i][0] + pos_x;
					t.y = PatternInfo::HexPattern4[i][1] + pos_y;
					t.z = PatternInfo::HexPattern4[i][2] + pos_z;
					t.start_time = std::chrono::system_clock::now() + std::chrono::milliseconds(charging_time);
					timer_queue.push(t);
				}
			}
			break;
			case 99:
			{
				timer_event t;
				t.ev = EVENT_BOSS_TILE_ATTACK;
				t.obj_id = client_id;
				t.target_id = target_id;
				t.game_room_id = game_room_id;
				t.x = pos_x;
				t.y = pos_y;
				t.z = pos_z;
				t.start_time = std::chrono::system_clock::now() + std::chrono::milliseconds(charging_time);
				timer_queue.push(t);
			}
			break;
			case 5:
			{
				timer_event t;
				t.ev = EVENT_BOSS_TILE_ATTACK;
				t.obj_id = client_id;
				t.target_id = game_room[game_room_id]->find_max_distance_player();
				target_id = t.target_id;
				t.game_room_id = game_room_id;
				t.x = clients[t.target_id]->x;
				t.y = clients[t.target_id]->y;
				t.z = clients[t.target_id]->z;
				t.start_time = std::chrono::system_clock::now() + std::chrono::milliseconds(charging_time);
				timer_queue.push(t);
			}
			break;
			case 6:// 보스가 보는 방향으로 지진
			{
				dir = game_room[game_room_id]->boss_id->direction;
				for (int i = 0; i < 5; ++i) {
					timer_event t;
					t.ev = EVENT_BOSS_TILE_ATTACK;
					t.obj_id = client_id;
					t.target_id = target_id;
					t.game_room_id = game_room_id;
					t.x = PatternInfo::HexCellAround[dir][0] * i + pos_x;
					t.y = PatternInfo::HexCellAround[dir][1] * i + pos_y;
					t.z = PatternInfo::HexCellAround[dir][2] * i + pos_z;
					t.start_time = std::chrono::system_clock::now() + std::chrono::milliseconds(charging_time);
					timer_queue.push(t);
				}
			}
			break;
			default:
				std::cout << "wrong pattern type\n";
				break;
			}
			//해당 게임 룸에 있는 모든 오브젝트한테 보내야됨
			// gamestart도 여러번 들어가는듯
			for (const auto pl : game_room[game_room_id]->player_ids) {

				send_effect(pl->id, client_id, target_id, pattern_type, charging_time, dir, pos_x, pos_y, pos_z);
			}
			exp_over_pool.push(exp_over);
		}
		break;
		case OP_BOSS_TILE_ATTACK:
		{
			int pos_x = *(reinterpret_cast<int*>(exp_over->_net_buf));
			int pos_y = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int)));
			int pos_z = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 2));
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf + sizeof(int) * 3));
			do_npc_tile_attack(game_room_id, pos_x, pos_y, pos_z);
			// 맞았을 때 처리
			exp_over_pool.push(exp_over);
		}
		break;
		case OP_GAME_END:
		{
			int game_room_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			//보스 체력 확인하고
			int boss_id = game_room[game_room_id]->boss_id->id;
			//if(clients[boss_id]->hp<10;
			//체력에 따라 클리어 유무
			for (const auto p : game_room[game_room_id]->player_ids)
				send_game_end(p->id, GAME_CLEAR);
			for (const auto p : game_room[game_room_id]->player_ids)
				send_change_scene(p->id, FIELD_MAP);

			game_room[game_room_id]->isGaming = false;



			exp_over_pool.push(exp_over);
		}
		break;

		default:
			break;
		}
	}
}

void Network::game_start(int room_id)
{
	int boss_id = game_room[room_id]->boss_id->id;
	int map_type = game_room[room_id]->map_type;

	const std::vector<PatternInfo>& pt = maps[map_type]->GetPatternTime();


	Witch* boss = reinterpret_cast<Witch*>(clients[boss_id]);

	/*
	* 현재 필요한 것
	* - 보스 비 유도 공격 2가지
	* - 보스 유도 공격 1가지
	* 이걸 구현하기 위해 필요한 것
	* - noteType에 타입 지정 -> 비 유도부터 순서대로 5번~7번
	* - 가장 멀리 떨어진 플레이어를 찾는 코드
	* - 보스가 보는 방향으로 지진
	*	- 맵의 끝까지 보내야됨 -> 현재 좌표에서 맵의 끝까지 몇 칸인지 계산하는 함수가 필요함
	*	- 지진을 나타낼 임시 에니메이션
	* * 일단 유도공격은 나중에
	*/
	for (const auto& t : pt) {
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
			tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time - t.speed);
			tev.charging_time = t.speed;
			tev.pivotType = t.pivotType;
			timer_queue.push(tev);// 여기 move를 사용해도 될까?
			break;
		case 3:// 패턴 3번
		case 4:// 패턴 4번
		case 99:// 단일 장판 공격
		case 5: // 가장 멀리있는 적에게 물줄기 발사
		case 6: // 보스가 보는 방향으로 지진
			tev.ev = EVENT_BOSS_TILE_ATTACK_START;
			tev.obj_id = boss_id;
			tev.type = t.type;
			tev.x = t.x;
			tev.y = t.y;
			tev.z = t.z;
			tev.game_room_id = room_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
			tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time - t.speed);
			tev.charging_time = t.speed;
			tev.pivotType = t.pivotType;
			timer_queue.push(tev);
			break;

		case 10: // 유도 공격 -> 패링할 수 있음
			/*
			* 유도공격은 패링할 수 있음
			* 서버가 알 수 있는 정보
			*	- 패링할 수 있는 시간
			*	- 클라이언트에서 보낸 패링 패킷의 도착 시간
			*
			* 그러면 클라이언트에서 패킷이 오면 해당 플레이어가 어느 방에서 플레이중인지 찾고
			* 해당 방에서 게임이 진행된 시간을 계산한 다음
			* 패링가능한 패턴을 찾아서(찾는건 시간으로 이진탐색을 하자)
			* 비교해보면서 포함되는게 있으면 패링 성공으로 패킷을 보낸다.
			*
			* 해야할 일
			*	- 유도 공격 이펙트 패킷을 보낼 이벤트를 추가한다.
			*	- 추가된 이벤트에서 클라이언트에 패킷을 보내면 클라이언트에서 이펙트를 출력
			*	- 출력된 이펙트를 보고 플레이어가 패링 패킷을 서버에 전송
			*	- 서버에 전송된 패킷을 보고 서버에서
			*		- 클라이언트가 플레이중인 게임 방을 찾음
			*		- 해당 게임 방의 게임 플레이 시간과 패킷이 온시간을 계산
			*		- 계산된 시간대에 패링가능한 공격이 있었는지 확인한 후 만약 있었다면
			*			- 패링 성공 패킷 클라이언트로 전송 -> 이 때 타겟인 플레이어가 패링 패킷을 보낸경우에만 패링가능 -> 패킷에 자신의 번호가 들어있어야함
			*			- 없으면 고냥 실패
			*	- 끝 -
			*/

			tev.ev = EVENT_PLAYER_PARRYING;
			tev.obj_id = boss_id;
			tev.type = t.type;
			tev.x = t.x;
			tev.y = t.y;
			tev.z = t.z;
			tev.game_room_id = room_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
			tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time - t.speed);
			tev.charging_time = t.speed;
			tev.pivotType = t.pivotType;
			timer_queue.push(tev);
			break;
		case -600:
			tev.ev = EVENT_GAME_END;
			tev.game_room_id = room_id;
			//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(timeByBeat * i);
			tev.start_time = game_room[room_id]->start_time + std::chrono::milliseconds(t.time);

			timer_queue.push(std::move(tev));
			break;
		default:
			std::cout << "잘못된 패턴 타입" << std::endl;
			break;
		}
	}

	/*
	// 맵 중앙으로 옮기자
	for (int i : game_room[room_id]->player_ids) {
		clients[i]->x = maps[game_room[room_id]->map_type]->LengthX / 2;
		clients[i]->z = maps[game_room[room_id]->map_type]->LengthZ / 2;
		clients[i]->y = -clients[i]->z - clients[i]->x;

		for (int j : game_room[room_id]->player_ids)
			send_move_object(j, i);
	}*/
}