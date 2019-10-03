namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;

    class ScheduleFilterVm : ViewModelBase
    {
        ILogger logger;
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

        public ScheduleFilterVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.ScheduleFilter)
        {
            this.logger = loggerFactory.Create<ScheduleFilterVm>();
            this.ModuleFilterSelected = new Command<ModuleFilter>(ChangeModuleFilter);
            this.DateFilterSelected = new Command<DateFilter>(ChangeDateFilter);
            this.SessionFilterSelected = new Command<bool>(ChangeSessionFilter);
        }

        public void ChangeModuleFilter(ModuleFilter moduleFilter)
        {
            this.moduleFilter = moduleFilter;
            Send(ViewModels.Schedule, nameof(this.ModuleFilter), moduleFilter);
        }
        public void ChangeDateFilter(DateFilter dateFilter)
        {
            this.dateFilter = dateFilter;
            Send(ViewModels.Schedule, nameof(this.DateFilter), dateFilter);
        }
        public void ChangeSessionFilter(bool sessionFilter)
        {
            this.sessionFilter = sessionFilter;
            Send(ViewModels.Schedule, nameof(this.SessionFilter), sessionFilter);
        }
    }
}