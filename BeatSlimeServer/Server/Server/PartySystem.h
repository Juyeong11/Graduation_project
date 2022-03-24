#pragma once

//class GameObject;
class PartySystem
{
	std::array<GameObject*, MAX_OBJECT>& Clients;
	std::array<GameObject*, MAX_USER> sortedClients;
	int lastSearchPlayerID;
	bool findHealer;
	bool findDPS;
	bool findTank;
	const int step = 10;
	int playerID;
public:
	PartySystem(std::array<GameObject*, MAX_OBJECT>& clients);
	GameObject** GetSinglePlayerList(char* name);
	void SetPlayerID(int id);
};

