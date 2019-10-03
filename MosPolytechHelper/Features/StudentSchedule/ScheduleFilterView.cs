namespace MosPolytechHelper.Features.StudentSchedule
{
    using Android.Content;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;
    using System.ComponentModel;

    class ScheduleFilterView : PopupWindow
    {
        ScheduleFilterVm viewModel;
        Spinner scheduleDateFilter;
        Spinner scheduleModuleFilter;
        Switch scheduleSessionFilter;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // TODO: Replace on properties
                case nameof(this.viewModel.DateFilter):
                    scheduleDateFilter.SetSelection((int)this.viewModel.DateFilter);
                    break;
                case nameof(this.viewModel.ModuleFilter):
                    scheduleModuleFilter.SetSelection((int)this.viewModel.ModuleFilter);
                    break;
                case nameof(this.viewModel.SessionFilter):
                    scheduleSessionFilter.Checked = this.viewModel.SessionFilter;
                    break;
                default:
                    // TODO: Change this
                    //this.logger.Warn("Event OnPropertyChanged was not procces correctly. Property: {0}, class: {1}", e.PropertyName, this);
                    break;
            }
        }

        public ScheduleFilterView(View contentView, int width, int height, ILoggerFactory loggerFactory,
            IMediator<ViewModels, VmMessage> mediator)
            : base(contentView, width, height)
        {
            this.viewModel = new ScheduleFilterVm(loggerFactory, mediator);
        }



        public override void ShowAsDropDown(View anchor, int xoff, int yoff, [GeneratedEnum] GravityFlags gravity)
        {
            var prefs = this.ContentView.Context.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            var prefsEditor = prefs.Edit();

            this.scheduleDateFilter = this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_date_filter);
            var dateFilter = prefs.GetInt("ScheduleDateFilter", 0);
            this.scheduleDateFilter.SetSelection(dateFilter);
            this.scheduleDateFilter.ItemSelected += (obj, arg) =>
            {
                this.viewModel.DateFilterSelected.Execute(arg.Position);
                if (dateFilter != arg.Position)
                {
                    dateFilter = arg.Position;
                    prefsEditor.PutInt("ScheduleDateFilter", arg.Position);
                    prefsEditor.Apply();
                }
            };

            this.scheduleModuleFilter = this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_module_filter);
            var moduleFilter = prefs.GetInt("ScheduleModuleFilter", 0);
            this.scheduleModuleFilter.SetSelection(moduleFilter);
            this.scheduleModuleFilter.ItemSelected += (obj, arg) =>
            {
                this.viewModel.ModuleFilterSelected.Execute(arg.Position);
                if (moduleFilter != arg.Position)
                {
                    moduleFilter = arg.Position;
                    prefsEditor.PutInt("ScheduleModuleFilter", arg.Position);
                    prefsEditor.Apply();
                }
            };

            this.scheduleSessionFilter = this.ContentView.FindViewById<Switch>(Resource.Id.switch_schedule_session_filter);
            var sessionFilter = prefs.GetBoolean("ScheduleSessionFilter", false);
            this.scheduleSessionFilter.Checked = sessionFilter;
            this.scheduleSessionFilter.CheckedChange += (obj, arg) =>
            {
                this.viewModel.SessionFilterSelected.Execute(arg.IsChecked);
                if (sessionFilter != arg.IsChecked)
                {
                    sessionFilter = arg.IsChecked;
                    prefsEditor.PutBoolean("ScheduleSessionFilter", arg.IsChecked);
                    prefsEditor.Apply();
                }
            };

            base.ShowAsDropDown(anchor, xoff, yoff, gravity);


        }
    }
}