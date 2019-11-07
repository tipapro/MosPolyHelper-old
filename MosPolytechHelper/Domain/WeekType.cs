namespace MosPolyHelper.Domain
{
    using ProtoBuf;

    [ProtoContract]
    public enum WeekType
    {
        None = 0,
        Odd = 1,
        Even = 2
    }
}