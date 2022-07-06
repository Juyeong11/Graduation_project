using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public enum playerState
{
    Idle,
    move,
}

public class PlayerManager : MonoBehaviour
{
    public bool isDebug;

    public Animator JumpTrigger;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public HexCellPosition selfCoord;
    //public HexDirection selfDirection;
    public HexGrid grid;

    public Transform PlayerTransform;
    public Transform EnemyTransform;

    Image PlayerHPImage;
    Image PlayerPrevHPImage;

    public HPManager HP;

    bool playerAttacking;
    bool playerDieAnimTriggered;
    List<(Beat,float)> SettledBallBeats;

    public bool isThisCurrentPlayingPlayerObject;
    public int playerClassofSkill;
    public int playerLevelofSkill;

    Cell nowOnCellTag;

    public void Start()
    {
        HP = new HPManager();
        playerAttacking = false;
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfCoord.direction = HexDirection.Up;
        SettledBallBeats = new List<(Beat,float)>();
        playerDieAnimTriggered = false;
        nowOnCellTag = null;

        //DEBUG :
        //playerClassofSkill = 2;

        HP.Initialized(false);
        //onPlayerFly.Invoke();
    }
    public void LoginOk()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfCoord.direction = HexDirection.Up;
        //onPlayerFly.Invoke();
    }

    public void SetHPImages(Image hp, Image prev)
    {
        PlayerHPImage = hp;
        PlayerPrevHPImage = prev;
    }

    public void JumpTrig()
    {
        JumpTrigger.SetTrigger("Jump");
    }
    public void StunTrig()
    {
        JumpTrigger.SetTrigger("Stun");
    }
    public void ParryTrig()
    {
        JumpTrigger.SetTrigger("Parry");
    }
    public void AttackTrig()
    {
        JumpTrigger.SetTrigger("Attack");
    }

    public void SkillTrig()
    {
        switch(playerClassofSkill)
        {
            case 0:
            break;
            case 1:
            break;
            case 2:
                JumpTrigger.SetTrigger("Heal");
            break;
        }

    }

    void Update()
    {
        if (GameManager.data.isGameStart)
        {
            playerAttacking = false;

            if (isThisCurrentPlayingPlayerObject)
            {
                KeyHandler();
                BallBeatCheck();
            }
            PlayerRotateToLookAt();
            PlayerWCheck();

            if (isThisCurrentPlayingPlayerObject&&nowOnCellTag.obejct)
            {
                //print($"{nowOnCellTag.x}, {nowOnCellTag.y}, {nowOnCellTag.z}");
                PlayerItemCheck();
            }

        }
        CalcHPSlider();
    }

    public void PlayerWCheck()
    {
        if (selfCoord.coordinates.W != grid.cellMaps.Get(selfCoord.coordinates).w + 1)
        {
            //if (isThisCurrentPlayingPlayerObject)
            //{
            //    Debug.LogError(">Player W Coordinate Error!< [ Self W : " + selfCoord.coordinates.W + ", Cell W : " + grid.cellMaps.Get(selfCoord.coordinates).w + 1 + " ]");
            //}
            //print("Self W : " + selfCoord.coordinates.W);
            selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;

        }
        PlayerTransform.position = calculatePlayerPosition();

        //Debug.Log("z : " + gameObject.transform.position.z);

    }

    public void PlayerRotateToLookAt()
    {
        switch(selfCoord.direction)
        {
            case HexDirection.LeftUp:
                //Vector3 c = Vector3.Cross(transform.rotation.eulerAngles, new Vector3(0, -120, 0));
                //transform.Rotate(0, c.x * 3f, 0);

                transform.rotation = Quaternion.Euler(new Vector3(0, -120 - 90, 0));
                break;
            case HexDirection.Up:
                transform.rotation = Quaternion.Euler(new Vector3(0, -90 - 90, 0));
                break;
            case HexDirection.RightUp:
                transform.rotation = Quaternion.Euler(new Vector3(0, -30 - 90, 0));
                break;
            case HexDirection.Down:
                transform.rotation = Quaternion.Euler(new Vector3(0, 90 - 90, 0));
                break;
            case HexDirection.LeftDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 120 - 90, 0));
                break;
            case HexDirection.RightDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 30 - 90, 0));
                break;
        }
    }

    public void PlayerSpinDirection(int x, int y, int z)
    {
        if (selfCoord.coordinates.X - 1 == x && selfCoord.coordinates.Z + 1 == z)
        {
            selfCoord.direction = HexDirection.LeftUp;
        }
        else if (selfCoord.coordinates.X == x && selfCoord.coordinates.Z + 1 == z)
        {
            selfCoord.direction = HexDirection.Up;
        }
        else if (selfCoord.coordinates.X + 1 == x && selfCoord.coordinates.Z == z)
        {
            selfCoord.direction = HexDirection.RightUp;
        }
        else if (selfCoord.coordinates.X - 1 == x && selfCoord.coordinates.Z == z)
        {
            selfCoord.direction = HexDirection.LeftDown;
        }
        else if (selfCoord.coordinates.X == x && selfCoord.coordinates.Z - 1 == z)
        {
            selfCoord.direction = HexDirection.Down;
        }
        else if (selfCoord.coordinates.X + 1 == x && selfCoord.coordinates.Z - 1 == z)
        {
            selfCoord.direction = HexDirection.RightDown;
        }
    }

    public void Beat()
    {
        //Debug.Log("BEAT");
        grid.pPosition = selfCoord.coordinates;
        selfCoord.beat();
    }

    bool KeyCheck(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W:
                selfCoord.direction = HexDirection.LeftUp;
                break;
            case KeyCode.E:
                selfCoord.direction = HexDirection.Up;
                break;
            case KeyCode.R:
                selfCoord.direction = HexDirection.RightUp;
                break;
            case KeyCode.S:
                selfCoord.direction = HexDirection.LeftDown;
                break;
            case KeyCode.D:
                selfCoord.direction = HexDirection.Down;
                break;
            case KeyCode.F:
                selfCoord.direction = HexDirection.RightDown;
                break;
        }

        if (GameManager.data.getIsGoodTiming() != 0)
        {
            return true;
        }
        return false;
    }

    bool ParryCheck(KeyCode k)
    {
        if (PatternManager.data.Factory.parryCheck())
        {
            return true;
        }
        return false;
    }

    public void PlayerMove(int x, int y, int z)
    {
        //GameManager.data.setMoved();
        // if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
        selfCoord.SetPosition(x, y, z);
        //selfDirection = HexDirection.LeftUp;
    }
    void KeyHandler()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     playerAttacking = true;
        // }

        if (isDebug)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                CineCameraShake.instance.ShakeCamera(25);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SkillTrig();
            }
        }

        if (Input.GetKeyDown(KeyCode.W) && KeyCheck(KeyCode.W))
        {
            if (Network.isOnline)
            {
                
                GameManager.data.setMoved();

                Network.SendMovePacket((byte)Protocol.DIR.LEFTUP);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(-1, 0, 1, grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck(KeyCode.E))
        {
            if (Network.isOnline)
            {
                GameManager.data.setMoved();
               
                Network.SendMovePacket((byte)Protocol.DIR.UP);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(0, -1, 1, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y -1, selfCoord.coordinates.Z + 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.R) && KeyCheck(KeyCode.R))
        {
            if (Network.isOnline)
            {
                GameManager.data.setMoved();
                
                Network.SendMovePacket((byte)Protocol.DIR.RIGHTUP);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(1, -1, 0, grid.cellMaps.Get(selfCoord.coordinates.X+1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                    
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck(KeyCode.S))
        {
            if (Network.isOnline)
            {
                GameManager.data.setMoved();
                
                Network.SendMovePacket((byte)Protocol.DIR.LEFTDOWN);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(-1, 1, 0, grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck(KeyCode.D))
        {
            if (Network.isOnline)
            {
                GameManager.data.setMoved();
                
                Network.SendMovePacket((byte)Protocol.DIR.DOWN);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z - 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z -1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(0, 1, -1, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z-1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.F) && KeyCheck(KeyCode.F))
        {
            if (Network.isOnline)
            {
                GameManager.data.setMoved();
                
                Network.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(1, 0, -1, grid.cellMaps.Get(selfCoord.coordinates.X+1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).w - selfCoord.coordinates.W + 1);
                    }
                        JumpTrig();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.J) && KeyCheck(KeyCode.J))
        {
            if (Network.isOnline)
            {
                if (playerClassofSkill != 0 || Vector3.Distance(EnemyTransform.position, PlayerTransform.position) < 5f)   
                {
                    GameManager.data.setMoved();

                    Network.SendUseSkillPacket();
                }

            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) && ParryCheck(KeyCode.Space))
        {

            if (Network.isOnline)
            {
                Network.SendParryingPacket();
            }
        }
    }

    void resetPosition()
    {
        //gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0f, gameObject.transform.position.z);

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, (2f * GameManager.data.beatCounter / GameManager.data.timeByBeat) - 0.1f, gameObject.transform.position.z);
    }

    public void SetBallBeat(Beat now,Beat b)
    {
        Debug.Log(GameManager.data.timeByBeat / (float)b.GetBeatTime() * 0.75f);
        SettledBallBeats.Add((now + b, GameManager.data.timeByBeat / (float)b.GetBeatTime() * 0.75f));
    }

    public void BallBeatCheck()
    {
        if (SettledBallBeats.Count > 0)
        {
            if (SettledBallBeats[0].Item1.GetBeatTime() + GameManager.data.JudgementTiming < GameManager.data.nowBeat.GetBeatTime())
            {
                VFXManager.data.HitSounder(SettledBallBeats[0].Item2);
                SettledBallBeats.RemoveAt(0);
                GameManager.data.MidANote.noteEnd();
            }
            else if (SettledBallBeats[0].Item1.GetBeatTime() - GameManager.data.JudgementTiming < GameManager.data.nowBeat.GetBeatTime()
                && playerAttacking)
            {
                VFXManager.data.HitSounder(1);
                SettledBallBeats.RemoveAt(0);
                GameManager.data.MidANote.notePerfect();
            }
        }
    }


    //Lerp
    public Vector3 calculatePlayerPosition()
    {
        int beatTime = GameManager.data.timeByBeat;

        float tick = LerpSquare((Time.time - selfCoord.preBeatedTime) * 1000f / beatTime);
        float newX;
        float newY;
        float newZ;

        newX = Mathf.Lerp(selfCoord.preCoordinates.X * 0.866f, selfCoord.coordinates.X * 0.866f, tick);
        newZ = Mathf.Lerp(selfCoord.preCoordinates.X * 0.5f + selfCoord.preCoordinates.Z, selfCoord.coordinates.X * 0.5f + selfCoord.coordinates.Z, tick);
        newY = SlimeWLerp(HexCellPosition.calculateWPosition(selfCoord.preCoordinates.W), HexCellPosition.calculateWPosition(selfCoord.coordinates.W), tick);

        if (tick >= 1f)
        {
            selfCoord.preCoordinates = selfCoord.coordinates;
        }

        return new Vector3(newX, newY, newZ);
    }

    public float LerpSquare(float tick)
    {
        if (tick < 0.3f)
            return 0f;
        else if (tick < 0.7f)
        {
            return (tick - 0.3f) * 2.5f;
        }
        else
            return 1f;
    }

    public float SlimeWLerp(float a, float b, float t)
    {
        float skyHigh = (a + b) * 0.5f + 1.7f;

        if (t < 0.5f)
        {
            return Mathf.Lerp(a, skyHigh, t);
        }
        else
        {
            return Mathf.Lerp(skyHigh, b, t);
        }
    }

    public void CalcHPSlider()
    {
        HP.hpUpdate(Time.deltaTime);
        PlayerHPImage.fillAmount = (float)HP.CurrentHP / (float)HP.MaxHp;
        PlayerPrevHPImage.fillAmount = (float)HP.prevHP / (float)HP.MaxHp;

        if (!HP.isAlive && !playerDieAnimTriggered)
        {
            //슬라임 죽는 모션
            JumpTrigger.SetTrigger("Dead");
            playerDieAnimTriggered = true;
        }
    }

    public void cellTag()
    {
        nowOnCellTag = GameManager.data.grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y, selfCoord.coordinates.Z);
    }

    private void PlayerItemCheck()
    {
        //print(nowOnCellTag.state);
        switch (nowOnCellTag.state)
        {
            case cellState.Item1:
                nowOnCellTag.GetItemFromThisCell();
                changeColor(Color.red);
                Debug.Log("Item1");
            break;
            case cellState.Item2:
            nowOnCellTag.GetItemFromThisCell();
                changeColor(Color.yellow);
            break;
            case cellState.Item3:
            nowOnCellTag.GetItemFromThisCell();

            break;
            case cellState.Item4:
            nowOnCellTag.GetItemFromThisCell();
            
            break;
            case cellState.Item5:
            nowOnCellTag.GetItemFromThisCell();

            break;
            case cellState.Item6:
            nowOnCellTag.GetItemFromThisCell();
            break;
        }
    }

    private SkinnedMeshRenderer mesh;

    public void changeColor(Color c)
    {
        mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        mesh.material = Instantiate(mesh.material);
        mesh.material.SetColor("Color_658ca3492a0d4c5294b395208005e893", c);
    }
}
