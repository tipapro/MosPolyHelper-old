using MosPolyHelper.Utilities;
using MosPolyHelper.Utilities.Interfaces;
using MosPolyHelper.Domains.ScheduleDomain;
using MosPolyHelper.Features.Common;
using MosPolyHelper.Features.Schedule.Common;

namespace MosPolyHelper.Features.Schedule
{
    class SchedulePreferencesVm : ViewModelBase
    {
        ModuleFilter moduleFilter;
        DateFilter dateFilter;
        bool sessionFilter;

        public ModuleFilter ModuleFilter
        {
            get => this.moduleFilter;
            set => SetValue(ref this.moduleFilter, value);
        }
        public DateFilter DateFilter
        {
            get => this.dateFilter;
            set => SetValue(ref this.dateFilter, value);
        }
        public bool SessionFilter
        {
            get => this.sessionFilter;
            set => SetValue(ref this.sessionFilter, value);
        }

        public ICommand ModuleFilterSelected { get; set; }
        public ICommand DateFilterSelected { get; set; }
        public ICommand SessionFilterSelected { get; set; }

        public void ChangeModuleFilter(ModuleFilter moduleFilter)
        {
            this.moduleFilter = moduleFilter;
            Send(ViewModels.ScheduleLessonInfo, nameof(this.ModuleFilter), moduleFilter);
        }
        public void ChangeDateFilter(DateFilter dateFilter)
        {
            this.dateFilter = dateFilter;
            Send(ViewModels.ScheduleLessonInfo, nameof(this.DateFilter), dateFilter);
        }
        public void ChangeSessionFilter(bool sessionFilter)
        {
            this.sessionFilter = sessionFilter;
            Send(ViewModels.ScheduleLessonInfo, nameof(this.SessionFilter), sessionFilter);
        }

        ScheduleTarget scheduleTarget;
        bool showEmptyLessons;
        bool showColoredLessons;

        public ScheduleTarget ScheduleTarget
        {
            get => this.scheduleTarget;
            set => SetValue(ref this.scheduleTarget, value);
        }

        public ICommand ScheduleTargetSelected { get; set; }
        public ICommand ButtonGoToScheduleManagerClicked { get; set; }

        public SchedulePreferencesVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.SchedulePreferences)
        {
            this.ScheduleTargetSelected = new Command<ScheduleTarget>(ChangeScheduleTarget);
            this.ButtonGoToScheduleManagerClicked = new Command(GoToScheduleManagerFrament);

            this.ModuleFilterSelected = new Command<ModuleFilter>(ChangeModuleFilter);
            this.DateFilterSelected = new Command<DateFilter>(ChangeDateFilter);
            this.SessionFilterSelected = new Command<bool>(ChangeSessionFilter);
        }

        public void ChangeScheduleTarget(ScheduleTarget scheduleTarget)
        {
            this.scheduleTarget = scheduleTarget;
            Send(ViewModels.ScheduleLessonInfo, nameof(this.ScheduleTarget), scheduleTarget);
        }
        public void GoToScheduleManagerFrament()
        {
            Send(ViewModels.ScheduleLessonInfo, "ChangeFragment", ScheduleFragments.ScheduleManager);
        }
    }

    public enum ScheduleTarget
    {
        Student,
        Teacher
    }
}