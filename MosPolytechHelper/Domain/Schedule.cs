namespace MosPolytechHelper.Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<KeyValuePair<string, Schedule.Daily>>
    {
        Dictionary<string, Daily> schedule { get; set; }

        public DateTime FirstModuleLastDate;
        public DateTime SecondModuleEarlyDate;

        public Group Group { get; set; }
        public bool IsSession { get; set; }
        public Schedule.Filter ScheduleFilter { get; set; }
        public Schedule()
        {
            this.Group = new Group();
            this.schedule = new Dictionary<string, Daily>();
        }

        public Schedule(Dictionary<string, Daily> schedule, Group group, bool isSession,
            DateTime firstModuleLastDate, DateTime secondModuleEarlytDate)
        {
            this.Group = group;
            this.schedule = schedule;
            this.FirstModuleLastDate = firstModuleLastDate;
            this.SecondModuleEarlyDate = secondModuleEarlytDate;
        }

        public Daily this[string index]
        {
            get => this.schedule[index];
            set => this.schedule[index] = value;
        }

        IEnumerator<KeyValuePair<string, Daily>> IEnumerable<KeyValuePair<string, Daily>>.GetEnumerator() =>
            this.schedule.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            this.schedule.GetEnumerator();

        public Daily GetSchedule(DateTime date)
        {
            if (this.IsSession)
            {
                foreach (var (sessionDate, dailySchedule) in this.schedule)
                {
                    // TODO: try - catch
                    if (DateTime.Parse(sessionDate) == date.Date)
                    {
                        return this.ScheduleFilter.GetFilteredSchedule(dailySchedule, date,
                            this.FirstModuleLastDate, this.SecondModuleEarlyDate);
                    }
                }
                return null;
            }
            else
            {
                string index = (date.DayOfWeek == DayOfWeek.Sunday ?
                    7 : (int)date.DayOfWeek).ToString();
                if (this.schedule.ContainsKey(index))
                {
                    return this.ScheduleFilter.GetFilteredSchedule(this.schedule[index], date,
                        this.FirstModuleLastDate, this.SecondModuleEarlyDate);
                }
                return null;
            }
        }
    }
}