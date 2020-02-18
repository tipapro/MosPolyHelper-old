namespace MosPolyHelper.Domains.ScheduleDomain
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [ProtoContract(IgnoreListHandling = true)]
    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        [ProtoMember(1)]
        Daily[] dailyShedules { get; set; } // get and set for correct work of protofub

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
        [ProtoMember(4)]
        public string Version { get; set; }
        [ProtoMember(5)]
        public DateTime From { get; set; }
        [ProtoMember(6)]
        public DateTime To { get; set; }

        public Schedule()
        {
        }

        public Schedule(Schedule.Daily[] schedule, Group group, bool isByDate, DateTime lastUpdate, string version)
        {
            this.dailyShedules = schedule;
            this.Group = group;
            this.IsByDate = isByDate;
            this.LastUpdate = lastUpdate;
            this.Version = version;
            (this.From, this.To) = GetScheduduleBorders();
        }

        public Schedule(Schedule.Daily[] schedule, Group group, bool isByDate, DateTime lastUpdate, string version,
            DateTime from, DateTime to)
        {
            this.dailyShedules = schedule;
            this.Group = group;
            this.IsByDate = isByDate;
            this.LastUpdate = lastUpdate;
            this.Version = version;
            this.From = from;
            this.To = to;
        }

        public void SetUpGroup()
        {
            foreach (var dailySchedule in this.dailyShedules)
            {
                foreach (var lesson in dailySchedule)
                {
                    lesson.Group = this.Group;
                }
            }
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

        (DateTime From, DateTime To) GetScheduduleBorders()
        {
            var from = DateTime.MaxValue;
            var to = DateTime.MinValue;
            foreach (var dailySchedule in this.dailyShedules)
            {
                foreach (var lesson in dailySchedule)
                {
                    if (lesson.DateFrom < from)
                    {
                        from = lesson.DateFrom;
                    }
                    if (lesson.DateTo > to)
                    {
                        to = lesson.DateTo;
                    }
                }
            }
            return (from, to);
        }

        public Daily GetSchedule(int position)
        {
            return this.dailyShedules[position];
        }

        public Daily GetSchedule(DateTime date, Schedule.Filter ScheduleFilter)
        {
            date = date.Date;
            if (this.IsByDate)
            {
                return this[date.Ticks];
            }
            else
            {
                return ScheduleFilter.GetFilteredSchedule(this[(long)date.DayOfWeek], date);
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

        public void ToNormal()
        {
            if (this.IsByDate)
            {
                foreach (var dailySchedule in dailyShedules)
                {
                    var date = new DateTime(dailySchedule.Day);
                    dailySchedule.Day = (int)date.DayOfWeek;
                    foreach (var lesson in dailySchedule)
                    {
                        lesson.DateFrom = lesson.DateTo = date;
                    }
                }
                this.IsByDate = false;
            }
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