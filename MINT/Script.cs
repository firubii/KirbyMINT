using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using MINT;

namespace MINT
{
    public class Script
    {
        private struct Class
        {
            public string Name;
            public uint Hash;
            public uint Flags;
            public List<Variable> Variables;
            public List<Method> Methods;
            public List<Constant> Constants;
        }
        private struct Variable
        {
            public string Type;
            public string Name;
            public uint Hash;
            public uint Flags;
        }
        private struct Method
        {
            public string Name;
            public byte[] Data;
            public uint Hash;
            public uint Flags;
        }
        private struct Constant
        {
            public string Name;
            public uint Value;
        }

        public bool DecompileFailure = false;

        public List<string> DecompiledScript = new List<string>();
        public List<byte> CompiledScript = new List<byte>();
        private Game game;

        public Script(byte[] script, Dictionary<uint, string> hashes, Game g)
        {
            game = g;
            if (g == Game.RDL)
            {
                using (BigEndianBinaryReader reader = new BigEndianBinaryReader(new MemoryStream(script)))
                {
                    ReadRDL(reader);
                }
            }
            else
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(script)))
                {
                    Read(reader, hashes);
                }
            }
        }
        public Script(string[] script, Game g)
        {
            game = g;
            if (g == Game.RDL)
            {
                WriteRDL(script.ToList());
            }
            else
            {
                Write(script.ToList());
            }
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> hashes)
        {
            Opcodes opcodes = new Opcodes();
            Dictionary<byte, string> opcodeNames = new Dictionary<byte, string>();
            Dictionary<byte, Format> opcodeFormats = new Dictionary<byte, Format>();
            switch (game)
            {
                case Game.TDX:
                    {
                        opcodeNames = opcodes.TDX_OpcodeNames;
                        opcodeFormats = opcodes.TDX_OpcodeFormats;
                        break;
                    }
                case Game.KPR:
                    {
                        opcodeNames = opcodes.KPR_OpcodeNames;
                        opcodeFormats = opcodes.KPR_OpcodeFormats;
                        break;
                    }
                case Game.KSA:
                    {
                        opcodeNames = opcodes.KSA_OpcodeNames;
                        opcodeFormats = opcodes.KSA_OpcodeFormats;
                        break;
                    }
            }

            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            uint scriptnameoffset = reader.ReadUInt32();
            uint sdatalist = reader.ReadUInt32();
            uint xreflist = reader.ReadUInt32();
            uint classlist = reader.ReadUInt32();
            reader.BaseStream.Seek(scriptnameoffset, SeekOrigin.Begin);
            string scriptname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
            DecompiledScript.Add("script " + scriptname);
            DecompiledScript.Add("{");
            reader.BaseStream.Seek(sdatalist, SeekOrigin.Begin);
            uint sdatalen = reader.ReadUInt32();
            byte[] sdata = reader.ReadBytes((int)sdatalen);
            /*string sdataText = $"    SDATA: (0x{sdatalen.ToString("X")}) ";
            sdataText += "{";
            for (int i = 0; i < sdatalen; i++)
            {
                sdataText += $" {sdata[i].ToString("X2")}";
            }
            sdataText += " }";
            script.Add(sdataText);*/
            reader.BaseStream.Seek(xreflist, SeekOrigin.Begin);
            uint xrefcount = reader.ReadUInt32();
            List<string> xref = new List<string>();
            for (int i = 0; i < xrefcount; i++)
            {
                byte[] hash = reader.ReadBytes(0x4);
                uint xrefHash = uint.Parse(BitConverter.ToUInt32(hash, 0).ToString("X8"), System.Globalization.NumberStyles.HexNumber);
                if (hashes.ContainsKey(xrefHash))
                {
                    xref.Add(hashes[xrefHash]);
                }
                else
                {
                    xref.Add(xrefHash.ToString("X8"));
                }
            }
            /*string xrefListText = $"    XREF: (0x{xrefcount.ToString("X")}) " + "\n    {";
            for (int i = 0; i < xrefcount; i++)
            {
                xrefListText += $"\n        {xref[i]}";
            }
            xrefListText += "\n    }";
            script.Add(xrefListText);*/
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
                uint constlist = reader.ReadUInt32();
                uint flags = reader.ReadUInt32();
                reader.BaseStream.Seek(nameoffset, SeekOrigin.Begin);
                string name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

                string classflagText = "";

                DecompiledScript.Add($"\n\t[{flags}] {classflagText}class {name}");
                DecompiledScript.Add("\t{");
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
                    uint varflags = reader.ReadUInt32();
                    reader.BaseStream.Seek(varnameoffset, SeekOrigin.Begin);
                    string varname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    reader.BaseStream.Seek(vartypeoffset, SeekOrigin.Begin);
                    string vartype = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

                    string varflagText = "";
                    /*if (varflags != 0)
                    {
                        if ((varflags & (1 << 0)) != 0)
                        {
                            varflagText += "init ";
                        }
                    }*/

                    DecompiledScript.Add($"\t\t[{varflags}] {varflagText}{vartype} {varname}");
                }
                reader.BaseStream.Seek(constlist, SeekOrigin.Begin);
                uint constcount = reader.ReadUInt32();
                List<uint> constoffsets = new List<uint>();
                for (int c = 0; c < constcount; c++)
                {
                    constoffsets.Add(reader.ReadUInt32());
                }
                for (int c = 0; c < constcount; c++)
                {
                    reader.BaseStream.Seek(constoffsets[c], SeekOrigin.Begin);
                    uint constnameoffset = reader.ReadUInt32();
                    uint constval = reader.ReadUInt32();
                    reader.BaseStream.Seek(constnameoffset, SeekOrigin.Begin);
                    string constname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    DecompiledScript.Add($"\t\tconst {constname} = {constval}");
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
                    uint methoddataoffset = reader.ReadUInt32();
                    uint methodflags = reader.ReadUInt32();
                    reader.BaseStream.Seek(methodnameoffset, SeekOrigin.Begin);
                    string methodname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

                    string methodflagText = "";
                    /*if (methodflags != 0)
                    {
                        if ((methodflags & (1 << 2)) != 0)
                        {
                            methodflagText += "loop ";
                        }
                        if ((methodflags & (1 << 0)) != 0)
                        {
                            methodflagText += "init ";
                        }
                    }*/

                    DecompiledScript.Add($"\n\t\t[{methodflags}] {methodflagText}{methodname}");
                    DecompiledScript.Add("\t\t{");
                    reader.BaseStream.Seek(methoddataoffset, SeekOrigin.Begin);
                    for (int b = 0; b < reader.BaseStream.Length; b++)
                    {
                        byte w = reader.ReadByte();
                        byte z = reader.ReadByte();
                        byte x = reader.ReadByte();
                        byte y = reader.ReadByte();
                        ushort v = BitConverter.ToUInt16(new byte[] { x, y }, 0);
                        short sv = BitConverter.ToInt16(new byte[] { x, y }, 0);
                        //if (w == 0x5C)
                        //    Console.WriteLine($"{scriptname}:{methodname} - {w.ToString("X2")} {z.ToString("X2")} {x.ToString("X2")} {y.ToString("X2")}");
                        if (opcodeNames.Keys.Contains(w))
                        {
                            try
                            {
                                string cmd = "\t\t\t" + opcodeNames[w];
                                switch (opcodeFormats[w])
                                {
                                    case Format.None:
                                        {
                                            break;
                                        }
                                    case Format.Z:
                                        {
                                            cmd += $" r{z.ToString("X2")}";
                                            break;
                                        }
                                    case Format.X:
                                        {
                                            cmd += $" r{x.ToString("X2")}";
                                            break;
                                        }
                                    case Format.RetY:
                                    case Format.Y:
                                        {
                                            cmd += $" r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.sV:
                                        {
                                            uint val = BitConverter.ToUInt32(sdata, v);
                                            string vstring = "0x" + val.ToString("X");
                                            if (vstring.Length == 10) vstring = BitConverter.ToSingle(sdata, v).ToString() + "f";
                                            cmd += $" {vstring}";
                                            break;
                                        }
                                    case Format.sZV:
                                        {
                                            uint val = BitConverter.ToUInt32(sdata, v);
                                            string vstring = "0x" + val.ToString("X");
                                            if (vstring.Length == 10) vstring = BitConverter.ToSingle(sdata, v).ToString() + "f";
                                            cmd += $" r{z.ToString("X2")}, {vstring}";
                                            break;
                                        }
                                    case Format.strV:
                                        {
                                            string strV = "";
                                            List<byte> strBytes = new List<byte>();
                                            for (int s = v; s < sdata.Length; s++)
                                            {
                                                if (sdata[s] != 0x00)
                                                {
                                                    strBytes.Add(sdata[s]);
                                                }
                                                else
                                                {
                                                    strV = Encoding.UTF8.GetString(strBytes.ToArray());
                                                    break;
                                                }
                                            }
                                            cmd += $" \"{strV}\"";
                                            break;
                                        }
                                    case Format.strZV:
                                        {
                                            string strV = "";
                                            List<byte> strBytes = new List<byte>();
                                            for (int s = v; s < sdata.Length; s++)
                                            {
                                                if (sdata[s] != 0x00)
                                                {
                                                    strBytes.Add(sdata[s]);
                                                }
                                                else
                                                {
                                                    strV = Encoding.UTF8.GetString(strBytes.ToArray());
                                                    break;
                                                }
                                            }
                                            cmd += $" r{z.ToString("X2")}, \"{strV}\"";
                                            break;
                                        }
                                    case Format.xV:
                                        {
                                            cmd += $" {xref[v]}";
                                            break;
                                        }
                                    case Format.xZV:
                                        {
                                            cmd += $" r{z.ToString("X2")}, {xref[v]}";
                                            break;
                                        }
                                    case Format.xZX:
                                        {
                                            cmd += $" r{z.ToString("X2")}, {xref[x]}";
                                            break;
                                        }
                                    case Format.shV:
                                        {
                                            cmd += $" {sv}";
                                            break;
                                        }
                                    case Format.shZV:
                                        {
                                            cmd += $" r{z.ToString("X2")}, {sv}";
                                            break;
                                        }
                                    case Format.ZX:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{x.ToString("X2")}";
                                            break;
                                        }
                                    case Format.aZX:
                                        {
                                            cmd += $" [{z.ToString("X2")}] r{x.ToString("X2")}";
                                            break;
                                        }
                                    case Format.aaZX:
                                        {
                                            cmd += $" [{z.ToString("X2")} {x.ToString("X2")}]";
                                            break;
                                        }
                                    case Format.ZY:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.XY:
                                        {
                                            cmd += $" r{x.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.ZXY:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{x.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.nZXY:
                                        {
                                            cmd += $" {z.ToString("X2")}, {x.ToString("X2")}, {y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.aZXY:
                                        {
                                            cmd += $" [{z.ToString("X2")}] r{x.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.ZXxY:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{x.ToString("X2")}, {xref[y]}";
                                            break;
                                        }
                                    case Format.XZxY:
                                        {
                                            cmd += $" r{x.ToString("X2")}, r{z.ToString("X2")}, {xref[y]}";
                                            break;
                                        }
                                    case Format.LDP:
                                        {
                                            cmd += $" r{z.ToString("X2")}, ";
                                            if (x >= 0x80)
                                            {
                                                uint val = BitConverter.ToUInt32(sdata, 4 * (x - 128));
                                                string vstring = "0x" + val.ToString("X");
                                                if (vstring.Length == 10) vstring = BitConverter.ToSingle(sdata, 4 * (x - 128)).ToString() + "f";
                                                cmd += $"{vstring}, ";
                                            }
                                            else
                                            {
                                                cmd += $"r{x.ToString("X2")}, ";
                                            }
                                            if (y >= 0x80)
                                            {
                                                uint val = BitConverter.ToUInt32(sdata, 4 * (y - 128));
                                                string vstring = "0x" + val.ToString("X");
                                                if (vstring.Length == 10) vstring = BitConverter.ToSingle(sdata, 4 * (y - 128)).ToString() + "f";
                                                cmd += $"{vstring}";
                                            }
                                            else
                                            {
                                                cmd += $"r{y.ToString("X2")}";
                                            }
                                            break;
                                        }
                                    case Format.LDPstr:
                                        {
                                            cmd += $" r{z.ToString("X2")}, ";
                                            if (x >= 0x80)
                                            {
                                                string strV = "";
                                                List<byte> strBytes = new List<byte>();
                                                for (int s = 4 * (x - 128); s < sdata.Length; s++)
                                                {
                                                    if (sdata[s] != 0x00)
                                                    {
                                                        strBytes.Add(sdata[s]);
                                                    }
                                                    else
                                                    {
                                                        strV = Encoding.UTF8.GetString(strBytes.ToArray());
                                                        break;
                                                    }
                                                }
                                                cmd += $"\"{strV}\", ";
                                            }
                                            else
                                            {
                                                cmd += $"r{x.ToString("X2")}, ";
                                            }
                                            if (y >= 0x80)
                                            {
                                                string strV = "";
                                                List<byte> strBytes = new List<byte>();
                                                for (int s = 4 * (y - 128); s < sdata.Length; s++)
                                                {
                                                    if (sdata[s] != 0x00)
                                                    {
                                                        strBytes.Add(sdata[s]);
                                                    }
                                                    else
                                                    {
                                                        strV = Encoding.UTF8.GetString(strBytes.ToArray());
                                                        break;
                                                    }
                                                }
                                                cmd += $"\"{strV}\"";
                                            }
                                            else
                                            {
                                                cmd += $"r{y.ToString("X2")}";
                                            }
                                            break;
                                        }
                                    case Format.Ret:
                                        {
                                            if (game == Game.KSA)
                                            {
                                                if ((methodflags & (1 << 7)) != 0)
                                                {
                                                    cmd += $" r{y.ToString("X2")}";
                                                }
                                            }
                                            break;
                                        }
                                }
                                DecompiledScript.Add(cmd);
                            }
                            catch
                            {
                                Console.WriteLine($"\n!! ERROR !!\nFailed to analyze command!\nERROR DATA:\n-ORIGIN-\nSCRIPT: {scriptname}\nFUNCTION: {methodname}\nOFFSET: 0x{(reader.BaseStream.Position - 4).ToString("X8")}\nFULL COMMAND: {w.ToString("X2")} {z.ToString("X2")} {x.ToString("X2")} {y.ToString("X2")}\n-SCRIPT DATA-\nSDATA LENGTH: 0x{sdatalen.ToString("X")}\nXREF COUNT: 0x{xrefcount.ToString("X")}");
                                throw new Exception("CommandReadFailure");
                                DecompileFailure = true;
                                return;
                            }
                        }
                        else
                        {
                            DecompiledScript.Add("\t\t\t" + $"{w.ToString("X2")} {z.ToString("X2")} {x.ToString("X2")} {y.ToString("X2")}");
                        }
                        if (opcodeFormats[w] == Format.Ret || opcodeFormats[w] == Format.RetY)
                        {
                            break;
                        }
                    }
                    DecompiledScript.Add("\t\t}");
                }
                DecompiledScript.Add("\t}");
            }
            DecompiledScript.Add("}");
        }

        public void Write(List<string> script)
        {
            if (script[0].StartsWith("script "))
            {
                Opcodes opcodes = new Opcodes();
                Dictionary<byte, string> opcodeNames = new Dictionary<byte, string>();
                Dictionary<byte, Format> opcodeFormats = new Dictionary<byte, Format>();
                if (game == Game.TDX)
                {
                    opcodeNames = opcodes.TDX_OpcodeNames;
                    opcodeFormats = opcodes.TDX_OpcodeFormats;
                }
                else if (game == Game.KPR)
                {
                    opcodeNames = opcodes.KPR_OpcodeNames;
                    opcodeFormats = opcodes.KPR_OpcodeFormats;
                }
                else if (game == Game.KSA)
                {
                    opcodeNames = opcodes.KSA_OpcodeNames;
                    opcodeFormats = opcodes.KSA_OpcodeFormats;
                }

                string scriptname = script[0].Remove(0, 7);

                //Script prep
                for (int i = 0; i < script.Count; i++)
                {
                    script[i] = script[i].TrimStart(new char[] { '\t', ' ' });
                    script[i] = script[i].TrimEnd(new char[] { '\t', ' ' });
                }
                for (int i = 0; i < script.Count; i++)
                {
                    if (script[i].StartsWith("#") || script[i] == "")
                    {
                        script.RemoveAt(i);
                        i--;
                    }
                    else if (script[i].Contains("#"))
                    {
                        script[i] = script[i].Remove(script[i].IndexOf('#'), script[i].Length - (script[i].IndexOf('#'))).TrimEnd(new char[] { ' ' });
                    }
                }

                //SDATA
                List<byte[]> sdata = new List<byte[]>();
                uint sdataLen = 0;
                //Ints & Floats (LDP)
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.LDP)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    byte[] b = { };
                                    uint o = 0;
                                    if (f == Format.LDP)
                                    {
                                        if (line[a].StartsWith("0x"))
                                        {
                                            b = BitConverter.GetBytes(uint.Parse(line[a].Remove(0, 2).TrimEnd(new char[] { ',' }), System.Globalization.NumberStyles.HexNumber));
                                        }
                                        else if (line[a].Contains(".") || line[a].Contains("f"))
                                        {
                                            b = BitConverter.GetBytes(float.Parse(line[a].Replace("f", "").TrimEnd(new char[] { ',' })));
                                        }
                                        else
                                        {
                                            b = BitConverter.GetBytes(uint.Parse(line[a].TrimEnd(new char[] { ',' })));
                                        }
                                        for (int c = 0; c < sdata.Count; c++)
                                        {
                                            if (BitConverter.ToUInt32(b, 0) == BitConverter.ToUInt32(sdata[c], 0))
                                            {
                                                break;
                                            }
                                            o += (uint)sdata[c].Length;
                                        }
                                        if (o == sdataLen)
                                        {
                                            sdata.Add(b);
                                            sdataLen += 4;
                                        }
                                        line[a] = (0x80 + (o / 4)).ToString();
                                    }
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }
                //Strings (LDP)
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.LDPstr)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    string baseStr = "";
                                    int l = a;
                                    for (int c = a; c < line.Length; c++)
                                    {
                                        baseStr += line[c];
                                        if (line[c].EndsWith("\""))
                                        {
                                            a = c;
                                            break;
                                        }
                                        else
                                        {
                                            baseStr += " ";
                                        }
                                    }
                                    byte[] b = { };
                                    uint o = 0;
                                    if (f == Format.LDPstr)
                                    {
                                        List<byte> str = Encoding.UTF8.GetBytes(baseStr.TrimStart(new char[] { '\"' }).TrimEnd(new char[] { '\"', ',' })).ToList();
                                        str.AddRange(new byte[] { 0x00 });
                                        while (!str.Count.ToString("X").EndsWith("0") && !str.Count.ToString("X").EndsWith("4") && !str.Count.ToString("X").EndsWith("8") && !str.Count.ToString("X").EndsWith("C"))
                                        {
                                            str.Add(0xFF);
                                        }
                                        b = str.ToArray();
                                        for (int c = 0; c < sdata.Count; c++)
                                        {
                                            if (Encoding.UTF8.GetString(str.ToArray()) == Encoding.UTF8.GetString(sdata[c]))
                                            {
                                                break;
                                            }
                                            o += (uint)sdata[c].Length;
                                        }
                                        if (o == sdataLen)
                                        {
                                            sdata.Add(b);
                                            sdataLen += (uint)str.Count;
                                        }
                                        line[l] = (0x80 + (o / 4)).ToString();
                                    }
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }
                //Ints & Floats
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.sV || f == Format.sZV)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    byte[] b = { };
                                    uint o = 0;
                                    if (f == Format.sV || f == Format.sZV)
                                    {
                                        if (line[a].StartsWith("0x"))
                                        {
                                            b = BitConverter.GetBytes(uint.Parse(line[a].Remove(0, 2).TrimEnd(new char[] { ',' }), System.Globalization.NumberStyles.HexNumber));
                                        }
                                        else if (line[a].Contains(".") || line[a].Contains("f"))
                                        {
                                            b = BitConverter.GetBytes(float.Parse(line[a].Replace("f", "").TrimEnd(new char[] { ',' })));
                                        }
                                        else
                                        {
                                            b = BitConverter.GetBytes(uint.Parse(line[a].TrimEnd(new char[] { ',' })));
                                        }
                                        for (int c = 0; c < sdata.Count; c++)
                                        {
                                            if (BitConverter.ToUInt32(b, 0) == BitConverter.ToUInt32(sdata[c], 0))
                                            {
                                                break;
                                            }
                                            o += (uint)sdata[c].Length;
                                        }
                                        if (o == sdataLen)
                                        {
                                            sdata.Add(b);
                                            sdataLen += 4;
                                        }
                                        line[a] = o.ToString();
                                    }
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }
                //Strings
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.strV || f == Format.strZV)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    string baseStr = "";
                                    int l = a;
                                    for (int c = a; c < line.Length; c++)
                                    {
                                        baseStr += line[c];
                                        if (line[c].EndsWith("\""))
                                        {
                                            a = c;
                                            break;
                                        }
                                        else
                                        {
                                            baseStr += " ";
                                        }
                                    }
                                    byte[] b = { };
                                    uint o = 0;
                                    if (f == Format.strV || f == Format.strZV)
                                    {
                                        List<byte> str = Encoding.UTF8.GetBytes(baseStr.TrimStart(new char[] { '\"' }).TrimEnd(new char[] { '\"', ',' })).ToList();
                                        str.AddRange(new byte[] { 0x00 });
                                        while (!str.Count.ToString("X").EndsWith("0") && !str.Count.ToString("X").EndsWith("4") && !str.Count.ToString("X").EndsWith("8") && !str.Count.ToString("X").EndsWith("C"))
                                        {
                                            str.Add(0xFF);
                                        }
                                        b = str.ToArray();
                                        for (int c = 0; c < sdata.Count; c++)
                                        {
                                            if (Encoding.UTF8.GetString(str.ToArray()) == Encoding.UTF8.GetString(sdata[c]))
                                            {
                                                break;
                                            }
                                            o += (uint)sdata[c].Length;
                                        }
                                        if (o == sdataLen)
                                        {
                                            sdata.Add(b);
                                            sdataLen += (uint)str.Count;
                                        }
                                        line[l] = o.ToString();
                                    }
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }

                //XREF
                List<uint> xref = new List<uint>();
                //For byte indexes
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.xZX || f == Format.ZXxY || f == Format.XZxY)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    uint x;
                                    if (line[a].Length == 8 && !line[a].Contains("."))
                                    {
                                        x = uint.Parse(line[a].TrimEnd(new char[] { ',' }), System.Globalization.NumberStyles.HexNumber);
                                    }
                                    else
                                    {
                                        List<string> str = new List<string>();
                                        for (int c = a; c < line.Length; c++)
                                        {
                                            str.Add(line[c]);
                                        }
                                        ScriptHashCalculator scriptHash = new ScriptHashCalculator(string.Join(" ", str).TrimEnd(new char[] { ',' }));
                                        x = BitConverter.ToUInt32(scriptHash.Hash, 0);
                                    }
                                    if (!xref.Contains(x))
                                    {
                                        xref.Add(x);
                                    }
                                    line[a] = xref.IndexOf(x).ToString();
                                    break;
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }
                //For short indexes
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.xV || f == Format.xZV)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    uint x;
                                    if (line[a].Length == 8 && !line[a].Contains("."))
                                    {
                                        x = uint.Parse(line[a].TrimEnd(new char[] { ',' }), System.Globalization.NumberStyles.HexNumber);
                                    }
                                    else
                                    {
                                        List<string> str = new List<string>();
                                        for (int c = a; c < line.Length; c++)
                                        {
                                            str.Add(line[c]);
                                        }
                                        ScriptHashCalculator scriptHash = new ScriptHashCalculator(string.Join(" ", str).TrimEnd(new char[] { ',' }));
                                        x = BitConverter.ToUInt32(scriptHash.Hash, 0);
                                    }
                                    if (!xref.Contains(x))
                                    {
                                        xref.Add(x);
                                    }
                                    line[a] = xref.IndexOf(x).ToString();
                                    break;
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }

                //Classes
                List<Class> classes = new List<Class>();
                for (int i = 0; i < script.Count; i++)
                {
                    if (script[i].Contains("class "))
                    {
                        Class cl = new Class();
                        Regex regex = new Regex(@"\[\d+\]");
                        MatchCollection matches = regex.Matches(script[i]);
                        cl.Flags = uint.Parse(matches[0].ToString().Remove(0, 1).Remove(matches[0].Length - 2, 1));
                        cl.Name = script[i].Remove(0, matches[0].Length + 7);
                        ScriptHashCalculator scriptHash = new ScriptHashCalculator(cl.Name);
                        cl.Hash = BitConverter.ToUInt32(scriptHash.Hash, 0);
                        uint bracket = 0;

                        cl.Variables = new List<Variable>();
                        cl.Methods = new List<Method>();
                        cl.Constants = new List<Constant>();

                        //Variables
                        for (int c = i; c < script.Count; c++)
                        {
                            if (c + 1 < script.Count)
                            {
                                if (script[c + 1].Contains("{"))
                                {
                                    bracket++;
                                    c++;
                                }
                            }
                            if (script[c] == "}")
                            {
                                bracket--;
                                if (bracket == 0)
                                {
                                    break;
                                }
                            }
                            if (bracket == 1)
                            {
                                matches = regex.Matches(script[c]);
                                if (matches.Count > 0)
                                {
                                    Variable var = new Variable();
                                    var.Flags = uint.Parse(matches[0].ToString().Remove(0, 1).Remove(matches[0].Length - 2, 1));
                                    string[] v = script[c].Remove(0, matches[0].Length + 1).Split(' ');
                                    var.Type = v[0];
                                    var.Name = v[1];
                                    scriptHash = new ScriptHashCalculator($"{cl.Name}.{v[1]}");
                                    var.Hash = BitConverter.ToUInt32(scriptHash.Hash, 0);
                                    cl.Variables.Add(var);
                                }
                            }
                            matches = regex.Matches("");
                        }

                        //Methods
                        for (int c = i; c < script.Count; c++)
                        {
                            if (c + 1 < script.Count)
                            {
                                if (script[c + 1].Contains("{"))
                                {
                                    bracket++;
                                }
                            }
                            if (script[c] == "}")
                            {
                                bracket--;
                                if (bracket == 0)
                                {
                                    break;
                                }
                            }
                            if (bracket == 2)
                            {
                                if (script[c].Contains("{"))
                                {
                                    matches = regex.Matches(script[c]);
                                }
                                else if (c + 1 < script.Count)
                                {
                                    if (script[c + 1].Contains("{"))
                                    {
                                        matches = regex.Matches(script[c]);
                                    }
                                }
                                if (matches.Count > 0)
                                {
                                    Method method = new Method();
                                    method.Flags = uint.Parse(matches[0].ToString().Remove(0, 1).Remove(matches[0].Length - 2, 1));
                                    string m = script[c].Remove(0, matches[0].Length + 1);
                                    method.Name = m;
                                    scriptHash = new ScriptHashCalculator($"{cl.Name}.{m.Remove(0, m.Split(' ')[0].Length + 1)}");
                                    method.Hash = BitConverter.ToUInt32(scriptHash.Hash, 0);
                                    c += 2;
                                    List<byte> data = new List<byte>();
                                    for (int d = c; d < script.Count; d++)
                                    {
                                        string[] line = script[d].Replace(",", "").Split(' ');
                                        if (opcodeNames.ContainsValue(line[0]))
                                        {
                                            byte w = opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key;
                                            Format f = opcodeFormats[w];
                                            switch (f)
                                            {
                                                case Format.Z:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(0xFF);
                                                        data.Add(0xFF);
                                                        break;
                                                    }
                                                case Format.X:
                                                    {
                                                        data.Add(w);
                                                        data.Add(0xFF);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(0xFF);
                                                        break;
                                                    }
                                                case Format.Y:
                                                    {
                                                        data.Add(w);
                                                        data.Add(0xFF);
                                                        data.Add(0xFF);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        break;
                                                    }
                                                case Format.sV:
                                                case Format.strV:
                                                case Format.xV:
                                                    {
                                                        byte[] v = BitConverter.GetBytes(ushort.Parse(line[1]));
                                                        data.Add(w);
                                                        data.Add(0xFF);
                                                        data.AddRange(v);
                                                        break;
                                                    }
                                                case Format.sZV:
                                                case Format.strZV:
                                                case Format.xZV:
                                                    {
                                                        byte[] v = BitConverter.GetBytes(ushort.Parse(line[2]));
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.AddRange(v);
                                                        break;
                                                    }
                                                case Format.shV:
                                                    {
                                                        byte[] v = BitConverter.GetBytes(short.Parse(line[1]));
                                                        data.Add(w);
                                                        data.Add(0xFF);
                                                        data.AddRange(v);
                                                        break;
                                                    }
                                                case Format.shZV:
                                                    {
                                                        byte[] v = BitConverter.GetBytes(short.Parse(line[2]));
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.AddRange(v);
                                                        break;
                                                    }
                                                case Format.ZX:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(0xFF);
                                                        break;
                                                    }
                                                case Format.xZX:
                                                    {
                                                        byte[] v = BitConverter.GetBytes(ushort.Parse(line[2]));
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[2]));
                                                        data.Add(0xFF);
                                                        break;
                                                    }
                                                case Format.ZXxY:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[3]));
                                                        break;
                                                    }
                                                case Format.XZxY:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[3]));
                                                        break;
                                                    }
                                                case Format.nZXY:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1], System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[2], System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[3], System.Globalization.NumberStyles.HexNumber));
                                                        break;
                                                    }
                                                case Format.ZXY:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        data.Add(byte.Parse(line[3].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        break;
                                                    }
                                                case Format.LDP:
                                                case Format.LDPstr:
                                                    {
                                                        data.Add(w);
                                                        data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        if (line[2].StartsWith("r"))
                                                        {
                                                            data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        }
                                                        else
                                                        {
                                                            data.Add(byte.Parse(line[2]));
                                                        }
                                                        if (line[3].StartsWith("r"))
                                                        {
                                                            data.Add(byte.Parse(line[3].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        }
                                                        else
                                                        {
                                                            data.Add(byte.Parse(line[3]));
                                                        }
                                                        break;
                                                    }
                                                case Format.Ret:
                                                    {
                                                        data.Add(w);
                                                        data.Add(0xFF);
                                                        data.Add(0xFF);
                                                        if (line.Length > 1)
                                                        {
                                                            data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                        }
                                                        else
                                                        {
                                                            data.Add(0x00);
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                        else if (script[d] == "}")
                                        {
                                            method.Data = data.ToArray();
                                            cl.Methods.Add(method);
                                            break;
                                        }
                                    }
                                }
                            }
                            matches = regex.Matches("");
                        }

                        //Constants
                        for (int c = i; c < script.Count; c++)
                        {
                            if (c + 1 < script.Count)
                            {
                                if (script[c + 1].Contains("{"))
                                {
                                    bracket++;
                                    c++;
                                }
                            }
                            if (script[c] == "}")
                            {
                                bracket--;
                                if (bracket == 0)
                                {
                                    break;
                                }
                            }
                            if (bracket == 1)
                            {
                                if (script[c].StartsWith("const "))
                                {
                                    Constant constant = new Constant();
                                    string[] d = script[c].Remove(0, 6).Split(' ');
                                    constant.Name = d[0];
                                    constant.Value = uint.Parse(d[2]);
                                    cl.Constants.Add(constant);
                                }
                            }
                        }

                        classes.Add(cl);
                    }
                }

                //File Building
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(new byte[] {
                    0x58, 0x42, 0x49, 0x4E, 0x34, 0x12, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE9, 0xFD, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                List<byte> sdataraw = new List<byte>();
                for (int i = 0; i < sdata.Count; i++)
                {
                    sdataraw.AddRange(sdata[i]);
                }

                writer.Write(sdataraw.Count);
                writer.Write(sdataraw.ToArray());
                writer.Write(0);

                uint pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x18, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                writer.Write(xref.Count);
                for (int i = 0; i < xref.Count; i++)
                {
                    writer.Write(xref[i]);
                }

                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x1C, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                List<uint> clOffsets = new List<uint>();
                List<uint> clNameOffsets = new List<uint>();
                List<uint[]> vTypeOffsets = new List<uint[]>();
                List<uint[]> vNameOffsets = new List<uint[]>();
                List<uint[]> mNameOffsets = new List<uint[]>();
                List<uint[]> cNameOffsets = new List<uint[]>();
                writer.Write(classes.Count);
                for (int i = 0; i < classes.Count; i++)
                {
                    clOffsets.Add((uint)writer.BaseStream.Position);
                    writer.Write(0);
                }
                for (int i = 0; i < classes.Count; i++)
                {
                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(clOffsets[i], SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);

                    clNameOffsets.Add((uint)writer.BaseStream.Position);
                    writer.Write(0);
                    writer.Write(classes[i].Hash);
                    writer.Write((uint)writer.BaseStream.Position + 0x10);
                    uint mListOffset = (uint)writer.BaseStream.Position;
                    writer.Write(0);
                    uint cListOffset = (uint)writer.BaseStream.Position;
                    writer.Write(0);
                    writer.Write(classes[i].Flags);

                    List<uint> vOffsets = new List<uint>();
                    List<uint> vt = new List<uint>();
                    List<uint> vn = new List<uint>();
                    writer.Write(classes[i].Variables.Count);
                    for (int c = 0; c < classes[i].Variables.Count; c++)
                    {
                        vOffsets.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                    }
                    for (int c = 0; c < classes[i].Variables.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(vOffsets[c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        vn.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                        writer.Write(classes[i].Variables[c].Hash);
                        vt.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                        writer.Write(classes[i].Variables[c].Flags);
                    }

                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(mListOffset, SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    List<uint> mOffsets = new List<uint>();
                    List<uint> mn = new List<uint>();
                    writer.Write(classes[i].Methods.Count);
                    for (int c = 0; c < classes[i].Methods.Count; c++)
                    {
                        mOffsets.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                    }
                    for (int c = 0; c < classes[i].Methods.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(mOffsets[c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        mn.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                        writer.Write(classes[i].Methods[c].Hash);
                        writer.Write((uint)writer.BaseStream.Position + 0x8);
                        writer.Write(classes[i].Methods[c].Flags);
                        writer.Write(classes[i].Methods[c].Data);
                    }

                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(cListOffset, SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    List<uint> cOffsets = new List<uint>();
                    List<uint> cn = new List<uint>();
                    writer.Write(classes[i].Constants.Count);
                    for (int c = 0; c < classes[i].Constants.Count; c++)
                    {
                        cOffsets.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                    }
                    for (int c = 0; c < classes[i].Constants.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(cOffsets[c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        cn.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                        writer.Write(classes[i].Constants[c].Value);
                    }
                    vTypeOffsets.Add(vt.ToArray());
                    vNameOffsets.Add(vn.ToArray());
                    mNameOffsets.Add(mn.ToArray());
                    cNameOffsets.Add(cn.ToArray());
                }

                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x10, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                writer.Write(scriptname.Length);
                writer.Write(Encoding.UTF8.GetBytes(scriptname));
                while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                {
                    writer.Write((byte)0);
                }
                writer.Write(0);

                for (int i = 0; i < classes.Count; i++)
                {
                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(clNameOffsets[i], SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);

                    writer.Write(classes[i].Name.Length);
                    writer.Write(Encoding.UTF8.GetBytes(classes[i].Name));
                    while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                    {
                        writer.Write((byte)0);
                    }
                    writer.Write(0);

                    for (int c = 0; c < classes[i].Variables.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(vTypeOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Variables[c].Type.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Variables[c].Type));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);

                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(vNameOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Variables[c].Name.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Variables[c].Name));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);
                    }

                    for (int c = 0; c < classes[i].Methods.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(mNameOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Methods[c].Name.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Methods[c].Name));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);
                    }

                    for (int c = 0; c < classes[i].Constants.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(cNameOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Constants[c].Name.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Constants[c].Name));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);
                    }
                }

                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x8, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.Begin);

                CompiledScript = stream.GetBuffer().Take((int)pos).ToList();

            }
        }

        public void ReadRDL(BigEndianBinaryReader reader)
        {
            reader.BaseStream.Seek(0x14, SeekOrigin.Begin);
            uint scriptnameoffset = reader.ReadUInt32();
            uint sdatalist = reader.ReadUInt32();
            uint xreflist = reader.ReadUInt32();
            uint classlist = reader.ReadUInt32();
            reader.BaseStream.Seek(scriptnameoffset, SeekOrigin.Begin);
            string scriptname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
            DecompiledScript.Add("script " + scriptname);
            DecompiledScript.Add("{");
            reader.BaseStream.Seek(sdatalist, SeekOrigin.Begin);
            uint sdatalen = reader.ReadUInt32();
            byte[] sdata = reader.ReadBytes((int)sdatalen);
            /*string sdataText = $"    SDATA: (0x{sdatalen.ToString("X")}) ";
            sdataText += "{";
            for (int i = 0; i < sdatalen; i++)
            {
                sdataText += $" {sdata[i].ToString("X2")}";
            }
            sdataText += " }";
            script.Add(sdataText);*/
            reader.BaseStream.Seek(xreflist, SeekOrigin.Begin);
            uint xrefcount = reader.ReadUInt32();
            List<string> xref = new List<string>();
            for (int i = 0; i < xrefcount; i++)
            {
                uint pos = (uint)reader.BaseStream.Position;
                reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);
                xref.Add(Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32())));
                reader.BaseStream.Seek(pos + 4, SeekOrigin.Begin);
            }
            /*string xrefListText = $"    XREF: (0x{xrefcount.ToString("X")}) " + "\n    {";
            for (int i = 0; i < xrefcount; i++)
            {
                xrefListText += $"\n        {xref[i]}";
            }
            xrefListText += "\n    }";
            script.Add(xrefListText);*/
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
                uint varlist = reader.ReadUInt32();
                uint methodlist = reader.ReadUInt32();
                reader.BaseStream.Seek(nameoffset, SeekOrigin.Begin);
                string name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

                DecompiledScript.Add($"\n\tclass {name}");
                DecompiledScript.Add("\t{");
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
                    uint vartypeoffset = reader.ReadUInt32();
                    reader.BaseStream.Seek(varnameoffset, SeekOrigin.Begin);
                    string varname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    reader.BaseStream.Seek(vartypeoffset, SeekOrigin.Begin);
                    string vartype = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

                    DecompiledScript.Add($"\t\t{vartype} {varname}");
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
                    uint methoddataoffset = reader.ReadUInt32();
                    reader.BaseStream.Seek(methodnameoffset, SeekOrigin.Begin);
                    string methodname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

                    DecompiledScript.Add($"\n\t\t{methodname}");
                    DecompiledScript.Add("\t\t{");
                    reader.BaseStream.Seek(methoddataoffset, SeekOrigin.Begin);
                    Opcodes opcodes = new Opcodes();
                    Dictionary<byte, string> opcodeNames = opcodes.RDL_OpcodeNames;
                    Dictionary<byte, Format> opcodeFormats = opcodes.RDL_OpcodeFormats;
                    for (int b = 0; b < reader.BaseStream.Length; b++)
                    {
                        byte w = reader.ReadByte();
                        byte z = reader.ReadByte();
                        byte x = reader.ReadByte();
                        byte y = reader.ReadByte();
                        ushort v = InvertEndianness(BitConverter.ToUInt16(new byte[] { x, y }, 0));
                        short sv = InvertEndianness(BitConverter.ToInt16(new byte[] { x, y }, 0));
                        //if (w == 0x65)
                        //    Console.WriteLine($"{scriptname}:{script.Count + 1} - {w.ToString("X2")} {z.ToString("X2")} {x.ToString("X2")} {y.ToString("X2")}");
                        if (opcodeNames.Keys.Contains(w))
                        {
                            try
                            {
                                string cmd = "\t\t\t" + opcodeNames[w];
                                switch (opcodeFormats[w])
                                {
                                    case Format.Ret:
                                    case Format.None:
                                        {
                                            break;
                                        }
                                    case Format.Z:
                                        {
                                            cmd += $" r{z.ToString("X2")}";
                                            break;
                                        }
                                    case Format.X:
                                    case Format.RetX:
                                        {
                                            cmd += $" r{x.ToString("X2")}";
                                            break;
                                        }
                                    case Format.Y:
                                        {
                                            cmd += $" r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.sV:
                                        {
                                            uint val = InvertEndianness(BitConverter.ToUInt32(sdata, v));
                                            string vstring = "0x" + val.ToString("X");
                                            if (vstring.Length == 10) vstring = InvertEndianness(BitConverter.ToSingle(sdata, v)).ToString() + "f";
                                            cmd += $" {vstring}";
                                            break;
                                        }
                                    case Format.sZV:
                                        {
                                            uint val = InvertEndianness(BitConverter.ToUInt32(sdata, v));
                                            string vstring = "0x" + val.ToString("X");
                                            if (vstring.Length == 10) vstring = InvertEndianness(BitConverter.ToSingle(sdata, v)).ToString() + "f";
                                            cmd += $" r{z.ToString("X2")}, {vstring}";
                                            break;
                                        }
                                    case Format.strV:
                                        {
                                            string strV = "";
                                            List<byte> strBytes = new List<byte>();
                                            for (int s = v; s < sdata.Length; s++)
                                            {
                                                if (sdata[s] != 0x00)
                                                {
                                                    strBytes.Add(sdata[s]);
                                                }
                                                else
                                                {
                                                    strV = Encoding.UTF8.GetString(strBytes.ToArray());
                                                    break;
                                                }
                                            }
                                            cmd += $" \"{strV}\"";
                                            break;
                                        }
                                    case Format.strZV:
                                        {
                                            string strV = "";
                                            List<byte> strBytes = new List<byte>();
                                            for (int s = v; s < sdata.Length; s++)
                                            {
                                                if (sdata[s] != 0x00)
                                                {
                                                    strBytes.Add(sdata[s]);
                                                }
                                                else
                                                {
                                                    strV = Encoding.UTF8.GetString(strBytes.ToArray());
                                                    break;
                                                }
                                            }
                                            cmd += $" r{z.ToString("X2")}, \"{strV}\"";
                                            break;
                                        }
                                    case Format.xV:
                                        {
                                            cmd += $" {xref[v]}";
                                            break;
                                        }
                                    case Format.xZV:
                                        {
                                            cmd += $" r{z.ToString("X2")}, {xref[v]}";
                                            break;
                                        }
                                    case Format.xZX:
                                        {
                                            cmd += $" r{z.ToString("X2")}, {xref[x]}";
                                            break;
                                        }
                                    case Format.shV:
                                        {
                                            cmd += $" {sv}";
                                            break;
                                        }
                                    case Format.shZV:
                                        {
                                            cmd += $" r{z.ToString("X2")}, {sv}";
                                            break;
                                        }
                                    case Format.ZX:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{x.ToString("X2")}";
                                            break;
                                        }
                                    case Format.aZX:
                                        {
                                            cmd += $" [{z.ToString("X2")}] r{x.ToString("X2")}";
                                            break;
                                        }
                                    case Format.ZY:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.XY:
                                        {
                                            cmd += $" r{x.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.ZXY:
                                        {
                                            cmd += $" r{z.ToString("X2")}, r{x.ToString("X2")}, r{y.ToString("X2")}";
                                            break;
                                        }
                                    case Format.nZXY:
                                        {
                                            cmd += $" {z.ToString("X2")}, {x.ToString("X2")}";
                                            break;
                                        }
                                }
                                DecompiledScript.Add(cmd);
                            }
                            catch
                            {
                                Console.WriteLine($"\n!! ERROR !!\nFailed to analyze command!\nERROR DATA:\n-ORIGIN-\nSCRIPT: {scriptname}\nFUNCTION: {methodname}\nOFFSET: 0x{(reader.BaseStream.Position - 4).ToString("X8")}\nFULL COMMAND: {w.ToString("X2")} {z.ToString("X2")} {x.ToString("X2")} {y.ToString("X2")}\n-SCRIPT DATA-\nSDATA LENGTH: 0x{sdatalen.ToString("X")}\nXREF COUNT: 0x{xrefcount.ToString("X")}");
                                throw new Exception("CommandReadFailure");
                                DecompileFailure = true;
                                return;
                            }
                        }
                        else
                        {
                            DecompiledScript.Add("\t\t\t" + $"{w.ToString("X2")} {z.ToString("X2")} {x.ToString("X2")} {y.ToString("X2")}");
                        }
                        if (opcodeFormats[w] == Format.Ret || opcodeFormats[w] == Format.RetX)
                        {
                            break;
                        }
                    }
                    DecompiledScript.Add("\t\t}");
                }
                DecompiledScript.Add("\t}");
            }
            DecompiledScript.Add("}");
        }

        public void WriteRDL(List<string> script)
        {
            if (script[0].StartsWith("script "))
            {
                Opcodes opcodes = new Opcodes();
                Dictionary<byte, string> opcodeNames = opcodes.RDL_OpcodeNames;
                Dictionary<byte, Format> opcodeFormats = opcodes.RDL_OpcodeFormats;

                string scriptname = script[0].Remove(0, 7);

                //Script prep
                for (int i = 0; i < script.Count; i++)
                {
                    script[i] = script[i].TrimStart(new char[] { '\t', ' ' });
                    script[i] = script[i].TrimEnd(new char[] { '\t', ' ' });
                }
                for (int i = 0; i < script.Count; i++)
                {
                    if (script[i].StartsWith("#") || script[i] == "")
                    {
                        script.RemoveAt(i);
                        i--;
                    }
                    else if (script[i].Contains("#"))
                    {
                        script[i] = script[i].Remove(script[i].IndexOf('#'), script[i].Length - (script[i].IndexOf('#'))).TrimEnd(new char[] { ' ' });
                    }
                }

                //SDATA
                List<byte[]> sdata = new List<byte[]>();
                uint sdataLen = 0;
                //Ints & Floats
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.sV || f == Format.sZV)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    byte[] b = { };
                                    uint o = 0;
                                    if (f == Format.sV || f == Format.sZV)
                                    {
                                        if (line[a].StartsWith("0x"))
                                        {
                                            b = BitConverter.GetBytes(InvertEndianness(uint.Parse(line[a].Remove(0, 2).TrimEnd(new char[] { ',' }), System.Globalization.NumberStyles.HexNumber)));
                                        }
                                        else if (line[a].Contains(".") || line[a].Contains("f"))
                                        {
                                            b = BitConverter.GetBytes(InvertEndianness(float.Parse(line[a].Replace("f", "").TrimEnd(new char[] { ',' }))));
                                        }
                                        else
                                        {
                                            b = BitConverter.GetBytes(InvertEndianness(uint.Parse(line[a].TrimEnd(new char[] { ',' }))));
                                        }
                                        for (int c = 0; c < sdata.Count; c++)
                                        {
                                            if (InvertEndianness(BitConverter.ToUInt32(b, 0)) == InvertEndianness(BitConverter.ToUInt32(sdata[c], 0)))
                                            {
                                                break;
                                            }
                                            o += (uint)sdata[c].Length;
                                        }
                                        if (o == sdataLen)
                                        {
                                            sdata.Add(b);
                                            sdataLen += 4;
                                        }
                                        line[a] = o.ToString();
                                    }
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }
                //Strings
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.strV || f == Format.strZV)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    string baseStr = "";
                                    int l = a;
                                    for (int c = a; c < line.Length; c++)
                                    {
                                        baseStr += line[c];
                                        if (line[c].EndsWith("\""))
                                        {
                                            a = c;
                                            break;
                                        }
                                        else
                                        {
                                            baseStr += " ";
                                        }
                                    }
                                    byte[] b = { };
                                    uint o = 0;
                                    if (f == Format.strV || f == Format.strZV)
                                    {
                                        List<byte> str = Encoding.UTF8.GetBytes(baseStr.TrimStart(new char[] { '\"' }).TrimEnd(new char[] { '\"', ',' })).ToList();
                                        str.AddRange(new byte[] { 0x00 });
                                        while (!str.Count.ToString("X").EndsWith("0") && !str.Count.ToString("X").EndsWith("4") && !str.Count.ToString("X").EndsWith("8") && !str.Count.ToString("X").EndsWith("C"))
                                        {
                                            str.Add(0xFF);
                                        }
                                        b = str.ToArray();
                                        for (int c = 0; c < sdata.Count; c++)
                                        {
                                            if (Encoding.UTF8.GetString(str.ToArray()) == Encoding.UTF8.GetString(sdata[c]))
                                            {
                                                break;
                                            }
                                            o += (uint)sdata[c].Length;
                                        }
                                        if (o == sdataLen)
                                        {
                                            sdata.Add(b);
                                            sdataLen += (uint)str.Count;
                                        }
                                        line[l] = o.ToString();
                                    }
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }

                //XREF
                List<string> xref = new List<string>();
                //For short indexes
                for (int i = 0; i < script.Count; i++)
                {
                    string[] line = script[i].Split(' ');
                    if (opcodeNames.ContainsValue(line[0]))
                    {
                        Format f = opcodeFormats[opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key];
                        if (f == Format.xV || f == Format.xZV)
                        {
                            for (int a = 1; a < line.Length; a++)
                            {
                                if (!line[a].StartsWith("r"))
                                {
                                    string x;
                                    List<string> str = new List<string>();
                                    for (int c = a; c < line.Length; c++)
                                    {
                                        str.Add(line[c]);
                                    }
                                    x = string.Join(" ", str);
                                    if (!xref.Contains(x))
                                    {
                                        xref.Add(x);
                                    }
                                    line[a] = xref.IndexOf(x).ToString();
                                    break;
                                }
                            }
                            script[i] = string.Join(" ", line);
                        }
                    }
                }

                //Classes
                List<Class> classes = new List<Class>();
                for (int i = 0; i < script.Count; i++)
                {
                    if (script[i].Contains("class "))
                    {
                        Class cl = new Class();
                        cl.Name = script[i].Remove(0, 6);
                        uint bracket = 0;

                        cl.Variables = new List<Variable>();
                        cl.Methods = new List<Method>();

                        //Variables
                        for (int c = i; c < script.Count; c++)
                        {
                            if (c + 1 < script.Count)
                            {
                                if (script[c + 1].Contains("{"))
                                {
                                    bracket++;
                                    c++;
                                }
                            }
                            if (script[c] == "}")
                            {
                                bracket--;
                                if (bracket == 0)
                                {
                                    break;
                                }
                            }
                            if (bracket == 1)
                            {
                                if (!script[c].Contains("{") && !script[c].Contains("}") && !script[c].Contains("(") && !script[c].Contains(")"))
                                {
                                    Variable var = new Variable();
                                    string[] v = script[c].Split(' ');
                                    var.Type = v[0];
                                    var.Name = v[1];
                                    cl.Variables.Add(var);
                                }
                            }
                        }

                        //Methods
                        for (int c = i; c < script.Count; c++)
                        {
                            if (c + 1 < script.Count)
                            {
                                if (script[c + 1].Contains("{"))
                                {
                                    bracket++;
                                }
                            }
                            if (script[c] == "}")
                            {
                                bracket--;
                                if (bracket == 0)
                                {
                                    break;
                                }
                            }
                            if (bracket == 2)
                            {
                                Method method = new Method();
                                string m = script[c];
                                method.Name = m;
                                c += 2;
                                List<byte> data = new List<byte>();
                                for (int d = c; d < script.Count; d++)
                                {
                                    string[] line = script[d].Replace(",", "").Split(' ');
                                    if (opcodeNames.ContainsValue(line[0]))
                                    {
                                        byte w = opcodeNames.FirstOrDefault(x => x.Value == line[0]).Key;
                                        Format f = opcodeFormats[w];
                                        switch (f)
                                        {
                                            case Format.Ret:
                                            case Format.None:
                                                {
                                                    data.Add(w);
                                                    data.Add(0xFF);
                                                    data.Add(0xFF);
                                                    data.Add(0xFF);
                                                    break;
                                                }
                                            case Format.Z:
                                                {
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(0xFF);
                                                    data.Add(0xFF);
                                                    break;
                                                }
                                            case Format.RetX:
                                            case Format.X:
                                                {
                                                    data.Add(w);
                                                    data.Add(0xFF);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(0xFF);
                                                    break;
                                                }
                                            case Format.Y:
                                                {
                                                    data.Add(w);
                                                    data.Add(0xFF);
                                                    data.Add(0xFF);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    break;
                                                }
                                            case Format.sV:
                                            case Format.strV:
                                            case Format.xV:
                                                {
                                                    byte[] v = BitConverter.GetBytes(InvertEndianness(ushort.Parse(line[1])));
                                                    data.Add(w);
                                                    data.Add(0xFF);
                                                    data.AddRange(v);
                                                    break;
                                                }
                                            case Format.sZV:
                                            case Format.strZV:
                                            case Format.xZV:
                                                {
                                                    byte[] v = BitConverter.GetBytes(InvertEndianness(ushort.Parse(line[2])));
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.AddRange(v);
                                                    break;
                                                }
                                            case Format.shV:
                                                {
                                                    byte[] v = BitConverter.GetBytes(InvertEndianness(short.Parse(line[1])));
                                                    data.Add(w);
                                                    data.Add(0xFF);
                                                    data.AddRange(v);
                                                    break;
                                                }
                                            case Format.shZV:
                                                {
                                                    byte[] v = BitConverter.GetBytes(InvertEndianness(short.Parse(line[2])));
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.AddRange(v);
                                                    break;
                                                }
                                            case Format.ZX:
                                                {
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(0xFF);
                                                    break;
                                                }
                                            case Format.aZX:
                                                {
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1].Remove(3, 1).Remove(0, 1), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(0xFF);
                                                    break;
                                                }
                                            case Format.nZXY:
                                                {
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1], System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(byte.Parse(line[2], System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(0xFF);
                                                    break;
                                                }
                                            case Format.ZXY:
                                                {
                                                    data.Add(w);
                                                    data.Add(byte.Parse(line[1].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(byte.Parse(line[2].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    data.Add(byte.Parse(line[3].Replace("r", ""), System.Globalization.NumberStyles.HexNumber));
                                                    break;
                                                }
                                        }
                                    }
                                    else if (script[d] == "}")
                                    {
                                        c = d - 1;
                                        method.Data = data.ToArray();
                                        cl.Methods.Add(method);
                                        break;
                                    }
                                }
                            }
                        }

                        classes.Add(cl);
                    }
                }

                //File Building
                MemoryStream stream = new MemoryStream();
                BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream);

                writer.Write(new byte[] {
                    0x58, 0x42, 0x49, 0x4E, 0x12, 0x34, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0xA4,
                    0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                List<byte> sdataraw = new List<byte>();
                for (int i = 0; i < sdata.Count; i++)
                {
                    sdataraw.AddRange(sdata[i]);
                }

                writer.Write(sdataraw.Count);
                writer.Write(sdataraw.ToArray());
                writer.Write(0);

                uint pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x1C, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                List<uint> xrefOffsets = new List<uint>();
                writer.Write(xref.Count);
                for (int i = 0; i < xref.Count; i++)
                {
                    xrefOffsets.Add((uint)writer.BaseStream.Position);
                    writer.Write(0);
                }

                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x20, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                List<uint> clOffsets = new List<uint>();
                List<uint> clNameOffsets = new List<uint>();
                List<uint[]> vTypeOffsets = new List<uint[]>();
                List<uint[]> vNameOffsets = new List<uint[]>();
                List<uint[]> mNameOffsets = new List<uint[]>();
                writer.Write(classes.Count);
                for (int i = 0; i < classes.Count; i++)
                {
                    clOffsets.Add((uint)writer.BaseStream.Position);
                    writer.Write(0);
                }
                for (int i = 0; i < classes.Count; i++)
                {
                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(clOffsets[i], SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);

                    clNameOffsets.Add((uint)writer.BaseStream.Position);
                    writer.Write(0);
                    writer.Write((uint)writer.BaseStream.Position + 0x8);
                    uint mListOffset = (uint)writer.BaseStream.Position;
                    writer.Write(0);

                    List<uint> vOffsets = new List<uint>();
                    List<uint> vt = new List<uint>();
                    List<uint> vn = new List<uint>();
                    writer.Write(classes[i].Variables.Count);
                    for (int c = 0; c < classes[i].Variables.Count; c++)
                    {
                        vOffsets.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                    }
                    for (int c = 0; c < classes[i].Variables.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(vOffsets[c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        vn.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                        vt.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                    }

                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(mListOffset, SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    List<uint> mOffsets = new List<uint>();
                    List<uint> mn = new List<uint>();
                    writer.Write(classes[i].Methods.Count);
                    for (int c = 0; c < classes[i].Methods.Count; c++)
                    {
                        mOffsets.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                    }
                    for (int c = 0; c < classes[i].Methods.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(mOffsets[c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        mn.Add((uint)writer.BaseStream.Position);
                        writer.Write(0);
                        writer.Write((uint)writer.BaseStream.Position + 0x4);
                        writer.Write(classes[i].Methods[c].Data);
                    }
                    vTypeOffsets.Add(vt.ToArray());
                    vNameOffsets.Add(vn.ToArray());
                    mNameOffsets.Add(mn.ToArray());
                }

                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x14, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                writer.Write(scriptname.Length);
                writer.Write(Encoding.UTF8.GetBytes(scriptname));
                while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                {
                    writer.Write((byte)0);
                }
                writer.Write(0);

                for (int i = 0; i < xref.Count; i++)
                {
                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(xrefOffsets[i], SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);

                    writer.Write(xref[i].Length);
                    writer.Write(Encoding.UTF8.GetBytes(xref[i]));
                    while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                    {
                        writer.Write((byte)0);
                    }
                    writer.Write(0);
                }

                for (int i = 0; i < classes.Count; i++)
                {
                    pos = (uint)writer.BaseStream.Position;
                    writer.BaseStream.Seek(clNameOffsets[i], SeekOrigin.Begin);
                    writer.Write(pos);
                    writer.BaseStream.Seek(0, SeekOrigin.End);

                    writer.Write(classes[i].Name.Length);
                    writer.Write(Encoding.UTF8.GetBytes(classes[i].Name));
                    while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                    {
                        writer.Write((byte)0);
                    }
                    writer.Write(0);

                    for (int c = 0; c < classes[i].Variables.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(vTypeOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Variables[c].Type.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Variables[c].Type));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);

                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(vNameOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Variables[c].Name.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Variables[c].Name));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);
                    }

                    for (int c = 0; c < classes[i].Methods.Count; c++)
                    {
                        pos = (uint)writer.BaseStream.Position;
                        writer.BaseStream.Seek(mNameOffsets[i][c], SeekOrigin.Begin);
                        writer.Write(pos);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        writer.Write(classes[i].Methods[c].Name.Length);
                        writer.Write(Encoding.UTF8.GetBytes(classes[i].Methods[c].Name));
                        while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                        {
                            writer.Write((byte)0);
                        }
                        writer.Write(0);
                    }
                }

                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(0x8, SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.Begin);

                CompiledScript = stream.GetBuffer().Take((int)pos).ToList();

            }
        }

        private short InvertEndianness(short val)
        {
            return (short)(((val & 0x00ff) << 8) +
                           ((val & 0xff00) >> 8));
        }

        private ushort InvertEndianness(ushort val)
        {
            return (ushort)(((val & 0x00ff) << 8) +
                            ((val & 0xff00) >> 8));
        }

        private int InvertEndianness(int val)
        {
            return (int)(((val & 0x000000ff) << 24) +
                         ((val & 0x0000ff00) << 8) +
                         ((val & 0x00ff0000) >> 8) +
                         ((val & 0xff000000) >> 24));
        }

        private uint InvertEndianness(uint val)
        {
            return ((val & 0x000000ff) << 24) +
                   ((val & 0x0000ff00) << 8) +
                   ((val & 0x00ff0000) >> 8) +
                   ((val & 0xff000000) >> 24);
        }

        private float InvertEndianness(float val)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(val).Reverse().ToArray(), 0);
        }
    }
}
