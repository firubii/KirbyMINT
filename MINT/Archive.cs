using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MINT
{
    public class Archive
    {
        private struct Dir
        {
            public string Name;
            public uint FileCount;
            public uint DirCount;
            public uint ID;
            public uint ParentID;
        }

        public Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
        public Game game;

        public Archive() { }
        public Archive(string filename)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                Read(reader);
            }
        }
        public Archive(string dir, string output, Game g)
        {
            game = g;
            Write(dir, output);
        }

        public void Read(BinaryReader reader)
        {
            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            uint gameId = reader.ReadUInt32();
            if (gameId == 327681)
            {
                game = Game.TDX;
            }
            else if (gameId == 196865)
            {
                game = Game.KPR;
            }
            else if (gameId == 17105154)
            {
                game = Game.KSA;
            }
            reader.BaseStream.Seek(0x1C, SeekOrigin.Begin);
            uint count = reader.ReadUInt32();
            uint filelist = reader.ReadUInt32();
            reader.BaseStream.Seek(filelist, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Seek(filelist + (i * 0x8), SeekOrigin.Begin);
                uint nameoffset = reader.ReadUInt32();
                uint fileoffset = reader.ReadUInt32();
                reader.BaseStream.Seek(nameoffset, SeekOrigin.Begin);
                string name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                reader.BaseStream.Seek(fileoffset + 0x8, SeekOrigin.Begin);
                uint filelen = reader.ReadUInt32();
                reader.BaseStream.Seek(-0xC, SeekOrigin.Current);
                byte[] file = reader.ReadBytes((int)filelen);
                files.Add(name, file);
            }
        }

        public void Write(string dir, string output)
        {
            string rootDir = Directory.GetParent(dir).FullName + "\\" + dir.Split('\\').Last();
            string[] dirs = Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories);
            string[] files = Directory.GetFiles(rootDir, "*.mint", SearchOption.AllDirectories);
            List<string> fileNames = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                fileNames.Add(files[i].Remove(files[i].Length - 5, 5).Remove(0, rootDir.Length).TrimStart(new char[] { '\\' }).Replace('\\', '.'));
            }
            BinaryWriter writer = new BinaryWriter(new FileStream(output, FileMode.Create));
            writer.Write(new byte[] { 0x58, 0x42, 0x49, 0x4E, 0x34, 0x12, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE9, 0xFD, 0x00, 0x00 });
            if (game == Game.TDX)
            {
                writer.Write(327681);
            }
            else if (game == Game.KPR)
            {
                writer.Write(196865);
            }
            else if (game == Game.KSA)
            {
                writer.Write(17105154);
            }
            writer.Write(dirs.Length);
            writer.Write(0x24);
            writer.Write(files.Length);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            Console.WriteLine("Creating directory data...");
            List<Dir> dirData = new List<Dir>();
            Dictionary<string, uint> ids = new Dictionary<string, uint>();
            uint id = 0;
            for (int i = 0; i < dirs.Length; i++)
            {
                Dir d = new Dir();
                d.Name = dirs[i].Remove(0, rootDir.Length).TrimStart(new char[] { '\\' }).Replace('\\', '.');
                d.FileCount = (uint)Directory.GetFiles(dirs[i], "*.mint", SearchOption.TopDirectoryOnly).Length;
                d.DirCount = (uint)Directory.GetDirectories(dirs[i], "*", SearchOption.TopDirectoryOnly).Length;
                d.ID = id;
                if (Directory.GetParent(dirs[i]).FullName.Remove(0, rootDir.Length).TrimStart(new char[] { '\\' }).Replace('\\', '.') != "")
                {
                    d.ParentID = ids[Directory.GetParent(dirs[i]).FullName.Remove(0, rootDir.Length).TrimStart(new char[] { '\\' }).Replace('\\', '.')];
                }
                else
                {
                    d.ParentID = 0;
                }
                ids.Add(d.Name, d.ID);
                dirData.Add(d);
                id++;
            }
            List<uint> dirNameOffsets = new List<uint>();
            List<uint> pIdOffsets = new List<uint>();
            for (int i = 0; i < dirs.Length; i++)
            {
                writer.Write(dirData[i].DirCount);
                pIdOffsets.Add((uint)writer.BaseStream.Position);
                writer.Write(0);
                dirNameOffsets.Add((uint)writer.BaseStream.Position);
                writer.Write(0);
                writer.Write(dirData[i].FileCount);
                writer.Write(dirData[i].ID);
            }
            Dictionary<uint, uint> w = new Dictionary<uint, uint>();
            for (int i = 0; i < dirs.Length; i++)
            {
                uint o = 0;
                if (!w.ContainsKey(dirData[i].ParentID))
                {
                    w.Add(dirData[i].ParentID, (uint)writer.BaseStream.Position);
                    writer.Write(dirData[i].ParentID);
                }
                o = w[dirData[i].ParentID];
                writer.BaseStream.Seek(pIdOffsets[i], SeekOrigin.Begin);
                writer.Write(o);
                writer.BaseStream.Seek(0, SeekOrigin.End);
            }
            uint pos = (uint)writer.BaseStream.Position;
            writer.BaseStream.Seek(0x20, SeekOrigin.Begin);
            writer.Write(pos);
            writer.BaseStream.Seek(0, SeekOrigin.End);

            List<uint> nameOffsets = new List<uint>();
            List<uint> fileOffsets = new List<uint>();
            for (int i = 0; i < files.Length; i++)
            {
                nameOffsets.Add((uint)writer.BaseStream.Position);
                writer.Write(0);
                fileOffsets.Add((uint)writer.BaseStream.Position);
                writer.Write(0);
            }
            Console.Write("Compiling files...");
            uint progress = 1;
            pos = 0;
            for (int i = 0; i < files.Length; i++)
            {
                //Console.WriteLine(fileNames[i]);
                Console.Write($"\rCompiling files... {progress}/{files.Length} - {(int)(((float)progress / (float)files.Length) * 100)}%");
                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(fileOffsets[i], SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                Script script = new Script(File.ReadAllLines(files[i]), game);
                writer.Write(script.CompiledScript.ToArray());
                progress++;
            }
            Console.WriteLine("\nWriting names...");
            pos = (uint)writer.BaseStream.Position;
            writer.BaseStream.Seek(0x24, SeekOrigin.Begin);
            writer.Write(pos);
            writer.BaseStream.Seek(0, SeekOrigin.End);
            while ((writer.BaseStream.Length).ToString("X").Last() != '0')
            {
                writer.Write((byte)0);
            }
            for (int i = 0; i < dirs.Length; i++)
            {
                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(dirNameOffsets[i], SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                writer.Write(dirData[i].Name.Length);
                writer.Write(Encoding.UTF8.GetBytes(dirData[i].Name));
                while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                {
                    writer.Write((byte)0);
                }
                writer.Write(0);
            }
            for (int i = 0; i < files.Length; i++)
            {
                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(nameOffsets[i], SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                writer.Write(fileNames[i].Length);
                writer.Write(Encoding.UTF8.GetBytes(fileNames[i]));
                while ((writer.BaseStream.Length).ToString("X").Last() != '0' && (writer.BaseStream.Length).ToString("X").Last() != '4' && (writer.BaseStream.Length).ToString("X").Last() != '8' && (writer.BaseStream.Length).ToString("X").Last() != 'C')
                {
                    writer.Write((byte)0);
                }
                writer.Write(0);
            }

            writer.Flush();
            writer.Dispose();
            writer.Close();
        }
    }
}
