using System.Collections;
using System.Collections.Generic;

//이동
public class EMoving
{
    //언제
    public Beat when;
    //텔레포트인가 스무스한 이동인가 blah...
    public int which;
    //어디로
    public HexCoordinates where;
}

//장판기
public class EAttack
{
    //언제
    public Beat when;
    //어떤 스킬을
    public int which;
    //어디다가
    public HexCoordinates where;
    //어느 방향으로
    public HexDirection direction;
}

//마법화살
public class ENote
{
    //언제
    public Beat when;
    //어떤 스킬을
    public int which;
    //누구에게
    public int who;
}