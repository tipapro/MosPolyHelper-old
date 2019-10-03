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

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // TODO: Replace on properties
                case nameof(this.viewModel.ScheduleTarget):
                    scheduleTargetPreference.SetSelection((int)this.viewModel.ScheduleTarget);
                    break;
                case nameof(this.viewModel.ScheduleType):
                    scheduleTypePreference.SetSelection((int)this.viewModel.ScheduleType);
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
        }


        public override void ShowAsDropDown(View anchor, int xoff, int yoff, [GeneratedEnum] GravityFlags gravity)
        {
            var prefs = this.ContentView.Context.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            var prefsEditor = prefs.Edit();

            this.scheduleTargetPreference = this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_target);
            var dateFilter = prefs.GetInt("ScheduleTargetPreference", 0);
            this.scheduleTargetPreference.SetSelection(dateFilter);
            this.scheduleTargetPreference.ItemSelected += (obj, arg) =>
            {
                this.viewModel.ScheduleTargetSelected.Execute(arg.Position);
                if (dateFilter != arg.Position)
                {
                    dateFilter = arg.Position;
                    prefsEditor.PutInt("ScheduleTargetPreference", arg.Position);
                    prefsEditor.Apply();
                }
            };

            this.scheduleTypePreference = this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_type);
            var moduleFilter = prefs.GetInt("ScheduleTypePreference", 0);
            this.scheduleTypePreference.SetSelection(moduleFilter);
            this.scheduleTypePreference.ItemSelected += (obj, arg) =>
            {
                this.viewModel.ScheduleTypeSelected.Execute(arg.Position);
                if (moduleFilter != arg.Position)
                {
                    moduleFilter = arg.Position;
                    prefsEditor.PutInt("ScheduleTypePreference", arg.Position);
                    prefsEditor.Apply();
                }
            };
            base.ShowAsDropDown(anchor, xoff, yoff, gravity);
        }
    }
}