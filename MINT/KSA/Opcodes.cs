using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MINT;

namespace MINT.KSA
{
    public class Opcodes
    {
        public Dictionary<byte, string> opcodeNames = new Dictionary<byte, string>()
        {
            {0x01, "setTrue" },
            {0x02, "setFalse" },
            {0x03, "load" },
            {0x04, "loadString" },
            {0x05, "moveRegister" },

            {0x08, "getField" },

            {0x0D, "xref_unknown_0D" },

            {0x0F, "xref_unknown_0F" },

            {0x15, "setField" },

            {0x19, "addi" },
            {0x1A, "subi" },
            {0x1B, "multi" },
            {0x1C, "divi" },
            {0x1D, "modi" },
            {0x1E, "inci" },
            {0x1F, "negi" },

            {0x20, "addf" },
            {0x21, "subf" },
            {0x22, "multf" },
            {0x23, "divf" },
            {0x24, "modf" },
            {0x25, "incf" },
            {0x26, "negf" },

            {0x27, "intLess" },
            {0x28, "intLessOrEqual" },
            {0x29, "intEqual" },
            {0x2A, "intNotEqual" },

            {0x2B, "floatLess" },
            {0x2C, "floatLessOrEqual" },
            {0x2D, "floatEqual" },
            {0x2E, "floatNotEqual" },

            {0x3E, "bitAnd" },
            {0x3F, "bitOr" },
            {0x40, "bitXor" },

            {0x44, "jump" },
            {0x45, "jumpIfTrue" },
            {0x46, "jumpIfFalse" },

            {0x47, "declare" },
            {0x48, "return" },
            {0x49, "call" },
            {0x4A, "callSystem3" },
            {0x4B, "callSystem2" },
            {0x4C, "callSystem" },

            {0x53, "xref_unknown_53" },
            {0x54, "xref_unknown_54" },
            {0x55, "xref_unknown_55" },

            {0x67, "new" },

            {0x6B, "getConstant" }
        };
        public Dictionary<byte, Format> opcodeFormats = new Dictionary<byte, Format>()
        {
            {0x01, Format.Z },
            {0x02, Format.Z },
            {0x03, Format.sZV },
            {0x04, Format.strZV },
            {0x05, Format.ZX },

            {0x08, Format.xZV },

            {0x0D, Format.xZV },

            {0x0F, Format.xZV },

            {0x15, Format.xZV },

            {0x19, Format.ZXY },
            {0x1A, Format.ZXY },
            {0x1B, Format.ZXY },
            {0x1C, Format.ZXY },
            {0x1D, Format.ZXY },
            {0x1E, Format.Z },
            {0x1F, Format.Z },

            {0x20, Format.ZXY },
            {0x21, Format.ZXY },
            {0x22, Format.ZXY },
            {0x23, Format.ZXY },
            {0x24, Format.ZXY },
            {0x25, Format.Z },
            {0x26, Format.Z },

            {0x27, Format.ZXY },
            {0x28, Format.ZXY },
            {0x29, Format.ZXY },
            {0x2A, Format.ZXY },

            {0x2B, Format.ZXY },
            {0x2C, Format.ZXY },
            {0x2D, Format.ZXY },
            {0x2E, Format.ZXY },

            {0x3E, Format.ZXY },
            {0x3F, Format.ZXY },
            {0x40, Format.ZXY },

            {0x44, Format.shV },
            {0x45, Format.shZV },
            {0x46, Format.shZV },

            {0x47, Format.nZXY },
            {0x48, Format.Y },
            {0x49, Format.xZV },
            {0x4A, Format.xZV },
            {0x4B, Format.xZV },
            {0x4C, Format.xZV },

            {0x53, Format.xZV },
            {0x54, Format.xZV },
            {0x55, Format.xZV },

            {0x67, Format.xZV },

            {0x6B, Format.xZV }
        };
    }
}
