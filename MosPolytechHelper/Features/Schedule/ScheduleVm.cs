namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Schedule.Common;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xamarin.Essentials;

    public class ScheduleVm : ViewModelBase
    {
        readonly ScheduleModel model;

        string groupTitle;
        WeekType weekType;
        string[] groupList;
        Schedule schedule;
        DateTime date;
        ScheduleType scheduleType;

        void HandleMessage(VmMessage message)
        {
            if (message.Count == 1)
            {
                if (message[0] is string propName)
                {
                    switch (propName)
                    {
                        case "ResaveSchedule":
                            this.model.SaveScheduleAsync(this.Schedule);
                            break;
                    }
                }
            }
            else if (message.Count == 2)
            {
                if (message[0] is string propName)
                {
                    switch (propName)
                    {
                        case "ChangeFragment" when message[1] is ScheduleFragments scheduleFragment:
                            FragmentChanged?.Invoke(scheduleFragment);
                            break;
                        //case "ShowColoredLessons" when message[1] is bool showColoredLessons:
                        //    this.ShowColoredLessons = showColoredLessons;
                        //    OnPropertyChanged(nameof(this.Schedule));
                        //    break;
                        case "ChangeDate" when message[1] is DateTime date:
                            this.Date = date;
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
            set
            {
                SetValue(ref this.schedule, value);
                ScheduleEndDownloading?.Invoke();
            }
        }
        public DateTime Date
        {
            get => this.date;
            set => SetValue(ref this.date, value);
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
        public ScheduleType ScheduleType
        {
            get => this.scheduleType;
            set => SetValue(ref this.scheduleType, value);
        }
        public ICommand SubmitGroupCommand { get; }
        public ICommand GoHomeCommand { get; }
        public Schedule.Filter ScheduleFilter { get; set; }
        public bool ShowEmptyLessons { get; set; }
        public bool ShowColoredLessons { get; set; }
        public bool IsAdvancedSearch { get; set; }

        public ICommand ScheduleTypeChanged { get; }
        public ICommand LessonClickCommand { get; }
        public ICommand ScheduleCalendarCommand { get; }
        public ICommand DateFilterSelected { get; set; }
        public ICommand SessionFilterSelected { get; set; }
        public ICommand EmptyLessonsSelected { get; set; }

        public event Action ScheduleEndDownloading;
        public event Action ScheduleBeginDownloading;

        public ScheduleVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator, bool isSession,
            Schedule.Filter scheduleFilter) : base(mediator, ViewModels.Schedule)
        {
            this.model = new ScheduleModel(loggerFactory);
            this.groupList = new string[0];
            this.IsSession = isSession;
            this.IsAdvancedSearch = false;
            this.SubmitGroupCommand = new Command(SubmitGroupTitle);
            this.GoHomeCommand = new Command(GoHome);
            this.LessonClickCommand = new Command<Tuple<Lesson, DateTime>>(OpenLessonInfo);
            this.ScheduleCalendarCommand = new Command<DateTime>(OpenCalendar);
            this.DateFilterSelected = new Command<DateFilter>(ChangeDateFilter);
            this.SessionFilterSelected = new Command<bool>(ChangeSessionFilter);
            this.EmptyLessonsSelected = new Command<bool>(ChangeEmptyLessons);
            this.ScheduleFilter = scheduleFilter;
            Subscribe(HandleMessage);
            GetGroupList(true);
            this.ScheduleTypeChanged = new Command<ScheduleType>(ChangeScheduleType);
        }

        public void UpdateSchedule()
        {
            SetUpScheduleAsync(true, withoutIndicator: true);
        }

        public void ChangeDateFilter(DateFilter dateFilter)
        {
            this.ScheduleFilter.DateFilter = dateFilter;
            OnPropertyChanged(nameof(this.Schedule));
        }
        public void ChangeSessionFilter(bool sessionFilter)
        {
            this.ScheduleFilter.SessionFilter = sessionFilter;
            OnPropertyChanged(nameof(this.Schedule));
        }

        public void ChangeEmptyLessons(bool showEmptyLessons)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            OnPropertyChanged(nameof(this.Schedule));
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

        // old Tuple (reference) type to avoid (un)boxing of value type
        public void OpenLessonInfo(Tuple<Lesson, DateTime> par)
        {
            Send(ViewModels.ScheduleLessonInfo, "LessonInfo", par.Item1, par.Item2);
        }

        public void OpenCalendar(DateTime date)
        {
            Send(ViewModels.ScheduleCalendar, "CalendarMode", this.Schedule, date, this.ScheduleFilter, this.IsAdvancedSearch);
        }

        public void GoHome()
        {
            OnPropertyChanged(nameof(this.Schedule));
        }

        public async void SetUpScheduleAsync(bool downloadNew, bool notMainThread = false, bool withoutIndicator = false)
        {
            if (!withoutIndicator)
            {
                ScheduleBeginDownloading?.Invoke();
            }
            if (string.IsNullOrEmpty(this.GroupTitle))
            {
                this.Schedule = null;
                return;
            }
            await this.model.GetScheduleAsync(this.GroupTitle, this.IsSession, downloadNew);
            if (this.model.Schedule == null && !downloadNew)
            {
                await this.model.GetScheduleAsync(this.GroupTitle, this.IsSession, !downloadNew);
            }
            if (notMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => this.Schedule = this.model.Schedule);
            }
            else
            {
                this.schedule = this.model.Schedule;
                OnPropertyChanged(nameof(this.Schedule));
                ScheduleEndDownloading?.Invoke();
            }
        }

        public async Task<(Schedule[] Schedules, string[] LessonTitles, string[] Teachers, string[] Auditoriums, string[] LessonTypes)>
            GetAdvancedSearchData(IList<string> groupList, CancellationToken ct, Action<int> onProgressChanged)
        {

            this.model.DownloadProgressChanged += onProgressChanged;
            return await this.model.GetSchedules(groupList, ct);

        }

        public void ChangeScheduleType(ScheduleType scheduleType)
        {
            this.scheduleType = scheduleType;
            this.IsSession = scheduleType == ScheduleType.Session;
            SetUpScheduleAsync(true);
        }

        public void OnDateChanged(DateTime date)
        {
            this.date = date;
        }
    }
    public enum ScheduleType
    {
        Regular,
        Session
    }
}