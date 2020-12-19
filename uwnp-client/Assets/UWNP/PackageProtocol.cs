using ProtoBuf;
using System.IO;

namespace UWNP
{
    public enum PackageType
    {
        HEARTBEAT = 1,
        REQUEST = 2,
        PUSH = 3,
        KICK = 4,
        RESPONSE = 5, 
        HANDSHAKE = 6,
        ERROR = 7,
        NOTIFY = 8
    }

    public class PackageProtocol
    {
        public static byte[] Encode(PackageType type)
        {
            Package sr = new Package() {
                packageType = (uint)type
            };
            return Serialize(sr);
        }

        public static byte[] Encode<T>(PackageType type, uint packID, string route, T info)
        {
            Package sr = new Package(){
                packageType = (uint)type,
                packID = packID,
                route = route,
                buff = Serialize<T>(info)
            };
            return Serialize(sr);
        }

        public static byte[] Serialize<T>(T info) {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, info);
            byte[] buff = ms.ToArray();
            ms.Close();
            return buff;
        }

        public static Package Decode(byte[] buff)
        {
            //protobuf反序列化
            MemoryStream mem = new MemoryStream(buff);
            Package rs = Serializer.Deserialize<Package>(mem);
            mem.Close();
            return rs;
        }
    }
}