namespace MosPolytechHelper.Features.StudentTimetable
{
    using Android.OS;
    using Android.Support.V4.View;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using MosPolytechHelper.Adapters;
    using MosPolytechHelper.Common.Interfaces;
    using System;
    using System.ComponentModel;

    class TimetableView : Android.Support.V4.App.Fragment
    {
        TimetableVm viewModel;
        View view;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        Android.Support.V4.App.Fragment[] fragments;
        ViewPager viewPager;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.FullTimetable):
                    (this.viewPager.Adapter as ViewPagerAdapter).UpdateTimetable(this.viewModel.FullTimetable);
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

        public TimetableView(ILoggerFactory loggerFactory)
        {
            this.viewModel = new TimetableVm(loggerFactory);
            this.viewModel.PropertyChanged += OnPropertyChanged;
            this.logger = loggerFactory.Create<TimetableView>();
        }

        public static TimetableView NewInstance(ILoggerFactory loggerFactory)
        {
            var fragment = new TimetableView(loggerFactory);
            //fragment.Arguments = new Bundle();
            //fragment.Arguments.PutString("title", title);
            //fragment.Arguments.PutString("icon", icon);
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //if (Arguments != null)
            //{
            //    if (Arguments.ContainsKey("title"))
            //        this.title = (string)Arguments.Get("title");
            //    if (Arguments.ContainsKey("icon"))
            //        this.icon = (string)Arguments.Get("icon");
            //}
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.view = inflater.Inflate(Resource.Layout.fragment_student_timetable, container, false);


            this.viewPager = this.view.FindViewById<ViewPager>(Resource.Id.viewpager);
            //this.viewPager.PageSelected += ViewPager_PageSelected;
            var viewPagerAdaper = new ViewPagerAdapter(this.Context, this.viewModel.FullTimetable);
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