using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using MINT;
using MINT.TDX;

namespace KirbyMINT
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Contains("-x"))
                {
                    int index = args.ToList().IndexOf("-x") + 1;
                    if (args[index].EndsWith(".bin"))
                    {
                        string dir = Directory.GetCurrentDirectory() + "\\MINT";
                        if (Directory.Exists(dir))
                            Directory.Delete(dir, true);
                        Directory.CreateDirectory(dir);
                        Archive archive = new Archive(args[index]);
                        string[] hashes = { };
                        if (archive.game == Game.TDX)
                        {
                            if (File.Exists(Directory.GetCurrentDirectory() + "\\hash_tdx.txt"))
                            {
                                hashes = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\hash_tdx.txt");
                            }
                        }
                        List<uint> hashIds = new List<uint>();
                        List<string> hashNames = new List<string>();
                        if (hashes.Length > 0)
                        {
                            Console.WriteLine("Reading hash database file...");
                            for (int i = 0; i < hashes.Length; i++)
                            {
                                hashIds.Add(uint.Parse(string.Join("", hashes[i].Take(8)), System.Globalization.NumberStyles.HexNumber));
                                hashNames.Add(string.Join("", hashes[i].Skip(9)));
                            }
                        }
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            Console.WriteLine("Dumping script " + pair.Key);
                            Script script = new Script(pair.Value, hashIds.ToArray(), hashNames.ToArray());
                            string name = pair.Key;
                            byte[] file = pair.Value;
                            string filedir = dir + "\\" + (name + ".txt").Replace("." + name.Split('.').Last() + ".txt", "").Replace(".", "\\");
                            if (!Directory.Exists(filedir))
                                Directory.CreateDirectory(filedir);
                            filedir = dir + "\\" + name.Replace(".", "\\") + ".txt";
                            File.WriteAllLines(filedir, script.script);
                        }
                    }
                }
                if (args.Contains("-hash"))
                {
                    int index = args.ToList().IndexOf("-hash") + 1;
                    if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                    {
                        Archive archive = new Archive(args[index]);
                        List<string> hashes = new List<string>();
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            Console.WriteLine("Analyzing script " + pair.Key);
                            ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value);
                            hashes.AddRange(scriptHashReader.hashes);
                        }
                        if (archive.game == Game.TDX)
                        {
                            File.WriteAllLines(Directory.GetCurrentDirectory() + "\\hash_tdx.txt", hashes);
                        }
                        else if (archive.game == Game.KPR)
                        {
                            File.WriteAllLines(Directory.GetCurrentDirectory() + "\\hash_kpr.txt", hashes);
                        }
                        else if (archive.game == Game.KSA)
                        {
                            File.WriteAllLines(Directory.GetCurrentDirectory() + "\\hash_ksa.txt", hashes);
                        }
                    }
                    else if (Directory.Exists(args[index]))
                    {
                        Archive archive = new Archive();
                        List<string> hashes = new List<string>();
                        string[] files = Directory.GetFiles(args[index], "*.bin");
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.WriteLine("Analyzing Archive " + files[i]);
                            archive = new Archive(files[i]);
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                Console.WriteLine("Analyzing script " + pair.Key);
                                ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value);
                                hashes.AddRange(scriptHashReader.hashes);
                            }
                        }
                        if (archive.game == Game.TDX)
                        {
                            File.WriteAllLines(Directory.GetCurrentDirectory() + "\\hash_tdx.txt", hashes);
                        }
                        else if (archive.game == Game.KPR)
                        {
                            File.WriteAllLines(Directory.GetCurrentDirectory() + "\\hash_kpr.txt", hashes);
                        }
                        else if (archive.game == Game.KSA)
                        {
                            File.WriteAllLines(Directory.GetCurrentDirectory() + "\\hash_ksa.txt", hashes);
                        }
                        Console.WriteLine("Hashes found: " + hashes.Count);
                        Console.WriteLine("Files analyzed: " + files.Length);
                    }
                }
            }
        }
    }
}
