namespace MosPolyHelper.Features.Schedule
{
    using Android.Transitions;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Features.Common;
    using System.ComponentModel;

    class SchedulePreferencesView : PopupWindow
    {
        readonly ILogger logger;
        readonly SchedulePreferencesVm viewModel;
        readonly Spinner scheduleTargetPreference;
        readonly Spinner scheduleTypePreference;
        readonly Button scheduleGoToScheduleManager;
        readonly Switch scheduleShowEmptyLessons;
        readonly Switch scheduleShowColoredLessons;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.ScheduleTarget):
                    this.scheduleTargetPreference.SetSelection((int)this.viewModel.ScheduleTarget);
                    break;
                case nameof(this.viewModel.ScheduleType):
                    this.scheduleTypePreference.SetSelection((int)this.viewModel.ScheduleType);
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
            var prefs = contentView.Context.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);

            this.scheduleTargetPreference = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_target);
            this.viewModel.ScheduleTarget = (ScheduleTarget)prefs.GetInt("ScheduleTargetPreference", 0);
            this.scheduleTargetPreference.SetSelection((int)this.viewModel.ScheduleTarget);
            this.scheduleTargetPreference.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.ScheduleTarget != arg.Position)
                {
                    this.viewModel.ScheduleTargetSelected.Execute(arg.Position);
                    prefs.Edit().PutInt("ScheduleTargetPreference", arg.Position).Apply();
                }
            };

            this.scheduleTypePreference = contentView.FindViewById<Spinner>(Resource.Id.spinner_text_schedule_type);
            this.viewModel.ScheduleType = (ScheduleType)prefs.GetInt("ScheduleTypePreference", 0);
            this.scheduleTypePreference.SetSelection((int)this.viewModel.ScheduleType);
            this.scheduleTypePreference.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.ScheduleType != arg.Position)
                {
                    this.viewModel.ScheduleTypeSelected.Execute(arg.Position);
                    prefs.Edit().PutInt("ScheduleTypePreference", arg.Position).Apply();
                }
            };

            this.scheduleGoToScheduleManager = contentView.FindViewById<Button>(Resource.Id.button_goto_schedule_manager);
            this.scheduleGoToScheduleManager.Click += (obj, arg) =>
            {
                this.viewModel.ButtonGoToScheduleManagerClicked.Execute(null);
                Dismiss();
            };

            this.scheduleShowEmptyLessons = contentView.FindViewById<Switch>(Resource.Id.switch_schedule_show_empty_lessons);
            this.viewModel.ShowEmptyLessons = prefs.GetBoolean("ScheduleShowEmptyLessons", false);
            this.scheduleShowEmptyLessons.Checked = this.viewModel.ShowEmptyLessons;
            this.scheduleShowEmptyLessons.CheckedChange += (obj, arg) =>
            {
                if (this.viewModel.ShowEmptyLessons != arg.IsChecked)
                {
                    this.viewModel.ShowEmptyLessonsSelected.Execute(arg.IsChecked);
                    prefs.Edit().PutBoolean("ScheduleShowEmptyLessons", arg.IsChecked).Apply();
                }
            };

            this.scheduleShowColoredLessons = contentView.FindViewById<Switch>(Resource.Id.switch_schedule_show_colored_lessons);
            this.viewModel.ShowColoredLessons = prefs.GetBoolean("ScheduleShowColoredLessons", false);
            this.scheduleShowColoredLessons.Checked = this.viewModel.ShowColoredLessons;
            this.scheduleShowColoredLessons.CheckedChange += (obj, arg) =>
            {
                if (this.viewModel.ShowColoredLessons != arg.IsChecked)
                {
                    this.viewModel.ShowColoredLessonsSelected.Execute(arg.IsChecked);
                    prefs.Edit().PutBoolean("ScheduleShowColoredLessons", arg.IsChecked).Apply();
                }
            };
        }
    }
}