using System;
using System.Runtime.InteropServices;


namespace Protocol
{
    
    public class CONSTANTS
    {

        public const int MAX_NAME_SIZE = 20;
        public const int MAX_CLIENT = 10;

        public const byte CS_PACKET_LOGIN = 1;
        public const byte CS_PACKET_MOVE = 2;
        public const byte CS_PACKET_TIMER = 3;


        public const byte SC_PACKET_LOGIN_OK = 1;
        public const byte SC_PACKET_MOVE = 2;
        public const byte SC_PACKET_PUT_OBJECT = 3;
        public const byte SC_PACKET_REMOVE_OBJECT = 4;
        public const byte SC_PACKET_MOVE_OBJECT = 5;
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
    public class cs_packet_timer : ISerializeble<cs_packet_timer>
    {
        public byte size;
        public byte type;
        public double timestamp;
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
        public short x, y, z;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class sc_packet_move_object : ISerializeble<sc_packet_move_object>
    {
        public byte size;
        public byte type;
        public int id;
        public short x, y, z;
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
    public class sc_packet_timer : ISerializeble<sc_packet_timer>
    {
        public byte size;
        public byte type;
        public double timestamp;
    }
}
