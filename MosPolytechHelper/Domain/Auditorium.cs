namespace MosPolyHelper.Domain
{
    using ProtoBuf;

    [ProtoContract]
    public class Auditorium
    {
        Auditorium()
        {
        }

        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Color { get; set; }

        public Auditorium(string name, string color)
        {
            this.Name = name;
            this.Color = color;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Auditorium aud2))
            {
                return false;
            }
            return this.Name == aud2.Name || this.Color == aud2.Color;
        }

        public override int GetHashCode()
        {
            return (this.Name + this.Color).GetHashCode();
        }
    }
}