using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Crc32C;

namespace MINT
{
    public class ScriptHashCalculator
    {
        public byte[] Hash;

        public ScriptHashCalculator(string name)
        {
            Crc32CAlgorithm crc = new Crc32CAlgorithm();
            Hash = crc.ComputeHash(Encoding.UTF8.GetBytes(name));
            for (int i = 0; i < Hash.Length; i++)
            {
                Hash[i] = (byte)(255 - Hash[i]);
            }
        }
    }
}