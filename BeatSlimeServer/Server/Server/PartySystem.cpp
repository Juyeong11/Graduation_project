#include"stdfx.h"
#include"Client.h"
#include "PartySystem.h"

PartySystem::PartySystem(std::array<GameObject*, MAX_OBJECT>& clients): Clients(clients), lastSearchPlayerID(0)
{
	std::copy(Clients.begin(), Clients.begin()+MAX_USER, sortedClients.begin());
	findHealer = false;
	findDPS = false;
	findTank = false;
}

void PartySystem::SetPlayerID(int id) {
	playerID = id;
}

GameObject** PartySystem::GetSinglePlayerList(char* name)
{
	if (lastSearchPlayerID >= MAX_USER) {
		lastSearchPlayerID = 0;
	}
	std::partial_sort(sortedClients.begin() + lastSearchPlayerID, sortedClients.begin() + lastSearchPlayerID + step, sortedClients.end(),
		[&](GameObject* a, GameObject* b) 
		{
			int a_distance = abs(Clients[playerID]->x - a->x) + abs(Clients[playerID]->z - a->z);
			int b_distance = abs(Clients[playerID]->x - b->x) + abs(Clients[playerID]->z - b->z);
			return a_distance < b_distance;
		});

	return sortedClients.data() + lastSearchPlayerID;
	
	
}