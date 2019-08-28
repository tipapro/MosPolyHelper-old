namespace MosPolytechHelper.Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class FullTimetable : IEnumerable<KeyValuePair<string, DailyTimetable>>
    {
        Dictionary<string, DailyTimetable> timetable { get; set; }

        public Group Group { get; set; }
        public bool IsSession { get; set; }
        public bool IsFirstWeekEven { get; set; }

        public FullTimetable()
        {
            this.Group = new Group();
            this.timetable = new Dictionary<string, DailyTimetable>();
        }

        public FullTimetable(Dictionary<string, DailyTimetable> timetable, Group group, bool isSession)
        {
            this.Group = group;
            this.timetable = timetable;
        }

        public DailyTimetable this[string index]
        {
            get => timetable[index];
            set => timetable[index] = value;
        }

        IEnumerator<KeyValuePair<string, DailyTimetable>> IEnumerable<KeyValuePair<string, DailyTimetable>>.GetEnumerator() => 
            this.timetable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            this.timetable.GetEnumerator();

        public DailyTimetable GetTimetable(DateTime date)
        {
            if (IsSession)
            {
                foreach (var (sessionDate, dailyTimetable) in this.timetable)
                {
                    // TODO: try - catch
                    if (DateTime.Parse(sessionDate) == date.Date)
                        return dailyTimetable;
                }
                return null;
            }
            else
            {
                string index = (date.DayOfWeek == DayOfWeek.Sunday ?
                    7 : (int)date.DayOfWeek).ToString();
                if (this.timetable.ContainsKey(index))
                    return this.timetable[index];
                else
                    return null;
            }
        }
    }
}