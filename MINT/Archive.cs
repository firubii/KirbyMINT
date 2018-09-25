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
                uint stringlen = reader.ReadUInt32();
                string name = string.Join("", reader.ReadChars((int)stringlen));
                reader.BaseStream.Seek(fileoffset + 0x8, SeekOrigin.Begin);
                uint filelen = reader.ReadUInt32();
                reader.BaseStream.Seek(-0xC, SeekOrigin.Current);
                byte[] file = reader.ReadBytes((int)filelen);
                files.Add(name, file);
            }
        }
    }
}
