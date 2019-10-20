namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;

    class ScheduleManagerVm : ViewModelBase
    {
        ILogger logger;

        public ScheduleManagerVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.ScheduleManager)
        {
            this.logger = loggerFactory.Create<ScheduleManagerVm>();
        }
    }
}