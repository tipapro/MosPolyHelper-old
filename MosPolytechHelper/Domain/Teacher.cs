namespace MosPolytechHelper.Domain
{
    using ProtoBuf;

    [ProtoContract]
    public class Teacher
    {
        Teacher()
        { 
        }

        [ProtoMember(1)]
        public string[] Name { get; set; }

        public Teacher(string[] name)
        {
            this.Name = name;
        }

        public string GetFullName()
        {
            return string.Join(" ", Name);
        }

        public string GetShortName()
        {
            bool isVacancy = false;
            foreach (var name in Name)
            {
                if (name.Contains("вакансия", System.StringComparison.OrdinalIgnoreCase))
                    {
                    isVacancy = true;
                    break;
                }
            }
            if (isVacancy || (Name[0].Length > 1 && char.IsUpper(Name[0][0]) == char.IsUpper(Name[0][1])))
            {
                return string.Join("\u00A0", Name);
            }
            else
            {
                string shortName = Name[0];
                for (int j = 1; j < Name.Length; j++)
                {
                    shortName += "\u00A0" + Name[j][0] + ".";
                }
                return shortName;
            }
        }
    }
}