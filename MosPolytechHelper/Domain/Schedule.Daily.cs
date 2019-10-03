namespace MosPolytechHelper.Domain
{
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        [JsonObject]
        public class Daily : IEnumerable<Lesson>
        {
            [JsonProperty]
            Lesson[] schedule { get; set; }

            [JsonProperty]
            public long Day { get; set; }
            [JsonIgnore]
            public int Count => this.schedule.Length;

            public Daily(Lesson[] schedule, long day)
            {
                this.schedule = schedule;
                this.Day = day;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Daily otherSchedule))
                    return false;
                if (this.schedule.Length != otherSchedule.schedule.Length)
                    return false;
                for (int i = 0; i < this.schedule.Length; i++)
                {
                    if (!this.schedule[i].Equals(otherSchedule.schedule[i]))
                        return false;
                }
                return true;
            }
            public override int GetHashCode()
            {
                string hashCode = string.Empty;
                foreach (var lesson in this.schedule)
                {
                    hashCode += lesson.SubjectName;
                }

                return hashCode.GetHashCode();
            }

            IEnumerator<Lesson> IEnumerable<Lesson>.GetEnumerator() =>
                ((IEnumerable<Lesson>)this.schedule).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() =>
                this.schedule.GetEnumerator();

            public Lesson this[int index]
            {
                get => this.schedule[index];
                set => this.schedule[index] = value;
            }

            public static (string, string) GetLessonTime(int lessonPosition, bool groupIsEvening, DateTime groupDateFrom)
            {
                switch (lessonPosition)
                {
                    case 0:
                        return ("9:00", "10:30");
                    case 1:
                        return ("10:40", "12:10");
                    case 2:
                        return ("12:20", "13:50");
                    case 3:
                        return ("14:30", "16:00");
                    case 4:
                        return ("16:10", "17:40");
                    case 5:
                        if (groupIsEvening)
                        {
                            // TODO: Fix for evening 1
                            if (groupDateFrom >= new DateTime(2018, 1, 22))
                                return ("18:30", "20:00"); // TODO: 2018 year!!! Fix
                            return ("18:20", "19:40");
                        }
                        else
                        {
                            return ("17:50", "19:20");
                        }

                    case 6:
                        if (groupIsEvening)
                        {
                            // TODO: Fix for evening 2
                            if (groupDateFrom >= new DateTime(2018, 1, 22))
                                return ("20:10", "21:40");  // TODO: 2018 year!!! Fix
                            return ("19:50", "21:10");
                        }
                        else
                        {
                            return ("19:30", "21:00");
                        }

                    default:
                        //this.logger.Warn("Suspicious behavior: Unplanned lesson number {num}. " +
                        //"Additional data: {groupIsEvening}, {groupDateFrom}", lessonPosition, groupIsEvening, groupDateFrom);
                        return (null, null);
                }
            }
        }
    }
}