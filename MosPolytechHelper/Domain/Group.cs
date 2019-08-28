namespace MosPolytechHelper.Domain
{
    using System;

    public class Group
    {
        public string Title { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsEvening { get; set; }
        public string Comment { get; set; }

        public Group() { }

        public Group(string title, DateTime dateFrom, DateTime dateTo, bool isEvening, string comment)
        {
            this.Title = title;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.IsEvening = isEvening;
            this.Comment = comment;
        }
    }
}