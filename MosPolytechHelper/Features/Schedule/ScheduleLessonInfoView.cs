namespace MosPolyHelper.Features.Schedule
{
    using Android.Graphics;
    using Android.OS;
    using Android.Text;
    using Android.Text.Style;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.Widget;
    using AndroidX.DrawerLayout.Widget;
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Utilities;

    class ScheduleLessonInfoView : FragmentBase
    {
        ScheduleLessonInfoVm viewModel;

        public ScheduleLessonInfoView() : base(Fragments.ScheduleLessonInfo)
        {
            viewModel = new ScheduleLessonInfoVm(DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_lesson_info, container, false);
            var toolbar = view.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);

            if (toolbar != null)
            {
                (this.Activity as MainView)?.SetSupportActionBar(toolbar);
            }
            (this.Activity as MainView)?.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            (this.Activity as MainView)?.SupportActionBar.SetHomeButtonEnabled(true);
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);

            var time = viewModel.Lesson.GetTime(this.viewModel.Date);
            view.FindViewById<TextView>(Resource.Id.text_schedule_type).Text = viewModel.Lesson.Type;
            view.FindViewById<TextView>(Resource.Id.text_schedule_title).Text = viewModel.Lesson.Title;
            view.FindViewById<TextView>(Resource.Id.text_schedule_time).Text = 
                viewModel.Date.ToString("dddd, dd MMMM, ") + $"с {time.StartTime} до {time.EndTime}";
            SetAuditoriums(view.FindViewById<TextView>(Resource.Id.text_schedule_auditoriums), viewModel.Lesson);
            view.FindViewById<TextView>(Resource.Id.text_schedule_date).Text = 
                viewModel.Lesson.DateFrom.ToString("с dddd, dd MMMM ") + viewModel.Lesson.DateTo.ToString("до dddd, dd MMMM");
            view.FindViewById<TextView>(Resource.Id.text_schedule_other_info).Text = viewModel.Lesson.Module.ToString() + " " +
                viewModel.Lesson.Week.ToString();
            view.FindViewById<TextView>(Resource.Id.text_schedule_group).Text = viewModel.Lesson.Group.Title + " " +
                viewModel.Lesson.Group.Course + " " + viewModel.Lesson.Group.DateFrom.ToString("dddd, dd MMMM") + " " +
                viewModel.Lesson.Group.DateTo.ToString("dddd, dd MMMM") + " " 
                + viewModel.Lesson.Group.IsEvening + " " + viewModel.Lesson.Group.Comment;

            return view;
        }

        void SetAuditoriums(TextView textView, Lesson lesson)
        {
            using var auditoriums = new SpannableStringBuilder();
            if (lesson.Auditoriums == null)
            {
                textView.SetText("", TextView.BufferType.Normal);
                return;
            }
            for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
            {
                var color = Color.ParseColor(lesson.Auditoriums[i].Color);
                color = Color.HSVToColor(new float[] { color.GetHue(), color.GetSaturation(), color.GetBrightness() * 3f });
                auditoriums.Append(lesson.Auditoriums[i].Name.ToLower() + ", ",
                    new ForegroundColorSpan(color),
                    SpanTypes.ExclusiveExclusive);
            }
            if (lesson.Auditoriums.Length != 0)
            {
                var color = Color.ParseColor(lesson.Auditoriums[^1].Color);
                color = Color.HSVToColor(new float[] { color.GetHue(), color.GetSaturation(), color.GetBrightness() * 3f });
                auditoriums.Append(lesson.Auditoriums[^1].Name.ToLower(),
                    new ForegroundColorSpan(color),
                    SpanTypes.ExclusiveExclusive);
            }

            textView.SetText(auditoriums, TextView.BufferType.Normal);
        }

        public static ScheduleLessonInfoView NewInstance()
        {
            var fragment = new ScheduleLessonInfoView();
            return fragment;
        }
    }
}