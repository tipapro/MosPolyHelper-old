namespace MosPolyHelper.Common
{
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
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
        const string IsSession = "isSession";
        const string GroupInfoKey = "group";
        const string ScheduleGridKey = "grid";

        const string GroupTitleKey = "title";
        const string GroupDateFromKey = "dateFrom";
        const string GroupDateToKey = "dateTo";
        const string GroupEveningKey = "evening";
        const string GroupCommentKey = "comment";

        const string LessonSubjectKey = "subject";
        const string LessonTeacherKey = "teacher";
        const string LessonDateFromKey = "date_from";
        const string LessonDateToKey = "date_to";
        const string LessonAuditoriumsKey = "auditories";
        const string LessonTypeKey = "type";
        const string LessonWeekKey = "week";

        const string FirstModuleKey = "first_module";
        const string SecondModuleKey = "second_module";
        const string NoModuleKey = "no_module";

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
                this.logger.Warn($"Key {GroupDateFromKey} wasn't founded");
            }
            var dateTo = jToken[GroupDateToKey]?.ToObject<DateTime>();
            if (!dateTo.HasValue)
            {
                dateFrom = DateTime.MaxValue;
                this.logger.Warn($"Key {GroupDateToKey} wasn't founded");
            }
            bool? isEvening = jToken[GroupEveningKey]?.ToObject<bool>();
            if (!isEvening.HasValue)
            {
                isEvening = false;
                this.logger.Warn($"Key {GroupEveningKey} wasn't founded");
            }
            string comment = jToken[GroupCommentKey]?.ToObject<string>();

            return new Group(title, dateFrom.Value, dateTo.Value, isEvening.Value, comment);
        }

        Schedule.Daily[] ConvertToScheduleArray(JToken jToken, bool isSession)
        {
            var serSchedule = jToken as JObject ??
                throw new JsonException($"Key {ScheduleGridKey} wasn't founded");
            var schedule = new List<Schedule.Daily>(isSession ? SessionDaysNumber : WeekDayNumber);
            var lessonList = new List<Lesson>();
            // Cycle for each day
            foreach (var (day, serDailySchedule) in serSchedule)
            {
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
                        var lesson = ConvertToLesson(serLesson, index);
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
                schedule.Add(new Schedule.Daily(lessonList.ToArray(),
                    isSession ? DateTime.Parse(day).ToBinary() : long.Parse(day)));
            }
            return schedule.ToArray();
        }

        Lesson ConvertToLesson(JToken jToken, string index)
        {
            string subjectTitle = jToken[LessonSubjectKey]?.ToObject<string>();
            if (subjectTitle == null)
            {
                return null;
            }
            int order = int.Parse(index) - 1;
            string[] teachers = ConvertToTeachers(jToken[LessonTeacherKey]);
            var dateFrom = jToken[LessonDateFromKey]?.ToObject<DateTime>();
            if (!dateFrom.HasValue)
            {
                dateFrom = DateTime.MinValue;
                this.logger.Warn($"Key {GroupDateFromKey} wasn't founded");
            }
            var dateTo = jToken[LessonDateToKey]?.ToObject<DateTime>();
            if (!dateTo.HasValue)
            {
                dateTo = DateTime.MaxValue;
                this.logger.Warn($"Key {GroupDateToKey} wasn't founded");
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
                auditoriums, type, week, module);
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
                this.logger.Warn($"Key {LessonTeacherKey} wasn't founded");
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
                () => JObject.Parse(serializedObj)[GroupListKey]?.ToObject<JArray>()?.Values<string>()?.ToArray());
        }

        public async Task<Schedule> ConvertToScheduleAsync(string serializedObj)
        {
            return await Task.Run(() =>
            {
                var serObj = JObject.Parse(serializedObj);
                if (serObj[StatusKey]?.ToObject<string>() != "ok")
                {
                    this.logger.Warn($"Status of converting schedule is not \"ok\": {nameof(serializedObj)}", serializedObj);
                }

                bool isSession = serObj[IsSession]?.ToObject<bool>() ??
                    throw new JsonException($"Key {IsSession} doesn't found");
                var group = ConvertToGroup(serObj[GroupInfoKey]);
                var schedule = ConvertToScheduleArray(serObj[ScheduleGridKey], isSession);

                return new Schedule(schedule, group, isSession, DateTime.Now);
            });
        }
    }
}