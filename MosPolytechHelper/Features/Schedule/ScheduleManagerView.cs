//namespace MosPolyHelper.Features.Schedule
//{
//    using Android.App;
//    using Android.OS;
//    using Android.Support.V7.App;
//    using Android.Views;
//    using MosPolyHelper.Common;
//    using MosPolyHelper.Common.Interfaces;
//    using MosPolyHelper.Features.Common;

//    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
//    class ScheduleManagerView : AppCompatActivity
//    {
//        ScheduleManagerVm viewModel;
//        ILogger logger;
//        View view; 


//        protected override void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);
//            SetContentView(Resource.Layout.fragment_schedule_manager);
            
//            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_m);
//            SetSupportActionBar(toolbar);

//            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
//            SupportActionBar.SetHomeButtonEnabled(true);

//            var loggerFactory = DependencyInjector.GetILoggerFactory();
//            this.logger = loggerFactory.Create<ScheduleManagerView>();
//            viewModel = new ScheduleManagerVm(loggerFactory, DependencyInjector.GetIMediator());
//        }

//        public override bool OnSupportNavigateUp()
//        {
//            Finish();
//            return true;
//        }
//    }
//}