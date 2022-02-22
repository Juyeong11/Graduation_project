//#pragma once
//
///*
//* 각 스킬의 공동 동작
//*  스킬 레벨
//* 차별점
//*/
//
//나중에는 Delay CoolTime등을 게임 들어가면서 스크립트에서 읽어 오도록 만들까?
//계속 다시 빌드 할 수 는없으니까
//class PlayerSkill
//{
//public:
//	~PlayerSkill() {};
//
//	int Delay;
//	int CoolTime;
//	int Speed;
//	int Damage;
//	int SkillLevel;
//};
//
//class WaterArrow : public PlayerSkill
//{
//public:
//	virtual int get_skill_type() {
//		return skill_type;
//	}
//private:
//	static constexpr int range[] = { LEFTUP, UP, RIGHTUP, LEFTDOWN, DOWN, RIGHTDOWN };
//	int skill_type;
//};
//
//class Quake : public PlayerSkill
//{
//public:
//	virtual int get_skill_type() {
//		return skill_type;
//
//	}
//private:
//	static constexpr int range[] = { LEFTUP, UP, RIGHTUP, LEFTDOWN, DOWN, RIGHTDOWN };
//	int skill_type;
//
//};
//
//class Heal : public PlayerSkill
//{
//public:
//	virtual int get_skill_type() {
//		return skill_type;
//
//	}
//private:
//	static constexpr int range[] = { LEFTUP, UP, RIGHTUP, LEFTDOWN, DOWN, RIGHTDOWN };
//	int skill_type;
//
//};