namespace MosPolyHelper.Domain
{
    using System;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        public class Filter
        {
            Module? DetermineModule(Schedule.Daily dailySchedule, DateTime date)
            {
                (bool Currently, bool NotStarted) firstModuleCounter = (false, false),
                    secondModuleCounter = (false, false);
                foreach (var lesson in dailySchedule)
                {
                    if (lesson.Module == Module.None)
                    {
                        continue;
                    }
                    if (date >= lesson.DateFrom && date <= lesson.DateTo)
                    {
                        if (lesson.Module == Module.First)
                        {
                            firstModuleCounter.Currently = true;
                        }
                        else
                        {
                            secondModuleCounter.Currently = true;
                        }
                    }
                    else if (date < lesson.DateFrom)
                    {
                        if (lesson.Module == Module.First)
                        {
                            firstModuleCounter.NotStarted = true;
                        }
                        else
                        {
                            secondModuleCounter.NotStarted = true;
                        }
                    }
                }

                if (firstModuleCounter.Currently && secondModuleCounter.Currently)
                {
                    return null;
                }
                if (firstModuleCounter.Currently)
                {
                    return Module.First;
                }
                if (secondModuleCounter.Currently)
                {
                    return Module.Second;
                }
                if (firstModuleCounter.NotStarted && secondModuleCounter.NotStarted)
                {
                    return null;
                }
                if (firstModuleCounter.NotStarted)
                {
                    return Module.First;
                }
                if (secondModuleCounter.NotStarted)
                {
                    return Module.Second;
                }
                return Module.None;
            }

            WeekType DetermineWeekType(DateTime date)
            {
                if (this.FirstWeekType == WeekType.None)
                {
                    return WeekType.None;
                }
                const int FirstDay = 213;   // 1st August (or 31st July for leap year) 
                // FirstDay / date.DayOfYear == 1 if FirstDay > date.DayOfYear and 0 if FirstDay < date.DayOfYear
                int firstDayYear = date.Year - FirstDay / date.DayOfYear;
                var firstDayDate = new DateTime(firstDayYear, 8, 1);
                int timeSpan = (GetFirstWeekDay(firstDayDate) - GetFirstWeekDay(date)).Days;
                if ((timeSpan % 2 == 0) == (this.FirstWeekType == WeekType.Even))
                {
                    return WeekType.Even;
                }
                else
                {
                    return WeekType.Odd;
                }
            }

            DateTime GetFirstWeekDay(DateTime date)
            {
                var dayOfWeek = date.DayOfWeek;
                // Separately because DayOfWeek.Sunday == 0
                if (dayOfWeek == DayOfWeek.Sunday)
                    return date.AddDays((int)DayOfWeek.Monday - 7);
                return date.AddDays((int)DayOfWeek.Monday - (int)dayOfWeek);
            }

            public static Filter DefaultFilter =>
                new Filter(DateFilter.Show, ModuleFilter.Off, WeekFilter.Off, false, WeekType.None);

            public DateFilter DateFitler { get; set; }
            public ModuleFilter ModuleFilter { get; set; }
            public WeekFilter WeekFilter { get; set; }
            public WeekType FirstWeekType { get; set; }
            public bool SessionFilter { get; set; }

            public Filter() : this(DateFilter.Show, ModuleFilter.Off, WeekFilter.Off, false, WeekType.None)
            {
            }

            public Filter(DateFilter dateFilter, ModuleFilter moduleFilter, WeekFilter weekFilter,
                bool sessionFilter, WeekType firstWeekType)
            {
                this.DateFitler = dateFilter;
                this.ModuleFilter = moduleFilter;
                this.WeekFilter = weekFilter;
                this.SessionFilter = sessionFilter;
                this.FirstWeekType = firstWeekType;
            }

            public Schedule.Daily GetFilteredSchedule(Schedule.Daily dailySchedule, DateTime date)
            {
                if (dailySchedule == null)
                {
                    return null;
                }
                var lessonList = new List<Lesson>(dailySchedule.Count);
                var currModule = DetermineModule(dailySchedule, date);
                var currWeek = DetermineWeekType(date);
                foreach (var lesson in dailySchedule)
                {
                    if (this.DateFitler == DateFilter.Hide)
                    {
                        if (date > lesson.DateTo)
                        {
                            continue;
                        }
                        if (date < lesson.DateFrom &&
                            !lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) &&
                            !lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) &&
                            !lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                    if (this.ModuleFilter != ModuleFilter.Off)
                    {
                        if (lesson.Module != Module.None &&
                            ((this.ModuleFilter == ModuleFilter.First && lesson.Module != Module.First) ||
                            (this.ModuleFilter == ModuleFilter.Second && lesson.Module != Module.Second) ||
                            (this.ModuleFilter == ModuleFilter.Auto && currModule != null && lesson.Module != currModule)))
                        {
                            continue;
                        }
                    }

                    if (this.SessionFilter)
                    {
                        if ((date < lesson.DateFrom || date > lesson.DateTo)
                            && (lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) ||
                            lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) ||
                            lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                    }

                    if (this.WeekFilter != WeekFilter.Off)
                    {
                        if (lesson.Week != WeekType.None &&
                            ((this.WeekFilter == WeekFilter.Odd && lesson.Week != WeekType.Odd) ||
                            (this.WeekFilter == WeekFilter.Even && lesson.Week != WeekType.Even) ||
                            (this.WeekFilter == WeekFilter.Auto && currWeek == WeekType.None &&
                            lesson.Week != currWeek)))
                        {
                            continue;
                        }
                    }

                    lessonList.Add(lesson);
                }
                return new Schedule.Daily(lessonList.ToArray(), dailySchedule.Day);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Schedule.Filter filter2))
                {
                    return false;
                }
                return this.DateFitler == filter2.DateFitler && this.ModuleFilter == filter2.ModuleFilter
                    && this.SessionFilter == filter2.SessionFilter && this.WeekFilter == filter2.WeekFilter
                    && this.FirstWeekType == filter2.FirstWeekType;
            }

            public override int GetHashCode()
            {
                string hash = string.Empty;
                hash += this.DateFitler.GetHashCode() + this.FirstWeekType.GetHashCode() +
                    this.ModuleFilter.GetHashCode() + this.SessionFilter.GetHashCode() + this.WeekFilter.GetHashCode();
                return hash.GetHashCode();
            }
        }
    }
    public enum ModuleFilter
    {
        Off,
        First,
        Second,
        Auto
    }

    public enum WeekFilter
    {
        Off,
        Odd,
        Even,
        Auto
    }

    public enum DateFilter
    {
        Show,
        Desaturate,
        Hide
    }
}