namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    class TimetableConverter : ITimetableConverter
    {
        #region Constant Keys
        const string GroupListKey = "groups";

        const string StatusKey = "status";
        const string IsSession = "isSession";
        const string GroupInfoKey = "group";
        const string TimetableGridKey = "grid";

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
        #endregion Constant Keys

        ILogger logger;

        public TimetableConverter(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<TimetableConverter>();
        }

        public async Task<string[]> ConvertToGroupList(string serializedObj)
        {
            return await Task.Run(
                () => JObject.Parse(serializedObj)[GroupListKey]?.ToObject<JArray>()?.Values<string>().ToArray());
        }

        public async Task<FullTimetable> ConvertToFullTimetableAsync(string serializedObj)
        {
            return await Task.Run(() =>
            {
                var serObj = JObject.Parse(serializedObj);
                if (serObj[StatusKey]?.ToObject<string>() != "ok")
                    this.logger.Warn("Status of converted schedule is not \"ok\": {text}", serializedObj);

                bool isSession = serObj[IsSession]?.ToObject<bool>() ??
                    throw new JsonException($"Key {IsSession} doesn't found");
                var group = ConvertToGroup(serObj[GroupInfoKey]);
                var timetable = ConvertToTimetableDict(serObj[TimetableGridKey]);

                return new FullTimetable(timetable, group, isSession);
            });
        }

        public Group ConvertToGroup(JToken jToken)
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

        public Dictionary<string, DailyTimetable> ConvertToTimetableDict(JToken jToken)
        {
            var serTimetable = jToken?.ToObject<Dictionary<string, JToken>>() ??
                throw new JsonException($"Key {TimetableGridKey} wasn't founded");
            var timetable = new Dictionary<string, DailyTimetable>();
            foreach (var (day, dailyTimetableToken) in serTimetable)
            {
                var lessonList = new List<Lesson>();
                var serDailyTimetable = dailyTimetableToken.ToObject<Dictionary<string, JArray>>();
                foreach (var (index, serLessonList) in serDailyTimetable)
                {
                    foreach (var serLesson in serLessonList)
                    {
                        var lesson = ConvertToLesson(serLesson, index);
                        if (lesson != null)
                            lessonList.Add(lesson);
                    }
                }
                timetable.Add(day, new DailyTimetable(lessonList.ToArray()));
            }
            return timetable;
        }

        public Lesson ConvertToLesson(JToken jToken, string index)
        {
            string subjectName = jToken[LessonSubjectKey]?.ToObject<string>();
            if (subjectName == null)
                return null;
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
                dateFrom = DateTime.MaxValue;
                this.logger.Warn($"Key {GroupDateToKey} wasn't founded");
            }
            var auditoriums = ConvertToAuditoriums(jToken[LessonAuditoriumsKey]);
            string type = jToken[LessonTypeKey]?.ToObject<string>();
            var week = ConvertToWeekType(jToken[LessonWeekKey]);
            var module = ConvertToModule(jToken);

            return new Lesson(order, subjectName, teachers, dateFrom.Value, dateTo.Value,
                auditoriums, type, week, module);
        }

        public Module ConvertToModule(JToken jToken)
        {
            bool? firstModule = jToken[FirstModuleKey]?.ToObject<bool>();
            bool? secondModule = jToken[SecondModuleKey]?.ToObject<bool>();
            bool? noModule = jToken[NoModuleKey]?.ToObject<bool>();
            if (noModule.HasValue && noModule.Value)
                return Module.None;
            if (firstModule.HasValue && firstModule.Value)
                return Module.First;
            if (secondModule.HasValue && secondModule.Value)
                return Module.Second;
            return Module.None;
        }

        public string[] ConvertToTeachers(JToken jToken)
        {
            string teacher = jToken?.ToObject<string>();
            if (string.IsNullOrEmpty(teacher))
                this.logger.Warn($"Key {LessonTeacherKey} wasn't founded");
            return teacher?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        public Auditorium[] ConvertToAuditoriums(JToken jToken)
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

        public WeekType ConvertToWeekType(JToken jToken)
        {
            string week = jToken?.ToObject<string>();
            if (string.IsNullOrEmpty(week))
                return WeekType.None;
            if (week.Contains("odd", StringComparison.OrdinalIgnoreCase))
                return WeekType.Odd;
            if (week.Contains("even", StringComparison.OrdinalIgnoreCase))
                return WeekType.Even;
            return WeekType.None;
        }
    }
}