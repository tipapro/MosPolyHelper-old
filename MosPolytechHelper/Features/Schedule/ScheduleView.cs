namespace MosPolyHelper.Features.Schedule
{
    using Android.Content;
    using Android.Graphics;
    using Android.OS;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using AndroidX.Core.View;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.Preference;
    using AndroidX.ViewPager.Widget;
    using Google.Android.Material.BottomAppBar;
    using Google.Android.Material.BottomSheet;
    using Google.Android.Material.Tabs;
    using MosPolyHelper.Adapters;
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Features.Schedule.Common;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading;

    class ScheduleView : FragmentBase
    {
        ScheduleVm viewModel;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        ViewPager viewPager;
        bool isNormalMode = true;
        Button scheduleTypePreference;
        ObservableCollection<string> checkedGroups;
        ObservableCollection<string> checkedLessonTypes;
        ObservableCollection<string> checkedTeachers;
        ObservableCollection<string> checkedLessonTitles;
        ObservableCollection<string> checkedAuditoriums;
        Schedule[] schedules;
        string[] lessonTitles;
        string[] teachers;
        string[] auditoriums;
        string[] lessonTypes;
        CancellationTokenSource cts;

        string regularString;
        string sessionString;

        bool isAdvancedSearch;

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
                    this.scheduleTypePreference.Text = GetTypeText(this.viewModel.ScheduleType);
                    break;
                default:
                    this.logger.Warn("Event OnPropertyChanged was not proccessed correctly. Property: {PropertyName}", e.PropertyName);
                    break;
            }
        }

        void OnLessonClick(Lesson lesson, DateTime date)
        {
            var fragment = ScheduleLessonInfoView.NewInstance();
            this.viewModel.LessonClick(lesson, date);
            (this.Activity as MainView)?.ChangeFragment(fragment, false);
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
            SetUpSchedule(schedule, DateTime.MinValue, modeChanged);
        }

        void SetUpSchedule(Schedule schedule, DateTime toDate, bool modeChanged = false)
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
            var normalAdapter = this.viewPager.Adapter as DailyShedulePageAdapter;
            var gridAdapter = this.viewPager.Adapter as DailySheduleGridPageAdapter;
            if (this.isNormalMode)
            {
                if (!modeChanged && (normalAdapter?.Schedule?.IsSession != schedule?.IsSession) ||
                    modeChanged && (gridAdapter?.Schedule?.IsSession != schedule?.IsSession))
                {
                    currDate = DateTime.Today;
                }
                else if (modeChanged && gridAdapter != null)
                {
                    currDate = toDate == DateTime.MinValue ? gridAdapter.CurrentDate : toDate;
                }
                else
                {
                    currDate = normalAdapter?.FirstPosDate.AddDays(this.viewPager.CurrentItem) ?? DateTime.Today;
                }
                this.viewPager.Adapter = normalAdapter = new DailyShedulePageAdapter(
                    schedule, this.viewModel.ScheduleFilter, this.viewModel.ShowEmptyLessons, this.viewModel.ShowColoredLessons, this.isAdvancedSearch);
                this.viewPager.CurrentItem = (currDate - normalAdapter.FirstPosDate).Days;
                normalAdapter.LessonClick += OnLessonClick;
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
                    this.viewPager.Adapter = gridAdapter = new DailySheduleGridPageAdapter(
                        schedule, this.viewModel.ScheduleFilter, this.viewModel.ShowEmptyLessons, this.viewModel.ShowColoredLessons);
                    gridAdapter.ItemClick += toDate =>
                    {
                        SetNormalMode();
                        SetUpSchedule(schedule, toDate, true);
                    };
                }
                else
                {
                    gridAdapter.BuildSchedule(schedule, this.viewModel.ScheduleFilter, this.viewModel.ShowEmptyLessons,
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
            this.checkedGroups = new ObservableCollection<string>();
            this.checkedLessonTitles = new ObservableCollection<string>();
            this.checkedLessonTypes = new ObservableCollection<string>();
            this.checkedTeachers = new ObservableCollection<string>();
            this.checkedAuditoriums = new ObservableCollection<string>();
        }

        public ScheduleView(ScheduleVm vm) : base(Fragments.ScheduleMain)
        {
            this.viewModel = vm;
            this.checkedGroups = new ObservableCollection<string>();
            this.checkedLessonTitles = new ObservableCollection<string>();
            this.checkedLessonTypes = new ObservableCollection<string>();
            this.checkedTeachers = new ObservableCollection<string>();
            this.checkedAuditoriums = new ObservableCollection<string>();
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
            this.isNormalMode = false;
            var tab = this.View.FindViewById<PagerTabStrip>(Resource.Id.tab_schedule);
            tab.Visibility = ViewStates.Gone;
        }

        public void SetNormalMode()
        {
            this.isNormalMode = true;
            var tab = this.View.FindViewById<PagerTabStrip>(Resource.Id.tab_schedule);
            tab.Visibility = ViewStates.Gone;
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

        class BottomSheetCallbacks : BottomSheetBehavior.BottomSheetCallback
        {
            public event Action<View, float> Slided;
            public event Action<View, int> StateChanged;

            public override void OnSlide(View bottomSheet, float slideOffset)
            {
                Slided?.Invoke(bottomSheet, slideOffset);
            }
            public override void OnStateChanged(View bottomSheet, int newState)
            {
                StateChanged?.Invoke(bottomSheet, newState);
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var homeBtn = this.View.FindViewById<ImageButton>(Resource.Id.button_home);
            // TODO: Move to Vm
            homeBtn.Click += (obj, arg) =>
            {
                if (this.isNormalMode)
                {
                    if (this.viewPager?.Adapter is DailyShedulePageAdapter adapter)
                    {
                        this.viewPager?.SetCurrentItem((DateTime.Today - adapter.FirstPosDate).Days, false);
                        ParabolicTransformer.SetViewToDefault(adapter.GetView(this.viewPager.CurrentItem));
                    }
                }
                else
                {
                    (this.viewPager.Adapter as DailySheduleGridPageAdapter)?.GoHome();
                }

            };
            var bottomSheet = this.View.FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
            var behavior = BottomSheetBehavior.From(bottomSheet);
            behavior.State = BottomSheetBehavior.StateHidden;

            //var chipGroup = this.view.FindViewById<ChipGroup>(Resource.Id.chipGroup);


            var textGroups = this.View.FindViewById<TextView>(Resource.Id.text_groups);
            textGroups.Click += (obj, arg) =>
            {
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.SimpleFilter(this.viewModel.GroupList, this.checkedGroups)));
            };
            var blockLayout = this.View.FindViewById<RelativeLayout>(Resource.Id.layout_block);
            var progressBar = blockLayout.FindViewById<ProgressBar>(Resource.Id.progressBar);
            var progressText = blockLayout.FindViewById<TextView>(Resource.Id.text_progress);
            var cancelBtn = blockLayout.FindViewById<Button>(Resource.Id.btn_cancel);
            cancelBtn.Click += (obj, arg) =>
            {
                this.cts.Cancel();
                cancelBtn.Visibility = ViewStates.Gone;
            };
            this.checkedGroups.CollectionChanged +=
                (obj, arg) =>
                {
                    textGroups.Text = string.Join(", ", this.checkedGroups);
                    if (textGroups.Text == string.Empty)
                    {
                        textGroups.Text = "Any groups";
                    }
                    blockLayout.Visibility = ViewStates.Visible;
                };
            var acceptGroupsBtn = this.View.FindViewById<Button>(Resource.Id.btn_acceptGroups);
            acceptGroupsBtn.Click += async (obj, arg) =>
            {
                blockLayout.Visibility = ViewStates.Visible;
                progressBar.Visibility = ViewStates.Visible;
                progressText.Visibility = ViewStates.Visible;
                cancelBtn.Visibility = ViewStates.Visible;
                this.cts = new CancellationTokenSource();
                try
                {
                    (this.schedules, this.lessonTitles, this.teachers, this.auditoriums, this.lessonTypes) =
                    await this.viewModel.GetAdvancedSearchData(
                        this.checkedGroups.Count == 0 ? this.viewModel.GroupList : this.checkedGroups as IList<string>,
                        this.cts.Token, progress => Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                        {
                            progressBar.Progress = progress;
                            progressText.Text = progress / 100 + " %";
                        }));
                    Toast.MakeText(this.Context, "Расписания загружены", ToastLength.Short).Show();
                    blockLayout.Visibility = ViewStates.Gone;
                    progressBar.Visibility = ViewStates.Gone;
                    progressText.Visibility = ViewStates.Gone;
                    progressBar.Progress = 0;
                    progressText.Text = "0 %";
                    cancelBtn.Visibility = ViewStates.Gone;
                }
                catch (System.OperationCanceledException)
                {
                    this.cts.Dispose();
                    Toast.MakeText(this.Context, "Загрузка отменена", ToastLength.Short).Show();
                    progressBar.Progress = 0;
                    progressText.Text = "0 %";
                    blockLayout.Visibility = ViewStates.Visible;
                    progressBar.Visibility = ViewStates.Gone;
                    progressText.Visibility = ViewStates.Gone;
                }
            };


            var textLessonTitles = this.View.FindViewById<TextView>(Resource.Id.text_lesson_titles);
            textLessonTitles.Click += (obj, arg) =>
            {
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.AdvancedFilter(this.lessonTitles, this.checkedLessonTitles)));
            };
            this.checkedLessonTitles.CollectionChanged +=
                (obj, arg) =>
                {
                    textLessonTitles.Text = string.Join(", ", this.checkedLessonTitles);
                    if (textLessonTitles.Text == string.Empty)
                    {
                        textLessonTitles.Text = "Any lesson titles";
                    }
                };

            var textTeachers = this.View.FindViewById<TextView>(Resource.Id.text_teachers);
            textTeachers.Click += (obj, arg) =>
            {
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.AdvancedFilter(this.teachers, this.checkedTeachers)));
            };
            this.checkedTeachers.CollectionChanged +=
                (obj, arg) =>
                {
                    textTeachers.Text = string.Join(", ", this.checkedTeachers);
                    if (textTeachers.Text == string.Empty)
                    {
                        textTeachers.Text = "Any teachers";
                    }
                };

            var textAuditoriums = this.View.FindViewById<TextView>(Resource.Id.text_auditoriums);
            textAuditoriums.Click += (obj, arg) =>
            {
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.AdvancedFilter(this.auditoriums, this.checkedAuditoriums)));
            };
            this.checkedAuditoriums.CollectionChanged +=
                (obj, arg) =>
                {
                    textAuditoriums.Text = string.Join(", ", this.checkedAuditoriums);
                    if (textAuditoriums.Text == string.Empty)
                    {
                        textAuditoriums.Text = "Any auditoriums";
                    }
                };

            var textLessonTypes = this.View.FindViewById<TextView>(Resource.Id.text_lesson_types);
            textLessonTypes.Click += (obj, arg) =>
            {
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.SimpleFilter(this.lessonTypes, this.checkedLessonTypes)));
            };
            this.checkedLessonTypes.CollectionChanged +=
                (obj, arg) =>
                {
                    textLessonTypes.Text = string.Join(", ", this.checkedLessonTypes);
                    if (textLessonTypes.Text == string.Empty)
                    {
                        textLessonTypes.Text = "Any lesson types";
                    }
                };
            var applyButton = this.View.FindViewById<Button>(Resource.Id.btn_search);
            applyButton.Click += (obj, arg) =>
            {
                var q = new Schedule.AdvancedSerach();
                var newSchedule = q.Filter(this.schedules,
                    this.checkedLessonTitles.Count == 0 ? this.lessonTitles : this.checkedLessonTitles as IList<string>,
                    this.checkedLessonTypes.Count == 0 ? this.lessonTypes : this.checkedLessonTypes as IList<string>,
                    this.checkedAuditoriums.Count == 0 ? this.auditoriums : this.checkedAuditoriums as IList<string>,
                    this.checkedTeachers.Count == 0 ? this.teachers : this.checkedTeachers as IList<string>);
                this.isAdvancedSearch = true;
                this.viewModel.Schedule = newSchedule;
                this.textGroupTitle.Text = this.checkedGroups.Count == 1 ? this.checkedGroups[0] : "Any";
            };


            var advancedSearchBtn = this.View.FindViewById<ImageButton>(Resource.Id.button_advanced_search);
            advancedSearchBtn.Click += (obj, arg) =>
            {
                var bottomSheet = this.View.FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
                var behavior = BottomSheetBehavior.From(bottomSheet);
                behavior.State = BottomSheetBehavior.StateExpanded;
            };


            var filterBtn = this.View.FindViewById<ImageButton>(Resource.Id.button_schedule_filter);
            filterBtn.Click += (obj, arg) =>
            {
                if (!(this.Activity is MainView mainActivity))
                {
                    return;
                }
                if (!(mainActivity.CurrentPopupWindow is SchedulePreferencesView))
                {
                    var inflater = LayoutInflater.From(this.Activity);
                    var layout = inflater.Inflate(Resource.Layout.popup_schedule_preferences, null);
                    (this.Activity as MainView).CurrentPopupWindow = new SchedulePreferencesView(layout, WindowManagerLayoutParams.WrapContent,
                        WindowManagerLayoutParams.WrapContent, DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator())
                    {
                        //popupPreferences.ShowAtLocation(FindViewById<RelativeLayout>(Resource.Id.layout_main), GravityFlags.Top | GravityFlags.Right, 0, 0);
                        OutsideTouchable = true,
                        Focusable = true
                    };
                }
                if (!mainActivity.CurrentPopupWindow.IsShowing)
                {
                    mainActivity.CurrentPopupWindow.ShowAsDropDown(this.scheduleTypePreference, 0, 0, GravityFlags.Right);
                }
                else
                {
                    mainActivity.CurrentPopupWindow.Dismiss();
                }
            };

            var gridBtn = this.View.FindViewById<ImageButton>(Resource.Id.button_grid_mode);
            // TODO: Move to Vm
            gridBtn.Click += (obj, arg) =>
            {
                if (this.isNormalMode)
                {
                    SetGridMode();
                }
                else
                {
                    SetNormalMode();
                }
                SetUpSchedule(this.viewModel.Schedule, true);
            };

            var prefs = PreferenceManager.GetDefaultSharedPreferences(this.Context);
            this.textGroupTitle = this.View.FindViewById<AutoCompleteTextView>(Resource.Id.text_group_title);
            this.textGroupTitle.Adapter = new ArrayAdapter<string>(
                this.Activity, Resource.Layout.item_group_list, this.viewModel.GroupList);
            this.textGroupTitle.Text = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle);
            this.textGroupTitle.KeyPress +=
                (obj, arg) =>
                {
                    if (arg.Event.Action != KeyEventActions.Up)
                    {
                        arg.Handled = false;
                        return;
                    }
                    if (arg.KeyCode == Keycode.Back)
                    {
                        if ((obj as AutoCompleteTextView).IsFocused)
                        {
                            (obj as AutoCompleteTextView).ClearFocus();
                        }
                        this.Activity?.OnBackPressed();
                        return;
                    }
                    else if (arg.KeyCode == Keycode.Enter)
                    {
                        this.isAdvancedSearch = false;
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
                        return;
                    }
                    else
                    {
                        arg.Handled = false;
                    }
                };
            this.textGroupTitle.ItemClick += (obj, arg) =>
            {
                this.isAdvancedSearch = false;
                this.viewModel.GroupTitle = (string)arg.Parent.GetItemAtPosition(arg.Position);
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
            this.scheduleTypePreference = this.Activity?.FindViewById<Button>(Resource.Id.button_schedule_type);
            this.viewModel.ScheduleType = (ScheduleType)prefs.GetInt(PreferencesConstants.ScheduleTypePreference, 0);
            this.regularString = this.Context.GetString(Resource.String.text_schedule_type_regular);
            this.sessionString = this.Context.GetString(Resource.String.text_schedule_type_session);
            this.scheduleTypePreference.Text = GetTypeText(this.viewModel.ScheduleType);

            this.scheduleTypePreference.Click += (obj, arg) =>
            {
                var type = this.viewModel.ScheduleType == ScheduleType.Regular ? ScheduleType.Session : ScheduleType.Regular;
                this.viewModel.ScheduleTypeChanged.Execute(type);
                (obj as Button).Text = GetTypeText(type);
                prefs.Edit().PutInt(PreferencesConstants.ScheduleTypePreference, (int)this.viewModel.ScheduleType).Apply();
            };

        }

        string GetTypeText(ScheduleType type)
        {
            if (type == ScheduleType.Regular)
            {
                return this.regularString;
            }
            else
            {
                return this.sessionString;
            }
        }
        public class ParabolicTransformer : Java.Lang.Object, ViewPager.IPageTransformer
        {

            public void TransformPage(View view, float position)
            {
                if (position < -1)
                {
                    view.TranslationX = 0f;
                    view.Alpha = 0f;
                }
                else if (position <= 1)
                {
                    view.TranslationX = view.Width * position * position * position / 3;

                    view.Alpha = 1f;
                }
                else
                {
                    view.TranslationX = 0f;
                    view.Alpha = 0f;
                }
            }

            public static void SetViewToDefault(View view)
            {
                view.TranslationX = 0;
                view.Alpha = 1f;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_schedule, container, false);
            this.viewPager = view.FindViewById<ViewPager>(Resource.Id.viewpager);
            this.viewPager.SetPageTransformer(false, new ParabolicTransformer());
            var tabLayout = view.FindViewById<TabLayout>(Resource.Id.tab_day_week);
            var tabs = new TabLayout.Tab[] { tabLayout.NewTab(), tabLayout.NewTab(), tabLayout.NewTab(),
                tabLayout.NewTab(), tabLayout.NewTab(), tabLayout.NewTab(), tabLayout.NewTab() };
            for (int i = 0; i < 7; i++)
            {
                tabs[i].View.Clickable = false;
            }
            tabLayout.AddTab(tabs[1]);
            tabLayout.AddTab(tabs[2]);
            tabLayout.AddTab(tabs[3]);
            tabLayout.AddTab(tabs[4]);
            tabLayout.AddTab(tabs[5]);
            tabLayout.AddTab(tabs[6]);
            tabLayout.AddTab(tabs[0]);

            this.viewPager.PageScrolled += (obj, arg) =>
            {
                if (this.viewPager.Adapter is DailyShedulePageAdapter adapter)
                {
                    int pos = arg.Position + (int)Math.Round(arg.PositionOffset);
                    var day = adapter.FirstPosDate.DayOfWeek;
                    var tab = tabs[((int)day + pos % 7) % 7];
                    if (!tab.IsSelected)
                    {
                        tab.Select();
                    }
                }
            };

            var tab = view.FindViewById<PagerTabStrip>(Resource.Id.tab_schedule);
            tab.TextAlignment = TextAlignment.Center;
            tab.TabIndicatorColor = Color.Black.ToArgb();
            if (this.viewModel.Schedule != null)
            {
                SetUpSchedule(this.viewModel.Schedule);
            }
            //float scale = view.Context.Resources.DisplayMetrics.Density;
            //int dp8InPx = (int)(44 * scale + 0.5f);
            //var toolbar = view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //var layoutParams = ((RelativeLayout.LayoutParams)toolbar.LayoutParameters).Height = dp8InPx;
            //toolbar.RequestLayout();
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            var bottomAppBar = view.FindViewById<BottomAppBar>(Resource.Id.bottomAppBar);
            (this.Activity as MainView).SetSupportActionBar(bottomAppBar);
            (this.Activity as MainView).SupportActionBar.SetDisplayShowTitleEnabled(false);
            bottomAppBar.NavigationClick += (obj, arg) => drawer.OpenDrawer(GravityCompat.Start);
            return view;
        }

        protected override void Dispose(bool disposing)
        {
            this.viewModel.PropertyChanged -= OnPropertyChanged;
            base.Dispose(disposing);
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