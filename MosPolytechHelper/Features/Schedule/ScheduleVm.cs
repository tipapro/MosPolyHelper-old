namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Common;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Schedule.Common;
    using System;
    using Xamarin.Essentials;

    public class ScheduleVm : ViewModelBase
    {
        readonly ScheduleModel model;

        string groupTitle;
        WeekType weekType;
        string[] groupList;
        Schedule schedule;

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
                            OnPropertyChanged(nameof(this.Schedule));
                            break;
                        case "DateFilter" when message[1] is DateFilter dateFilter:
                            this.ScheduleFilter.DateFitler = dateFilter;
                            OnPropertyChanged(nameof(this.Schedule));
                            break;
                        case "SessionFilter" when message[1] is bool sessionFilter:
                            this.ScheduleFilter.SessionFilter = sessionFilter;
                            OnPropertyChanged(nameof(this.Schedule));
                            break;
                        case "ScheduleType" when message[1] is ScheduleType scheduleType:
                            this.IsSession = scheduleType == ScheduleType.Session;
                            SetUpScheduleAsync(true);
                            break;
                        case "ChangeFragment" when message[1] is ScheduleFragments scheduleFragment:
                            FragmentChanged?.Invoke(scheduleFragment);
                            break;
                        case "ShowEmptyLessons" when message[1] is bool showEmptyLessons:
                            this.ShowEmptyLessons = showEmptyLessons;
                            OnPropertyChanged(nameof(this.Schedule));
                            break;
                        case "ShowColoredLessons" when message[1] is bool showColoredLessons:
                            this.ShowColoredLessons = showColoredLessons;
                            OnPropertyChanged(nameof(this.Schedule));
                            break;
                    }
                }
            }
        }


        public event Action<ScheduleFragments> FragmentChanged;
        public event Action<string> Announced
        {
            add => this.model.Announce += value;
            remove => this.model.Announce -= value;
        }

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
        public bool IsSession { get; set; }
        public string[] GroupList
        {
            get => this.groupList;
            set => SetValue(ref this.groupList, value);
        }
        public ICommand Submit { get; private set; }
        public Schedule.Filter ScheduleFilter { get; set; }
        public bool ShowEmptyLessons { get; set; }
        public bool ShowColoredLessons { get; set; }


        public ScheduleVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator, bool isSession,
            Schedule.Filter scheduleFilter) : base(mediator, ViewModels.Schedule)
        {
            this.model = new ScheduleModel(loggerFactory);
            this.groupList = new string[0];
            this.IsSession = isSession;
            this.Submit = new Command(SubmitGroupTitle);
            this.ScheduleFilter = scheduleFilter;
            Subscribe(HandleMessage);
            GetGroupList(true);
        }

        public void GetGroupList(bool downloadNew)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                this.GroupList = (await this.model.GetGroupListAsync(downloadNew)) ?? new string[0]);
        }

        public void SubmitGroupTitle()
        {
            SetUpScheduleAsync(true);
        }

        public async void SetUpScheduleAsync(bool downloadNew, bool isPreloading = false)
        {
            if (string.IsNullOrEmpty(this.GroupTitle))
            {
                return;
            }
            await this.model.GetScheduleAsync(this.GroupTitle, this.IsSession, downloadNew, this.ScheduleFilter);
            if (this.model.Schedule == null && !downloadNew)
            {
                await this.model.GetScheduleAsync(this.GroupTitle, this.IsSession, !downloadNew, this.ScheduleFilter);
            }
            if (isPreloading)
            {
                MainThread.BeginInvokeOnMainThread(() => this.Schedule = this.model.Schedule);
            }
            else
            {
                this.Schedule = this.model.Schedule;
            }
        }
    }


}