using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MINT.KSA;

namespace MINT
{
    public class Archive
    {
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
        public Archive(string dir, string output)
        {
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
            string[] dirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);
            string[] files = Directory.GetFiles(dir, "*.mint", SearchOption.AllDirectories);
            List<string> fileNames = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                fileNames.Add(files[i].Remove(files[i].Length - 5, 5).Remove(0, dir.Length + 1).Replace('\\', '.'));
            }
            BinaryWriter writer = new BinaryWriter(new FileStream(output, FileMode.Create));
            writer.Write(new byte[] {
                0x58, 0x42, 0x49, 0x4E, 0x34, 0x12, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE9, 0xFD, 0x00, 0x00,
                0x02, 0x01, 0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00 });
            writer.Write(files.Length);
            writer.Write(0x30);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
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
            uint pos = 0;
            for (int i = 0; i < files.Length; i++)
            {
                //Console.WriteLine(fileNames[i]);
                Console.Write($"\rCompiling files... {progress}/{files.Length} - {(int)(((float)progress / (float)files.Length) * 100)}%");
                pos = (uint)writer.BaseStream.Position;
                writer.BaseStream.Seek(fileOffsets[i], SeekOrigin.Begin);
                writer.Write(pos);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                MINT.KSA.Script script = new Script(File.ReadAllLines(files[i]));
                writer.Write(script.compScript.ToArray());
                progress++;
            }
            pos = (uint)writer.BaseStream.Position;
            writer.BaseStream.Seek(0x24, SeekOrigin.Begin);
            writer.Write(pos);
            writer.BaseStream.Seek(0, SeekOrigin.End);
            while ((writer.BaseStream.Length).ToString("X").Last() != '0')
            {
                writer.Write((byte)0);
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
