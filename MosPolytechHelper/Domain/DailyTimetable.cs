namespace MosPolytechHelper.Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class DailyTimetable : IEnumerable<Lesson>
    {
        Lesson[] timetable { get; set; }

        public int Count => this.timetable.Length;

        public DailyTimetable(Lesson[] timetable)
        {
            this.timetable = timetable;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DailyTimetable otherTimetable))
                return false;
            if (this.timetable.Length != otherTimetable.timetable.Length)
                return false;
            for (int i = 0; i < this.timetable.Length; i++)
            {
                if (!this.timetable[i].Equals(otherTimetable.timetable[i]))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            string hashCode = string.Empty;
            foreach (var lesson in this.timetable)
            {
                hashCode += lesson.SubjectName;
            }

            return hashCode.GetHashCode();
        }

        IEnumerator<Lesson> IEnumerable<Lesson>.GetEnumerator() =>
            ((IEnumerable<Lesson>)this.timetable).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            this.timetable.GetEnumerator();

        public Lesson this[int index]
        {
            get => this.timetable[index];
            set => this.timetable[index] = value;
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