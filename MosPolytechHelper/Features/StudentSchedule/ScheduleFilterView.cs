namespace MosPolytechHelper.Features.StudentSchedule
{
    using Android.Content;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;
    using System.ComponentModel;

    class ScheduleFilterView : PopupWindow
    {
        ScheduleFilterVm viewModel;
        Spinner scheduleDateFilter;
        Spinner scheduleModuleFilter;
        Switch scheduleSessionFilter;
        Context context;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // TODO: Replace on properties
                case nameof(this.viewModel.DateFilter):
                    this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_date_filter)
                        .SetSelection((int)this.viewModel.DateFilter);
                    break;
                case nameof(this.viewModel.ModuleFilter):
                    this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_module_filter)
                        .SetSelection((int)this.viewModel.ModuleFilter);
                    break;
                case nameof(this.viewModel.SessionFilter):
                    this.ContentView.FindViewById<Switch>(Resource.Id.switch_schedule_session_filter).Checked = 
                        this.viewModel.SessionFilter;
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
            this.scheduleDateFilter = this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_date_filter);
            this.scheduleDateFilter.ItemSelected += (obj, arg) =>
            {
                this.viewModel.DateFilterSelected.Execute(arg.Position);
            };

            this.scheduleModuleFilter = this.ContentView.FindViewById<Spinner>(Resource.Id.spinner_schedule_module_filter);
            this.scheduleModuleFilter.ItemSelected += (obj, arg) =>
            {
                this.viewModel.ModuleFilterSelected.Execute(arg.Position);
            };

            this.scheduleSessionFilter = this.ContentView.FindViewById<Switch>(Resource.Id.switch_schedule_session_filter);
            this.scheduleSessionFilter.CheckedChange += (obj, arg) =>
            {
                this.viewModel.SessionFilterSelected.Execute(arg.IsChecked);
            };

            base.ShowAsDropDown(anchor, xoff, yoff, gravity);


        }
    }
}