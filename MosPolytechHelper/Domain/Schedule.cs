namespace MosPolyHelper.Domain
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [ProtoContract(IgnoreListHandling = true)]
    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        [ProtoMember(1)]
        Daily[] dailyShedules { get; set; }

        [ProtoIgnore]
        public DateTime LastUpdate { get; set; }
        [ProtoMember(2)]
        public Group Group { get; set; }
        [ProtoMember(3)]
        public bool IsByDate { get; set; }
        [ProtoIgnore]
        public bool IsSession { get; set; }
        [ProtoIgnore]
        public int Count => this.dailyShedules?.Length ?? 0;
        [ProtoIgnore]
        public Schedule.Filter ScheduleFilter { get; set; }

        public Schedule()
        {
            this.Group = new Group();
        }

        public Schedule(Schedule.Daily[] schedule, Group group, bool isByDate, DateTime lastUpdate)
        {
            this.dailyShedules = schedule;
            this.Group = group;
            this.IsByDate = isByDate;
            this.LastUpdate = lastUpdate;
        }

        Daily this[long day]
        {
            get
            {
                foreach (var dailySchedule in this.dailyShedules)
                {
                    if (dailySchedule.Day == day)
                    {
                        return dailySchedule;
                    }
                }
                return null;
            }
        }

        IEnumerator<Daily> IEnumerable<Daily>.GetEnumerator()
        {
            foreach (var dailySchedule in this.dailyShedules)
            {
                yield return dailySchedule;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() =>
            this.dailyShedules.GetEnumerator();

        public Daily GetSchedule(int position)
        {
            return this.dailyShedules[position];
        }
        public Daily GetSchedule(DateTime date)
        {
            date = date.Date;
            if (this.IsByDate)
            {
                return this[date.Ticks];
            }
            else
            {
                return this.ScheduleFilter.GetFilteredSchedule(this[(long)date.DayOfWeek], date);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Schedule sch2))
            {
                return false;
            }
            if (this.dailyShedules.Length != sch2.dailyShedules.Length)
            {
                return false;
            }
            for (int i = 0; i < this.dailyShedules.Length; i++)
            {
                if (!this.dailyShedules[i].Equals(sch2.dailyShedules[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            string hash = "";
            for (int i = 0; i < this.dailyShedules.Length; i++)
            {
                hash += this.dailyShedules[i].GetHashCode();
            }
            return hash.GetHashCode();
        }
    }
}