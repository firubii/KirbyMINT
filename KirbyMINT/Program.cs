using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using MINT;
using MINT.TDX;
using MINT.KSA;

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
                        string dir;
                        if (args.Contains("-o"))
                        {
                            int dIndex = args.ToList().IndexOf("-o") + 1;
                            dir = args[dIndex];
                        }
                        else
                        {
                            dir = Directory.GetCurrentDirectory() + "\\MINT";
                        }
                        if (!Directory.Exists(dir))
                        {
                            Console.WriteLine("Directory does not exist! Creating...");
                            Directory.CreateDirectory(dir);
                        }
                        Console.WriteLine("Reading archive...");
                        Archive archive = new Archive(args[index]);
                        Dictionary<uint, string> hashList = new Dictionary<uint, string>();
                        Console.Write("Reading hashes...");
                        int progress = 1;
                        if (args.Contains("-h") && File.Exists(args[args.ToList().IndexOf("-h") + 1]))
                        {
                            int hindex = args.ToList().IndexOf("-h") + 1;
                            string[] hashes = File.ReadAllLines(args[hindex]);
                            for (int i = 0; i < hashes.Length; i++)
                            {
                                hashList.Add(uint.Parse(string.Join("", hashes[i].Take(8)), System.Globalization.NumberStyles.HexNumber), string.Join("", hashes[i].Skip(9)));
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                Console.Write($"\rReading hashes... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                ScriptHashReader scriptHashReader = new ScriptHashReader(archive.files[pair.Key]);
                                string[] hashes = scriptHashReader.hashes.ToArray();
                                for (int i = 0; i < hashes.Length; i++)
                                {
                                    hashList.Add(uint.Parse(string.Join("", hashes[i].Take(8)), System.Globalization.NumberStyles.HexNumber), string.Join("", hashes[i].Skip(9)));
                                }
                                progress++;
                            }
                        }
                        Console.Write("\nDecompiling scripts...");
                        progress = 1;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            if (archive.game == Game.TDX)
                            {
                                MINT.TDX.Script script = new MINT.TDX.Script(pair.Value, hashList);
                                if (!script.decompileFailure)
                                {
                                    Console.Write($"\rDecompiling scripts... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                    string name = pair.Key;
                                    byte[] file = pair.Value;
                                    string filedir = dir + "\\" + (name + ".mint").Replace("." + name.Split('.').Last() + ".mint", "").Replace(".", "\\");
                                    if (!Directory.Exists(filedir))
                                        Directory.CreateDirectory(filedir);
                                    filedir = dir + "\\" + name.Replace(".", "\\") + ".mint";
                                    File.WriteAllLines(filedir, script.script);
                                    progress++;
                                }
                                else
                                {
                                    Console.WriteLine("Stopping.");
                                    return;
                                }
                            }
                            else if (archive.game == Game.KPR)
                            {
                                MINT.KPR.Script script = new MINT.KPR.Script(pair.Value, hashList);
                                if (!script.decompileFailure)
                                {
                                    Console.Write($"\rDecompiling scripts... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                    string name = pair.Key;
                                    byte[] file = pair.Value;
                                    string filedir = dir + "\\" + (name + ".mint").Replace("." + name.Split('.').Last() + ".mint", "").Replace(".", "\\");
                                    if (!Directory.Exists(filedir))
                                        Directory.CreateDirectory(filedir);
                                    filedir = dir + "\\" + name.Replace(".", "\\") + ".mint";
                                    File.WriteAllLines(filedir, script.script);
                                    progress++;
                                }
                                else
                                {
                                    Console.WriteLine("Stopping.");
                                    return;
                                }
                            }
                            else if (archive.game == Game.KSA)
                            {
                                MINT.KSA.Script script = new MINT.KSA.Script(pair.Value, hashList);
                                if (!script.decompileFailure)
                                {
                                    Console.Write($"\rDecompiling scripts... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                    string name = pair.Key;
                                    byte[] file = pair.Value;
                                    string filedir = dir + "\\" + (name + ".mint").Replace("." + name.Split('.').Last() + ".mint", "").Replace(".", "\\");
                                    if (!Directory.Exists(filedir))
                                        Directory.CreateDirectory(filedir);
                                    filedir = dir + "\\" + name.Replace(".", "\\") + ".mint";
                                    File.WriteAllLines(filedir, script.script);
                                    progress++;
                                }
                                else
                                {
                                    Console.WriteLine("Stopping.");
                                    return;
                                }
                            }
                        }
                        Console.WriteLine("\nFinished.");
                    }
                }
                else if (args.Contains("-bin"))
                {
                    int index = args.ToList().IndexOf("-bin") + 1;
                    if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                    {
                        string dir;
                        if (args.Contains("-o"))
                        {
                            int dIndex = args.ToList().IndexOf("-o") + 1;
                            dir = args[dIndex];
                        }
                        else
                        {
                            dir = Directory.GetCurrentDirectory() + "\\BIN";
                        }
                        if (!Directory.Exists(dir))
                        {
                            Console.WriteLine("Directory does not exist! Creating...");
                            Directory.CreateDirectory(dir);
                        }
                        Archive archive = new Archive(args[index]);
                        Console.Write("Extracting files...");
                        int progress = 1;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            Console.Write($"\rExtracting files... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                            File.WriteAllBytes(dir + "\\" + pair.Key + ".bin", pair.Value);
                            progress++;
                        }
                        Console.WriteLine("\nFinished.");
                    }
                }
                else if (args.Contains("-hash"))
                {
                    int index = args.ToList().IndexOf("-hash") + 1;
                    if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                    {
                        Archive archive = new Archive(args[index]);
                        List<string> hashes = new List<string>();
                        Console.Write("Reading hashes...");
                        int progress = 1;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            Console.Write($"\rReading hashes... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                            ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value);
                            hashes.AddRange(scriptHashReader.hashes);
                            progress++;
                        }
                        string output;
                        if (args.Contains("-o"))
                        {
                            int dIndex = args.ToList().IndexOf("-o") + 1;
                            output = args[dIndex];
                        }
                        else
                        {
                            output = Directory.GetCurrentDirectory() + "\\hash_" + archive.game.ToString().ToLower() + ".txt";
                        }
                        File.WriteAllLines(output, hashes);
                        Console.WriteLine("\nFinished.");
                    }
                    else if (Directory.Exists(args[index]))
                    {
                        Archive archive = new Archive();
                        List<string> hashes = new List<string>();
                        string[] files = Directory.GetFiles(args[index], "*.bin");
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.Write("\nReading hashes from archive " + files[i]);
                            archive = new Archive(files[i]);
                            int progress = 1;
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                Console.Write($"\rReading hashes from archive {files[i]} - {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value);
                                hashes.AddRange(scriptHashReader.hashes);
                                progress++;
                            }
                        }
                        string output;
                        if (args.Contains("-o"))
                        {
                            int dIndex = args.ToList().IndexOf("-o") + 1;
                            output = args[dIndex];
                        }
                        else
                        {
                            output = Directory.GetCurrentDirectory() + "\\hash_" + archive.game.ToString().ToLower() + ".txt";
                        }
                        File.WriteAllLines(output, hashes);
                        Console.WriteLine("\nFinished.");
                    }
                }
            }
        }
    }
}
