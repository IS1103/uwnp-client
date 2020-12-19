using ProtoBuf;
using System.IO;
using UnityEngine;

namespace UWNP
{
    public class MessageProtocol
    {
        public static Message<T> DecodeMsg<T>(byte[] buff)
        {
            if (buff == null) return new Message<T>();

            Message<T> rsInfo = new Message<T>();
            //protobuf反序列化
            MemoryStream mem = new MemoryStream(buff);
            Message<byte[]> rsb = Serializer.Deserialize<Message<byte[]>>(mem);
            mem.Close();
            rsInfo.err = rsb.err;
            rsInfo.errMsg = rsb.errMsg;

            T tm = DecodeInfo<T>(rsb.info);
            rsInfo.info = tm;

            return rsInfo;
        }

        public static Message<T> Decode<T>(byte[] buff)
        {
            Message<T> rs;
            try
            {
                if (buff == null) return new Message<T>();
                //protobuf反序列化
                MemoryStream mem = new MemoryStream(buff);
                rs = Serializer.Deserialize<Message<T>>(mem);
                mem.Close();
                return rs;
            }
            catch (System.Exception)
            {
                rs = new Message<T>();
                MemoryStream mem = new MemoryStream(buff);
                Message<byte[]> rsb = Serializer.Deserialize<Message<byte[]>>(mem);
                mem.Close();
                rs.err = rsb.err;
                rs.errMsg = rsb.errMsg;
                throw;
            }
        }

        public static T MyMethod<T>(byte[] buff) where T : Object
        {
            return null;
        }

        public static T DecodeInfo<T>(byte[] buff)
        {
            if (buff == null) return default;
            MemoryStream mem = new MemoryStream(buff);
            T rs = Serializer.Deserialize<T>(mem);
            mem.Close();
            return rs;
        }
    }
}