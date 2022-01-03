#include "stdfx.h"


#include "DataBase.h"
#include "Network.h"

Network* Network::instance = nullptr;

Network* Network::GetInstance()
{
	return instance;
}

void error_display(const char* err_p, int err_no)
{
	WCHAR* lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, err_no,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf, 0, 0);
	std::cout << err_p << std::endl;
	std::wcout << lpMsgBuf << std::endl;
	//while (true);
	LocalFree(lpMsgBuf);
}
Network::Network() {
	//�ν��Ͻ��� �� ����!!
	assert(instance == nullptr);
	instance = this;

	for (int i = 0; i < MAX_USER; ++i) {
		clients[i] = new Client;
	}
	for (int i = MAX_USER; i <= NPC_ID_END; ++i) {
		clients[i] = new Npc();
	}
	for (int i = 0; i < MAX_OBJECT; ++i) {
		clients[i]->id = i;
	}
	for (int i = 0; i < MAX_OBJECT; ++i) {
		exp_over_pool.push(new EXP_OVER);
	}
	Initialize_NPC();
	DB = new DataBase;
}
Network::~Network() {
	//�����尡 ����� �� �̱� ������ ���� �� �ʿ䰡 ����
//accpet������ �� ������ �����
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
void Network::send_attack_player(int c_id, int target, int receiver)
{
	sc_packet_attack packet;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_ATTACK;
	packet.id = c_id;
	packet.target_id = target;
	packet.direction = DOWN;
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

	if (true == is_npc(target) && false == reinterpret_cast<Npc*>(clients[target])->is_active) {
		reinterpret_cast<Npc*>(clients[target])->is_active = true;
		timer_event t;
		t.ev = EVENT_ENEMY_MOVE;
		t.obj_id = target;
		t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);
		timer_queue.push(t);
	}
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
	std::cout << "Maximum Number of Clients Overflow!!\n";
	return -1;
}

void Network::do_npc_move(int npc_id) {

	std::unordered_set<int> can_attacklist;

	for (int i = 0; i < MAX_USER; ++i) {
		Client* obj = reinterpret_cast<Client*>(clients[i]);
		if (obj->state != ST_INGAME) continue;


			if (true == can_attack(npc_id, obj->id))
				can_attacklist.insert(obj->id);
		
	}
	

	for (auto pl : can_attacklist) {
		if (true == reinterpret_cast<Npc*>(clients[npc_id])->is_active) {
			timer_event t;
			t.ev = EVENT_ENEMY_ATTACK;
			t.obj_id = npc_id;
			t.target_id = pl;
			t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(3);
			timer_queue.push(t);
			break;
		}
	}
	if (true == reinterpret_cast<Npc*>(clients[npc_id])->is_active) {
		timer_event t;
		t.ev = EVENT_ENEMY_MOVE;
		t.obj_id = npc_id;
		t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);
		//timer_queue.push(t);
	}
}

void Network::do_npc_attack(int npc_id, int target_id, int reciver) {

	// �ǰݿ� ���� �˰�����

	// ��Ŷ ����
	Client* cl = reinterpret_cast<Client*>(clients[target_id]);
	cl->vl.lock();
	std::unordered_set cp_vl = cl->viewlist;
	cl->vl.unlock();
	for (int id : cp_vl) {
		if(is_player(id))
			send_attack_player(npc_id, target_id, id);
	}
	send_attack_player(npc_id, target_id, target_id);

	if (true == reinterpret_cast<Npc*>(clients[npc_id])->is_active) {
		timer_event t;
		t.ev = EVENT_ENEMY_ATTACK;
		t.obj_id = npc_id;
		t.target_id = target_id;
		t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(3);
		timer_queue.push(t);
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

		//�ٸ� Ŭ���̾�Ʈ���� ���ο� Ŭ���̾�Ʈ�� ������ �˸�
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

			// ���� ���� Ŭ���̾�Ʈ�� ������ �ִٸ� �� ����Ʈ�� �ְ� put packet�� ������.
			other->vl.lock();
			other->viewlist.insert(client_id);
			other->vl.unlock();

			send_put_object(other->id, client_id);
		}

		//���� ������ Ŭ���̾�Ʈ���� ���� ��ü���� ��Ȳ�� �˷���
		for (auto* other : clients) {
			//���⼭ NPC�� �˷������

			if (other->id == client_id) continue;
			other->state_lock.lock();
			if (ST_INGAME != other->state) {
				other->state_lock.unlock();
				continue;
			}
			other->state_lock.unlock();

			if (false == is_near(other->id, client_id))
				continue;

			// ������ �ִ� Ŭ���̾�Ʈ�� ������ �ִٸ� �� ����Ʈ�� �ְ� put packet�� ������.
			cl.vl.lock();
			cl.viewlist.insert(other->id);
			cl.vl.unlock();

			send_put_object(client_id, other->id);
		}
	}
	break;
	case CS_PACKET_MOVE:
	{
		cs_packet_move* packet = reinterpret_cast<cs_packet_move*>(p);
		//cl.last_move_time = packet->move_time;
		short& x = cl.x;
		short& y = cl.y;
		short& z = cl.z;
		cl.direction = packet->direction;
		switch (packet->direction) {
		case DIR::LEFTUP:		if (true) { x--; z++; break; }
		case DIR::UP:			if (true) { y--; z++;  break; }
		case DIR::RIGHTUP:		if (true) { x++; y--; break; }
		case DIR::LEFTDOWN:		if (true) { x--; y++; break; }
		case DIR::DOWN:			if (true) { y++; z--; break; }
		case DIR::RIGHTDOWN:	if (true) { x++; z--; break; }
		default:
			std::cout << "Invalid move in client " << client_id << std::endl;
			exit(-1);
		}

		// �̵��� Ŭ���̾�Ʈ�� ���� nearlist ����
		// �� unordered_set�̿��� �ұ�?
		// �󸶳� �߰����� �𸣰�, �����ʹ� id�̱� ������ �ߺ������� ������ִ�. id�� ���о��ϴ� ��찡 �ֳ�?
		// ���͸� ������ ��ó�� �ִ��� �˻��� �ӵ��� ������
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

		//lock�ð��� ���̱� ���� �ڷḦ �����ؼ� ���
		cl.vl.lock();
		std::unordered_set<int> my_vl{ cl.viewlist };
		cl.vl.unlock();


		// ���������ν� �þ߿� ���� �÷��̾� Ȯ�� �� �߰�
		for (int other : nearlist) {
			// cl�� �丮��Ʈ�� ������
			if (0 == my_vl.count(other)) {
				// cl�� �丮��Ʈ�� �߰��ϰ�
				cl.vl.lock();
				cl.viewlist.insert(other);
				cl.vl.unlock();
				// �������� �׸���� ��Ŷ�� ������.
				send_put_object(cl.id, other);

				//npc�� send�� ���Ѵ�.
				//npc�� �丮��Ʈ�� ���� �ڽ��� �� �� �ִ� �÷��̾ �ִٸ� isActive������ ���� �����δ�.
				//�÷��̾�� ���� NPC�� ������ �̺�Ʈ�� �����Ѵ�.
				if (true == is_npc(other)) {
					//lock�� �־�� �ϳ�? atomic��������
					//reinterpret_cast<Npc*>(clients[other])->is_active = true;
					//timer_event t;
					//t.ev = EVENT_NPC_MOVE;
					//t.obj_id = other;
					//t.start_time = std::chrono::system_clock::now() + std::chrono::seconds(1);
					//timer_queue.push(t);
					continue;
				}

				Client* otherPlayer = reinterpret_cast<Client*>(clients[other]);
				// ������ ���̸� ��뿡�Ե� ���δٴ� ���̴�
				// ��� �丮��Ʈ�� Ȯ���Ѵ�.
				otherPlayer->vl.lock();

				// ��� �丮��Ʈ�� ������
				if (0 == otherPlayer->viewlist.count(cl.id)) {
					// �丮��Ʈ�� �߰��ϰ� cl�� �׸���� ����
					otherPlayer->viewlist.insert(cl.id);
					otherPlayer->vl.unlock();
					send_put_object(other, cl.id);
				}
				// ��� �丮��Ʈ�� ������ �̵� ��Ŷ ����
				else {
					otherPlayer->vl.unlock();
					send_move_object(other, cl.id);
				}

			}
			//��� �þ߿� �����ϴ� �÷��̾� ó��
			else {

				if (true == is_npc(other)) continue;
				Client* otherPlayer = reinterpret_cast<Client*>(clients[other]);
				otherPlayer->vl.lock();
				//���濡 �丮��Ʈ�� ���� �ִ��� Ȯ��
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


		// ���������ν� �þ߿��� ���� �÷��̾� Ȯ�� �� ����
		for (int other : my_vl) {
			// nearlist�� ������
			if (0 == nearlist.count(other)) {
				// �����׼� �����
				cl.vl.lock();
				cl.viewlist.erase(other);
				cl.vl.unlock();
				send_remove_object(cl.id, other);

				//npc�� view����Ʈ�� ������ ���� �ʴ�.
				if (true == is_npc(other)) {
					//reinterpret_cast<Npc*>(clients[other])->is_active = false;
					continue;
				}
				Client* otherPlayer = reinterpret_cast<Client*>(clients[other]);

				// ���浵 ���� �����.
				otherPlayer->vl.lock();
				//�ִٸ� ����
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
			std::cout << "�� ���� �Ϸ� update �� insert ����\n";
			DB->read_map_data();

			//DB->db_map_data; -> ���⼭ ã�Ҵµ� �ش� id�� ���� ������ ����

			for (const std::pair<int, Map>& m : DB->client_map_data) {
				auto re = std::find_if(DB->db_map_data.cbegin(), DB->db_map_data.cend(), [&](const Map& a) {
					return a.id == m.first;
					});
				if (re == DB->db_map_data.cend()) {
					//�����ؾ��� ������
					DB->insert_map_data(m.second);
				}
				else {
					if (*re == m.second) {
						//������ ������
					}
					else {
						//������Ʈ�ؾ��� ������
						DB->update_map_data(m.second);

					}
				}
			}

			// �����ؾ��� ��

			// �����ؾ��� ��
		}
	}
	break;
	default:
		std::cout << "�̻��� ��Ŷ ����\n";
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
			//�ϳ��� ���Ͽ� ���� Recvȣ���� ������ �ϳ� -> EXP_OVER(����, WSAOVERLAPPED) ���� ����
			//��Ŷ�� �߰��� �߷��� ä�� ������ �� �ִ�. -> ���ۿ� ���ξ��ٰ� ������ �� �����Ϳ� ���� -> ������ ���� ũ�⸦ ����� �� ��ġ���� �ޱ� ��������
			//��Ŷ�� ���� �� �ѹ��� ������ �� �ִ�.	 -> ù ��°�� �������̴� �߶� ó������
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
				std::cout << num_byte << " �۽Ź��� ���� ��\n";
				std::cout << "Ŭ���̾�Ʈ ���� ����\n";
				disconnect_client(client_id);
			}
			exp_over_pool.push(exp_over);
		}
		break;
		case OP_ACCEPT:
		{
			std::cout << "Accept Completed.\n";
			SOCKET c_socket = *(reinterpret_cast<SOCKET*>(exp_over->_net_buf)); // Ȯ�� overlapped����ü�� �־� �ξ��� ��Ĺ�� ������
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

			// exp_over ��Ȱ��
			ZeroMemory(&exp_over->_wsa_over, sizeof(exp_over->_wsa_over));
			c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);
			//char* ���۸� socket*�� �ٲ� ������ ����ų �� �ֵ��� �Ѵ�. ���ϵ� �������ε�?
			*(reinterpret_cast<SOCKET*>(exp_over->_net_buf)) = c_socket;

			AcceptEx(g_s_socket, c_socket, exp_over->_net_buf + sizeof(SOCKET), 0, sizeof(SOCKADDR_IN) + 16,
				sizeof(SOCKADDR_IN) + 16, NULL, &exp_over->_wsa_over);
		}
		break;
		case OP_ENEMY_MOVE:
			do_npc_move(client_id);
			exp_over_pool.push(exp_over);
			break;
		case OP_ENEMY_ATTACK: {
			int target_id = *(reinterpret_cast<int*>(exp_over->_net_buf));
			do_npc_attack(client_id, target_id, target_id);
			exp_over_pool.push(exp_over);
		}
							break;
		default:
			break;
		}
	}
}