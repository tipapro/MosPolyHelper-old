namespace MosPolytechHelper.Domain
{
    using ProtoBuf;

    [ProtoContract]
    public enum WeekType
    {
        Odd,
        Even,
        None
    }
}