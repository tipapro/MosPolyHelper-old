namespace MosPolytechHelper.Features.StudentSchedule
{
    using Android.OS;
    using Android.Support.V4.View;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using MosPolytechHelper.Adapters;
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;
    using System;
    using System.ComponentModel;

    class ScheduleView : Android.Support.V4.App.Fragment
    {
        ScheduleVm viewModel;
        View view;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        Android.Support.V4.App.Fragment[] fragments;
        ViewPager viewPager;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.FullSchedule):
                    (this.viewPager.Adapter as ViewPagerAdapter).UpdateSchedule(this.viewModel.FullSchedule);
                    break;
                case nameof(this.viewModel.WeekType):

                    break;
                case nameof(this.viewModel.GroupList):
                    this.textGroupTitle.Adapter = new ArrayAdapter<string>(this.Context, Resource.Layout.item_group_list, this.viewModel.GroupList);
                    break;
                default:
                    // TODO: Change this
                    this.logger.Warn("Event OnPropertyChanged was not procces correctly. Property: {0}, class: {1}", e.PropertyName, this);
                    break;
            }
        }

        public ScheduleView()
        {
        }

        public static ScheduleView NewInstance()
        {
            var fragment = new ScheduleView();
            //fragment.Arguments = new Bundle();
            //fragment.Arguments.PutString("title", title);
            //fragment.Arguments.PutString("icon", icon);
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var loggerFactory = (this.Context as MainActivity).GetILoggerFactory();
            this.viewModel = new ScheduleVm(loggerFactory, (this.Context as MainActivity).GetIMediator());
            this.viewModel.PropertyChanged += OnPropertyChanged;
            this.logger = loggerFactory.Create<ScheduleView>();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.view = inflater.Inflate(Resource.Layout.fragment_student_schedule, container, false);


            this.viewPager = this.view.FindViewById<ViewPager>(Resource.Id.viewpager);
            //this.viewPager.PageSelected += ViewPager_PageSelected;
            var viewPagerAdaper = new ViewPagerAdapter(this.Context, this.viewModel.FullSchedule);
            this.viewPager.Adapter = viewPagerAdaper;
            this.viewPager.PageSelected += (obj, arg) =>
            {
                this.viewModel.Date = DateTime.Today.AddDays(arg.Position - viewPagerAdaper.FirstPos);
            };
            this.viewPager.SetCurrentItem(viewPagerAdaper.FirstPos, false);
            //var pagerTabStrip = this.viewPager.FindViewById<com.refractored.PagerSlidingTabStrip>(Resource.Id.pager_tab_strip);
            //pagerTabStrip.SetViewPager(viewPager);

            this.textGroupTitle = (this.view.Context as MainActivity).FindViewById<AutoCompleteTextView>(Resource.Id.text_group_title);
            var adapter = new ArrayAdapter<string>(this.Context, Resource.Layout.item_group_list, this.viewModel.GroupList);
            this.textGroupTitle.Adapter = adapter;
            this.textGroupTitle.AfterTextChanged += (obj, arg) => this.viewModel.GroupTitle = (obj as AutoCompleteTextView)?.Text;
            this.textGroupTitle.KeyPress +=
                (obj, arg) =>
                {
                    if (arg.KeyCode != Keycode.Enter)
                        return;
                    if (arg.Event.Action != KeyEventActions.Up)
                        return;
                    this.viewModel.Submit.Execute(null);
                    (obj as AutoCompleteTextView).DismissDropDown();
                    (obj as AutoCompleteTextView).RequestFocus();
                    var inputManager = (InputMethodManager)this.Activity.GetSystemService(Android.Content.Context.InputMethodService);
                    var currentFocus = this.Activity.CurrentFocus;
                    if (currentFocus != null)
                    {
                        inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
                    }
                };
            this.textGroupTitle.KeyPress += (obj, arg) =>
            {
                if (arg.KeyCode != Keycode.Back)
                    return;
                if (arg.Event.Action != KeyEventActions.Up)
                    return;
                (this.view.Context as MainActivity).OnBackPressed();
            };
            this.textGroupTitle.ItemClick += (obj, arg) =>
            {
                this.viewModel.Submit.Execute(null);
                (obj as AutoCompleteTextView).RequestFocus();
                var inputManager = (InputMethodManager)this.Activity.GetSystemService(Android.Content.Context.InputMethodService);
                var currentFocus = this.Activity.CurrentFocus;
                if (currentFocus != null)
                {
                    inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
                }
            };

            return this.view;
        }
    }
}