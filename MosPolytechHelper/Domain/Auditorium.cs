namespace MosPolytechHelper.Domain
{
    public class Auditorium
    {
        public string Name { get; set; }
        public string Color { get; set; }

        public Auditorium(string name, string color)
        {
            this.Name = name;
            this.Color = color;
        }
    }
}