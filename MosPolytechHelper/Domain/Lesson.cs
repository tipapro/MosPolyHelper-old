namespace MosPolytechHelper.Domain
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Lesson
    {
        public int Order { get; set; }
        public string SubjectName { get; set; }
        public string[][] Teachers { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public Auditorium[] Auditoriums { get; set; }
        public string Type { get; set; }
        public WeekType Week { get; set; }
        public Module Module { get; set; }

        [JsonConstructor]
        Lesson() { }

        public Lesson(int order, string subjectName, string[] teachers, DateTime dateFrom, DateTime dateTo,
            Auditorium[] auditoriums, string type, WeekType week, Module module)
        {
            this.Order = order;
            this.SubjectName = subjectName;
            this.Teachers = new string[teachers.Length][];
            for (int i = 0; i < teachers.Length; i++)
            {
                this.Teachers[i] = teachers[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.Auditoriums = auditoriums;
            this.Type = type;
            this.Week = week;
            this.Module = module;
        }

        public IEnumerable<string> GetAuditoriums()
        {
            foreach (var au in this.Auditoriums)
                yield return au.Name;
        }
        public IEnumerable<string> GetShortTeacherNames()
        {
            foreach (string[] teacher in this.Teachers)
            {
                if (teacher[0].Length > 1 && char.IsUpper(teacher[0][0]) == char.IsUpper(teacher[0][1]))
                {
                    yield return string.Join(" ", teacher);
                }
                else
                {
                    string shortName = teacher[0];
                    for (int j = 1; j < teacher.Length; j++)
                    {
                        shortName += "\u00A0" + teacher[j][0] + ".";
                    }
                    yield return shortName;
                }
            }
        }
    }
}