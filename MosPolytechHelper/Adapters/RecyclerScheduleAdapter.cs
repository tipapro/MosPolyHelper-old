namespace MosPolyHelper.Adapters
{
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Support.V7.Widget;
    using Android.Text;
    using Android.Text.Style;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Domain;
    using System;
    using System.Collections.Generic;

    public class RecyclerScheduleAdapter : RecyclerView.Adapter
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
        bool desaturateOtherDates;
        int itemCount;
        readonly List<(int position, int order)> toInsert;

        readonly Color[] lessonTimeColors = new Color[]
        {
            new Color(244, 122, 113),   // Red
            new Color(255, 209, 117),   // Orange
            new Color(145, 221, 136),   // Green
            new Color(128, 222, 255),   // Blue
            new Color(126, 139, 255),   // Indigo
            new Color(145, 106, 200),   // Purple
            new Color(147, 158, 159),   // Gray
            new Color(206, 221, 235)    // One color mode
        };
        readonly Color[] lessonTypeColors = new Color[]
        {
            new Color(128, 74, 249),    // CourseProject
            new Color(235, 65, 65),     // Exam
            new Color(236, 105, 65),    // Credit
            new Color(227, 126, 200),   // Consultation
            new Color(236, 187, 93),    // Laboratory
            new Color(160, 212, 79),    // Practice
            new Color(116, 185, 244),   // Lecture
            new Color(193, 193, 193)    // Other
        };

        Color GetLessonTypeColor(string lessonType)
        {
            if (lessonType.Contains(CourseProject, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[0];
            }
            else if (lessonType.Contains(Exam, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[1];
            }
            else if (lessonType.Contains(Credit, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[2];
            }
            else if (lessonType.Contains(Consultation, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[3];
            }
            else if (lessonType.Contains(Laboratory, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[4];
            }
            else if (lessonType.Contains(Practice, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[5];
            }
            else if (lessonType.Contains(Lecture, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[6];
            }
            else if (lessonType.Contains(Other, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[7];
            }
            else
            {
                return Color.Gray;
            }
        }

        public override int ItemCount
        {
            get => this.itemCount;
        }

        public DateTime Date { get; set; }
        public Group GroupInfo { get; set; }
        public bool ShowEmptyLessons { get; set; }
        public bool ShowColoredLessons { get; set; }
        public event Action DailyScheduleChanged;

        public RecyclerScheduleAdapter(TextView nullMessage, Schedule.Daily dailySchedule, bool desaturateOtherDates,
            DateTime date, Group grouInfo, bool showEmptyLessons, bool showColoredLessons)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.toInsert = new List<(int position, int order)>(7);
            this.dailySchedule = dailySchedule;
            this.nullMessage = nullMessage;
            this.nullMessage.Visibility = this.dailySchedule != null && this.dailySchedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
            this.desaturateOtherDates = desaturateOtherDates;
            this.Date = date;
            this.GroupInfo = grouInfo;
            SetUpCount();
        }

        public void BuildSchedule(Schedule.Daily dailySchedule, Schedule.Filter scheduleFilter, DateTime date, Group grouInfo,
            bool showEmptyLessons, bool showColoredLessons)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.toInsert.Clear();
            this.dailySchedule = dailySchedule;
            this.GroupInfo = grouInfo;
            this.Date = date;
            this.desaturateOtherDates = scheduleFilter?.DateFitler == DateFilter.Desaturate;
            this.nullMessage.Visibility = this.dailySchedule != null && this.dailySchedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
            SetUpCount();
            DailyScheduleChanged?.Invoke();
            NotifyDataSetChanged();
        }

        public void SetUpCount()
        {
            if (this.dailySchedule == null)
            {
                this.itemCount = 0;
                return;
            }
            if (this.dailySchedule.Count == 0)
            {
                this.itemCount = 0;
                return;
            }
            if (this.ShowEmptyLessons)
            {
                int currOrder = 0;
                for (int i = 0; i < this.dailySchedule.Count; i++)
                {
                    int order = this.dailySchedule.GetLesson(i).Order;
                    if (currOrder > order)
                    {
                        currOrder = order;
                    }
                    else if (currOrder < order)
                    {
                        this.toInsert.Add((i + this.toInsert.Count, currOrder));
                        i--;
                    }
                    currOrder++;
                }
                this.itemCount = this.dailySchedule.Count + this.toInsert.Count;
                return;
            }
            else
            {
                this.itemCount = this.dailySchedule.Count;
                return;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context).Inflate(Resource.Layout.item_schedule, viewGroup, false);
            view.Enabled = false;
            var vh = new ScheduleViewHolder(view, OnItemClick);
            DailyScheduleChanged += vh.OnDailyScheduleChanged;
            return vh;
        }

        void OnItemClick(int position)
        {
            NotifyItemChanged(position);
        }

        void SetHead(ScheduleViewHolder viewHolder, int order)
        {
            viewHolder.LessonTime.SetTextColor(this.ShowColoredLessons ? Color.White : new Color(120, 142, 161));
            viewHolder.LessonOrder.SetTextColor(this.ShowColoredLessons ? Color.White : new Color(120, 142, 161));

            (viewHolder.HeadLayout.Background as GradientDrawable).SetColor(
                this.lessonTimeColors[this.ShowColoredLessons ?
                order % (this.lessonTimeColors.Length - 1) : this.lessonTimeColors.Length - 1]);

            var (timeStart, timeEnd) = Lesson.GetLessonTime(this.Date, order, this.GroupInfo.IsEvening,
                this.GroupInfo.DateFrom);
            string time = timeStart + " - " + timeEnd;
            viewHolder.LessonTime.SetText(time, TextView.BufferType.Normal);
            viewHolder.LessonOrder.SetText($"#{order + 1}", TextView.BufferType.Normal);
        }

        void SetMargin(ScheduleViewHolder viewHolder, int position)
        {
            float scale = viewHolder.LessonLayout.Context.Resources.DisplayMetrics.Density;
            int dp8InPx = (int)(8 * scale + 0.5f);
            (viewHolder.LessonLayout.LayoutParameters as LinearLayout.LayoutParams)
                    .SetMargins(dp8InPx, position == 0 ? dp8InPx : 0, dp8InPx, dp8InPx);
        }

        void SetLessonType(ScheduleViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string type = lesson.Type;
            viewHolder.LessonType.SetTextColor(enabled ? GetLessonTypeColor(lesson.Type) : new Color(225, 225, 225));
            viewHolder.LessonType.SetText(type, TextView.BufferType.Normal);
            viewHolder.LessonType.Enabled = enabled;
        }

        void SetAuditoriums(ScheduleViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            using (var auditoriums = new SpannableStringBuilder())
            {
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
                        auditoriums.Append(lesson.Auditoriums[i].Name + ", ",
                            new ForegroundColorSpan(Color.ParseColor(lesson.Auditoriums[i].Color)), SpanTypes.ExclusiveExclusive);
                    }
                    if (lesson.Auditoriums.Length != 0)
                    {
                        auditoriums.Append(lesson.Auditoriums[lesson.Auditoriums.Length - 1].Name,
                            new ForegroundColorSpan(Color.ParseColor(lesson.Auditoriums[lesson.Auditoriums.Length - 1].Color)),
                            SpanTypes.ExclusiveExclusive);
                    }
                }
                else
                {
                    for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
                    {
                        auditoriums.Append(lesson.Auditoriums[i].Name + ", ",
                            new ForegroundColorSpan(new Color(225, 225, 225)), SpanTypes.ExclusiveExclusive);
                    }
                    if (lesson.Auditoriums.Length != 0)
                    {
                        auditoriums.Append(lesson.Auditoriums[lesson.Auditoriums.Length - 1].Name,
                            new ForegroundColorSpan(new Color(225, 225, 225)),
                            SpanTypes.ExclusiveExclusive);
                    }
                }
                if (viewHolder.ShowFullInfo)
                {
                    viewHolder.LessonAuditoriums.SetSingleLine(false);
                }
                else
                {
                    viewHolder.LessonAuditoriums.SetSingleLine(true);
                }
                viewHolder.LessonAuditoriums.SetText(auditoriums, TextView.BufferType.Normal);
            }
        }

        void SetTitle(ScheduleViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string title = lesson.Title;
            viewHolder.LessonTitle.SetTextColor(enabled ? new Color(95, 107, 117) : new Color(215, 215, 215));
            viewHolder.LessonTitle.SetText(title, TextView.BufferType.Normal);
            viewHolder.LessonTitle.Enabled = enabled;
        }

        void SetTeachers(ScheduleViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string teachers = string.Join(", ", viewHolder.ShowFullInfo ?
                lesson.GetFullTecherNames() : lesson.GetShortTeacherNames());
            if (string.IsNullOrEmpty(teachers))
            {
                viewHolder.LessonTeachers.Visibility = ViewStates.Gone;
            }
            else
            {
                viewHolder.LessonTitle.SetTextColor(enabled ? new Color(95, 107, 117) : new Color(215, 215, 215));
                viewHolder.LessonTeachers.SetText(teachers, TextView.BufferType.Normal);
                viewHolder.LessonTeachers.Enabled = enabled;
                viewHolder.LessonTeachers.Visibility = ViewStates.Visible;
            }
        }

        void SetOtherInfo(ScheduleViewHolder viewHolder, Lesson lesson, bool enabled)
        {
            string otherInfo = string.Empty;
            // Display type of week
            if (lesson.Week != WeekType.None)
            {
                otherInfo = viewHolder.LessonLayout.Context.Resources.GetString(lesson.Week == WeekType.Odd ?
                   Resource.String.odd_week : Resource.String.even_week);
            }
            // Display module
            if (lesson.Module != Module.None)
            {
                if (!string.IsNullOrEmpty(otherInfo))
                {
                    otherInfo += "\n";
                }
                otherInfo += viewHolder.LessonLayout.Context.Resources.GetString(lesson.Module == Module.First ?
                    Resource.String.first_module : Resource.String.second_module);
            }
            // Display dates
            if (lesson.DateFrom != DateTime.MinValue && lesson.DateTo != DateTime.MaxValue)
            {
                if (!string.IsNullOrEmpty(otherInfo))
                {
                    otherInfo += "\n";
                }
                if (lesson.DateFrom == lesson.DateTo)
                {
                    otherInfo += lesson.DateFrom.ToString("d MMM").Replace('.', '\0');
                }
                else
                {
                    otherInfo += lesson.DateFrom.ToString("d MMM").Replace('.', '\0')
                        + " - " + lesson.DateTo.ToString("d MMM").Replace('.', '\0').Replace('.', '\0');
                }
            }
            if (string.IsNullOrEmpty(otherInfo))
            {
                viewHolder.LessonOtherInfo.Visibility = ViewStates.Gone;
            }
            else
            {
                viewHolder.LessonOtherInfo.SetTextColor(enabled ? new Color(191, 197, 202) : new Color(230, 230, 230));
                viewHolder.LessonOtherInfo.SetText(otherInfo, TextView.BufferType.Normal);
                viewHolder.LessonOtherInfo.Enabled = enabled;
                viewHolder.LessonOtherInfo.Visibility = ViewStates.Visible;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(vh is ScheduleViewHolder viewHolder))
            {
                return;
            }
            float scale = viewHolder.LessonLayout.Context.Resources.DisplayMetrics.Density;
            int dp6InPx = (int)(6 * scale + 0.5f);

            bool isCurrentPosEmpty = false;
            int posotionOffset = 0;
            for (; posotionOffset < this.toInsert.Count; posotionOffset++)
            {
                if (position == this.toInsert[posotionOffset].position)
                {
                    isCurrentPosEmpty = true;
                    break;
                }
                else if (position < this.toInsert[posotionOffset].position)
                {
                    break;
                }
            }
            if (isCurrentPosEmpty)
            {
                SetHead(viewHolder, this.toInsert[posotionOffset].order);
                SetMargin(viewHolder, position);
                viewHolder.HeadLayout.Visibility = ViewStates.Visible;
                viewHolder.BodyLayout.Visibility = ViewStates.Visible;
                var emptyLesson = new Lesson(posotionOffset, " ", new string[] { "tipapro" }, DateTime.MinValue,
                    DateTime.MaxValue, null, "", WeekType.None, Module.None);
                SetLessonType(viewHolder, emptyLesson, true);
                SetAuditoriums(viewHolder, emptyLesson, true);
                SetTitle(viewHolder, emptyLesson, true);
                SetTeachers(viewHolder, emptyLesson, true);
                SetOtherInfo(viewHolder, emptyLesson, true);
                viewHolder.LineScheduleBottomBorder.Visibility = ViewStates.Gone;
                viewHolder.LineScheduleTopBorder.Visibility = ViewStates.Gone;
                //(viewHolder.HeadLayout.Background as GradientDrawable).SetCornerRadius(dp6InPx);
                (viewHolder.HeadLayout.Background as GradientDrawable).SetCornerRadii(new float[]
                { dp6InPx, dp6InPx, dp6InPx, dp6InPx, 0, 0, 0, 0 });
                (viewHolder.LessonLayout.Background as GradientDrawable).SetCornerRadius(dp6InPx);
                return;
            }
            else
            {
                viewHolder.BodyLayout.Visibility = ViewStates.Visible;
            }
            int fixedPosition = position - posotionOffset;
            Lesson lesson;
            try
            {
                lesson = this.dailySchedule?.GetLesson(fixedPosition);
                if (lesson == null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            SetMargin(viewHolder, position);
            (viewHolder.LessonLayout.Background as GradientDrawable).SetCornerRadius(dp6InPx);
            if (fixedPosition != 0 && this.dailySchedule.GetLesson(fixedPosition - 1)?.Order == lesson.Order)
            {
                viewHolder.HeadLayout.Visibility = ViewStates.Gone;
                (viewHolder.LessonLayout.Background as GradientDrawable).SetCornerRadii(new float[]
                { 0, 0, 0, 0, dp6InPx, dp6InPx, dp6InPx, dp6InPx });
                viewHolder.LineScheduleTopBorder.Visibility = ViewStates.Visible;
            }
            else
            {
                SetHead(viewHolder, lesson.Order);
                (viewHolder.HeadLayout.Background as GradientDrawable).SetCornerRadii(new float[]
                { dp6InPx, dp6InPx, dp6InPx, dp6InPx, 0, 0, 0, 0 });
                viewHolder.HeadLayout.Visibility = ViewStates.Visible;
                viewHolder.LineScheduleTopBorder.Visibility = ViewStates.Gone;
            }
            if ((fixedPosition != this.dailySchedule.Count - 1) &&
                this.dailySchedule.GetLesson(fixedPosition + 1)?.Order == lesson.Order)
            {
                viewHolder.LineScheduleBottomBorder.Visibility = ViewStates.Visible;
                var lessonLayoutParams = viewHolder.LessonLayout.LayoutParameters as LinearLayout.LayoutParams;
                lessonLayoutParams.BottomMargin = 0;
                (viewHolder.LessonLayout.Background as GradientDrawable).SetCornerRadii(new float[]
                { dp6InPx, dp6InPx, dp6InPx, dp6InPx, 0, 0, 0, 0 });
            }
            else
            {
                viewHolder.LineScheduleBottomBorder.Visibility = ViewStates.Invisible;
            }
            if (fixedPosition != 0 && fixedPosition != this.dailySchedule.Count - 1)
            {
                if ((this.dailySchedule.GetLesson(fixedPosition - 1)?.Order == lesson.Order)
                    && (this.dailySchedule.GetLesson(fixedPosition + 1)?.Order == lesson.Order))
                {
                    (viewHolder.LessonLayout.Background as GradientDrawable).SetCornerRadius(0);
                }
            }

            bool enabledFrom = lesson.DateFrom <= this.Date || !this.desaturateOtherDates;
            bool enabledTo = lesson.DateTo >= this.Date || !this.desaturateOtherDates;
            bool enabled = enabledFrom && enabledTo;
            if (lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) ||
                lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) ||
                lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase))
            {
                enabled = enabledTo;
            }

            viewHolder.LessonLayout.Enabled = enabled;

            SetLessonType(viewHolder, lesson, enabled);
            SetAuditoriums(viewHolder, lesson, enabled);
            SetTitle(viewHolder, lesson, enabled);
            SetTeachers(viewHolder, lesson, enabled);
            SetOtherInfo(viewHolder, lesson, enabled);
        }


        public class ScheduleViewHolder : RecyclerView.ViewHolder
        {
            public bool ShowFullInfo { get; set; }
            public TextView LessonTime { get; }
            public TextView LessonOrder { get; }
            public TextView LessonTitle { get; }
            public TextView LessonType { get; }
            public TextView LessonAuditoriums { get; }
            public TextView LessonTeachers { get; }
            public LinearLayout LessonLayout { get; }
            public RelativeLayout HeadLayout { get; }
            public RelativeLayout BodyLayout { get; }
            public TextView LessonOtherInfo { get; }
            public View LineScheduleBottomBorder { get; }
            public View LineScheduleTopBorder { get; }

            public ScheduleViewHolder(View view, Action<int> OnItemClick) : base(view)
            {
                this.ShowFullInfo = false;
                this.LessonTitle = view.FindViewById<TextView>(Resource.Id.text_schedule_title);
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.text_schedule_time);
                this.LessonOrder = view.FindViewById<TextView>(Resource.Id.text_schedule_order);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.text_schedule_type);
                this.LessonTeachers = view.FindViewById<TextView>(Resource.Id.text_schedule_teachers);
                this.LessonAuditoriums = view.FindViewById<TextView>(Resource.Id.text_schedule_auditoriums);
                this.LessonOtherInfo = view.FindViewById<TextView>(Resource.Id.text_schedule_other_info);
                this.LessonLayout = view.FindViewById<LinearLayout>(Resource.Id.layout_schedule);
                this.LessonLayout.Click += (obj, arg) =>
                {
                    this.ShowFullInfo = !this.ShowFullInfo;
                    OnItemClick(this.LayoutPosition);
                };
                this.HeadLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_schedule_head);
                this.BodyLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_schedule_body);
                this.LineScheduleBottomBorder = view.FindViewById<View>(Resource.Id.line_schedule_bottom_border);
                this.LineScheduleTopBorder = view.FindViewById<View>(Resource.Id.line_schedule_top_border);

                this.LessonLayout.Background = this.LessonLayout.Background.Mutate();
                this.HeadLayout.Background = this.HeadLayout.Background.Mutate();
            }

            public void OnDailyScheduleChanged()
            {
                this.ShowFullInfo = false;
            }
        }
    }
}