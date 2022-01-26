using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDebugTool : MonoBehaviour
{
    public bool debug;
    public bool patternDebug;

    void Start()
    {
        if (patternDebug)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (debug)
        {
            //게임 시작
            if (Input.GetKeyDown(KeyCode.Return))
            {
                FieldGameManager.Net.debugOnline = true;

                Protocol.sc_packet_game_start game_start_packet = new Protocol.sc_packet_game_start();
                game_start_packet.type = Protocol.CONSTANTS.SC_PACKET_GAME_START;
                game_start_packet.size = (byte)Marshal.SizeOf(typeof(Protocol.sc_packet_game_start));
                game_start_packet.id1 = 0;
                game_start_packet.id2 = 1;
                game_start_packet.id3 = 2;
                game_start_packet.boss_id = 5000;
                game_start_packet.player_id = 0;

                Network.MessQueue.Enqueue(game_start_packet.GetBytes());
            }
            
            if(Input.GetKeyDown(KeyCode.I))
            {
                Protocol.sc_packet_move tmp = new Protocol.sc_packet_move();
                tmp.id = 5000;  //boss
                tmp.type = Protocol.CONSTANTS.SC_PACKET_MOVE;
                tmp.size = (byte)Marshal.SizeOf(typeof(Protocol.sc_packet_move));
                tmp.x = -1;
                tmp.y = 0;
                tmp.z = 1;

                //Debug.Log("Move Packet");

                Network.MessQueue.Enqueue(tmp.GetBytes());
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                GameManager.data.enemy.GetComponent<EnemyManager>().HP.Damage(150);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GameManager.data.player.GetComponent<PlayerManager>().HP.Damage(40);
            }
        }
    }
}
