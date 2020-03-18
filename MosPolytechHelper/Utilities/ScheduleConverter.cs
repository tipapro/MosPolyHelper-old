namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Utilities.Interfaces;
    using MosPolyHelper.Domains.ScheduleDomain;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    class ScheduleConverter : IScheduleConverter
    {
        #region Constants
        const string GroupListKey = "groups";

        const string StatusKey = "status";
        const string MessageKey = "message";
        const string IsSession = "isSession";
        const string GroupInfoKey = "group";
        const string ScheduleGridKey = "grid";

        const string GroupTitleKey = "title";
        const string GroupDateFromKey = "dateFrom";
        const string GroupDateToKey = "dateTo";
        const string GroupEveningKey = "evening";
        const string GroupCommentKey = "comment";
        const string GroupCourseKey = "course";

        const string LessonSubjectKey = "sbj";
        const string LessonTeacherKey = "teacher";
        const string LessonDateFromKey = "df";
        const string LessonDateToKey = "dt";
        const string LessonAuditoriumsKey = "auditories";
        const string LessonTypeKey = "type";
        const string LessonWeekKey = "week";

        const string FirstModuleKey = "fm";
        const string SecondModuleKey = "sm";
        const string NoModuleKey = "no";

        const string AuditoriumTitleKey = "title";
        const string AuditoriumColorKey = "color";

        const int SessionDaysNumber = 20;
        const int WeekDayNumber = 7;
        #endregion Constants

        ILogger logger;

        Group ConvertToGroup(JToken jToken)
        {
            string title = jToken[GroupTitleKey]?.ToObject<string>();
            var dateFrom = jToken[GroupDateFromKey]?.ToObject<DateTime>();
            if (!dateFrom.HasValue)
            {
                dateFrom = DateTime.MinValue;
                this.logger.Warn($"Key {GroupDateFromKey} wasn't found");
            }
            var dateTo = jToken[GroupDateToKey]?.ToObject<DateTime>();
            if (!dateTo.HasValue)
            {
                dateFrom = DateTime.MaxValue;
                this.logger.Warn($"Key {GroupDateToKey} wasn't found");
            }
            bool? isEvening = jToken[GroupEveningKey]?.ToObject<bool>();
            if (!isEvening.HasValue)
            {
                isEvening = false;
                this.logger.Warn($"Key {GroupEveningKey} wasn't found");
            }
            string comment = jToken[GroupCommentKey]?.ToObject<string>();
            int? course = jToken[GroupCourseKey]?.ToObject<int>();
            if (!course.HasValue)
            {
                course = 0;
                this.logger.Warn($"Key {GroupCourseKey} wasn't found");
            }

            return new Group(title, course.Value, dateFrom.Value, dateTo.Value, isEvening.Value, comment);
        }

        Schedule.Daily[] ConvertToScheduleArray(JToken jToken, bool isByDate, Group group)
        {
            var serSchedule = jToken as JObject ??
                throw new JsonException($"Key {ScheduleGridKey} wasn't found");
            var schedule = new List<Schedule.Daily>(isByDate ? SessionDaysNumber : WeekDayNumber);
            var lessonList = new List<Lesson>();
            // Cycle for each day
            foreach (var (serDay, serDailySchedule) in serSchedule)
            {
                if ((serDailySchedule as JObject).Count == 0)
                {
                    continue;
                }
                long day;
                DateTime date;
                if (isByDate)
                {
                    date = DateTime.Parse(serDay).Date;
                    day = date.Ticks;
                }
                else
                {
                    date = DateTime.MinValue;
                    day = long.Parse(serDay);
                    if (day == 7)
                    {
                        day = 0;
                    }
                }

                if (lessonList.Count != 0)
                {
                    lessonList = new List<Lesson>();
                }
                // Cycle for each position of lesson
                foreach (var (index, serLessonList) in serDailySchedule as JObject)
                {
                    // Cycle for each lesson per position
                    foreach (var serLesson in serLessonList)
                    {
                        if (serLesson.Type == JTokenType.Null)
                        {
                            continue;
                        }
                        var lesson = ConvertToLesson(serLesson, index, group, date, isByDate);
                        if (lesson == null)
                        {
                            continue;
                        }
                        lessonList.Add(lesson);
                    }
                }
                if (lessonList.Count == 0)
                {
                    continue;
                }
                schedule.Add(new Schedule.Daily(lessonList.ToArray(), day));
            }
            return schedule.ToArray();
        }

        Lesson ConvertToLesson(JToken jToken, string index, Group group, DateTime date, bool isByDate)
        {
            string subjectTitle = jToken[LessonSubjectKey]?.ToObject<string>();
            if (subjectTitle == null)
            {
                return null;
            }
            int order = int.Parse(index) - 1;
            string[] teachers = ConvertToTeachers(jToken[LessonTeacherKey]);
            var dateFrom = isByDate ? date : jToken[LessonDateFromKey]?.ToObject<DateTime>();
            if (!dateFrom.HasValue)
            {
                dateFrom = DateTime.MinValue;
                this.logger.Warn($"Key {GroupDateFromKey} wasn't found");
            }
            var dateTo = isByDate ? date : jToken[LessonDateToKey]?.ToObject<DateTime>();
            if (!dateTo.HasValue)
            {
                dateTo = DateTime.MaxValue;
                this.logger.Warn($"Key {GroupDateToKey} wasn't found");
            }
            if (dateTo < dateFrom)
            {
                var bufDateFrom = dateFrom;
                dateFrom = dateTo;
                dateTo = bufDateFrom;
            }
            var auditoriums = ConvertToAuditoriums(jToken[LessonAuditoriumsKey]);
            string type = jToken[LessonTypeKey]?.ToObject<string>();
            var week = ConvertToWeekType(jToken[LessonWeekKey]);
            var module = ConvertToModule(jToken);

            return new Lesson(order, subjectTitle, teachers, dateFrom.Value, dateTo.Value,
                auditoriums, type, week, module, group);
        }

        Module ConvertToModule(JToken jToken)
        {
            bool? firstModule = jToken[FirstModuleKey]?.ToObject<bool>();
            bool? secondModule = jToken[SecondModuleKey]?.ToObject<bool>();
            bool? noModule = jToken[NoModuleKey]?.ToObject<bool>();
            if (noModule.HasValue && noModule.Value)
            {
                return Module.None;
            }
            if (firstModule.HasValue && firstModule.Value)
            {
                return Module.First;
            }
            if (secondModule.HasValue && secondModule.Value)
            {
                return Module.Second;
            }
            return Module.None;
        }

        string[] ConvertToTeachers(JToken jToken)
        {
            string teacher = jToken?.ToObject<string>();
            if (string.IsNullOrEmpty(teacher))
            {
                this.logger.Warn($"Key {LessonTeacherKey} wasn't found");
            }
            return teacher?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        Auditorium[] ConvertToAuditoriums(JToken jToken)
        {
            var jArray = (JArray)jToken;
            var auditoriums = new Auditorium[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                string name = jArray[i][AuditoriumTitleKey]?.ToObject<string>();
                string color = jArray[i][AuditoriumColorKey]?.ToObject<string>();
                auditoriums[i] = new Auditorium(name, color);
            }
            return auditoriums;
        }

        WeekType ConvertToWeekType(JToken jToken)
        {
            string week = jToken?.ToObject<string>();
            if (string.IsNullOrEmpty(week))
            {
                return WeekType.None;
            }
            if (week.Contains("odd", StringComparison.OrdinalIgnoreCase))
            {
                return WeekType.Odd;
            }
            if (week.Contains("even", StringComparison.OrdinalIgnoreCase))
            {
                return WeekType.Even;
            }
            return WeekType.None;
        }

        //string GetLessonSubgroup(string subjectTitle)
        //{
        //    char[] charArray = subjectTitle.ToCharArray();
        //    for (int i = charArray.Length - 1; i >= 2; i--)
        //    {
        //        // Находим сочетание "...п/г..."
        //        if (charArray[i] != 'г' || charArray[i - 1] != '/' || charArray[i - 2] != 'п')
        //            continue;
        //        // Находим номер группы "...п/г 123 ..."
        //        var resCharArr = new List<char>(10);
        //        bool flag = false;
        //        for (int j = i + 1; !flag && j < charArray.Length - 1; j++)
        //        {
        //            if (char.IsWhiteSpace(charArray[j]))
        //                continue;
        //            flag = true;
        //            resCharArr.Add(charArray[j]);
        //            continue;
        //        }
        //        return new string(resCharArr.ToArray());
        //    }
        //    return null;
        //}

        public ScheduleConverter(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleConverter>();
        }

        public async Task<string[]> ConvertToGroupList(string serializedObj)
        {
            return await Task.Run(
                () => JObject.Parse(serializedObj)[GroupListKey]?.ToObject<JArray>()?.Values<string>()
                ?.OrderByDescending(str =>
                {
                    if (!string.IsNullOrEmpty(str) && !char.IsDigit(str[0]))
                    {
                        return "_" + str;
                    }
                    return str;
                }
                )?.ToArray());
        }

        public async Task<Schedule> ConvertToScheduleAsync(string serializedObj, Action<string> sendMessage = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var serObj = JObject.Parse(serializedObj);
                    if (serObj[StatusKey]?.ToObject<string>() != "ok")
                    {
                        this.logger.Warn($"Status of converting schedule is not \"ok\": {nameof(serializedObj)}", serializedObj);
                        string msg = serObj[MessageKey]?.ToObject<string>();
                        if (!string.IsNullOrEmpty(msg))
                        {
                            sendMessage?.Invoke(msg);
                        }
                    }

                    bool isByDate = serObj[IsSession]?.ToObject<bool>() ??
                        throw new JsonException($"Key {IsSession} doesn't found");
                    var group = ConvertToGroup(serObj[GroupInfoKey]);
                    var schedule = ConvertToScheduleArray(serObj[ScheduleGridKey], isByDate, group);

                    return new Schedule(schedule, group, isByDate, DateTime.Now, Schedule.RequiredVersion);
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public Task<Schedule> ConvertToScheduleAsync(string serializedObj) => throw new NotImplementedException();
    }
}