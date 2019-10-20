namespace MosPolytechHelper.Features.StudentSchedule
{
    using Android.Content;
    using Android.OS;
    using Android.Support.Design.Widget;
    using Android.Support.V4.View;
    using Android.Support.V4.Widget;
    using Android.Support.V7.App;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using MosPolytechHelper.Adapters;
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;
    using MosPolytechHelper.Features.StudentSchedule.Common;
    using System.ComponentModel;

    class ScheduleView : FragmentBase
    {
        ScheduleVm viewModel;
        View view;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        ViewPager viewPager;
        bool? prevIsSession;
        bool isSession;
        string groupTitle;
        Schedule.Filter scheduleFilter;


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
            if (this.viewPager == null)
            {
                return;
            }
            if (this.Context != null && !string.IsNullOrEmpty(schedule?.Group?.Comment))
            {
                Toast.MakeText(this.Context, schedule?.Group?.Comment, ToastLength.Long).Show();
            }
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

        public void OnFragmentChanged(ScheduleFragments scheduleFragment)
        {
            using (var intent = new Intent(this.Context, Java.Lang.Class.FromType(typeof(ScheduleManagerView))))
            {
                (this.Activity as MainActivity)?.StartActivity(intent);
            }
        }

        public ScheduleView() : base(Fragments.ScheduleMain)
        {
        }

        public ScheduleView(ScheduleVm vm) : base(Fragments.ScheduleMain)
        {
            this.viewModel = vm;
        }

        public static ScheduleView NewInstance()
        {
            var fragment = new ScheduleView();
            return fragment;
        }
        public static ScheduleView NewInstance(ScheduleVm vm)
        {
            var fragment = new ScheduleView(vm);
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var loggerFactory = DependencyInjector.GetILoggerFactory();
            this.logger = loggerFactory.Create<ScheduleView>();
            this.HasOptionsMenu = true;
            bool isPreloaded = this.viewModel != null;

            if (!isPreloaded)
            {
                this.viewModel = new ScheduleVm(loggerFactory, DependencyInjector.GetIMediator(), this.isSession, this.scheduleFilter);
            }
            this.viewModel.PropertyChanged += OnPropertyChanged;
            this.viewModel.FragmentChanged += OnFragmentChanged;

            this.viewModel.SubscribeOnAnnouncement((msg) =>
            {
                if (this.Activity != null)
                {
                    Toast.MakeText(this.Activity, msg, ToastLength.Long).Show();
                }
            });

            if (!isPreloaded)
            {
                this.viewModel.SetUpScheduleAsync(false);
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_schedule, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            var toolbarLayout = this.Activity.LayoutInflater.Inflate(Resource.Layout.toolbar_schedule, null);
            var toolbar = toolbarLayout.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            (this.Activity as MainActivity).SetSupportActionBar(toolbar);

            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this.Activity, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            var appBarLayout = (this.Activity as MainActivity).FindViewById<AppBarLayout>(Resource.Id.appbar);
            appBarLayout.RemoveAllViews();
            appBarLayout.AddView(toolbarLayout);

            var toolbarParams = (AppBarLayout.LayoutParams)toolbar.LayoutParameters;
            toolbarParams.ScrollFlags = 0;


            var prefs = this.Activity.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            this.textGroupTitle = toolbarLayout.FindViewById<AutoCompleteTextView>(Resource.Id.text_group_title);
            this.textGroupTitle.Adapter = new ArrayAdapter<string>(
                this.Activity, Resource.Layout.item_group_list, this.viewModel.GroupList);
            this.textGroupTitle.Text = prefs.GetString("ScheduleGroupTitle", this.viewModel.GroupTitle);
            this.textGroupTitle.AfterTextChanged += (obj, arg) =>
            {
                this.viewModel.GroupTitle = (obj as AutoCompleteTextView)?.Text;
                var prefsEditor = prefs.Edit();
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
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.view = inflater.Inflate(Resource.Layout.fragment_schedule, container, false);
            this.viewPager = this.view.FindViewById<ViewPager>(Resource.Id.viewpager);

            if (this.viewModel.Schedule != null)
            {
                SetUpSchedule(this.viewModel.Schedule);
            }

            return this.view;
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);

            var prefs = context.GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            this.scheduleFilter = Schedule.Filter.DefaultFilter;

            this.scheduleFilter.DateFitler = (DateFilter)prefs.GetInt("ScheduleDateFilter", (int)this.scheduleFilter.DateFitler);
            this.scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt("ScheduleModuleFilter", (int)this.scheduleFilter.ModuleFilter);
            this.scheduleFilter.SessionFilter = prefs.GetBoolean("ScheduleSessionFilter", this.scheduleFilter.SessionFilter);

            this.isSession = prefs.GetInt("ScheduleTypePreference", 0) == 1;
            this.groupTitle = prefs.GetString("ScheduleGroupTitle", null);
        }
    }
}