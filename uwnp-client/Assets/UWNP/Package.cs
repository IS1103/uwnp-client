using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UWNP
{
    [ProtoContract]
    public class Package
    {
        [ProtoMember(1)]
        public uint packageType;

        [ProtoMember(2)]
        public string route = null;

        [ProtoMember(3)] 
        public uint packID = 0;

        [ProtoMember(4)]
        public byte[] buff = null;
    }

    [ProtoContract]
    public class Message<T>
    {
        [ProtoMember(1)]
        public uint err;

        [ProtoMember(2)]
        public string errMsg = default;

        [ProtoMember(3)]
        public T info = default;

        public Message() { }

        public Message(uint err, string errMsg, T info)
        {
            this.err = err;
            this.errMsg = errMsg;
            this.info = info;
        }
    }

    [ProtoContract]
    public class HandShake
    {
        [ProtoMember(1)]
        public string token;
    }

    [ProtoContract]
    public class Heartbeat
    {
        [ProtoMember(1)]
        public uint heartbeat;
    }
}