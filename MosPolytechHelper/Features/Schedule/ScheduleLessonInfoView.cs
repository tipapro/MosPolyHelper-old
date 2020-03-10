namespace MosPolyHelper.Features.Schedule
{
    using Android.Graphics;
    using Android.OS;
    using Android.Text;
    using Android.Text.Style;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using AndroidX.DrawerLayout.Widget;
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Utilities;
    using System;

    class ScheduleLessonInfoView : FragmentBase
    {
        const string Exam = "экзамен";
        const string Credit = "зачет";
        readonly ScheduleLessonInfoVm viewModel;

        readonly Color[] lessonTypeColors = new Color[]
        {
            new Color(235, 65, 65),     // Exam, Credit
            new Color(41, 182, 246)   // Other
        };

        Color GetLessonTypeColor(string lessonType)
        {
            if (lessonType.Contains(Exam, StringComparison.OrdinalIgnoreCase) ||
                lessonType.Contains(Credit, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[0];
            }
            else
            {
                return this.lessonTypeColors[1];
            }
        }

        public bool NoteEdited { get; private set; }

        public ScheduleLessonInfoView() : base(Fragments.ScheduleLessonInfo)
        {
            this.viewModel = new ScheduleLessonInfoVm(DependencyInjector.GetILoggerFactory(), DependencyInjector.GetIMediator());
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

            if (this.viewModel.Lesson.IsEmpty())
            {
                view.FindViewById<TextView>(Resource.Id.text_schedule_title).Text = "Нет занятия";
                SetTime(view.FindViewById<TextView>(Resource.Id.text_schedule_time), this.viewModel.Lesson, this.viewModel.Date);
            }
            else
            {
                SetType(view.FindViewById<TextView>(Resource.Id.text_schedule_type), this.viewModel.Lesson);
                SetTitle(view.FindViewById<TextView>(Resource.Id.text_schedule_title), this.viewModel.Lesson);
                SetTime(view.FindViewById<TextView>(Resource.Id.text_schedule_time), this.viewModel.Lesson, this.viewModel.Date);
                SetAuditoriums(view.FindViewById<TextView>(Resource.Id.text_schedule_auditoriums), this.viewModel.Lesson);
                SetTeachers(view.FindViewById<TextView>(Resource.Id.text_schedule_teachers), this.viewModel.Lesson);
                SetDate(view.FindViewById<TextView>(Resource.Id.text_schedule_date), this.viewModel.Lesson);
                SetGroupInfo(view.FindViewById<TextView>(Resource.Id.text_schedule_group), this.viewModel.Lesson.Group);
                //var editNote = view.FindViewById<EditText>(Resource.Id.edit_text_schedule_note);
                //SetNote(editNote, this.viewModel.Lesson);
                //SetButton(view.FindViewById<ImageButton>(Resource.Id.button_note_accept), editNote);
            }
            return view;
        }

        void SetType(TextView textView, Lesson lesson)
        {
            textView.SetTextColor(GetLessonTypeColor(lesson.Type));
            textView.Text = lesson.Type.ToUpper();
        }

        void SetTitle(TextView textView, Lesson lesson)
        {
            textView.Text = lesson.Title;
        }

        void SetTime(TextView textView, Lesson lesson, DateTime date)
        {
            var (startTime, endTime) = lesson.GetTime(date);
            string dateStr = date.ToString("dddd, d MMMM, ");
            dateStr = char.ToUpper(dateStr[0]) + dateStr.Substring(1);
            textView.Text = dateStr + $"с {startTime} до {endTime}, {lesson.Order + 1}-е занятие";
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

        void SetTeachers(TextView textView, Lesson lesson)
        {
            textView.Text = string.Join(", ", lesson.GetFullTecherNames());
        }

        void SetDate(TextView textView, Lesson lesson)
        {
            if (lesson.DateFrom == lesson.DateTo)
            {
                textView.Text = lesson.DateFrom.ToString("d MMMM");
            }
            else
            {
                textView.Text = lesson.DateFrom.ToString("С d MMMM ") + lesson.DateTo.ToString("до d MMMM");
            }
        }

        void SetGroupInfo(TextView textView, Group group)
        {
            string text = "Группа " + group.Title + ", " + group.Course + "-й курс, длительность семестра: " +
                group.DateFrom.ToString("с d MMMM") + " " + group.DateTo.ToString("до d MMMM");
            if (group.IsEvening)
            {
                text += ", вечерняя";
            }
            if (!string.IsNullOrEmpty(group.Comment))
            {
                text += ", комментарий: " + group.Comment;
            }
            textView.Text = text;
        }

        void SetNote(EditText editText, Lesson lesson)
        {
            editText.Text = lesson.Note;
            editText.TextChanged += (obj, arg) =>
                {
                    NoteEdited = true;
                    lesson.Note = editText.Text;
                };
        }

        void SetButton(ImageButton imageButton, EditText editText)
        {
            editText.FocusChange += (obj, arg) =>
            {
                if (arg.HasFocus)
                {
                    imageButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    imageButton.Visibility = ViewStates.Gone;
                    if (this.NoteEdited)
                    {
                        this.viewModel.ResaveSchedule();
                        this.NoteEdited = false;
                    }
                }
            };
            imageButton.Click += (obj, arg) =>
            {
                var inputManager = (InputMethodManager)this.Activity?.GetSystemService(Android.Content.Context.InputMethodService);
                inputManager.HideSoftInputFromWindow(editText.WindowToken, HideSoftInputFlags.None);
                editText.ClearFocus();
            };
        }

        //string GetWeekTypeName(WeekType weekType)
        //{
        //    return weekType switch
        //    {
        //        WeekType.None => GetString(Resource.String.none_week),
        //        WeekType.Odd => GetString(Resource.String.odd_week),
        //        WeekType.Even => GetString(Resource.String.even_week),
        //        _ => GetString(Resource.String.none_week),
        //    };
        //}

        //string GetModuleName(Module module)
        //{
        //    return module switch
        //    {
        //        Module.None => GetString(Resource.String.none_module),
        //        Module.First => GetString(Resource.String.first_module),
        //        Module.Second => GetString(Resource.String.second_module),
        //        _ => GetString(Resource.String.none_module),
        //    };
        //}

        public override void OnStop()
        {
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            //if (this.NoteEdited)
            //{
            //    this.viewModel.ResaveSchedule();
            //    this.NoteEdited = false;
            //}
            base.OnStop();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            (this.Activity as MainView).SupportActionBar.SetDisplayShowTitleEnabled(false);
        }

        public static ScheduleLessonInfoView NewInstance()
        {
            var fragment = new ScheduleLessonInfoView();
            return fragment;
        }
    }
}