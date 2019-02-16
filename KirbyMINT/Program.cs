using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using MINT;

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
                    if (args.Contains("-f"))
                    {
                        if (args[index].EndsWith(".bin"))
                        {
                            Game game = Game.KSA;
                            if (args.Contains("-tdx"))
                            {
                                game = Game.TDX;
                            }
                            else if (args.Contains("-kpr"))
                            {
                                game = Game.KPR;
                            }
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
                            Console.WriteLine("Reading file...");
                            byte[] file = File.ReadAllBytes(args[index]);
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
                            Console.WriteLine("\nDecompiling script...");
                            Script script = new Script(file, hashList, game);
                            if (!script.DecompileFailure)
                            {
                                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\output.mint", script.DecompiledScript);
                                progress++;
                            }
                            else
                            {
                                Console.WriteLine("Stopping.");
                                return;
                            }
                            w.Stop();
                            Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                        }
                    }
                    else
                    {
                        if (args[index].EndsWith(".bin"))
                        {
                            bool rdl = args.Contains("-rdl");
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
                            Archive archive = new Archive(args[index], rdl);
                            Dictionary<uint, string> hashList = new Dictionary<uint, string>();
                            int progress = 1;
                            if (!rdl)
                            {
                                Console.Write("Reading hashes...");
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
                            }
                            Console.Write("\nDecompiling scripts...");
                            progress = 1;
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                Script script = new Script(pair.Value, hashList, archive.game);
                                if (!script.DecompileFailure)
                                {
                                    Console.Write($"\rDecompiling scripts... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                                    string name = pair.Key;
                                    byte[] file = pair.Value;
                                    string filedir = dir + "\\" + (name + ".mint").Replace("." + name.Split('.').Last() + ".mint", "").Replace(".", "\\");
                                    if (!Directory.Exists(filedir))
                                        Directory.CreateDirectory(filedir);
                                    filedir = dir + "\\" + name.Replace(".", "\\") + ".mint";
                                    File.WriteAllLines(filedir, script.DecompiledScript);
                                    progress++;
                                }
                                else
                                {
                                    Console.WriteLine("Stopping.");
                                    return;
                                }
                            }
                            w.Stop();
                            Console.WriteLine($"\nFinished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                        }
                    }
                }
                else if (args.Contains("-r"))
                {
                    int index = args.ToList().IndexOf("-r") + 1;
                    if (args.Contains("-f"))
                    {
                        if (File.Exists(args[index]))
                        {
                            Game game = Game.KSA;
                            if (args.Contains("-tdx"))
                            {
                                game = Game.TDX;
                            }
                            else if (args.Contains("-kpr"))
                            {
                                game = Game.KPR;
                            }
                            string[] txt = File.ReadAllLines(args[index]);
                            Script script = new Script(txt, game);
                            File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\output.bin", script.CompiledScript.ToArray());
                        }
                    }
                    else
                    {
                        Game game = Game.KSA;
                        if (args.Contains("-tdx"))
                        {
                            game = Game.TDX;
                        }
                        else if (args.Contains("-kpr"))
                        {
                            game = Game.KPR;
                        }
                        else if (args.Contains("-rdl"))
                        {
                            game = Game.RDL;
                        }
                        string output = args[index] + ".bin";
                        if (args.Contains("-o"))
                        {
                            output = args[args.ToList().IndexOf("-o") + 1];
                        }
                        Console.WriteLine($"Compiling {args[index]}...");
                        System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
                        Archive archive = new Archive(args[index], output, game);
                        w.Stop();
                        Console.WriteLine($"Finished. Operation completed in {(w.Elapsed.Minutes * 60) + w.Elapsed.Seconds}.{w.Elapsed.Milliseconds}s.");
                    }
                }
                else if (args.Contains("-bin"))
                {
                    int index = args.ToList().IndexOf("-bin") + 1;
                    if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                    {
                        bool rdl = args.Contains("-rdl");
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
                        Archive archive = new Archive(args[index], rdl);
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
                else if (args.Contains("-help"))
                {
                    PrintHelp();
                }
                else
                {
                    PrintHelp();
                }
            }
            else
            {
                PrintHelp();
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("\nUsage: KirbyMINT.exe <action> [options]");
            Console.WriteLine("\nActions:");
            Console.WriteLine("    -x <file>:            Extract and decompile a MINT Archive");
            Console.WriteLine("    -r <folder>:          Repack and compile a MINT Archive from a folder");
            Console.WriteLine("    -hash <file|folder>:  Dump hashes from a MINT Archive or collection of MINT Archives");
            Console.WriteLine("    -bin <file>:          Dump the raw data from a MINT Archive");
            Console.WriteLine("    -help:                Show this message");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("    -h <file>:            For decompiling only; Specifies hash file to use");
            Console.WriteLine("    -o <folder|file>:     Specifies output folder or file");
            Console.WriteLine("    -rdl:                 Sets target game to Kirby's Return to Dream Land");
            Console.WriteLine("    -tdx:                 Sets target game to Kirby: Triple Deluxe");
            Console.WriteLine("    -kpr:                 Sets target game to Kirby: Planet Robobot");
            Console.WriteLine("    -ksa:                 Sets target game to Kirby: Star Allies (Default)");
        }
    }
}
