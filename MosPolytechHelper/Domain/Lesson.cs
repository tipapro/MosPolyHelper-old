namespace MosPolyHelper.Domain
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;

    [ProtoContract]
    public class Lesson
    {
        readonly static (TimeSpan StartTime, TimeSpan EndTime) FirstPair = (new TimeSpan(9, 0, 0), new TimeSpan(10, 30, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) SecondPair = (new TimeSpan(10, 40, 0), new TimeSpan(12, 10, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) ThirdPair = (new TimeSpan(12, 20, 0), new TimeSpan(13, 50, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) FourthPair = (new TimeSpan(14, 30, 0), new TimeSpan(16, 00, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) FifthPair = (new TimeSpan(16, 10, 0), new TimeSpan(17, 40, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) SixthPair = (new TimeSpan(17, 50, 0), new TimeSpan(19, 20, 0));
        //readonly static (TimeSpan StartTime, TimeSpan EndTime) SeventhPair = (new TimeSpan(19, 30, 0), new TimeSpan(21, 00, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) SixthPairM = (new TimeSpan(18, 20, 0), new TimeSpan(19, 40, 0));
        //readonly static (TimeSpan StartTime, TimeSpan EndTime) SeventhPairM = (new TimeSpan(19, 50, 0), new TimeSpan(21, 10, 0));
        readonly static (TimeSpan StartTime, TimeSpan EndTime) SixthPairE = (new TimeSpan(18, 30, 0), new TimeSpan(20, 00, 0));
        //readonly static (TimeSpan StartTime, TimeSpan EndTime) SeventhPairE = (new TimeSpan(20, 10, 0), new TimeSpan(21, 40, 0));

        Lesson() { }

        bool CheckTeachersEquals(Teacher[] teachers)
        {
            if (this.Teachers == teachers)
            {
                return true;
            }
            else if (this.Teachers == null || teachers == null)
            {
                return false;
            }
            if (this.Teachers.Length != teachers.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Teachers.Length; i++)
            {
                if (this.Teachers[i] == teachers[i])
                {
                    return true;
                }
                else if (this.Teachers[i] == null || teachers[i] == null)
                {
                    return false;
                }
                if (this.Teachers[i].Name.Length != teachers[i].Name.Length)
                {
                    return false;
                }
                for (int j = 0; j < this.Teachers[i].Name.Length; j++)
                {
                    if (this.Teachers[i].Name[j] != teachers[i].Name[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        bool CheckAuditoriumsEquals(params Auditorium[] auditoriums)
        {
            if (this.Auditoriums == auditoriums)
            {
                return true;
            }
            else if (this.Auditoriums == null || auditoriums == null)
            {
                return false;
            }
            if (this.Auditoriums.Length != auditoriums.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Auditoriums.Length; i++)
            {
                if (this.Auditoriums[i].Equals(auditoriums[i]))
                {
                    return false;
                }
            }
            return true;
        }

        [ProtoMember(1)]
        public int Order { get; set; }
        [ProtoMember(2)]
        public string Title { get; set; }
        [ProtoMember(3)]
        public Teacher[] Teachers { get; set; }
        [ProtoMember(4)]
        public DateTime DateFrom { get; set; }
        [ProtoMember(5)]
        public DateTime DateTo { get; set; }
        [ProtoMember(6)]
        public Auditorium[] Auditoriums { get; set; }
        [ProtoMember(7)]
        public string Type { get; set; }
        [ProtoMember(8)]
        public WeekType Week { get; set; }
        [ProtoMember(9)]
        public Module Module { get; set; }
        [ProtoIgnore]
        // For advanced search
        public Group Group { get; set; }

        public Lesson(int order, string subjectTitle, string[] teachers, DateTime dateFrom, DateTime dateTo,
            Auditorium[] auditoriums, string type, WeekType week, Module module)
        {
            this.Order = order;
            this.Title = subjectTitle;
            this.Teachers = new Teacher[teachers.Length];
            for (int i = 0; i < teachers.Length; i++)
            {
                if (teachers[i] == "tipapro")
                {
                    this.Teachers[i] = new Teacher(new string[] { " " });
                    continue;
                }
                this.Teachers[i] = new Teacher(teachers[i].Replace(" - ", "-").Replace(" -", "-").Replace("- ", "-")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.Auditoriums = auditoriums;
            this.Type = type;
            this.Week = week;
            this.Module = module;
        }

        public IEnumerable<string> GetAuditoriumNames()
        {
            foreach (var au in this.Auditoriums)
                yield return au.Name;
        }

        public IEnumerable<string> GetFullTecherNames()
        {
            if (this.Teachers == null)
            {
                yield return string.Empty;
            }
            else
            {
                foreach (var teacher in this.Teachers)
                {
                    yield return teacher.GetFullName();
                }
            }
        }

        public IEnumerable<string> GetShortTeacherNames()
        {
            if (this.Teachers == null)
            {
                yield return "";
            }
            else
            {
                foreach (var teacher in this.Teachers)
                {
                    yield return teacher.GetShortName();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Lesson lesson2))
            {
                return false;
            }
            return this.Order == lesson2.Order && this.Title == lesson2.Title
                && CheckTeachersEquals(lesson2.Teachers) && this.DateFrom == lesson2.DateFrom
                && this.DateTo == lesson2.DateTo && CheckAuditoriumsEquals(this.Auditoriums)
                && this.Type == lesson2.Type && this.Week == lesson2.Week && this.Module == lesson2.Module;
        }

        public override int GetHashCode()
        {
            return (this.Order + this.Title + GetFullTecherNames() + this.DateFrom +
                this.DateTo + string.Concat(GetAuditoriumNames()) + this.Type + this.Week + this.Module).GetHashCode();
        }

        public static (string StartTime, string EndTime) GetLessonTime(DateTime date,
                int lessonPosition, bool groupIsEvening, DateTime groupDateFrom)
        {
            switch (lessonPosition)
            {
                case 0:
                    return ("09:00", "10:30");
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
                        if (groupDateFrom >= new DateTime(date.Year, 1, 22))
                        {
                            return ("18:30", "20:00");
                        }
                        else
                        {
                            return ("18:20", "19:40");
                        }
                    }
                    else
                    {
                        return ("17:50", "19:20");
                    }

                case 6:
                    if (groupIsEvening)
                    {
                        if (groupDateFrom >= new DateTime(date.Year, 1, 22))
                        {
                            return ("20:10", "21:40");
                        }
                        else
                        {
                            return ("19:50", "21:10");
                        }
                    }
                    else
                    {
                        return ("19:30", "21:00");
                    }

                default:
                    //this.logger.Warn("Suspicious behavior: Unplanned lesson number {num}. " +
                    //"Additional data: {groupIsEvening}, {groupDateFrom}", lessonPosition, groupIsEvening, groupDateFrom);
                    return (string.Empty, string.Empty);
            }
        }

        public static int GetCurrentLessonOrder(Schedule.Daily dailiSchedule, TimeSpan time, DateTime date, bool groupIsEvening, 
            DateTime groupDateFrom)
        {
            if (dailiSchedule == null)
            {
                return -1;
            }
            // 0, 1, 2 | 3, 4, 5, 6
            if (time > ThirdPair.EndTime)
            {
                if (time <= FourthPair.EndTime)
                {
                    return 3;
                }
                else if (time <= FifthPair.EndTime)
                {
                    return 4;
                }
                else if (groupIsEvening)
                {
                    if (groupDateFrom >= new DateTime(date.Year, 1, 22))
                    {
                        if (time <= SixthPairE.EndTime)
                        {
                            return 5;
                        }
                        else
                        {
                            return 6;
                        }
                    }
                    else
                    {
                        if (time <= SixthPairM.EndTime)
                        {
                            return 5;
                        }
                        else
                        {
                            return 6;
                        }
                    }
                }
                else
                {
                    if (time <= SixthPair.EndTime)
                    {
                        return 5;
                    }
                    else
                    {
                        return 6;
                    }
                }
            }
            else
            {
                if (time > SecondPair.EndTime)
                {
                    return 2;
                }
                else if (time > FirstPair.EndTime)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}