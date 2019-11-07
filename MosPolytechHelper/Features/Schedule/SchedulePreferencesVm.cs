using MosPolyHelper.Common;
using MosPolyHelper.Common.Interfaces;
using MosPolyHelper.Features.Common;
using MosPolyHelper.Features.Schedule.Common;

namespace MosPolyHelper.Features.Schedule
{
    class SchedulePreferencesVm : ViewModelBase
    {
        ScheduleTarget scheduleTarget;
        ScheduleType scheduleType;
        bool showEmptyLessons;
        bool showColoredLessons;

        public ScheduleTarget ScheduleTarget
        {
            get => this.scheduleTarget;
            set => SetValue(ref this.scheduleTarget, value);
        }
        public ScheduleType ScheduleType
        {
            get => this.scheduleType;
            set => SetValue(ref this.scheduleType, value);
        }
        public bool ShowEmptyLessons
        {
            get => this.showEmptyLessons;
            set => SetValue(ref this.showEmptyLessons, value);
        }
        public bool ShowColoredLessons
        {
            get => this.showColoredLessons;
            set => SetValue(ref this.showColoredLessons, value);
        }

        public ICommand ScheduleTargetSelected { get; set; }
        public ICommand ScheduleTypeSelected { get; set; }
        public ICommand ButtonGoToScheduleManagerClicked { get; set; }
        public ICommand ShowEmptyLessonsSelected { get; set; }
        public ICommand ShowColoredLessonsSelected { get; set; }

        public SchedulePreferencesVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.SchedulePreferences)
        {
            this.ScheduleTargetSelected = new Command<ScheduleTarget>(ChangeScheduleTarget);
            this.ScheduleTypeSelected = new Command<ScheduleType>(ChangeScheduleType);
            this.ButtonGoToScheduleManagerClicked = new Command(GoToScheduleManagerFrament);
            this.ShowEmptyLessonsSelected = new Command<bool>(ChangeShowEmptyLessons);
            this.ShowColoredLessonsSelected = new Command<bool>(ChangeShowColoredLessons);
        }

        public void ChangeScheduleTarget(ScheduleTarget scheduleTarget)
        {
            this.scheduleTarget = scheduleTarget;
            Send(ViewModels.Schedule, nameof(this.ScheduleTarget), scheduleTarget);
        }
        public void ChangeScheduleType(ScheduleType scheduleType)
        {
            this.scheduleType = scheduleType;
            Send(ViewModels.Schedule, nameof(this.ScheduleType), scheduleType);
        }
        public void GoToScheduleManagerFrament()
        {
            Send(ViewModels.Schedule, "ChangeFragment", ScheduleFragments.ScheduleManager);
        }
        public void ChangeShowEmptyLessons(bool showEmptyLessons)
        {
            this.showEmptyLessons = showEmptyLessons;
            Send(ViewModels.Schedule, nameof(this.ShowEmptyLessons), showEmptyLessons);
        }
        public void ChangeShowColoredLessons(bool showColoredLessons)
        {
            this.showColoredLessons = showColoredLessons;
            Send(ViewModels.Schedule, nameof(this.ShowColoredLessons), showColoredLessons);
        }
    }

    public enum ScheduleTarget
    {
        Student,
        Teacher
    }

    public enum ScheduleType
    {
        Everyday,
        Session
    }
}