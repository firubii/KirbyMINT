using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINT
{
    public enum Game
    {
        TDX,
        KPR,
        KSA
    }
    public enum Format
    {
        None, //No arguments
        Z, //1 argument - (register) byte 2
        X, //1 argument - (register) byte 3
        Y, //1 argument - (register) byte 4
        sV, //1 argument - read uint32 from sdata at ushort offset byte 3/4
        sZV, //2 arguments - (register) byte 2 & read uint32 from sdata at ushort offset byte 3/4
        strV, //1 argument - read string from sdata at ushort offset byte 3/4
        strZV, //2 arguments - (register) byte 2 & read string from sdata at ushort offset byte 3/4
        xV, //1 argument - read cmd hash/string from xref data, ushort index byte 3/4
        xZV, //2 arguments - (register) byte 2 & read cmd hash/string from xref data, ushort index byte 3/4
        xZX, //2 arguments - (register) byte 2 & read cmd hash/string from xref data, index byte 3
        shV, //1 argument - short byte 3/4
        shZV, //2 arguments - (register) byte 2 & short byte 3/4
        ZX, //2 arguments - (register) byte 2 & byte 3
        aZX, //2 arguments - (arg) byte 2 & (register) byte 3
        aaZX, //2 arguments - (arg) byte 2 & (arg) byte 3
        ZY, //2 arguments - byte 2 & byte 4
        ZXxY, //3 arguments - (register) byte 2 & byte 3 & read cmd hash/string from xref data, index byte 4
        aZXY, //3 arguments - (arg) byte 2 & (register) byte 3 & byte 4
        nZXY, //3 arguments - declare statement parameters
        ZXY, //3 arguments - (register) byte 2 & byte 3 & byte 4
        XY, //2 arguments - (register) byte 3 & byte 4
        LDP, //Loads a pair (wx, wy) into rz. If x or y is above 0x80, the 32 bit data is taken from SDATA_START + x or y - 128 instead
        LDPstr, //Same as LDP except it loads a null-terminated string instead
        Ret
    }
}
