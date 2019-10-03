namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;
    using System;

    class ScheduleVm : ViewModelBase
    {
        ScheduleModel model;

        string groupTitle;
        WeekType weekType;
        bool isSession;
        string[] groupList;
        Schedule fullSchedule;
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
                            this.ScheduleFilter.ModuleFilter = moduleFilter;
                            OnPropertyChanged(nameof(Schedule));
                            break;
                        case "DateFilter" when message[1] is DateFilter dateFilter:
                            this.ScheduleFilter.DateFitler = dateFilter;
                            OnPropertyChanged(nameof(Schedule));
                            break;
                        case "SessionFilter" when message[1] is bool sessionFilter:
                            this.ScheduleFilter.SessionFilter = sessionFilter;
                            OnPropertyChanged(nameof(Schedule));
                            break;
                        case "ScheduleType" when message[1] is ScheduleType scheduleType:
                            this.IsSession = scheduleType == ScheduleType.Session;
                            SetUpSchedule();
                            break;
                    }
                }
            }
        }

        public Schedule Schedule
        {
            get => this.fullSchedule;
            set => SetValue(ref this.fullSchedule, value);
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
            set
            {
                this.isSession = value;
            }
        }
        public string[] GroupList
        {
            get => this.groupList;
            set => SetValue(ref this.groupList, value);
        }
        public ICommand Submit { get; private set; }
        public Schedule.Filter ScheduleFilter
        {
            get => this.scheduleFilter;
            set
            {
                this.scheduleFilter = value;
            }
        }

        public ScheduleVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator, 
            Schedule.Filter scheduleFilter) : base(mediator, ViewModels.Schedule)
        {
            this.model = new ScheduleModel(loggerFactory);
            this.groupList = new string[0];
            this.Submit = new Command(SubmitGroupTitle);
            // this.date = DateTime.Now;
            this.ScheduleFilter = scheduleFilter;
            Subscribe(HandleMessage);
            GetGroupListAsync(true); // TODO: Offline mode
        }
        public void SubscribeOnAnnouncement(Action<string> act)
        {
            this.model.Announce += act;
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
            if (string.IsNullOrEmpty(GroupTitle))
            {
                return;
            }
            await this.model.GetScheduleAsync(this.GroupTitle, this.isSession, true, this.ScheduleFilter); // TODO: Offline mode
            this.Schedule = this.model.Schedule;
        }
    }


}