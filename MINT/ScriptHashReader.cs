using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MINT;
using Crc32C;

namespace MINT
{
    public class ScriptHashReader
    {
        public Dictionary<uint, string> hashes = new Dictionary<uint, string>();
        public List<uint> unknownHashes = new List<uint>();

        public ScriptHashReader(byte[] script)
        {
            hashes = new Dictionary<uint, string>();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(script)))
            {
                Read(reader);
            }
        }
        public ScriptHashReader(byte[] script, Dictionary<uint, string> hashList)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(script)))
            {
                ReadUnknownHashes(reader, hashList);
            }
        }

        public void Read(BinaryReader reader)
        {
            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            uint scriptnameoffset = reader.ReadUInt32();
            reader.BaseStream.Seek(0x1C, SeekOrigin.Begin);
            uint classlist = reader.ReadUInt32();
            reader.BaseStream.Seek(classlist, SeekOrigin.Begin);
            uint classcount = reader.ReadUInt32();
            List<uint> classoffsets = new List<uint>();
            for (int i = 0; i < classcount; i++)
            {
                classoffsets.Add(reader.ReadUInt32());
            }
            for (int i = 0; i < classcount; i++)
            {
                reader.BaseStream.Seek(classoffsets[i], SeekOrigin.Begin);
                uint nameoffset = reader.ReadUInt32();
                byte[] hash = reader.ReadBytes(0x4);
                uint varlist = reader.ReadUInt32();
                uint methodlist = reader.ReadUInt32();
                reader.BaseStream.Seek(nameoffset, SeekOrigin.Begin);
                string name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                uint classhash = BitConverter.ToUInt32(hash, 0);
                if (!hashes.ContainsKey(classhash))
                {
                    hashes.Add(classhash, name);
                }
                reader.BaseStream.Seek(varlist, SeekOrigin.Begin);
                uint varcount = reader.ReadUInt32();
                List<uint> varoffsets = new List<uint>();
                for (int v = 0; v < varcount; v++)
                {
                    varoffsets.Add(reader.ReadUInt32());
                }
                for (int v = 0; v < varcount; v++)
                {
                    reader.BaseStream.Seek(varoffsets[v], SeekOrigin.Begin);
                    uint varnameoffset = reader.ReadUInt32();
                    byte[] varhash = reader.ReadBytes(0x4);
                    uint vartypeoffset = reader.ReadUInt32();
                    reader.BaseStream.Seek(varnameoffset, SeekOrigin.Begin);
                    string varname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    reader.BaseStream.Seek(vartypeoffset, SeekOrigin.Begin);
                    string vartype = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    uint typehash = BitConverter.ToUInt32(new ScriptHashCalculator(vartype).Hash, 0);
                    if (!hashes.ContainsKey(typehash))
                    {
                        hashes.Add(typehash, vartype);
                    }
                    hashes.Add(BitConverter.ToUInt32(varhash, 0), $"{name}.{varname}");
                }
                reader.BaseStream.Seek(methodlist, SeekOrigin.Begin);
                uint methodcount = reader.ReadUInt32();
                List<uint> methodoffsets = new List<uint>();
                for (int m = 0; m < methodcount; m++)
                {
                    methodoffsets.Add(reader.ReadUInt32());
                }
                for (int m = 0; m < methodcount; m++)
                {
                    reader.BaseStream.Seek(methodoffsets[m], SeekOrigin.Begin);
                    uint methodnameoffset = reader.ReadUInt32();
                    byte[] methodhash = reader.ReadBytes(0x4);
                    reader.BaseStream.Seek(methodnameoffset, SeekOrigin.Begin);
                    string methodname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    string[] splitname = methodname.Split(' ');
                    for (int c = 0; c < splitname.Length; c++)
                    {
                        if (splitname[c].Contains('('))
                        {
                            List<string> strs = new List<string>();
                            for (int s = c; s < splitname.Length; s++)
                            {
                                strs.Add(splitname[s]);
                            }
                            methodname = string.Join(" ", strs);
                            break;
                        }
                    }
                    if (splitname[0] == "const")
                    {
                        uint typehash = BitConverter.ToUInt32(new ScriptHashCalculator(splitname[1]).Hash, 0);
                        if (!hashes.ContainsKey(typehash))
                        {
                            hashes.Add(typehash, splitname[1]);
                        }
                    }
                    else
                    {
                        uint typehash = BitConverter.ToUInt32(new ScriptHashCalculator(splitname[0]).Hash, 0);
                        if (!hashes.ContainsKey(typehash))
                        {
                            hashes.Add(typehash, splitname[0]);
                        }
                    }
                    hashes.Add(BitConverter.ToUInt32(methodhash, 0), $"{name}.{methodname}");
                }
            }
        }

        public void ReadUnknownHashes(BinaryReader reader, Dictionary<uint, string> hashList)
        {
            reader.BaseStream.Seek(0x18, SeekOrigin.Begin);
            uint xreflist = reader.ReadUInt32();
            reader.BaseStream.Seek(xreflist, SeekOrigin.Begin);
            uint xrefcount = reader.ReadUInt32();
            List<string> xref = new List<string>();
            for (int i = 0; i < xrefcount; i++)
            {
                byte[] hash = reader.ReadBytes(0x4);
                uint xrefHash = uint.Parse(BitConverter.ToUInt32(hash, 0).ToString("X8"), System.Globalization.NumberStyles.HexNumber);
                if (!hashList.ContainsKey(xrefHash))
                {
                    unknownHashes.Add(xrefHash);
                }
            }
        }
    }
}
