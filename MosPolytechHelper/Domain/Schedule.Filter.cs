namespace MosPolytechHelper.Domain
{
    using System;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<KeyValuePair<string, Schedule.Daily>>
    {
        public class Filter
        {
            static Filter empty;


            Module DetermineModule(DateTime date, DateTime firstModuleLastDate, DateTime secondModuleEarlyDate)
            {
                if (firstModuleLastDate == DateTime.MinValue && secondModuleEarlyDate == DateTime.MaxValue)
                {
                    return Module.None;
                }
                if (date <= firstModuleLastDate)
                {
                    return Module.First;
                }
                if (date >= secondModuleEarlyDate)
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
                int firstDayYear = date.Year - FirstDay / date.DayOfYear;
                var firstDayDate = new DateTime(firstDayYear, 8, 1);
                var timeSpan = (GetFirstWeekDay(firstDayDate) - GetFirstWeekDay(date)).Days;
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

            public static Filter Empty
            {
                get
                {
                    if (empty == null)
                    {
                        empty = new Filter(DateFilter.Show, ModuleFilter.Off, WeekFilter.Off, false, WeekType.None);
                    }
                    return empty;
                }
            }

            public DateFilter DateFitler { get; set; }
            public ModuleFilter ModuleFilter { get; set; }
            public WeekFilter WeekFilter { get; set; }
            public WeekType FirstWeekType { get; set; }
            public bool SessionFilter { get; set; }

            public Schedule.Daily GetFilteredSchedule(Schedule.Daily dailySchedule, DateTime date,
                DateTime firstModuleLastDate, DateTime secondModuleEarlyDate)
            {
                var lessonList = new List<Lesson>(dailySchedule.Count);
                var currModule = DetermineModule(date, firstModuleLastDate, secondModuleEarlyDate);
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
                return new Schedule.Daily(lessonList.ToArray());
            }

            public Filter(DateFilter dateFilter, ModuleFilter moduleFilter, WeekFilter weekFilter, bool sessionFilter, WeekType firstWeekType)
            {
                this.DateFitler = dateFilter;
                this.ModuleFilter = moduleFilter;
                this.WeekFilter = weekFilter;
                this.SessionFilter = sessionFilter;
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