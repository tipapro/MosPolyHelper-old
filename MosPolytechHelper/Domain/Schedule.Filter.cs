namespace MosPolytechHelper.Domain
{
    using System;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        public class Filter
        {
            Module DetermineModule(Schedule.Daily dailySchedule, DateTime date)
            {
                (int Currently, int Outdated) firstModuleCounter = (0, 0), secondModuleCounter = (0, 0);
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
                            firstModuleCounter.Currently++;
                        }
                        else
                        {
                            secondModuleCounter.Currently++;
                        }
                    }
                    else
                    {
                        if (lesson.Module == Module.First)
                        {
                            firstModuleCounter.Outdated++;
                        }
                        else
                        {
                            secondModuleCounter.Outdated++;
                        }
                    }
                }
                if (secondModuleCounter.Currently == 0)
                {
                    if (firstModuleCounter.Currently != 0 || secondModuleCounter.Outdated != 0)
                    {
                        return Module.First;
                    }
                }
                if (firstModuleCounter.Currently == 0)
                {
                    if (secondModuleCounter.Currently != 0 || firstModuleCounter.Outdated != 0)
                    {
                        return Module.Second;
                    }
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
                        if (date < lesson.DateFrom || date > lesson.DateTo)
                        {
                            continue;
                        }
                    }
                    if (this.ModuleFilter != ModuleFilter.Off)
                    {
                        if (lesson.Module != Module.None &&
                            ((this.ModuleFilter == ModuleFilter.First && lesson.Module != Module.First) ||
                            (this.ModuleFilter == ModuleFilter.Second && lesson.Module != Module.Second) ||
                            (this.ModuleFilter == ModuleFilter.Auto && currModule != Module.None &&
                            lesson.Module != currModule)))   // Auto
                        {
                            continue;
                        }
                    }

                    if (this.SessionFilter)
                    {
                        if (lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) ||
                            lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) ||
                            lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase))
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