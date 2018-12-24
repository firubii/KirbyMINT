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

            {0x08, "getField" },

            {0x15, "setField" },

            {0x47, "declare" },
            {0x48, "return" },
        };
        public Dictionary<byte, Format> opcodeFormats = new Dictionary<byte, Format>()
        {
            {0x01, Format.Z },
            {0x02, Format.Z },
            {0x03, Format.sZV },
            {0x04, Format.strZV },

            {0x08, Format.xZV },

            {0x15, Format.xZV },

            {0x47, Format.nZXY },
            {0x48, Format.None },
        };
    }
}
