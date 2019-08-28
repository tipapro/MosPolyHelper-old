namespace MosPolytechHelper.Features.StudentTimetable
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;
    using System;
    using System.Threading.Tasks;

    class TimetableVm : ViewModelBase
    {
        TimetableModel model;

        string groupTitle;
        WeekType weekType;
        bool isSession;
        string[] groupList;
        FullTimetable fullTimetable;
        DateTime date;

        public FullTimetable FullTimetable
        {
            get => this.fullTimetable;
            set => SetValue(ref this.fullTimetable, value);
        }
        public DateTime Date
        {
            get => this.date;
            set => this.date = value;
        }
        public WeekType WeekType
        {
            get => this.weekType;
            set => SetValue(ref this.weekType, value);
        }
        public string GroupTitle
        {
            get => this.groupTitle;
            set => SetValue(ref this.groupTitle, value);
        }
        public bool IsSession
        {
            get => this.isSession;
            set => SetValue(ref this.isSession, value);
        }
        public string[] GroupList
        {
            get => this.groupList;
            set => SetValue(ref this.groupList, value);
        }
        public ICommand Submit { get; private set; }

        public TimetableVm(ILoggerFactory loggerFactory)
        {
            this.model = new TimetableModel(loggerFactory);
            this.groupList = new string[0];
            this.Submit = new AsyncCommand(SubmitGroupTitle);
            this.date = DateTime.Now;
            GetGroupListAsync();
        }
        public async void GetGroupListAsync()
        {
            this.GroupList = (await this.model.GetGroupListAsync()) ?? new string[0];
        }
        public DateTime GetGroupDateFrom()
        {
            return this.model.FullTimetable?.Group?.DateFrom ?? DateTime.MinValue;
        }
        public async Task SubmitGroupTitle()
        {
            await this.model.GetTimetableAsync(this.GroupTitle, false);
            this.FullTimetable = this.model.FullTimetable;
        }
    }
}