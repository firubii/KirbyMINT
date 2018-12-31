using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MINT;

namespace MINT.KPR
{
    public class Opcodes
    {
        public Dictionary<byte, string> opcodeNames = new Dictionary<byte, string>()
        {
            {0x42, "declare" },
            {0x43, "return" },
            {0x44, "returnVal" },
        };
        public Dictionary<byte, Format> opcodeFormats = new Dictionary<byte, Format>()
        {
            {0x42, Format.nZXY },
            {0x43, Format.None },
            {0x44, Format.Y },
        };
    }
}
