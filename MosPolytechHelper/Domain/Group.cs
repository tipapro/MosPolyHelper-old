namespace MosPolyHelper.Domain
{
    using ProtoBuf;
    using System;

    [ProtoContract]
    public class Group
    {
        [ProtoMember(1)]
        public string Title { get; set; }
        [ProtoMember(2)]
        public DateTime DateFrom { get; set; }
        [ProtoMember(3)]
        public DateTime DateTo { get; set; }
        [ProtoMember(4)]
        public bool IsEvening { get; set; }
        [ProtoMember(5)]
        public string Comment { get; set; }

        public Group() { }

        public Group(string title, DateTime dateFrom, DateTime dateTo, bool isEvening, string comment)
        {
            this.Title = title;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.IsEvening = isEvening;
            this.Comment = comment;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Group group2))
            {
                return false;
            }
            return this.Title == group2.Title || this.DateFrom == group2.DateFrom
                || this.DateTo == group2.DateTo || this.IsEvening == group2.IsEvening
                || this.Comment == group2.Comment;
        }

        public override int GetHashCode()
        {
            return (this.Title + this.DateFrom + this.DateTo + this.IsEvening + this.Comment).GetHashCode();
        }
    }
}