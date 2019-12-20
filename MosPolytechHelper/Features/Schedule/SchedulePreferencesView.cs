namespace MosPolyHelper.Features.Schedule
{
    using Android.Support.V7.Preferences;
    using Android.Transitions;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Common;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
    using MosPolyHelper.Features.Common;
    using System.ComponentModel;

    class SchedulePreferencesView : PopupWindow
    {
        readonly ILogger logger;
        readonly SchedulePreferencesVm viewModel;
        readonly Spinner scheduleTargetPreference;
        readonly Button scheduleGoToScheduleManager;

        readonly Spinner scheduleDateFilter;
        readonly Spinner scheduleModuleFilter;
        readonly Switch scheduleSessionFilter;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.ScheduleTarget):
                    this.scheduleTargetPreference.SetSelection((int)this.viewModel.ScheduleTarget);
                    break;
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

        public SchedulePreferencesView(View contentView, int width, int height, ILoggerFactory loggerFactory,
            IMediator<ViewModels, VmMessage> mediator)
            : base(contentView, width, height)
        {
            SetEnterTransition(new Fade(FadingMode.In));
            SetExitTransition(new Fade(FadingMode.Out));
            this.viewModel = new SchedulePreferencesVm(loggerFactory, mediator);
            this.logger = loggerFactory.Create<SchedulePreferencesView>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(contentView.Context);

            //this.scheduleTargetPreference = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_target);
            //this.viewModel.ScheduleTarget = (ScheduleTarget)prefs.GetInt(PreferencesConstants.ScheduleTargetPreference, 0);
            //this.scheduleTargetPreference.SetSelection((int)this.viewModel.ScheduleTarget);
            //this.scheduleTargetPreference.ItemSelected += (obj, arg) =>
            //{
            //    if ((int)this.viewModel.ScheduleTarget != arg.Position)
            //    {
            //        this.viewModel.ScheduleTargetSelected.Execute(arg.Position);
            //        prefs.Edit().PutInt(PreferencesConstants.ScheduleTargetPreference, arg.Position).Apply();
            //    }
            //};



            this.scheduleGoToScheduleManager = contentView.FindViewById<Button>(Resource.Id.button_goto_schedule_manager);
            this.scheduleGoToScheduleManager.Click += (obj, arg) =>
            {
                this.viewModel.ButtonGoToScheduleManagerClicked.Execute(null);
                Dismiss();
            };

            //this.scheduleShowEmptyLessons = contentView.FindViewById<Switch>(Resource.Id.switch_schedule_show_empty_lessons);
            //this.viewModel.ShowEmptyLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowEmptyLessons, false);
            //this.scheduleShowEmptyLessons.Checked = this.viewModel.ShowEmptyLessons;
            //this.scheduleShowEmptyLessons.CheckedChange += (obj, arg) =>
            //{
            //    if (this.viewModel.ShowEmptyLessons != arg.IsChecked)
            //    {
            //        this.viewModel.ShowEmptyLessonsSelected.Execute(arg.IsChecked);
            //        prefs.Edit().PutBoolean(PreferencesConstants.ScheduleShowEmptyLessons, arg.IsChecked).Apply();
            //    }
            //};

            //this.scheduleShowColoredLessons = contentView.FindViewById<Switch>(Resource.Id.switch_schedule_show_colored_lessons);
            //this.viewModel.ShowColoredLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowColoredLessons, true);
            //this.scheduleShowColoredLessons.Checked = this.viewModel.ShowColoredLessons;
            //this.scheduleShowColoredLessons.CheckedChange += (obj, arg) =>
            //{
            //    if (this.viewModel.ShowColoredLessons != arg.IsChecked)
            //    {
            //        this.viewModel.ShowColoredLessonsSelected.Execute(arg.IsChecked);
            //        prefs.Edit().PutBoolean(PreferencesConstants.ScheduleShowColoredLessons, arg.IsChecked).Apply();
            //    }
            //};

            this.scheduleDateFilter = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_date_filter);
            this.viewModel.DateFilter = (DateFilter)prefs.GetInt(PreferencesConstants.ScheduleDateFilter, 0);
            this.scheduleDateFilter.SetSelection((int)this.viewModel.DateFilter);
            this.scheduleDateFilter.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.DateFilter != arg.Position)
                {
                    this.viewModel.DateFilterSelected.Execute(arg.Position);
                    prefs.Edit().PutInt(PreferencesConstants.ScheduleDateFilter, arg.Position).Apply();
                }
            };

            this.scheduleModuleFilter = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_module_filter);
            this.viewModel.ModuleFilter = (ModuleFilter)prefs.GetInt(PreferencesConstants.ScheduleModuleFilter, 0);
            this.scheduleModuleFilter.SetSelection((int)this.viewModel.ModuleFilter);
            this.scheduleModuleFilter.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.ModuleFilter != arg.Position)
                {
                    this.viewModel.ModuleFilterSelected.Execute(arg.Position);
                    prefs.Edit().PutInt(PreferencesConstants.ScheduleModuleFilter, arg.Position).Apply();
                }
            };

            this.scheduleSessionFilter = contentView.FindViewById<Switch>(Resource.Id.switch_schedule_session_filter);
            this.viewModel.SessionFilter = prefs.GetBoolean(PreferencesConstants.ScheduleSessionFilter, false);
            this.scheduleSessionFilter.Checked = this.viewModel.SessionFilter;
            this.scheduleSessionFilter.CheckedChange += (obj, arg) =>
            {
                if (this.viewModel.SessionFilter != arg.IsChecked)
                {
                    this.viewModel.SessionFilterSelected.Execute(arg.IsChecked);
                    prefs.Edit().PutBoolean(PreferencesConstants.ScheduleSessionFilter, arg.IsChecked).Apply();
                }
            };
        }
    }
}