namespace MosPolyHelper.Domains.ScheduleDomain
{
    using System;
    using System.Collections.Generic;

    public partial class Schedule : IEnumerable<Schedule.Daily>
    {
        public class AdvancedSerach
        {
            public Schedule Filter(IList<Schedule> schedules, IList<string> subjectTitles,
                IList<string> subjectTypes, IList<string> auditoriums, IList<string> teachers)
            {
                var dayList = new List<Lesson>[7] { new List<Lesson>(), new List<Lesson>(), new List<Lesson>(),
                    new List<Lesson>(), new List<Lesson>(), new List<Lesson>(), new List<Lesson>() };
                var from = DateTime.MaxValue;
                var to = DateTime.MinValue;
                foreach (var schedule in schedules)
                {
                    if (schedule.From < from)
                    {
                        from = schedule.From;
                    }
                    if (schedule.To > to)
                    {
                        to = schedule.To;
                    }
                    foreach (var dailySchedule in schedule)
                    {
                        foreach (var lesson in dailySchedule)
                        {
                            if ((subjectTitles.Count != 0 && !subjectTitles.Contains(lesson.Title)) ||
                                (subjectTypes.Count != 0 && !subjectTypes.Contains(lesson.Type)))
                            {
                                continue;
                            }

                            bool auditoriumFlag = true;
                            foreach (var auditorium in lesson.Auditoriums)
                            {
                                if (auditoriums.Count == 0 || auditoriums.Contains(auditorium.Name))
                                {
                                    auditoriumFlag = false;
                                    break;
                                }
                            }
                            if (auditoriumFlag)
                            {
                                continue;
                            }
                            bool teacherFlag = true;
                            foreach (var teacher in lesson.Teachers)
                            {
                                if (teachers.Count == 0 || teachers.Contains(teacher.GetFullName()))
                                {
                                    teacherFlag = false;
                                    break;
                                }
                            }
                            if (teacherFlag)
                            {
                                continue;
                            }
                            dayList[dailySchedule.Day].Add(lesson);
                        }
                    }
                }
                var dailySchedules = new Schedule.Daily[7];
                for (int i = 0; i < 7; i++)
                {
                    dayList[i].Sort();
                    dailySchedules[i] = new Schedule.Daily(dayList[i].ToArray(), i);
                }
                return new Schedule(dailySchedules, null, false, DateTime.Now, Schedule.RequiredVersion, from, to);
            }
        }
    }
}