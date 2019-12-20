namespace MosPolyHelper.Domain
{
    using ProtoBuf;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        [ProtoContract(IgnoreListHandling = true)]
        public class Daily// : IEnumerable<Lesson>
        {
            [ProtoMember(1)]
            Lesson[] lessons;

            Daily()
            {
            }

            [ProtoMember(2)]
            public long Day { get; set; }
            [ProtoIgnore]
            public int Count => this.lessons.Length;

            public Daily(Lesson[] lessons, long day)
            {
                this.lessons = lessons;
                this.Day = day;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Daily dailySch2))
                {
                    return false;
                }
                if (this.Day != dailySch2.Day)
                {
                    return false;
                }
                if (this.lessons.Length != dailySch2.lessons.Length)
                {
                    return false;
                }
                for (int i = 0; i < this.lessons.Length; i++)
                {
                    if (!this.lessons[i].Equals(dailySch2.lessons[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            public override int GetHashCode()
            {
                string hashCode = string.Empty;
                foreach (var lesson in this.lessons)
                {
                    hashCode += lesson.GetHashCode();
                }
                return (hashCode + this.Day).GetHashCode();
            }

            public Lesson GetLesson(int position)
            {
                return this[position];
            }

            public IEnumerator<Lesson> GetEnumerator()
            {
                foreach (var lesson in this.lessons)
                {
                    yield return lesson;
                }
            }

            public Lesson this[int index]
            {
                get => this.lessons[index];
                set => this.lessons[index] = value;
            }
        }
    }
}