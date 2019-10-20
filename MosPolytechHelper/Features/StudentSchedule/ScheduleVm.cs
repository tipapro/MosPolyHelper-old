namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;
    using MosPolytechHelper.Features.StudentSchedule.Common;
    using System;

    class ScheduleVm : ViewModelBase
    {
        ScheduleModel model;

        string groupTitle;
        WeekType weekType;
        bool isSession;
        string[] groupList;
        Schedule schedule;
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
                            SetUpScheduleAsync(true);
                            break;
                        case "ChangeFragment" when message[1] is ScheduleFragments scheduleFragment:
                            FragmentChanged?.Invoke(scheduleFragment);
                            break;
                    }
                }
            }
        }


        public event Action<ScheduleFragments> FragmentChanged;

        public Schedule Schedule
        {
            get => this.schedule;
            set => SetValue(ref this.schedule, value);
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

        public ScheduleVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator, bool isSession,
            Schedule.Filter scheduleFilter) : base(mediator, ViewModels.Schedule)
        {
            this.model = new ScheduleModel(loggerFactory);
            this.groupList = new string[0];
            this.isSession = isSession;
            this.Submit = new Command(SubmitGroupTitle);
            this.ScheduleFilter = scheduleFilter;
            Subscribe(HandleMessage);
            GetGroupListAsync(true);
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
            SetUpScheduleAsync(true);
        }

        public async void SetUpScheduleAsync(bool downloadNew)
        {
            if (string.IsNullOrEmpty(GroupTitle))
            {
                return;
            }
            await this.model.GetScheduleAsync(this.GroupTitle, this.isSession, downloadNew, this.ScheduleFilter);
            if (this.model.Schedule == null && !downloadNew)
            {
                await this.model.GetScheduleAsync(this.GroupTitle, this.isSession, !downloadNew, this.ScheduleFilter);
            }
            this.Schedule = this.model.Schedule;
        }
    }


}