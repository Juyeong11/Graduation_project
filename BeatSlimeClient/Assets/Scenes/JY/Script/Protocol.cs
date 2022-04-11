using System;
using System.Runtime.InteropServices;


namespace Protocol
{

    public class CONSTANTS
    {

        public const short SERVER_PORT = 4000;

        public const int WORLD_HEIGHT = 2000;
        public const int WORLD_WIDTH = 2000;
        public const int MAX_NAME_SIZE = 20;
        public const int CHAT_BUF_SIZE = 122;
        public const int MAX_USER = 5000;
        public const int MAX_NPC = 4;
        public const int NPC_ID_START = MAX_USER;
        public const int NPC_ID_END = MAX_USER + MAX_NPC - 1;
        public const int MAX_OBJECT = MAX_USER + MAX_NPC;

        public const byte CS_PACKET_LOGIN = 1;
        public const byte CS_PACKET_MOVE = 2;
        public const byte CS_PACKET_READ_MAP = 3;
        public const byte CS_PACKET_WRITE_MAP = 4;
        public const byte CS_PACKET_CHANGE_SCENE_READY = 5;
        public const byte CS_PACKET_GAME_START_READY = 6;
        public const byte CS_PACKET_PARRYING = 7;
        public const byte CS_PACKET_CHANGE_SCENE_DONE = 8;
        public const byte CS_PACKET_USE_SKILL = 9;
        public const byte CS_PACKET_CHANGE_SKILL = 10;
        public const byte CS_PACKET_PING_TEST = 11;
        //public const byte CS_PACKET_GET_SINGLE_PLAYER_LIST = 12;
        public const byte CS_PACKET_PARTY_REQUEST = 13;
        public const byte CS_PACKET_PARTY_REQUEST_ANWSER = 14;
        public const byte CS_PACKET_CHAT = 15;
        public const byte CS_PACKET_SET_PATH = 16;
        public const byte CS_PACKET_TELEPORT = 17;
        public const byte CS_PACKET_BUY = 18;

        public const byte SC_PACKET_LOGIN_OK = 1;
        public const byte SC_PACKET_MOVE = 2;
        public const byte SC_PACKET_PUT_OBJECT = 3;
        public const byte SC_PACKET_REMOVE_OBJECT = 4;
        public const byte SC_PACKET_GAME_START = 5;
        public const byte SC_PACKET_ATTACK = 6;
        public const byte SC_PACKET_MAP_DATA = 7;
        public const byte SC_PACKET_CHANGE_SCENE = 8;
        public const byte SC_PACKET_EFFECT = 9;
        public const byte SC_PACKET_GAME_END = 10;
        public const byte SC_PACKET_PARRYING = 11;
        public const byte SC_PACKET_GAME_INIT = 12;
        public const byte SC_PACKET_CHANGE_SKILL = 13;
        public const byte SC_PACKET_PING_TEST = 14;
        public const byte SC_PACKET_PARTY_REQUEST = 15;
        public const byte SC_PACKET_PARTY_REQUEST_ANWSER = 16;
        public const byte SC_PACKET_CHAT = 17;
        public const byte SC_PACKET_LOGIN_FAIL = 18;
        public const byte SC_PACKET_BUY_RESULT = 19;


    }
    enum DIR
    {
        LEFTUP, UP, RIGHTUP, LEFTDOWN, DOWN, RIGHTDOWN
    };

    enum OBJECT_TYPE
    {
        PLAPER, ENEMY
    };

    public class ISerializeble<T> where T : class
    {
        public ISerializeble() { }
        public byte[] GetBytes()
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, array, 0, size);
            Marshal.FreeHGlobal(ptr);
            return array;
        }
        public static T SetByteToVar(byte[] array)
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);
            var s = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return s;
        }


    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Map
    {
        public int id;
        public int x, y, z, w;
        public int color, type;

        public static Map SetByteToMap(byte[] array, int startIndex)
        {
            var size = Marshal.SizeOf(typeof(Map));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, startIndex, ptr, size);
            var s = (Map)Marshal.PtrToStructure(ptr, typeof(Map));
            Marshal.FreeHGlobal(ptr);
            return s;
        }
    }

    public class LandScape
    {
        public int id;
        public int x, y, z, w;
        public float offX, offY, offZ, offRotate, offScale;
        public int color, type;

    }

    //Client -> Server
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_login : ISerializeble<cs_packet_login>
    {
        public byte size;
        public byte type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.MAX_NAME_SIZE)]
        public byte[] name = new byte[CONSTANTS.MAX_NAME_SIZE];
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_change_scene_ready : ISerializeble<cs_packet_change_scene_ready>
    {
        public byte size;
        public byte type;
        public byte is_ready;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_change_scene_done : ISerializeble<cs_packet_change_scene_done>
    {
        public byte size;
        public byte type;
        public byte scene_num;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_game_start_ready : ISerializeble<cs_packet_game_start_ready>
    {
        public byte size;
        public byte type;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_move : ISerializeble<cs_packet_move>
    {
        public byte size;
        public byte type;
        public byte direction;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_read_map : ISerializeble<cs_packet_read_map>
    {
        public byte size;
        public byte type;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_write_map : ISerializeble<cs_packet_write_map>
    {
        public byte size;
        public byte type;
        public int id;
        public int x, y, z, w;
        public int color, block_type;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_parrying : ISerializeble<cs_packet_parrying>
    {
        public byte size;
        public byte type;
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_use_skill : ISerializeble<cs_packet_use_skill>
    {
        public byte size;
        public byte type;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_change_skill : ISerializeble<cs_packet_change_skill>
    {
        public byte size;
        public byte type;
        public byte skill_type;
        public byte skill_level;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_ping_test : ISerializeble<cs_packet_ping_test>
    {
        public byte size;
        public byte type;
        public int ping_time; //
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_party_request : ISerializeble<cs_packet_party_request>
    {
        public byte size;
        public byte type;
        public int id; //
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_party_request_anwser : ISerializeble<cs_packet_party_request_anwser>
    {
        public byte size;
        public byte type;
        public byte anwser; //
        public int requester; //
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_chat : ISerializeble<cs_packet_chat>
    {
        public byte size;
        public byte type;
        public byte sendType;
        public int reciver;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.CHAT_BUF_SIZE)]
        public byte[] mess = new byte[CONSTANTS.CHAT_BUF_SIZE];
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_set_path : ISerializeble<cs_packet_set_path>
    {
        public byte size;
        public byte type;
        public short x, z;
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_teleport : ISerializeble<cs_packet_teleport>
    {
        public byte size;
        public byte type;
        public byte pos;
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_buy : ISerializeble<cs_packet_buy>
    {
        public byte size;
        public byte type;
        public byte itemType;
    }
    //Server -> Client

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_login_ok : ISerializeble<sc_packet_login_ok>
    {
        public byte size;
        public byte type;
        public int id;
        public short x, y, z;
        public int money;
        public byte cur_skill_type;
        public byte cur_skill_level;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] skill_progress = new byte[3];
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_login_fail : ISerializeble<sc_packet_login_fail>
    {
        public byte size;
        public byte type;
        public byte reason;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_move : ISerializeble<sc_packet_move>
    {
        public byte size;
        public byte type;
        public int id;
        public int dir;
        public short x, y, z;
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_attack : ISerializeble<sc_packet_attack>
    {
        public byte size;
        public byte type;
        public int id;
        public int target_id;
        public byte direction;
        public int hp;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_effect : ISerializeble<sc_packet_effect>
    {
        public byte size;
        public byte type;
        public byte effect_type;
        public byte dir;
        public int id;
        public int target_id;
        public int charging_time;
        public int x,y,z;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_put_object : ISerializeble<sc_packet_put_object>
    {
        public byte size;
        public byte type;
        public int id;
        public short x, y, z;
        public byte obj_type;
        public byte direction;
        public byte skillType;
        public byte skillLevel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.MAX_NAME_SIZE)]
        public byte[] name = new byte[CONSTANTS.MAX_NAME_SIZE];
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_remove_object : ISerializeble<sc_packet_remove_object>
    {
        public byte size;
        public byte type;
        public int id;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_game_start : ISerializeble<sc_packet_game_start>
    {
        public byte size;
        public byte type;
        public int game_start_time;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_game_init : ISerializeble<sc_packet_game_init>
    {
        public byte size;
        public byte type;
        public int player_id;
        public int id1;
        public int id2;
        public int id3;
        public int boss_id;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_change_scene : ISerializeble<sc_packet_change_scene>
    {
        public byte size;
        public byte type;
        public char scene_num;

    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_map_data : ISerializeble<sc_packet_map_data>
    {
        public byte size;
        public byte type;

    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_game_end : ISerializeble<sc_packet_game_end>
    {
        public byte size;
        public byte type;
        public byte end_type;

    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_parrying : ISerializeble<sc_packet_parrying>
    {
        public byte size;
        public byte type;
        public int id;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_change_skill : ISerializeble<sc_packet_change_skill>
    {
        public byte size;
        public byte type;
        public int id;
        public byte skill_type;
        public byte skill_level;
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_ping_test : ISerializeble<sc_packet_ping_test>
    {
        public byte size;
        public byte type;
        public int ping_time;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_party_request : ISerializeble<sc_packet_party_request>
    {
        public byte size;
        public byte type;
        public int requester_id;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_party_request_anwser : ISerializeble<sc_packet_party_request_anwser>
    {
        public byte size;
        public byte type;
        public byte anwser;
        public int p_id;
       [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] ids = new int[3];
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_chat : ISerializeble<sc_packet_chat>
    {
        public byte size;
        public byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.CHAT_BUF_SIZE)]
        public byte[] mess = new byte[CONSTANTS.CHAT_BUF_SIZE];
        public int p_id;
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_buy_result : ISerializeble<sc_packet_buy_result>
    {
        public byte size;
        public byte type;
        public byte itemType;
        public byte result;
    }
}
