namespace MosPolyHelper.Adapters
{
    using Android.Graphics;
    using Android.Text;
    using Android.Text.Style;
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Domains.ScheduleDomain;
    using System;

    public class PairAdapter : RecyclerView.Adapter
    {
        #region LessonTypeConstants
        const string CourseProject = "кп";
        const string Exam = "экзамен";
        const string Credit = "зачет";
        const string Consultation = "консультация";
        const string Laboratory = "лаб";
        const string Practice = "практика";
        const string Lecture = "лекция";
        const string Other = "другое";
        #endregion LessonTypeConstants

        readonly TextView nullMessage;
        Schedule.Daily dailySchedule;
        Schedule.Filter filter;
        int currLessonOrder;
        int itemCount;
        readonly int[] orderMap = new int[7];

        readonly Color[] lessonTimeColors = new Color[]
        {
            new Color(239, 83, 80),   // Red
            new Color(255, 160, 0),   // Orange
            new Color(76, 175, 80),   // Green
            new Color(66, 165, 245),   // Blue
            new Color(57, 73, 171),   // Indigo
            new Color(106, 27, 154),   // Purple
            new Color(55, 71, 79),   // Gray
            new Color(206, 221, 235)    // One color mode
        };
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

        public override int ItemCount
        {
            get => this.itemCount;
        }

        public DateTime Date { get; set; }
        public bool ShowEmptyLessons { get; set; }
        public bool ShowColoredLessons { get; set; }
        public bool ShowGroup { get; set; }
        public event Action<Lesson> LessonClick;

        public PairAdapter(TextView nullMessage, Schedule.Daily dailySchedule, Schedule.Filter filter,
            DateTime date, bool showEmptyLessons, bool showColoredLessons, bool showGroup)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.ShowGroup = showGroup;
            Array.Fill(this.orderMap, -1);
            this.dailySchedule = dailySchedule;
            this.nullMessage = nullMessage;
            this.nullMessage.Visibility = this.dailySchedule != null && this.dailySchedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
            this.filter = filter;
            this.Date = date;
            SetUpCount();
        }

        public void BuildSchedule(Schedule.Daily dailySchedule, Schedule.Filter scheduleFilter, DateTime date,
            bool showEmptyLessons, bool showColoredLessons, bool showGroup)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.ShowGroup = showGroup;
            Array.Fill(this.orderMap, -1);
            this.dailySchedule = dailySchedule;
            this.Date = date;
            this.filter = scheduleFilter;
            this.nullMessage.Visibility = this.dailySchedule != null && this.dailySchedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
            SetUpCount();
            NotifyDataSetChanged();
        }

        public void SetUpCount()
        {
            if (this.dailySchedule == null || this.dailySchedule.Count == 0)
            {
                this.itemCount = 0;
                return;
            }
            int currLessonOrder = -1;
            int fixedOrder = -1;
            if (this.Date == DateTime.Today)
            {
                currLessonOrder = Lesson.GetCurrentLessonOrder(this.dailySchedule[0].Group, DateTime.Now.TimeOfDay, this.Date);
                foreach (var lesson in this.dailySchedule)
                {
                    if (this.filter.DateFitler != DateFilter.Desaturate ||
                        (this.Date >= lesson.DateFrom && this.Date <= lesson.DateTo))
                    {
                        fixedOrder = lesson.Order;
                    }
                    if (fixedOrder >= currLessonOrder)
                    {
                        break;
                    }
                }
            }
            this.currLessonOrder = fixedOrder;
            this.itemCount = this.dailySchedule.Count;
            if (this.ShowEmptyLessons)
            {
                if (fixedOrder >= currLessonOrder)
                {
                    this.currLessonOrder = currLessonOrder;
                }
                for (int i = 0; i < this.dailySchedule.Count; i++)
                {
                    this.orderMap[this.dailySchedule[i].Order] = i;
                }
                int maxOrder = this.dailySchedule[this.dailySchedule.Count - 1].Order;
                for (int i = 0; i < maxOrder; i++)
                {
                    if (this.orderMap[i] == -1)
                    {
                        this.itemCount++;
                    }
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int position)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_schedule, parent, false);
            view.Enabled = false;
            var vh = new ViewHolder(view, LessonClick);
            return vh;
        }


        void SetHead(ViewHolder viewHolder, Lesson lesson, bool prevEqual)
        {
            if (this.currLessonOrder == lesson.Order)
            {
                viewHolder.LessonTime.SetTextColor(new Color(0, 163, 0));
            }
            else
            {
                viewHolder.LessonTime.SetTextColor(new Color(121, 128, 134));
            }
            //new Color(120, 142, 161));
            //viewHolder.LessonOrder.SetTextColor(this.ShowColoredLessons ? Color.White : new Color(120, 142, 161));

            var (timeStart, timeEnd) = lesson.GetTime(this.Date);
            viewHolder.LessonOrder.SetText($"{lesson.Order + 1}", TextView.BufferType.Normal);
            //(viewHolder.LessonOrder.Background as GradientDrawable).SetColor(this.lessonTimeColors[lesson.Order]);
            if (prevEqual)
            {
                //viewHolder.LessonOrder.SetText(string.Empty, TextView.BufferType.Normal);
                viewHolder.LessonTime.Visibility = ViewStates.Visible;
                viewHolder.Divider.Visibility = ViewStates.Visible;
            }
            else
            {
                viewHolder.LessonTime.Visibility = ViewStates.Visible;
                viewHolder.Divider.Visibility = ViewStates.Visible;
            }

            viewHolder.LessonTime.SetText(timeStart + " - " + timeEnd, TextView.BufferType.Normal);

        }

        void SetLessonType(ViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string type = lesson.Type.ToUpper();
            if (this.ShowGroup)
            {
                type += " " + lesson.Group.Title;
            }
            viewHolder.LessonType.SetTextColor(enabled ? GetLessonTypeColor(lesson.Type) : new Color(225, 225, 225));
            viewHolder.LessonType.SetText(type, TextView.BufferType.Normal);
            viewHolder.LessonType.Enabled = enabled;
        }

        void SetAuditoriums(ViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            viewHolder.LessonAuditoriums.Visibility = ViewStates.Gone;
            using var auditoriums = new SpannableStringBuilder();
            viewHolder.LessonAuditoriums.Enabled = enabled;
            if (lesson.Auditoriums == null)
            {
                viewHolder.LessonAuditoriums.SetText("", TextView.BufferType.Normal);
                return;
            }
            if (enabled)
            {
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
            }
            else
            {
                for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
                {
                    auditoriums.Append(lesson.Auditoriums[i].Name.ToLower() + ", ",
                        new ForegroundColorSpan(new Color(225, 225, 225)), SpanTypes.ExclusiveExclusive);
                }
                if (lesson.Auditoriums.Length != 0)
                {
                    auditoriums.Append(lesson.Auditoriums[^1].Name.ToLower(),
                        new ForegroundColorSpan(new Color(225, 225, 225)),
                        SpanTypes.ExclusiveExclusive);
                }
            }
            viewHolder.LessonAuditoriums.SetText(auditoriums, TextView.BufferType.Normal);
        }
        Random r = new Random();
        void SetTitle(ViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string title = lesson.Title;
            //viewHolder.LessonTitle.SetTextColor(enabled ? new Color(95, 107, 117) : new Color(215, 215, 215));
            viewHolder.LessonTitle.SetText(title, TextView.BufferType.Normal);
            viewHolder.LessonTitle.Enabled = enabled;
            if (lesson.IsEmpty())
            {
                viewHolder.FavoriteIcon.Visibility = ViewStates.Gone;
                viewHolder.NoteIcon.Visibility = ViewStates.Gone;
                viewHolder.LessonAuditoriums.Visibility = ViewStates.Gone;
            }
            else
            {
                viewHolder.FavoriteIcon.Visibility = this.r.Next(2) == 1 ? ViewStates.Visible : ViewStates.Gone;
                if (this.r.Next(2) == 1)
                {
                    viewHolder.NoteIcon.Visibility = ViewStates.Visible;
                }
                else if (viewHolder.FavoriteIcon.Visibility == ViewStates.Visible)
                {
                    viewHolder.NoteIcon.Visibility = ViewStates.Invisible;
                }
                else
                {
                    viewHolder.NoteIcon.Visibility = ViewStates.Gone;
                }
                viewHolder.LessonAuditoriums.Visibility = ViewStates.Visible;
            }
            if (viewHolder.NoteIcon.Visibility == ViewStates.Gone)
            {
                float scale = viewHolder.LessonLayout.Context.Resources.DisplayMetrics.Density;
                int dp17InPx = (int)(16 * scale + 0.5f);
                (viewHolder.LessonTitle.LayoutParameters as RelativeLayout.LayoutParams).RightMargin = dp17InPx;
                (viewHolder.LessonTeachers.LayoutParameters as RelativeLayout.LayoutParams).RightMargin = dp17InPx;
            }
            else
            {
                (viewHolder.LessonTitle.LayoutParameters as RelativeLayout.LayoutParams).RightMargin = 0;
                (viewHolder.LessonTeachers.LayoutParameters as RelativeLayout.LayoutParams).RightMargin = 0;
            }
            if (viewHolder.FavoriteIcon.Visibility == ViewStates.Gone)
            {
                float scale = viewHolder.LessonLayout.Context.Resources.DisplayMetrics.Density;
                int dp17InPx = (int)(16 * scale + 0.5f);
                (viewHolder.LessonTeachers.LayoutParameters as RelativeLayout.LayoutParams).RightMargin = dp17InPx;
            }
            else
            {
                (viewHolder.LessonTeachers.LayoutParameters as RelativeLayout.LayoutParams).RightMargin = 0;
            }
        }

        void SetTeachers(ViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string teachers = string.Join(", ", lesson.GetShortTeacherNames());
            if (string.IsNullOrEmpty(teachers))
            {
                viewHolder.LessonTeachers.Visibility = ViewStates.Gone;
            }
            else
            {
                viewHolder.LessonTeachers.SetText(teachers, TextView.BufferType.Normal);
                viewHolder.LessonTeachers.Enabled = enabled;
                viewHolder.LessonTeachers.Visibility = ViewStates.Visible;
            }
        }

        void SetOtherInfo(ViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string otherInfo;
            if (lesson.DateFrom != DateTime.MinValue && lesson.DateTo != DateTime.MaxValue)
            {
                if (lesson.DateFrom == lesson.DateTo)
                {
                    otherInfo = lesson.DateFrom.ToString("d/MM");
                }
                else
                {
                    otherInfo = lesson.DateFrom.ToString("d/MM")
                        + " - " + lesson.DateTo.ToString("d/MM");
                }
                viewHolder.LessonOtherInfo.SetTextColor(enabled ? new Color(191, 197, 202) : new Color(230, 230, 230));
                viewHolder.LessonOtherInfo.SetText(otherInfo = string.Empty, TextView.BufferType.Normal);
                viewHolder.LessonOtherInfo.Enabled = enabled;
                viewHolder.LessonOtherInfo.Visibility = ViewStates.Visible;
            }
            else
            {
                viewHolder.LessonOtherInfo.Visibility = ViewStates.Gone;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(vh is ViewHolder viewHolder))
            {
                return;
            }
            viewHolder.Lesson = null;
            int fixedPos = position;
            if (this.ShowEmptyLessons)
            {
                int lastNotEmpty = -1;
                for (int i = 0; i < 6; i++)
                {
                    if (this.orderMap[i] == -1)
                    {
                        fixedPos--;
                        if (lastNotEmpty == fixedPos)
                        {
                            viewHolder.Lesson = Lesson.GetEmpty(i);
                            break;
                        }
                    }
                    else
                    {
                        lastNotEmpty = this.orderMap[i];
                        if (fixedPos <= this.orderMap[i])
                        {
                            break;
                        }
                    }
                }
                if (viewHolder.Lesson == null)
                {
                    viewHolder.Lesson = this.dailySchedule[fixedPos];
                }
            }
            else
            {
                viewHolder.Lesson = this.dailySchedule[position];
            }


            if (viewHolder.Lesson.IsEmpty())
            {
                SetHead(viewHolder, viewHolder.Lesson, false);
                SetLessonType(viewHolder, viewHolder.Lesson, true);
                SetAuditoriums(viewHolder, viewHolder.Lesson, true);
                SetTitle(viewHolder, viewHolder.Lesson, true);
                SetTeachers(viewHolder, viewHolder.Lesson, true);
                SetOtherInfo(viewHolder, viewHolder.Lesson, true);
                return;
            }

            SetHead(viewHolder, viewHolder.Lesson, fixedPos != 0
                && this.dailySchedule[fixedPos - 1].EqualsTime(viewHolder.Lesson, this.Date));

            bool enabledFrom = viewHolder.Lesson.DateFrom <= this.Date || this.filter?.DateFitler != DateFilter.Desaturate;
            bool enabledTo = viewHolder.Lesson.DateTo >= this.Date || this.filter?.DateFitler != DateFilter.Desaturate;
            bool enabled = enabledFrom && enabledTo;
            if (viewHolder.Lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) ||
                viewHolder.Lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) ||
                viewHolder.Lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase))
            {
                enabled = enabledTo;
            }

            float scale = viewHolder.LessonLayout.Context.Resources.DisplayMetrics.Density;
            int dp8InPx = (int)(8 * scale + 0.5f);

            if (fixedPos != this.dailySchedule.Count - 1 && this.dailySchedule[fixedPos + 1].EqualsTime(viewHolder.Lesson, this.Date))
            {
                (viewHolder.LessonLayout.LayoutParameters as FrameLayout.LayoutParams)
                    .SetMargins(dp8InPx, position == 0 ? dp8InPx : 0, dp8InPx, 0);
                if (fixedPos != 0 && this.dailySchedule[fixedPos - 1].EqualsTime(viewHolder.Lesson, this.Date))
                {
                    viewHolder.LessonLayout.Background =
                        viewHolder.LessonLayout.Context.GetDrawable(Resource.Drawable.shape_lesson_middle);
                }
                else
                {
                    viewHolder.LessonLayout.Background =
                           viewHolder.LessonLayout.Context.GetDrawable(Resource.Drawable.shape_lesson_top);
                }
            }
            else
            {
                (viewHolder.LessonLayout.LayoutParameters as FrameLayout.LayoutParams)
                    .SetMargins(dp8InPx, position == 0 ? dp8InPx : 0, dp8InPx, dp8InPx);
                if (fixedPos != 0 && this.dailySchedule[fixedPos - 1].EqualsTime(viewHolder.Lesson, this.Date))
                {
                    viewHolder.LessonLayout.Background =
                        viewHolder.LessonLayout.Context.GetDrawable(Resource.Drawable.shape_lesson_bottom);
                }
                else
                {
                    viewHolder.LessonLayout.Background =
                           viewHolder.LessonLayout.Context.GetDrawable(Resource.Drawable.shape_lesson);
                }
            }

            SetLessonType(viewHolder, viewHolder.Lesson, enabled);
            SetAuditoriums(viewHolder, viewHolder.Lesson, enabled);
            SetTitle(viewHolder, viewHolder.Lesson, enabled);
            SetTeachers(viewHolder, viewHolder.Lesson, enabled);
            SetOtherInfo(viewHolder, viewHolder.Lesson, enabled);
        }


        public class ViewHolder : RecyclerView.ViewHolder
        {
            public Lesson Lesson { get; set; }
            public TextView LessonTime { get; }
            public TextView LessonOrder { get; }
            public TextView LessonTitle { get; }
            public TextView LessonType { get; }
            public TextView LessonAuditoriums { get; }
            public TextView LessonTeachers { get; }
            public RelativeLayout LessonLayout { get; }
            public TextView LessonOtherInfo { get; }
            public View FavoriteIcon { get; }
            public View NoteIcon { get; }
            public View Divider { get; }

            public ViewHolder(View view, Action<Lesson> OnLessonClick) : base(view)
            {
                this.LessonTitle = view.FindViewById<TextView>(Resource.Id.text_schedule_title);
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.text_schedule_time);
                this.LessonOrder = view.FindViewById<TextView>(Resource.Id.text_schedule_order);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.text_schedule_type);
                this.LessonTeachers = view.FindViewById<TextView>(Resource.Id.text_schedule_teachers);
                this.LessonAuditoriums = view.FindViewById<TextView>(Resource.Id.text_schedule_auditoriums);
                this.LessonOtherInfo = view.FindViewById<TextView>(Resource.Id.text_schedule_other_info);
                this.LessonLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_schedule);
                this.LessonLayout.Click += (obj, arg) =>
                {
                    OnLessonClick(this.Lesson);
                };
                this.LessonOrder.Background = this.LessonOrder.Background.Mutate();
                this.FavoriteIcon = view.FindViewById<View>(Resource.Id.schedule_favorite_icon);
                this.NoteIcon = view.FindViewById<View>(Resource.Id.schedule_note_icon);
                this.Divider = view.FindViewById<View>(Resource.Id.schedule_divider);
            }
        }
    }
}