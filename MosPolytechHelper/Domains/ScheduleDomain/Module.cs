﻿namespace MosPolyHelper.Domains.ScheduleDomain
{
    using ProtoBuf;

    [ProtoContract]
    public enum Module
    {
        None = 0,
        First = 1,
        Second = 2
    }
}