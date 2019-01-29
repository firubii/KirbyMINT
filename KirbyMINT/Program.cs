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
                        System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
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
                                foreach (KeyValuePair<uint, string> p in scriptHashReader.hashes)
                                {
                                    if (!hashList.ContainsKey(p.Key))
                                    {
                                        hashList.Add(p.Key, p.Value);
                                    }
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
                        w.Stop();
                        Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
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
                        System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
                        Archive archive = new Archive(args[index]);
                        Console.Write("Extracting files...");
                        int progress = 1;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            Console.Write($"\rExtracting files... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                            File.WriteAllBytes(dir + "\\" + pair.Key + ".bin", pair.Value);
                            progress++;
                        }
                        w.Stop();
                        Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                    }
                }
                else if (args.Contains("-hash"))
                {
                    int index = args.ToList().IndexOf("-hash") + 1;
                    if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                    {
                        Archive archive = new Archive(args[index]);
                        Dictionary<uint, string> hashes = new Dictionary<uint, string>();
                        System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
                        Console.Write("Reading hashes...");
                        int progress = 1;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            Console.Write($"\rReading hashes... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                            ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value);
                            foreach (KeyValuePair<uint, string> p in scriptHashReader.hashes)
                            {
                                if (!hashes.ContainsKey(p.Key))
                                {
                                    hashes.Add(p.Key, p.Value);
                                }
                            }
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
                        List<string> hashTxt = new List<string>();
                        foreach (KeyValuePair<uint, string> p in hashes)
                        {
                            hashTxt.Add($"{p.Key.ToString("X8")} {p.Value}");
                        }
                        File.WriteAllLines(output, hashTxt);
                        w.Stop();
                        Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                    }
                    else if (Directory.Exists(args[index]))
                    {
                        Archive archive = new Archive();
                        Dictionary<uint, string> hashes = new Dictionary<uint, string>();
                        string[] files = Directory.GetFiles(args[index], "*.bin");
                        System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.Write("\nReading hashes from archive " + files[i]);
                            archive = new Archive(files[i]);
                            int progress = 1;
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                Console.Write($"\rReading hashes from archive {files[i]} - {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value);
                                foreach (KeyValuePair<uint, string> p in scriptHashReader.hashes)
                                {
                                    if (!hashes.ContainsKey(p.Key))
                                    {
                                        hashes.Add(p.Key, p.Value);
                                    }
                                }
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
                        List<string> hashTxt = new List<string>();
                        foreach (KeyValuePair<uint, string> p in hashes)
                        {
                            hashTxt.Add($"{p.Key.ToString("X8")} {p.Value}");
                        }
                        File.WriteAllLines(output, hashTxt);
                        w.Stop();
                        Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                    }
                }
                else if (args.Contains("-uhash"))
                {
                    if (args.Contains("-h"))
                    {
                        int hindex = args.ToList().IndexOf("-h") + 1;
                        int index = args.ToList().IndexOf("-uhash") + 1;
                        Dictionary<uint, string> hashList = new Dictionary<uint, string>();
                        string[] hashFile = File.ReadAllLines(args[hindex]);
                        for (int i = 0; i < hashFile.Length; i++)
                        {
                            hashList.Add(uint.Parse(string.Join("", hashFile[i].Take(8)), System.Globalization.NumberStyles.HexNumber), string.Join("", hashFile[i].Skip(9)));
                        }
                        if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                        {
                            Archive archive = new Archive(args[index]);
                            List<uint> unknownHashes = new List<uint>();
                            System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
                            Console.Write("Reading unknown hashes...");
                            int progress = 1;
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                Console.Write($"\rReading unknown hashes... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value, hashList);
                                unknownHashes.AddRange(scriptHashReader.unknownHashes);
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
                                output = Directory.GetCurrentDirectory() + "\\unk_hash_" + archive.game.ToString().ToLower() + ".txt";
                            }
                            List<string> hashTxt = new List<string>();
                            for (int i = 0; i < unknownHashes.Count; i++)
                            {
                                hashTxt.Add($"{unknownHashes[i].ToString("X8")}");
                            }
                            File.WriteAllLines(output, hashTxt);
                            w.Stop();
                            Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                        }
                        else if (Directory.Exists(args[index]))
                        {
                            List<uint> unknownHashes = new List<uint>();
                            Archive archive = new Archive();
                            string[] files = Directory.GetFiles(args[index], "*.bin");
                            System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
                            for (int i = 0; i < files.Length; i++)
                            {
                                Console.Write("\nReading unknown hashes from archive " + files[i]);
                                archive = new Archive(files[i]);
                                int progress = 1;
                                foreach (KeyValuePair<string, byte[]> pair in archive.files)
                                {
                                    Console.Write($"\rReading unknown hashes from archive {files[i]} - {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                    ScriptHashReader scriptHashReader = new ScriptHashReader(pair.Value, hashList);
                                    for (int h = 0; h < scriptHashReader.unknownHashes.Count; h++)
                                    {
                                        if (!unknownHashes.Contains(scriptHashReader.unknownHashes[h]))
                                        {
                                            unknownHashes.Add(scriptHashReader.unknownHashes[h]);
                                        }
                                    }
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
                                output = Directory.GetCurrentDirectory() + "\\unk_hash_" + archive.game.ToString().ToLower() + ".txt";
                            }
                            List<string> hashTxt = new List<string>();
                            for (int i = 0; i < unknownHashes.Count; i++)
                            {
                                hashTxt.Add($"{unknownHashes[i].ToString("X8")}");
                            }
                            File.WriteAllLines(output, hashTxt);
                            w.Stop();
                            Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Must supply hash file with -h");
                    }
                }
            }
        }
    }
}
