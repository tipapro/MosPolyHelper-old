namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;
    using System;
    using System.ComponentModel;

    class ScheduleVm : ViewModelBase
    {
        ScheduleModel model;

        string groupTitle;
        WeekType weekType;
        bool isSession;
        string[] groupList;
        Schedule fullSchedule;
        DateTime date;
        Schedule.Filter scheduleFilter;


        void HandleMessage(VmMessage message)
        {
            if (message.Count == 2)
            {
                if (message[0] is string propName)
                {
                    switch (propName)
                    {
                        case "ModuleFilter" when message[1] is ModuleFilter moduleFilter:
                            this.scheduleFilter.ModuleFilter = moduleFilter;
                            OnPropertyChanged(nameof(FullSchedule));
                            break;
                        case "DateFilter" when message[1] is DateFilter dateFilter:
                            this.scheduleFilter.DateFitler = dateFilter;
                            OnPropertyChanged(nameof(FullSchedule));
                            break;
                        case "SessionFilter" when message[1] is bool sessionFilter:
                            this.scheduleFilter.SessionFilter = sessionFilter;
                            OnPropertyChanged(nameof(FullSchedule));
                            break;
                    }
                }
            }
        }

        public Schedule FullSchedule
        {
            get => this.fullSchedule;
            set => SetValue(ref this.fullSchedule, value);
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

        public ScheduleVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.Schedule)
        {
            this.model = new ScheduleModel(loggerFactory);
            this.groupList = new string[0];
            this.Submit = new Command(SubmitGroupTitle);
            this.date = DateTime.Now;
            this.scheduleFilter = Schedule.Filter.Empty;
            Subscribe(HandleMessage);
            GetGroupListAsync(true); // TODO: Offline mode
        }
        public async void GetGroupListAsync(bool downloadNew)
        {
            this.GroupList = (await this.model.GetGroupListAsync(downloadNew)) ?? new string[0];
        }
        public DateTime GetGroupDateFrom()
        {
            return this.model.Schedule?.Group?.DateFrom ?? DateTime.MinValue;
        }
        public void SubmitGroupTitle()
        {
            SetUpSchedule();
        }

        public async void SetUpSchedule()
        {
            await this.model.GetScheduleAsync(this.GroupTitle, this.isSession, true, this.scheduleFilter); // TODO: Offline mode
            this.FullSchedule = this.model.Schedule;
        }
    }


}