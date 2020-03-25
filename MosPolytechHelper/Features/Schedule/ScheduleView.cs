namespace MosPolyHelper.Features.Schedule
{
    using Android.Content;
    using Android.OS;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using AndroidX.Core.View;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.Preference;
    using AndroidX.SwipeRefreshLayout.Widget;
    using AndroidX.ViewPager.Widget;
    using Google.Android.Material.BottomAppBar;
    using Google.Android.Material.BottomSheet;
    using Google.Android.Material.Tabs;
    using MosPolyHelper.Adapters;
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class ScheduleView : FragmentBase
    {
        ScheduleVm viewModel;
        ILogger logger;
        AutoCompleteTextView textGroupTitle;
        ViewPager viewPager;
        SwipeRefreshLayout swipeToRefresh;
        Button scheduleType;
        ObservableCollection<int> checkedGroups;
        ObservableCollection<int> checkedLessonTypes;
        ObservableCollection<int> checkedTeachers;
        ObservableCollection<int> checkedLessonTitles;
        ObservableCollection<int> checkedAuditoriums;
        Schedule[] schedules;
        string[] lessonTitles;
        string[] teachers;
        string[] auditoriums;
        string[] lessonTypes;
        CancellationTokenSource cts;

        string regularString;
        string sessionString;
        Switch scheduleSessionFilter;
        Spinner scheduleDateFilter;
        Switch scheduleEmptyPair;
        BottomSheetBehavior bottomSheetBehavior;

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
                case nameof(this.viewModel.IsSession):
                    this.scheduleType.Text = GetTypeText(this.viewModel.IsSession);
                    break;
                case nameof(this.viewModel.Date):
                    if (this.viewPager?.Adapter is DailyShedulePageAdapter adapter)
                    {
                        this.viewPager?.SetCurrentItem((this.viewModel.Date - adapter.FirstPosDate).Days, false);
                        //ParabolicTransformer.SetViewToDefault(adapter.GetView(this.viewPager.CurrentItem));
                    }
                    break;
                case nameof(this.viewModel.ScheduleFilter.DateFilter):
                    this.scheduleDateFilter.SetSelection((int)this.viewModel.ScheduleFilter.DateFilter);
                    break;
                case nameof(this.viewModel.ScheduleFilter.SessionFilter):
                    this.scheduleSessionFilter.Checked = this.viewModel.ScheduleFilter.SessionFilter;
                    break;
                default:
                    this.logger.Warn("Event OnPropertyChanged was not proccessed correctly. Property: {PropertyName}", e.PropertyName);
                    break;
            }
        }

        string GetTypeText(bool isSession)
        {
            return isSession ? this.sessionString : this.regularString;
        }

        void OnLessonClick(Lesson lesson, DateTime date)
        {
            var fragment = ScheduleLessonInfoView.NewInstance();
            this.viewModel.LessonClickCommand.Execute(new Tuple<Lesson, DateTime>(lesson, date));
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

        void SetUpSchedule(Schedule schedule, bool loading = false)
        {
            if (this.viewPager == null)
            {
                return;
            }
            if (this.Context != null && !string.IsNullOrEmpty(schedule?.Group?.Comment))
            {
                Toast.MakeText(this.Context, schedule?.Group?.Comment, ToastLength.Long).Show();
            }
            DateTime toDate;
            if (this.viewPager.Adapter is DailyShedulePageAdapter adapter)
            {
                adapter.NeedDispose = true;
                toDate = adapter.FirstPosDate.AddDays(this.viewPager.CurrentItem);
                if (toDate == DateTime.MinValue)
                {
                    toDate = DateTime.Today;
                }
            }
            else
            {
                toDate = DateTime.Today;
            }

            this.viewPager.Adapter = adapter = new DailyShedulePageAdapter(
                schedule, this.viewModel.ScheduleFilter, this.viewModel.ShowEmptyLessons, 
                this.viewModel.IsAdvancedSearch, loading);
            if (schedule != null)
            {
                adapter.OpenCalendar += date =>
                {
                    var fragment = ScheduleCalendarView.NewInstance();
                    if (this.viewPager.Adapter is DailyShedulePageAdapter adapter)
                    {
                        this.viewModel.ScheduleCalendarCommand.Execute(date);
                        (this.Activity as MainView)?.ChangeFragment(fragment, false);
                    }
                };
                this.viewPager.CurrentItem = (toDate - adapter.FirstPosDate).Days;
                adapter.LessonClick += OnLessonClick;
                this.viewPager.Adapter.NotifyDataSetChanged();
            }
            this.logger.Debug("Schedule set up");
        }

        public ScheduleView() : base(Fragments.ScheduleMain)
        {
            var loggerFactory = DependencyInjector.GetILoggerFactory();
            this.logger = loggerFactory.Create<ScheduleView>();

            this.checkedGroups = new ObservableCollection<int>();
            this.checkedLessonTitles = new ObservableCollection<int>();
            this.checkedLessonTypes = new ObservableCollection<int>();
            this.checkedTeachers = new ObservableCollection<int>();
            this.checkedAuditoriums = new ObservableCollection<int>();
        }

        public ScheduleView(ScheduleVm vm) : this()
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
        public static ScheduleView NewInstance(Task<ScheduleVm> vmTask)
        {
            var fragment = new ScheduleView(vmTask.GetAwaiter().GetResult());
            return fragment;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            var bottomAppBar = this.View.FindViewById<BottomAppBar>(Resource.Id.bottomAppBar);
            (this.Activity as MainView).SetSupportActionBar(bottomAppBar);
            (this.Activity as MainView).SupportActionBar.SetDisplayShowTitleEnabled(false);
            bottomAppBar.NavigationClick += (obj, arg) => drawer.OpenDrawer(GravityCompat.Start);
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
            return inflater.Inflate(Resource.Layout.fragment_schedule, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this.Context);

            this.viewPager = view.FindViewById<ViewPager>(Resource.Id.viewpager);
            this.viewPager.PageSelected += (obj, arg) =>
            {
                this.viewModel.OnDateChanged(
                    (this.viewPager.Adapter as DailyShedulePageAdapter).FirstPosDate.AddDays(arg.Position));
            };
            if (this.viewPager.Adapter == null)
            {
                if (this.viewModel.ScheduleDownloaded)
                {
                    SetUpSchedule(this.viewModel.Schedule);
                }
                else
                {
                    SetUpSchedule(null, true);
                }
            }
            //this.viewPager.SetPageTransformer(false, new ParabolicTransformer());
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
                    var tab = tabs[
                        ((int)adapter.FirstPosDate.DayOfWeek + arg.Position + (arg.PositionOffset < 0.5f ? 0 : 1)) % 7];
                    if (!tab.IsSelected)
                    {
                        tab.Select();
                    }
                }
            };

            this.swipeToRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.schedule_update);
            this.swipeToRefresh.Refresh += (obj, arg) =>
            {
                if (this.viewModel.IsAdvancedSearch)
                {
                    this.textGroupTitle.Text = this.viewModel.GroupTitle;
                    this.viewModel.IsAdvancedSearch = false;
                }
                this.viewModel.UpdateSchedule();
            };

            this.viewModel.ScheduleEndDownloading += ViewModel_ScheduleEndDownloading;
            this.viewModel.ScheduleBeginDownloading += ViewModel_ScheduleBeginDownloading;


            this.viewPager.PageScrollStateChanged += (obj, arg) =>
            {
                this.swipeToRefresh.Enabled = arg.State == ViewPager.ScrollStateIdle;
            };


            this.scheduleDateFilter = view.FindViewById<Spinner>(Resource.Id.spinner_schedule_date_filter);
            this.scheduleDateFilter.SetSelection((int)this.viewModel.ScheduleFilter.DateFilter);
            this.scheduleDateFilter.ItemSelected += (obj, arg) =>
            {
                if ((int)this.viewModel.ScheduleFilter.DateFilter != arg.Position)
                {
                    this.viewModel.DateFilterSelected.Execute(arg.Position);
                    prefs.Edit().PutInt(PreferencesConstants.ScheduleDateFilter, arg.Position).Apply();
                }
            };

            this.scheduleSessionFilter = view.FindViewById<Switch>(Resource.Id.switch_schedule_session_filter);
            this.scheduleSessionFilter.Checked = this.viewModel.ScheduleFilter.SessionFilter;
            this.scheduleSessionFilter.CheckedChange += (obj, arg) =>
            {
                if (this.viewModel.ScheduleFilter.SessionFilter != arg.IsChecked)
                {
                    this.viewModel.SessionFilterSelected.Execute(arg.IsChecked);
                    prefs.Edit().PutBoolean(PreferencesConstants.ScheduleSessionFilter, arg.IsChecked).Apply();
                }
            };

            this.scheduleEmptyPair = view.FindViewById<Switch>(Resource.Id.switch_schedule_empty_lessons);
            this.scheduleEmptyPair.Checked = this.viewModel.ShowEmptyLessons;
            this.scheduleEmptyPair.CheckedChange += (obj, arg) =>
            {
                if (this.viewModel.ShowEmptyLessons != arg.IsChecked)
                {
                    this.viewModel.EmptyLessonsSelected.Execute(arg.IsChecked);
                    prefs.Edit().PutBoolean(PreferencesConstants.ScheduleShowEmptyLessons, arg.IsChecked).Apply();
                }
            };

            var homeBtn = this.View.FindViewById<ImageButton>(Resource.Id.button_home);
            homeBtn.Click += (obj, arg) => this.viewModel.GoHomeCommand.Execute(null);

            SetUpBotomSheet(this.View);

            var advancedSearchBtn = this.View.FindViewById<ImageButton>(Resource.Id.button_advanced_search);
            advancedSearchBtn.Click += (obj, arg) =>
            {
                this.bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
            };

            this.regularString = this.Context.GetString(Resource.String.text_schedule_type_regular);
            this.sessionString = this.Context.GetString(Resource.String.text_schedule_type_session);
            this.scheduleType = this.View.FindViewById<Button>(Resource.Id.button_schedule_type);
            this.scheduleType.Text = GetTypeText(this.viewModel.IsSession);
            this.scheduleType.Click += (obj, arg) =>
            {
                var newIsSession = !this.viewModel.IsSession;
                this.viewModel.ScheduleTypeChanged.Execute(newIsSession);
                this.scheduleType.Text = GetTypeText(newIsSession);
                prefs.Edit().PutBoolean(PreferencesConstants.ScheduleTypePreference, newIsSession).Apply();
            };
            var filterBtn = view.FindViewById<ImageButton>(Resource.Id.button_schedule_filter);

            var settingsDrawer = view.FindViewById<DrawerLayout>(Resource.Id.drawer_layout_schedule);
            settingsDrawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            settingsDrawer.DrawerClosed += (obj, arg) => settingsDrawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            filterBtn.Click += (obj, arg) =>
            {
                settingsDrawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
                settingsDrawer.OpenDrawer(GravityCompat.End);
            };

            this.textGroupTitle = this.View.FindViewById<AutoCompleteTextView>(Resource.Id.text_group_title);
            SetUpGroupList(this.viewModel.GroupList);
            this.textGroupTitle.Text = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle);
            this.textGroupTitle.KeyPress +=
                (obj, arg) =>
                {
                    if (arg.Event.Action != KeyEventActions.Up)
                    {
                        arg.Handled = false;
                    }
                    else if (arg.KeyCode == Keycode.Back)
                    {
                        if (this.textGroupTitle.IsFocused)
                        {
                            this.textGroupTitle.ClearFocus();
                        }
                        this.Activity?.OnBackPressed();
                    }
                    else if (arg.KeyCode == Keycode.Enter)
                    {
                        this.viewModel.IsAdvancedSearch = false;
                        this.viewModel.GroupTitle = (obj as AutoCompleteTextView)?.Text;
                        #region logger
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
                        #endregion
                        prefs.Edit().PutString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle).Apply();
                        this.viewModel.SubmitGroupCommand.Execute(null);
                        this.textGroupTitle.DismissDropDown();
                        var inputManager = (InputMethodManager)this.Activity?.GetSystemService(Context.InputMethodService);
                        inputManager.HideSoftInputFromWindow(this.textGroupTitle.WindowToken, HideSoftInputFlags.None);
                        this.textGroupTitle.ClearFocus();
                    }
                    else
                    {
                        arg.Handled = false;
                    }
                };
            this.textGroupTitle.ItemClick += (obj, arg) =>
            {
                this.viewModel.IsAdvancedSearch = false;
                this.viewModel.GroupTitle = (string)arg.Parent.GetItemAtPosition(arg.Position);
                prefs.Edit().PutString(PreferencesConstants.ScheduleGroupTitle, this.viewModel.GroupTitle).Apply();
                this.viewModel.SubmitGroupCommand.Execute(null);
                var inputManager = (InputMethodManager)this.Activity?.GetSystemService(Context.InputMethodService);
                inputManager.HideSoftInputFromWindow(this.textGroupTitle.WindowToken, HideSoftInputFlags.None);
                this.textGroupTitle.ClearFocus();
            };
        }

        public override void OnResume()
        { 
            base.OnResume();
            if (string.IsNullOrEmpty(this.textGroupTitle.Text))
            {
                this.textGroupTitle.RequestFocus();
            }
        }

        void ViewModel_ScheduleBeginDownloading()
        {
            SetUpSchedule(null, true);
        }

        void ViewModel_ScheduleEndDownloading()
        {
            this.swipeToRefresh.Refreshing = false;
        }

        void ViewModel_Announced(string msg)
        {
            if (this.Activity != null && Xamarin.Essentials.MainThread.IsMainThread)
            {
                Toast.MakeText(this.Activity, msg, ToastLength.Short).Show();
            }
        }

        void SetUpBotomSheet(View view)
        {
            var bottomSheet = view.FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
            this.bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
            this.bottomSheetBehavior.State = BottomSheetBehavior.StateHidden;

            var textGroups = view.FindViewById<TextView>(Resource.Id.text_groups);
            textGroups.Click += (obj, arg) =>
            {
                if (this.Activity == null)
                {
                    return;
                }
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.SimpleFilter(this.viewModel.GroupList, this.checkedGroups)));
            };
            var textLessonTitles = view.FindViewById<TextView>(Resource.Id.text_lesson_titles);
            var textTeachers = view.FindViewById<TextView>(Resource.Id.text_teachers);
            var textAuditoriums = view.FindViewById<TextView>(Resource.Id.text_auditoriums);
            var textLessonTypes = view.FindViewById<TextView>(Resource.Id.text_lesson_types);
            var applyButton = view.FindViewById<Button>(Resource.Id.btn_search);

            var progressBar = bottomSheet.FindViewById<ProgressBar>(Resource.Id.progressBar);
            var progressText = bottomSheet.FindViewById<TextView>(Resource.Id.text_progress);
            var cancelBtn = bottomSheet.FindViewById<Button>(Resource.Id.btn_cancel);
            cancelBtn.Click += (obj, arg) =>
            {
                this.cts.Cancel();
                cancelBtn.Visibility = ViewStates.Gone;
            };
            this.checkedGroups.CollectionChanged +=
                (obj, arg) =>
                {
                    textGroups.Text = string.Join(", ", from index in this.checkedGroups
                                                        select this.viewModel.GroupList[index]);
                    if (textGroups.Text == string.Empty)
                    {
                        textGroups.Text = GetString(Resource.String.all_groups);
                    }
                    textLessonTitles.Visibility = ViewStates.Gone;
                    this.checkedLessonTitles.Clear();
                    textTeachers.Visibility = ViewStates.Gone;
                    this.checkedTeachers.Clear();
                    textAuditoriums.Visibility = ViewStates.Gone;
                    this.checkedAuditoriums.Clear();
                    textLessonTypes.Visibility = ViewStates.Gone;
                    this.checkedLessonTypes.Clear();
                    applyButton.Visibility = ViewStates.Gone;
                };
            var downloadShedulesBtn = view.FindViewById<Button>(Resource.Id.btn_acceptGroups);
            downloadShedulesBtn.Click += async (obj, arg) =>
            {
                downloadShedulesBtn.Enabled = false;
                textGroups.Enabled = false;
                textLessonTitles.Visibility = ViewStates.Gone;
                textTeachers.Visibility = ViewStates.Gone;
                textAuditoriums.Visibility = ViewStates.Gone;
                textLessonTypes.Visibility = ViewStates.Gone;
                applyButton.Visibility = ViewStates.Gone;
                progressBar.Visibility = ViewStates.Visible;
                progressText.Visibility = ViewStates.Visible;
                cancelBtn.Visibility = ViewStates.Visible;
                this.cts = new CancellationTokenSource();
                try
                {
                    (this.schedules, this.lessonTitles, this.teachers, this.auditoriums, this.lessonTypes) =
                    await this.viewModel.GetAdvancedSearchData(
                        this.checkedGroups.Count == 0 ? this.viewModel.GroupList :
                        (from index in this.checkedGroups
                         select this.viewModel.GroupList[index]).ToList() as IList<string>,
                        this.cts.Token, progress => Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                        {
                            progressBar.Progress = progress;
                            progressText.Text = progress / 100 + " %";
                        }));
                    Toast.MakeText(this.Context, "Расписания загружены", ToastLength.Short).Show();
                    downloadShedulesBtn.Enabled = true;
                    textGroups.Enabled = true;
                    textLessonTitles.Visibility = ViewStates.Visible;
                    textTeachers.Visibility = ViewStates.Visible;
                    textAuditoriums.Visibility = ViewStates.Visible;
                    textLessonTypes.Visibility = ViewStates.Visible;
                    applyButton.Visibility = ViewStates.Visible;

                    progressBar.Visibility = ViewStates.Gone;
                    progressText.Visibility = ViewStates.Gone;
                    progressBar.Progress = 0;
                    progressText.Text = "0 %";
                    cancelBtn.Visibility = ViewStates.Gone;
                }
                catch (System.OperationCanceledException)
                {
                    this.cts.Dispose();
                    downloadShedulesBtn.Enabled = true;
                    textGroups.Enabled = true;
                    Toast.MakeText(this.Context, "Загрузка отменена", ToastLength.Short).Show();
                    progressBar.Progress = 0;
                    progressText.Text = "0 %";
                    textLessonTitles.Visibility = ViewStates.Gone;
                    textTeachers.Visibility = ViewStates.Gone;
                    textAuditoriums.Visibility = ViewStates.Gone;
                    textLessonTypes.Visibility = ViewStates.Gone;
                    applyButton.Visibility = ViewStates.Gone;
                    progressBar.Visibility = ViewStates.Gone;
                    progressText.Visibility = ViewStates.Gone;
                }
            };



            textLessonTitles.Click += (obj, arg) =>
            {
                if (this.Activity == null)
                {
                    return;
                }
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.AdvancedFilter(this.lessonTitles, this.checkedLessonTitles)));
            };
            this.checkedLessonTitles.CollectionChanged +=
                (obj, arg) =>
                {
                    textLessonTitles.Text = string.Join(", ", from index in this.checkedLessonTitles
                                                              select this.lessonTitles[index]);
                    if (textLessonTitles.Text == string.Empty)
                    {
                        textLessonTitles.Text = GetString(Resource.String.all_subjects);
                    }
                };

            textTeachers.Click += (obj, arg) =>
            {
                if (this.Activity == null)
                {
                    return;
                }
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.AdvancedFilter(this.teachers, this.checkedTeachers)));
            };
            this.checkedTeachers.CollectionChanged +=
                (obj, arg) =>
                {
                    textTeachers.Text = string.Join(", ", from index in this.checkedTeachers
                                                          select this.teachers[index]);
                    if (textTeachers.Text == string.Empty)
                    {
                        textTeachers.Text = GetString(Resource.String.all_teachers);
                    }
                };

            textAuditoriums.Click += (obj, arg) =>
            {
                if (this.Activity == null)
                {
                    return;
                }
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.AdvancedFilter(this.auditoriums, this.checkedAuditoriums)));
            };
            this.checkedAuditoriums.CollectionChanged +=
                (obj, arg) =>
                {
                    textAuditoriums.Text = string.Join(", ", from index in this.checkedAuditoriums
                                                             select this.auditoriums[index]);
                    if (textAuditoriums.Text == string.Empty)
                    {
                        textAuditoriums.Text = GetString(Resource.String.all_auditoriums);
                    }
                };

            textLessonTypes.Click += (obj, arg) =>
            {
                if (this.Activity == null)
                {
                    return;
                }
                var dialog = ScheduleFilterView.NewInstance();
                dialog.Show(this.Activity.SupportFragmentManager, "qq");
                dialog.SetAdapter(new AdvancedSearchAdapter(
                    new AdvancedSearchAdapter.SimpleFilter(this.lessonTypes, this.checkedLessonTypes)));
            };
            this.checkedLessonTypes.CollectionChanged +=
                (obj, arg) =>
                {
                    textLessonTypes.Text = string.Join(", ", from index in this.checkedLessonTypes
                                                             select this.lessonTypes[index]);
                    if (textLessonTypes.Text == string.Empty)
                    {
                        textLessonTypes.Text = GetString(Resource.String.all_lesson_types);
                    }
                };
            applyButton.Click += (obj, arg) =>
            {
                SetUpSchedule(null, true);
                var filt = new Schedule.AdvancedSerach();
                var newSchedule = filt.Filter(this.schedules,
                    this.checkedLessonTitles.Count == 0 ? this.lessonTitles :
                    (from index in this.checkedLessonTitles
                     select this.lessonTitles[index]).ToList() as IList<string>,
                    this.checkedLessonTypes.Count == 0 ? this.lessonTypes :
                    (from index in this.checkedLessonTypes
                     select this.lessonTypes[index]).ToList() as IList<string>,
                    this.checkedAuditoriums.Count == 0 ? this.auditoriums :
                    (from index in this.checkedAuditoriums
                     select this.auditoriums[index]).ToList() as IList<string>,
                    this.checkedTeachers.Count == 0 ? this.teachers :
                    (from index in this.checkedTeachers
                     select this.teachers[index]).ToList() as IList<string>);
                this.viewModel.IsAdvancedSearch = true;
                this.viewModel.SetUpSchedule(newSchedule);
                this.textGroupTitle.Text = "...";
            };
        }

        //protected override void Dispose(bool disposing)
        //{
        //    this.viewModel.PropertyChanged -= OnPropertyChanged;
        //    this.viewModel.ScheduleEndDownloading -= ViewModel_ScheduleEndDownloading;
        //    this.viewModel.ScheduleBeginDownloading -= ViewModel_ScheduleBeginDownloading;
        //    base.Dispose(disposing);
        //}

        public override void OnDestroy()
        {
            this.viewModel.PropertyChanged -= OnPropertyChanged;
            this.viewModel.Announced -= ViewModel_Announced;
            base.OnDestroy();
        }

        public override void OnDestroyView()
        {
            this.viewModel.ScheduleEndDownloading -= ViewModel_ScheduleEndDownloading;
            this.viewModel.ScheduleBeginDownloading -= ViewModel_ScheduleBeginDownloading;
            base.OnDestroyView();
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);

            if (this.viewModel == null)
            {
                var prefs = PreferenceManager.GetDefaultSharedPreferences(this.Context);

                var scheduleFilter = Schedule.Filter.DefaultFilter;
                scheduleFilter.DateFilter = (DateFilter)prefs.GetInt(PreferencesConstants.ScheduleDateFilter,
                    (int)scheduleFilter.DateFilter);
                //scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt(PreferencesConstants.ScheduleModuleFilter, 
                //  (int)scheduleFilter.ModuleFilter);
                scheduleFilter.SessionFilter = prefs.GetBoolean(PreferencesConstants.ScheduleSessionFilter, 
                    scheduleFilter.SessionFilter);


                string groupTitle = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, 
                    DefaultSettings.ScheduleGroupTitle);

#warning fix on release
                bool isSession;
                try
                {
                    isSession = prefs.GetBoolean(PreferencesConstants.ScheduleTypePreference,
                        DefaultSettings.ScheduleTypePreference);
                }
                catch
                {
                    isSession = prefs.GetInt(PreferencesConstants.ScheduleTypePreference, 0) == 1;
                }

                this.viewModel = new ScheduleVm(DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator(),
                    isSession, scheduleFilter)
                {
                    GroupTitle = groupTitle
                };
                this.viewModel.ShowEmptyLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowEmptyLessons, 
                    DefaultSettings.ScheduleShowEmptyLessons);
                //this.viewModel.ShowColoredLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowColoredLessons, true);

                this.viewModel.SetUpScheduleAsync(false);
            }

            this.viewModel.PropertyChanged += OnPropertyChanged;
            this.viewModel.Announced += ViewModel_Announced;
        }
    }
}