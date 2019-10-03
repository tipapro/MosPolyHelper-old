namespace MosPolytechHelper.Domain
{
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [JsonObject]
    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        [JsonProperty]
        Schedule.Daily[] schedule { get; set; }

        [JsonIgnore]
        public DateTime LastUpdate { get; set; }
        [JsonProperty]
        public Group Group { get; set; }
        [JsonProperty]
        public bool IsSession { get; set; }
        [JsonIgnore]
        public int Count => this.schedule?.Length ?? 0;
        [JsonIgnore]
        public Schedule.Filter ScheduleFilter { get; set; }
        public Schedule()
        {
            this.Group = new Group();
        }

        public Schedule(Schedule.Daily[] schedule, Group group, bool isSession, DateTime lastUpdate)
        {
            this.schedule = schedule;
            this.Group = group;
            this.IsSession = isSession;
            this.LastUpdate = lastUpdate;
        }

        Daily this[long day]
        {
            get
            {
                foreach (var dailySchedule in this.schedule)
                {
                    if (dailySchedule.Day == day)
                    {
                        return dailySchedule;
                    }
                }
                return null;
            }
        }

        IEnumerator<Daily> IEnumerable<Daily>.GetEnumerator() =>
            this.schedule.GetEnumerator() as IEnumerator<Daily>;
        IEnumerator IEnumerable.GetEnumerator() =>
            this.schedule.GetEnumerator();

        public Daily GetSchedule(int position)
        {
            return this.schedule[position];
        }
        public Daily GetSchedule(DateTime date)
        {
            if (this.IsSession)
            {
                return this[date.ToBinary()];
            }
            else
            {        
                return ScheduleFilter.GetFilteredSchedule(this[(long)date.DayOfWeek], date);
            }
        }
    }
}