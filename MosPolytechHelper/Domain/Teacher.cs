namespace MosPolyHelper.Domain
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
            return string.Join(" ", this.Name);
        }

        public string GetShortName()
        {
            if (this.Name.Length == 0)
            {
                return null;
            }
            bool isVacancy = false;
            foreach (string name in this.Name)
            {
                if (name.Contains("вакансия", System.StringComparison.OrdinalIgnoreCase))
                {
                    isVacancy = true;
                    break;
                }
            }
            if (isVacancy || this.Name[0].Length > 1 && (char.IsUpper(this.Name[0][0]) == char.IsUpper(this.Name[0][1])))
            {
                return string.Join("\u00A0", this.Name);
            }
            else
            {
                string shortName = this.Name[0];
                for (int j = 1; j < this.Name.Length; j++)
                {
                    shortName += "\u00A0" + this.Name[j][0] + ".";
                }
                return shortName;
            }
        }
    }
}