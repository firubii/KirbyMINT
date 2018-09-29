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
                        Console.WriteLine("Reading archive...");
                        Archive archive = new Archive(args[index]);
                        List<uint> hashIds = new List<uint>();
                        List<string> hashNames = new List<string>();
                        Console.Write("Reading hashes...");
                        int progress = 0;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            progress++;
                            Console.Write($"\rReading hashes... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                            ScriptHashReader scriptHashReader = new ScriptHashReader(archive.files[pair.Key]);
                            string[] hashes = scriptHashReader.hashes.ToArray();
                            for (int i = 0; i < hashes.Length; i++)
                            {
                                hashIds.Add(uint.Parse(string.Join("", hashes[i].Take(8)), System.Globalization.NumberStyles.HexNumber));
                                hashNames.Add(string.Join("", hashes[i].Skip(9)));
                            }
                        }
                        Console.Write("\nDecompiling scripts...");
                        progress = 0;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            progress++;
                            Script script = new Script(pair.Value, hashIds.ToArray(), hashNames.ToArray());
                            Console.Write($"\rDecompiling scripts... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
                            string name = pair.Key;
                            byte[] file = pair.Value;
                            string filedir = dir + "\\" + (name + ".txt").Replace("." + name.Split('.').Last() + ".txt", "").Replace(".", "\\");
                            if (!Directory.Exists(filedir))
                                Directory.CreateDirectory(filedir);
                            filedir = dir + "\\" + name.Replace(".", "\\") + ".txt";
                            File.WriteAllLines(filedir, script.script);
                        }
                        Console.WriteLine("\nFinished.");
                    }
                }
                if (args.Contains("-hash"))
                {
                    int index = args.ToList().IndexOf("-hash") + 1;
                    if (args[index].EndsWith(".bin") && File.Exists(args[index]))
                    {
                        Archive archive = new Archive(args[index]);
                        List<string> hashes = new List<string>();
                        Console.Write("Reading hashes...");
                        int progress = 0;
                        foreach (KeyValuePair<string, byte[]> pair in archive.files)
                        {
                            progress++;
                            Console.Write($"\rReading hashes... {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
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
                            int progress = 0;
                            foreach (KeyValuePair<string, byte[]> pair in archive.files)
                            {
                                progress++;
                                Console.Write($"\rReading hashes from archive {files[i]} - {progress}/{archive.files.Count} - {(int)(((float)progress / (float)archive.files.Count) * 100)}%");
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
                        Console.WriteLine("\nFinished.");
                    }
                }
            }
        }
    }
}
