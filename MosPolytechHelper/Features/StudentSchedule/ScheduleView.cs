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
    using MosPolytechHelper.Domain;
    using System;
    using System.ComponentModel;

    class ScheduleView : Android.Support.V4.App.Fragment
    {
        ScheduleVm viewModel;
        View view;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        ViewPager viewPager;
        bool? prevIsSession;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.Schedule):
                    SetUpSchedule(this.viewModel.Schedule);
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

        void SetUpSchedule(Schedule schedule)
        {
            // Check on null
            // ...
            //if (schedule.IsSession)
            //    {
            //}
            //else
            //{
            //    (this.viewPager.Adapter as ViewPagerAdapter).SetCount(int.MaxValue);
            //}

            SetUpViewPagerAdapter(schedule);
            //if (this.viewPager.Adapter == null)
            //{
            //    if (schedule == null)
            //    {
            //        return;
            //    }

            //}
            //else
            //{
            //    (this.viewPager.Adapter as ViewPagerAdapter).UpdateSchedule(schedule);


            //    this.viewPager.SetCurrentItem((this.viewPager.Adapter as ViewPagerAdapter).FirstPos, false);
            //    (this.viewPager.Adapter as ViewPagerAdapter).UpdateSchedule2(schedule);
            //    if (this.prevIsSession != schedule.IsSession)
            //    {
            //        this.prevIsSession = schedule.IsSession;

            //    }
            //}
        }

        void SetUpViewPagerAdapter(Schedule schedule)
        {
            
            var viewPagerAdaper = new ViewPagerAdapter(schedule);
            int prevPos = this.viewPager.CurrentItem;
            this.viewPager.Adapter = viewPagerAdaper;
            if (this.prevIsSession != schedule?.IsSession)
            {
                this.viewPager.SetCurrentItem(viewPagerAdaper.FirstPos, false);
            } 
            else
            {
                this.viewPager.SetCurrentItem(prevPos, false);
            }
            this.viewPager.Adapter.NotifyDataSetChanged();
            this.prevIsSession = schedule?.IsSession;
        }

        public ScheduleView()
        {
        }

        public static ScheduleView NewInstance()
        {
            var fragment = new ScheduleView();
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var loggerFactory = DependencyInjector.GetILoggerFactory();
            this.logger = loggerFactory.Create<ScheduleView>();

            var scheduleFilter = Schedule.Filter.Empty;
            this.viewModel = new ScheduleVm(loggerFactory, DependencyInjector.GetIMediator(), scheduleFilter);
            this.viewModel.PropertyChanged += OnPropertyChanged;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            var scheduleFilter = Schedule.Filter.Empty;
            var prefs = this.Activity.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);

            scheduleFilter.DateFitler = (DateFilter)prefs.GetInt("ScheduleDateFilter", (int)scheduleFilter.DateFitler);
            scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt("ScheduleModuleFilter", (int)scheduleFilter.ModuleFilter);
            scheduleFilter.SessionFilter = prefs.GetBoolean("ScheduleSessionFilter", scheduleFilter.SessionFilter);

            this.viewModel.IsSession = prefs.GetInt("ScheduleTypePreference", 0) == 1;
            this.viewModel.GroupTitle = prefs.GetString("ScheduleGroupTitle", this.viewModel.GroupTitle);
            this.viewModel.ScheduleFilter = scheduleFilter;
            this.viewModel.SubscribeOnAnnouncement((msg) =>
            {
                if (this.Activity != null)
                {
                    Toast.MakeText(this.Activity, msg, ToastLength.Long).Show();
                }
            });
            this.viewModel.SetUpSchedule();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.view = inflater.Inflate(Resource.Layout.fragment_schedule, container, false);

            if (this.Activity == null)
            {
                throw new NullReferenceException("ScheduleView this.Activity == null");
            }

            //var toolbar = this.Activity.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //(this.Activity as AppCompatActivity).SupportActionBar?.Hide();
            //(this.Activity as AppCompatActivity).SetSupportActionBar(toolbar);

            var prefs = this.Activity.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            var prefsEditor = prefs.Edit();

            this.viewPager = this.view.FindViewById<ViewPager>(Resource.Id.viewpager);

            SetUpViewPagerAdapter(this.viewModel.Schedule);

            this.textGroupTitle = this.Activity.FindViewById<AutoCompleteTextView>(Resource.Id.text_group_title);
            this.textGroupTitle.Adapter = new ArrayAdapter<string>(
                this.Activity, Resource.Layout.item_group_list, this.viewModel.GroupList);
            this.textGroupTitle.Text = prefs.GetString("ScheduleGroupTitle", this.viewModel.GroupTitle);
            this.textGroupTitle.AfterTextChanged += (obj, arg) =>
            {
                this.viewModel.GroupTitle = (obj as AutoCompleteTextView)?.Text;
                prefsEditor.PutString("ScheduleGroupTitle", this.viewModel.GroupTitle);
                prefsEditor.Apply();
            };
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