using System;
using System.IO;
using System.Text;

namespace NS.Bot.Shared.Models
{
    public class ServerInfo
    {
        public ServerInfo(ref BinaryReader binReader)
        {
            Console.WriteLine("IN SERVERINFO");
            Console.WriteLine("Read HEADER");
            Header = binReader.ReadByte();
            Console.WriteLine("Read Protocol");
            Protocol = binReader.ReadByte();
            Console.WriteLine("Read Name");
            Name = ReadNullTerminatedString(ref binReader);
            Console.WriteLine("Read Map");
            Map = ReadNullTerminatedString(ref binReader);
            Console.WriteLine("Read Folder");
            Folder = ReadNullTerminatedString(ref binReader);
            Console.WriteLine("Read Game");
            Game = ReadNullTerminatedString(ref binReader);
            Console.WriteLine("Read Id");
            Id = binReader.ReadInt16();
            Console.WriteLine("Read Players");
            Players = binReader.ReadByte();
            Console.WriteLine("Read MaxPlayers");
            MaxPlayers = binReader.ReadByte();
            Console.WriteLine("Read Bots");
            Bots = binReader.ReadByte();
            Console.WriteLine("Read ServerType");
            ServerType = (ServerTypeFlags)binReader.ReadByte();
            Console.WriteLine("Read Environment");
            Environment = (EnvironmentFlags)binReader.ReadByte();
            Console.WriteLine("Read Visibility");
            Visibility = (VisibilityFlags)binReader.ReadByte();
            Console.WriteLine("Read Vac");
            Vac = (VacFlags)binReader.ReadByte();
            Console.WriteLine("Read Version");
            Version = ReadNullTerminatedString(ref binReader);
            Console.WriteLine("Read ExtraDataFlag");
            ExtraDataFlag = (ExtraDataFlags)binReader.ReadByte();
            GameId = 0;
            SteamId = 0;
            Keywords = null;
            Spectator = null;
            SpectatorPort = 0;
            Port = 0;
            if (ExtraDataFlag.HasFlag(ExtraDataFlags.Port))
            {
                Port = binReader.ReadInt16();
            }


            if (ExtraDataFlag.HasFlag(ExtraDataFlags.SteamId))
            {
                SteamId = binReader.ReadUInt64();
            }

            if (ExtraDataFlag.HasFlag(ExtraDataFlags.Spectator))
            {
                SpectatorPort = binReader.ReadInt16();
                Spectator = ReadNullTerminatedString(ref binReader);
            }

            if (ExtraDataFlag.HasFlag(ExtraDataFlags.Keywords))
            {
                Keywords = ReadNullTerminatedString(ref binReader);
            }

            if (ExtraDataFlag.HasFlag(ExtraDataFlags.GameId))
            {
                GameId = binReader.ReadUInt64();
            }
        }

        private byte Header { get; set; }
        public byte Protocol { get; set; }
        public string Name { get; set; }
        public string Map { get; set; }
        public string Folder { get; set; }
        public string Game { get; set; }
        public short Id { get; set; }
        public byte Players { get; set; }
        public byte MaxPlayers { get; set; }
        public byte Bots { get; set; }
        public ServerTypeFlags ServerType { get; set; }
        public EnvironmentFlags Environment { get; set; }
        public VisibilityFlags Visibility { get; set; }
        public VacFlags Vac { get; set; }
        public string Version { get; set; }
        public ExtraDataFlags ExtraDataFlag { get; set; }

        [Flags]
        public enum ExtraDataFlags : byte
        {
            GameId = 0x01,
            SteamId = 0x10,
            Keywords = 0x20,
            Spectator = 0x40,
            Port = 0x80
        }

        public enum VacFlags : byte
        {
            Unsecured = 0,
            Secured = 1
        }

        public enum VisibilityFlags : byte
        {
            Public = 0,
            Private = 1
        }

        public enum EnvironmentFlags : byte
        {
            Linux = 0x6C,
            Windows = 0x77,
            Mac = 0x6D,
            MacOsX = 0x6F
        }

        public enum ServerTypeFlags : byte
        {
            Dedicated = 0x64,
            NonDedicated = 0x6C,
            SourceTv = 0x70
        }

        public ulong GameId { get; set; }
        public ulong SteamId { get; set; }
        public string Keywords { get; set; }
        public string Spectator { get; set; }
        public short SpectatorPort { get; set; }
        public short Port { get; set; }

        private static string ReadNullTerminatedString(ref BinaryReader input)
        {
            var sb = new StringBuilder();
            var read = input.ReadChar();
            while (read != '\x00')
            {
                Console.WriteLine(read);
                sb.Append(read);
                read = input.ReadChar();
            }

            return sb.ToString();
        }
    }

}
