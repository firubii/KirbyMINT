using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MINT;

namespace MINT.TDX
{
    public class Opcodes
    {
        public Dictionary<byte, string> opcodeNames = new Dictionary<byte, string>()
        {
            { 0x01, "setTrue" },
            { 0x02, "setFalse" },
            { 0x03, "load" },
            { 0x04, "loadString"},
            { 0x05, "moveRegister" },
            { 0x06, "moveResult" },
            { 0x07, "setArg" },
            { 0x08, "setArg2" },
            { 0x09, "setArg3" },
            { 0x0A, "setArg?" },

            { 0x0C, "getStatic" },
            { 0x0D, "getDeref" },
            { 0x0E, "getField" },
            { 0x0F, "sizeOf" },

            { 0x12, "putDeref" },
            { 0x13, "putField" },
            { 0x14, "putStatic" },

            { 0x15, "addi" },
            { 0x16, "subi" },
            { 0x17, "multi" },
            { 0x18, "divi" },
            { 0x19, "modi" },

            { 0x1D, "inci" },
            { 0x1F, "negi" },

            { 0x46, "declare" },
            { 0x47, "return" },
            { 0x48, "returnVal" },
            { 0x49, "callLocal" },
            { 0x4A, "call?" },
            { 0x4B, "callExternal" }
        };
        public Dictionary<byte, Format> opcodeFormats = new Dictionary<byte, Format>()
        {
            { 0x01, Format.Z },
            { 0x02, Format.Z },
            { 0x03, Format.sZV },
            { 0x04, Format.strZV },
            { 0x05, Format.ZX },
            { 0x06, Format.Z },
            { 0x07, Format.aZX },
            { 0x08, Format.aZXY },
            { 0x09, Format.ZXY },
            { 0x0A, Format.aaZX },

            { 0x0C, Format.strZV },
            { 0x0D, Format.ZX },
            { 0x0E, Format.ZXxY },
            { 0x0F, Format.xZV },

            { 0x12, Format.ZX },
            { 0x13, Format.ZXxY },
            { 0x14, Format.xZV },

            { 0x15, Format.ZXY },
            { 0x16, Format.ZXY },
            { 0x17, Format.ZXY },
            { 0x18, Format.ZXY },
            { 0x19, Format.ZXY },

            { 0x1D, Format.Z },
            { 0x1F, Format.ZX },

            { 0x46, Format.nZXY },
            { 0x47, Format.None },
            { 0x48, Format.Y },
            { 0x49, Format.xV },
            { 0x4A, Format.xV },
            { 0x4B, Format.xV }
        };
    }
}
