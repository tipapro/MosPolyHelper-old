namespace MosPolyHelper.Features.Schedule
{
    using Android.Graphics;
    using Android.OS;
    using Android.Views;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Adapters;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Utilities;

    class ScheduleCalendarView : FragmentBase
    {
        readonly ScheduleCalendarVm viewModel;
        bool dateChanged;

        public ScheduleCalendarView() : base(Fragments.ScheduleCalendar)
        {
            this.viewModel = new ScheduleCalendarVm(DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator());
            this.dateChanged = false;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_schedule_calendar, container, false);
            var toolbar = view.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);

            string groupTitle = this.viewModel.Schedule.Group?.Title;
            if (string.IsNullOrEmpty(groupTitle))
            {
                groupTitle = GetString(Resource.String.advanced_search);
            }
            else
            {
                groupTitle = groupTitle + " (" + (this.viewModel.Schedule.IsSession ?
                    GetString(Resource.String.text_schedule_type_session_s) :
                    GetString(Resource.String.text_schedule_type_regular_s)) + ")";
            }
            toolbar.Title = groupTitle;

            (this.Activity as MainView)?.SetSupportActionBar(toolbar);
            (this.Activity as MainView)?.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            (this.Activity as MainView)?.SupportActionBar.SetHomeButtonEnabled(true);
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);

            var recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_schedule_day);
            var colorTitle = new Color(Context.GetColor(Resource.Color.calendarTitle));
            var colorCurrentTitle = new Color(Context.GetColor(Resource.Color.calendarCurrentTitle));
            var recyclerAdapter = new DailyShceduleGridAdapter(this.viewModel.Schedule, this.viewModel.ScheduleFilter,
                this.viewModel.IsAdvancedSearch, new Color(view.Context.GetColor(Resource.Color.calendarParagraph)),
                new Color(view.Context.GetColor(Resource.Color.calendarTimeBackground)), colorTitle, colorCurrentTitle);
            recyclerAdapter.ItemClick += date =>
            {
                this.viewModel.Date = date;
                this.dateChanged = true;
                this.Activity.OnBackPressed();
            };

            recyclerView.InterceptTouchEvent += (obj, arg) =>
            {
                if (arg.Event.Action == MotionEventActions.Down &&
                recyclerView.ScrollState == RecyclerView.ScrollStateSettling)
                {
                    recyclerView.StopScroll();
                }
                arg.Handled = false;
            };
            recyclerView.SetItemAnimator(null);
            recyclerView.SetLayoutManager(new GridLayoutManager(container.Context, 3));
            recyclerView.SetAdapter(recyclerAdapter);
            var q = new DividerItemDecoration(recyclerView.Context, DividerItemDecoration.Horizontal);
            var e = new DividerItemDecoration(recyclerView.Context, DividerItemDecoration.Vertical);
            q.Drawable = recyclerView.Context.GetDrawable(Resource.Drawable.all_divider);
            e.Drawable = recyclerView.Context.GetDrawable(Resource.Drawable.all_divider);
            recyclerView.AddItemDecoration(q);
            recyclerView.AddItemDecoration(e);

            recyclerView.ScrollToPosition((this.viewModel.Date - recyclerAdapter.FirstPosDate).Days);

            return view;
        }

        //    public DateTime CurrentDate
        //    {
        //        get
        //        {
        //            if (this.recyclerView.GetLayoutManager() is GridLayoutManager grid)
        //            {
        //                return this.recyclerAdapter.FirstPosDate.AddDays(grid.FindFirstCompletelyVisibleItemPosition());
        //            }
        //            else
        //            {
        //                return DateTime.Today;
        //            }
        //        }
        //        set
        //        {
        //            var day = (value - this.recyclerAdapter.FirstPosDate).Days;
        //            this.recyclerView?.ScrollToPosition(day < 0 ? 0 : day);
        //        }
        //    }
        //}



        public override void OnStop()
        {
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            if (this.dateChanged)
            {
                this.viewModel.DateChanged();
            }
            base.OnStop();
        }

        public static ScheduleCalendarView NewInstance()
        {
            return new ScheduleCalendarView();
        }
    }
}