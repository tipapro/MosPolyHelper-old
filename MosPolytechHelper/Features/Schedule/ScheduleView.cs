namespace MosPolyHelper.Features.Schedule
{
    using Android.Content;
    using Android.Graphics;
    using Android.OS;
    using Android.Support.V4.View;
    using Android.Support.V4.Widget;
    using Android.Support.V7.App;
    using Android.Support.V7.Preferences;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using MosPolyHelper.Adapters;
    using MosPolyHelper.Common;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Schedule.Common;
    using System;
    using System.ComponentModel;

    class ScheduleView : FragmentBase
    {
        ScheduleVm viewModel;
        View view;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        ViewPager viewPager;
        bool isNormalMode = true;
        Spinner scheduleTypePreference;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.Schedule):
                    SetUpSchedule(this.viewModel.Schedule);
                    break;
                case nameof(this.viewModel.GroupList):
                    SetUpGroupList(this.viewModel.GroupList);
                    break;
                case nameof(this.viewModel.ScheduleType):
                    this.scheduleTypePreference.SetSelection((int)this.viewModel.ScheduleType);
                    break;
                default:
                    this.logger.Warn("Event OnPropertyChanged was not proccessed correctly. Property: {PropertyName}", e.PropertyName);
                    break;
            }
        }

        void SetUpGroupList(string[] groupList)
        {
            if (this.textGroupTitle == null || this.Context == null)
            {
                return;
            }
            this.textGroupTitle.Adapter = new ArrayAdapter<string>(this.Context, Resource.Layout.item_group_list, groupList);
        }

        void SetUpSchedule(Schedule schedule, bool modeChanged = false)
        {
            if (this.viewPager == null)
            {
                return;
            }
            if (this.Context != null && !string.IsNullOrEmpty(schedule?.Group?.Comment))
            {
                Toast.MakeText(this.Context, schedule?.Group?.Comment, ToastLength.Long).Show();
            }
            DateTime currDate;
            var normalAdapter = this.viewPager.Adapter as ViewPagerNormalAdapter;
            var gridAdapter = this.viewPager.Adapter as ViewPagerGridAdapter;
            if (this.isNormalMode)
            {
                if (!modeChanged && (normalAdapter?.Schedule?.IsSession != schedule?.IsSession) ||
                    modeChanged && (gridAdapter?.Schedule?.IsSession != schedule?.IsSession))
                {
                    currDate = DateTime.Today;
                }
                else if (modeChanged && gridAdapter != null)
                {
                    currDate = gridAdapter.CurrentDate;
                }
                else
                {
                    currDate = normalAdapter?.FirstPosDate.AddDays(this.viewPager.CurrentItem) ?? DateTime.Today;
                }
                this.viewPager.Adapter = normalAdapter = new ViewPagerNormalAdapter(
                    schedule, this.viewModel.ShowEmptyLessons, this.viewModel.ShowColoredLessons);
                this.viewPager.CurrentItem = (currDate - normalAdapter.FirstPosDate).Days;
                this.viewPager.Adapter.NotifyDataSetChanged();
            }
            else
            {
                if (!modeChanged && (gridAdapter?.Schedule?.IsSession != schedule?.IsSession) ||
                    modeChanged && (normalAdapter?.Schedule?.IsSession != schedule?.IsSession))
                {
                    currDate = DateTime.Today;
                }
                else if (modeChanged && normalAdapter != null)
                {
                    currDate = normalAdapter.FirstPosDate.AddDays(this.viewPager.CurrentItem);
                }
                else
                {
                    currDate = gridAdapter?.CurrentDate ?? DateTime.Today;
                }
                if (gridAdapter == null)
                {
                    this.viewPager.Adapter = gridAdapter = new ViewPagerGridAdapter(
                        schedule, this.viewModel.ShowEmptyLessons, this.viewModel.ShowColoredLessons);
                }
                else
                {
                    gridAdapter.BuildSchedule(schedule, this.viewModel.ShowEmptyLessons,
                        this.viewModel.ShowColoredLessons);
                }
                gridAdapter.CurrentDate = currDate;
            }
            this.logger.Debug("Schedule set up");
        }

        public void OnFragmentChanged(ScheduleFragments scheduleFragment)
        {
            //using (var intent = new Intent(this.Context, Java.Lang.Class.FromType(typeof(ScheduleManagerView))))
            //{
            //    (this.Activity as MainActivity)?.StartActivity(intent);
            //}
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

        public void SetGridMode()
        {
            var tab = this.view.FindViewById<PagerTabStrip>(Resource.Id.tab_schedule);
            tab.Visibility = ViewStates.Gone;
        }

        public void SetNormalMode()
        {
            var tab = this.view.FindViewById<PagerTabStrip>(Resource.Id.tab_schedule);
            tab.Visibility = ViewStates.Visible;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var loggerFactory = DependencyInjector.GetILoggerFactory();
            this.logger = loggerFactory.Create<ScheduleView>();
            this.HasOptionsMenu = false;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            //inflater.Inflate(Resource.Menu.menu_schedule, menu);
            //base.OnCreateOptionsMenu(menu, inflater);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            var toolbar = this.view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            (this.Activity as MainActivity).SetSupportActionBar(toolbar);

            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this.Activity, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var homeBtn = this.view.FindViewById<ImageButton>(Resource.Id.button_home);
            // TODO: Move to Vm
            homeBtn.Click += (obj, arg) =>
            {
                if (this.isNormalMode)
                {
                    if ((this.viewPager?.Adapter as ViewPagerNormalAdapter) == null)
                    {
                        return;
                    }
                    this.viewPager?.SetCurrentItem(
                        (DateTime.Today - (this.viewPager.Adapter as ViewPagerNormalAdapter).FirstPosDate).Days, false);
                }
                else
                {
                    (this.viewPager.Adapter as ViewPagerGridAdapter)?.GoHome();
                }

            };

            var filterBtn = this.view.FindViewById<ImageButton>(Resource.Id.button_schedule_filter);
            filterBtn.Click += (obj, arg) =>
            {
                var mainActivity = this.Activity as MainActivity;
                if (mainActivity == null)
                {
                    return;
                }
                if (!(mainActivity.CurrentPopupWindow is SchedulePreferencesView))
                {
                    var inflater = LayoutInflater.From(this.Activity);
                    var layout = inflater.Inflate(Resource.Layout.popup_schedule_preferences, null);
                    (this.Activity as MainActivity).CurrentPopupWindow = new SchedulePreferencesView(layout, WindowManagerLayoutParams.WrapContent,
                        WindowManagerLayoutParams.WrapContent, DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator())
                    {
                        //popupPreferences.ShowAtLocation(FindViewById<RelativeLayout>(Resource.Id.layout_main), GravityFlags.Top | GravityFlags.Right, 0, 0);
                        OutsideTouchable = true,
                        Focusable = true
                    };
                }
                if (!mainActivity.CurrentPopupWindow.IsShowing)
                {
                    mainActivity.CurrentPopupWindow.ShowAsDropDown(
                            mainActivity.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar), 0, 0, GravityFlags.Right);
                }
                else
                {
                    mainActivity.CurrentPopupWindow.Dismiss();
                }
            };

            var gridBtn = this.view.FindViewById<ImageButton>(Resource.Id.button_grid_mode);
            // TODO: Move to Vm
            gridBtn.Click += (obj, arg) =>
            {
                if (this.isNormalMode)
                {
                    SetGridMode();
                    this.isNormalMode = false;
                }
                else
                {
                    SetNormalMode();
                    this.isNormalMode = true;
                }
                SetUpSchedule(this.viewModel.Schedule, true);
            };

            var prefs = PreferenceManager.GetDefaultSharedPreferences(this.Context);
            this.textGroupTitle = this.view.FindViewById<AutoCompleteTextView>(Resource.Id.text_group_title);
            this.textGroupTitle.Adapter = new ArrayAdapter<string>(
                this.Activity, Resource.Layout.item_group_list, this.viewModel.GroupList);
            this.textGroupTitle.Text = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle);
            this.textGroupTitle.KeyPress +=
                (obj, arg) =>
                {
                    if (arg.KeyCode != Keycode.Enter)
                    {
                        return;
                    }
                    if (arg.Event.Action != KeyEventActions.Up)
                    {
                        return;
                    }
                    this.viewModel.GroupTitle = (obj as AutoCompleteTextView)?.Text;
                    if (this.viewModel.GroupTitle.Length >= 2 && this.viewModel.GroupTitle[0] == '/'
                    && this.viewModel.GroupTitle[1] == 'l')
                    {
                        try
                        {
                            int count = -1;
                            if (this.viewModel.GroupTitle.Length > 2)
                            {
                                string[] strArray = this.viewModel.GroupTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (strArray.Length > 1)
                                {
                                    if (!int.TryParse(strArray[1], out count))
                                    {
                                        count = -1;
                                    }
                                }
                            }
                            if (this.Activity == null)
                            {
                                return;
                            }
                            string logs = LoggerFactory.ReadAllLogs(count);
                            if (logs == null)
                            {
                                Toast.MakeText(this.Activity, "Logs are not found", ToastLength.Long).Show();
                                return;
                            }
                            var clipboardManager = (ClipboardManager)this.Activity.GetSystemService(Context.ClipboardService);
                            var clipData = ClipData.NewPlainText("logs", logs);
                            clipboardManager.PrimaryClip = clipData;
                            Toast.MakeText(this.Activity, "Logs were copied to buffer", ToastLength.Long).Show();
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(ex);
                        }
                        return;
                    }
                    prefs.Edit().PutString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle).Apply();
                    this.viewModel.SubmitGroupCommand.Execute(null);
                    (obj as AutoCompleteTextView).DismissDropDown();
                    var currentFocus = this.Activity?.CurrentFocus;
                    if (currentFocus != null)
                    {
                        var inputManager = (InputMethodManager)this.Activity?.GetSystemService(Context.InputMethodService);
                        inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
                    }
                    (obj as AutoCompleteTextView).ClearFocus();
                };
            this.textGroupTitle.KeyPress += (obj, arg) =>
            {
                if (arg.KeyCode != Keycode.Back)
                {
                    return;
                }
                if (arg.Event.Action != KeyEventActions.Up)
                {
                    return;
                }
                if ((obj as AutoCompleteTextView).IsFocused)
                {
                    (obj as AutoCompleteTextView).ClearFocus();
                }
                (this.view.Context as MainActivity).OnBackPressed();
            };
            this.textGroupTitle.ItemClick += (obj, arg) =>
            {
                this.viewModel.GroupTitle = (obj as AutoCompleteTextView)?.Text;
                prefs.Edit().PutString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle).Apply();
                this.viewModel.SubmitGroupCommand.Execute(null);
                var currentFocus = this.Activity?.CurrentFocus;
                if (currentFocus != null)
                {
                    var inputManager = (InputMethodManager)this.Activity?.GetSystemService(Context.InputMethodService);
                    inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
                }
                (obj as AutoCompleteTextView).ClearFocus();
            };
            this.scheduleTypePreference = this.Activity?.FindViewById<Spinner>(Resource.Id.spinner_text_schedule_type);
            this.viewModel.ScheduleType = (ScheduleType)prefs.GetInt(PreferencesConstants.ScheduleTypePreference, 0);
            this.scheduleTypePreference.SetSelection((int)this.viewModel.ScheduleType);
            this.scheduleTypePreference.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.ScheduleType != arg.Position)
                {
                    this.viewModel.ScheduleTypeSelected.Execute(arg.Position);
                    prefs.Edit().PutInt(PreferencesConstants.ScheduleTypePreference, arg.Position).Apply();
                }
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.view = inflater.Inflate(Resource.Layout.fragment_schedule, container, false);
            this.viewPager = this.view.FindViewById<ViewPager>(Resource.Id.viewpager);
            var tab = this.view.FindViewById<PagerTabStrip>(Resource.Id.tab_schedule);
            tab.SetTextColor(Color.White.ToArgb());
            tab.TextAlignment = TextAlignment.Center;
            tab.TabIndicatorColor = Color.White.ToArgb();
            if (this.viewModel.Schedule != null)
            {
                SetUpSchedule(this.viewModel.Schedule);
            }
            return this.view;
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);

            var prefs = PreferenceManager.GetDefaultSharedPreferences(this.Context);


            if (this.viewModel == null)
            {
                var scheduleFilter = Schedule.Filter.DefaultFilter;
                scheduleFilter.DateFitler = (DateFilter)prefs.GetInt(PreferencesConstants.ScheduleDateFilter,
                    (int)scheduleFilter.DateFitler);
                //scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt(PreferencesConstants.ScheduleModuleFilter, 
                //  (int)scheduleFilter.ModuleFilter);
                scheduleFilter.SessionFilter = prefs.GetBoolean(PreferencesConstants.ScheduleSessionFilter, scheduleFilter.SessionFilter);
                string groupTitle = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, null);

                bool isSession = prefs.GetInt(PreferencesConstants.ScheduleTypePreference, 0) == 1;

                this.viewModel = new ScheduleVm(DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator(),
                    isSession, scheduleFilter)
                {
                    GroupTitle = groupTitle
                };
                this.viewModel.ShowEmptyLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowEmptyLessons, false);
                this.viewModel.ShowColoredLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowColoredLessons, true);

                if (groupTitle != null)
                {
                    this.viewModel.SetUpScheduleAsync(false);
                }
            }

            this.viewModel.PropertyChanged += OnPropertyChanged;
            this.viewModel.FragmentChanged += OnFragmentChanged;
            this.viewModel.Announced += (msg) =>
            {
                if (this.Activity != null && Xamarin.Essentials.MainThread.IsMainThread)
                {
                    Toast.MakeText(this.Activity, msg, ToastLength.Long).Show();
                }
            };
        }
    }
}