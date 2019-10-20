using MosPolytechHelper.Common;
using MosPolytechHelper.Common.Interfaces;
using MosPolytechHelper.Features.Common;
using MosPolytechHelper.Features.StudentSchedule.Common;

namespace MosPolytechHelper.Features.StudentSchedule
{
    class SchedulePreferencesVm : ViewModelBase
    {
        ILogger logger;

        ScheduleTarget scheduleTarget;
        ScheduleType scheduleType;

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

        public ICommand ScheduleTargetSelected { get; set; }
        public ICommand ScheduleTypeSelected { get; set; }
        public ICommand ButtonGoToScheduleManagerClicked { get; set; }

        public SchedulePreferencesVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.SchedulePreferences)
        {
            this.logger = loggerFactory.Create<SchedulePreferencesVm>();
            this.ScheduleTargetSelected = new Command<ScheduleTarget>(ChangeScheduleTarget);
            this.ScheduleTypeSelected = new Command<ScheduleType>(ChangeScheduleType);
            this.ButtonGoToScheduleManagerClicked = new Command(GoToScheduleManagerFrament);
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
            this.Send(ViewModels.Schedule, "ChangeFragment", ScheduleFragments.ScheduleManager);
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