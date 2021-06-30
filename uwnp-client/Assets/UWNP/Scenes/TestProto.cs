using ProtoBuf;

[ProtoContract]
public class TestRq
{
    [ProtoMember(1)]
    public uint packageType;
}

[ProtoContract]
public class TestRp
{
    [ProtoMember(1)]
    public uint packageType;
}

[ProtoContract]
public class TestRp2
{
    [ProtoMember(1)]
    public string info;
}

[ProtoContract]
public class TestNotify
{
    [ProtoMember(1)]
    public string name;
}

[ProtoContract]
public class TestPush
{
    [ProtoMember(1)]
    public string info;
}