namespace MosPolyHelper.Features.Schedule
{
    using Android.Transitions;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
    using MosPolyHelper.Features.Common;
    using System.ComponentModel;

    class ScheduleFilterView : PopupWindow
    {
        readonly ILogger logger;

        readonly ScheduleFilterVm viewModel;
        readonly Spinner scheduleDateFilter;
        readonly Spinner scheduleModuleFilter;
        readonly Switch scheduleSessionFilter;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.DateFilter):
                    this.scheduleDateFilter.SetSelection((int)this.viewModel.DateFilter);
                    break;
                case nameof(this.viewModel.ModuleFilter):
                    this.scheduleModuleFilter.SetSelection((int)this.viewModel.ModuleFilter);
                    break;
                case nameof(this.viewModel.SessionFilter):
                    this.scheduleSessionFilter.Checked = this.viewModel.SessionFilter;
                    break;
                default:
                    this.logger.Warn("Event OnPropertyChanged was not proccessed correctly. Property: {PropertyName}", e.PropertyName);
                    break;
            }
        }

        public ScheduleFilterView(View contentView, int width, int height, ILoggerFactory loggerFactory,
            IMediator<ViewModels, VmMessage> mediator)
            : base(contentView, width, height)
        {
            SetEnterTransition(new Fade(FadingMode.In));
            SetExitTransition(new Fade(FadingMode.Out));
            this.viewModel = new ScheduleFilterVm(loggerFactory, mediator);
            this.logger = loggerFactory.Create<ScheduleFilterView>();
            var prefs = contentView.Context.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);

            this.scheduleDateFilter = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_date_filter);
            this.viewModel.DateFilter = (DateFilter)prefs.GetInt("ScheduleDateFilter", 0);
            this.scheduleDateFilter.SetSelection((int)this.viewModel.DateFilter);
            this.scheduleDateFilter.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.DateFilter != arg.Position)
                {
                    this.viewModel.DateFilterSelected.Execute(arg.Position);
                    prefs.Edit().PutInt("ScheduleDateFilter", arg.Position).Apply();
                }
            };

            this.scheduleModuleFilter = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_module_filter);
            this.viewModel.ModuleFilter = (ModuleFilter)prefs.GetInt("ScheduleModuleFilter", 0);
            this.scheduleModuleFilter.SetSelection((int)this.viewModel.ModuleFilter);
            this.scheduleModuleFilter.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.ModuleFilter != arg.Position)
                {
                    this.viewModel.ModuleFilterSelected.Execute(arg.Position);
                    prefs.Edit().PutInt("ScheduleModuleFilter", arg.Position).Apply();
                }
            };

            this.scheduleSessionFilter = contentView.FindViewById<Switch>(Resource.Id.switch_schedule_session_filter);
            this.viewModel.SessionFilter = prefs.GetBoolean("ScheduleSessionFilter", false);
            this.scheduleSessionFilter.Checked = this.viewModel.SessionFilter;
            this.scheduleSessionFilter.CheckedChange += (obj, arg) =>
            {
                if (this.viewModel.SessionFilter != arg.IsChecked)
                {
                    this.viewModel.SessionFilterSelected.Execute(arg.IsChecked);
                    prefs.Edit().PutBoolean("ScheduleSessionFilter", arg.IsChecked).Apply();
                }
            };
        }
    }
}