namespace MosPolytechHelper.Features.StudentSchedule
{
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;
    using System.ComponentModel;

    class SchedulePreferencesView : PopupWindow
    {
        SchedulePreferencesVm viewModel;
        Spinner scheduleTargetPreference;
        Spinner scheduleTypePreference;
        Button scheduleGoToScheduleManager;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // TODO: Replace on properties
                case nameof(this.viewModel.ScheduleTarget):
                    this.scheduleTargetPreference.SetSelection((int)this.viewModel.ScheduleTarget);
                    break;
                case nameof(this.viewModel.ScheduleType):
                    this.scheduleTypePreference.SetSelection((int)this.viewModel.ScheduleType);
                    break;
                default:
                    // TODO: Change this
                    //this.logger.Warn("Event OnPropertyChanged was not procces correctly. Property: {0}, class: {1}", e.PropertyName, this);
                    break;
            }
        }

        public SchedulePreferencesView(View contentView, int width, int height, ILoggerFactory loggerFactory,
            IMediator<ViewModels, VmMessage> mediator)
            : base(contentView, width, height)
        {
            this.viewModel = new SchedulePreferencesVm(loggerFactory, mediator);


            var prefs = contentView.Context.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);

            this.scheduleTargetPreference = contentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_target);
            int dateFilter = prefs.GetInt("ScheduleTargetPreference", 0);
            this.scheduleTargetPreference.SetSelection(dateFilter);
            this.scheduleTargetPreference.ItemSelected += (obj, arg) =>
            {
                this.viewModel.ScheduleTargetSelected.Execute(arg.Position);
                if (dateFilter != arg.Position)
                {
                    dateFilter = arg.Position;
                    prefs.Edit().PutInt("ScheduleTargetPreference", arg.Position).Apply();
                }
            };

            this.scheduleTypePreference = contentView.FindViewById<Spinner>(Resource.Id.spinner_text_schedule_type);
            int moduleFilter = prefs.GetInt("ScheduleTypePreference", 0);
            this.scheduleTypePreference.SetSelection(moduleFilter);
            this.scheduleTypePreference.ItemSelected += (obj, arg) =>
            {
                this.viewModel.ScheduleTypeSelected.Execute(arg.Position);
                if (moduleFilter != arg.Position)
                {
                    moduleFilter = arg.Position;
                    prefs.Edit().PutInt("ScheduleTypePreference", arg.Position).Apply();
                }
            };

            this.scheduleGoToScheduleManager = contentView.FindViewById<Button>(Resource.Id.button_goto_schedule_manager);
            this.scheduleGoToScheduleManager.Click += (obj, arg) =>
            {
                this.viewModel.ButtonGoToScheduleManagerClicked.Execute(null);
                Dismiss();
            };
        }
    }
}