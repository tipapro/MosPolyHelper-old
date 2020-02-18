namespace MosPolyHelper.Domains.ScheduleDomain
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