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
        public const int MAX_USER = 5000;
        public const int MAX_NPC = 4;
        public const int NPC_ID_START = MAX_USER;
        public const int NPC_ID_END = MAX_USER + MAX_NPC - 1;
        public const int MAX_OBJECT = MAX_USER + MAX_NPC;

        public const byte CS_PACKET_LOGIN = 1;
        public const byte CS_PACKET_MOVE = 2;
        public const byte CS_PACKET_READ_MAP = 3;
        public const byte CS_PACKET_WRITE_MAP = 4;

        public const byte SC_PACKET_LOGIN_OK = 1;
        public const byte SC_PACKET_MOVE = 2;
        public const byte SC_PACKET_PUT_OBJECT = 3;
        public const byte SC_PACKET_REMOVE_OBJECT = 4;
        public const byte SC_PACKET_GAME_START = 5;
        public const byte SC_PACKET_ATTACK = 6;
        public const byte SC_PACKET_MAP_DATA = 7;
    }
    enum DIR
    {
        UP, DOWN, LEFTUP, RIGHTUP, LEFTDOWN, RIGHTDOWN
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
        public float offX, offY, offZ, offRotate;
        public int color, type;

    }

    //Client -> Server
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class cs_packet_login : ISerializeble<cs_packet_login>
    {
        public byte size;
        public byte type;

        /*        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.MAX_NAME_SIZE)]
                public byte[] name = new byte[CONSTANTS.MAX_NAME_SIZE];*/
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
    //Server -> Client

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_login_ok : ISerializeble<sc_packet_login_ok>
    {
        public byte size;
        public byte type;
        public int id;
        public short x, y, z;
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

        /*        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.MAX_NAME_SIZE)]
                public byte[] name = new byte[CONSTANTS.MAX_NAME_SIZE];*/
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
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_map_data : ISerializeble<sc_packet_map_data>
    {
        public byte size;
        public byte type;

    }
}
