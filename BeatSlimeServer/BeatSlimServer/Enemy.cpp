#include<cstring>
#include<random>
#include"Enemy.h"
#include"protocol.h"

std::random_device rd;
std::uniform_int_distribution<int> uid(-2, 2);
void Enemy::update(char* buf,int& buf_start)
{
	sc_packet_move_object pk;

	pk.id = _id;
	pk.size = sizeof(pk);
	pk.type = SC_PACKET_MOVE_OBJECT;
	pk.x = uid(rd);
	pk.y =uid(rd);
	pk.z = uid(rd) - 2;

	memcpy(buf + buf_start, &pk, sizeof(pk));

	buf_start += sizeof(pk);
}
void Enemy::PutObject(char* buf, int& buf_start)
{
	sc_packet_put_object packet;
	packet.id = _id;
	//strcpy_s(packet.name, other.name);
	packet.object_type = ENEMY;
	packet.size = sizeof(packet);
	packet.type = SC_PACKET_PUT_OBJECT;
	packet.x = x;
	packet.y = y;
	packet.z = z;

	memcpy(buf + buf_start, &packet, sizeof(packet));

	buf_start += sizeof(packet);
}