//// by nshmura, rewrited in c#

//namespace MosPolyHelper.CustomElements
//{
//    using Android.Animation;
//    using Android.Content;
//    using Android.Content.Res;
//    using Android.Graphics;
//    using Android.Support.V4.View;
//    using Android.Support.V7.Content.Res;
//    using Android.Support.V7.Widget;
//    using Android.Text;
//    using Android.Util;
//    using Android.Views;
//    using Android.Widget;
//    using System;
//    using System.Linq;

//    public class RecyclerTabLayout : RecyclerView
//    {

//        protected static long DEFAULT_SCROLL_DURATION = 200;
//        protected static float DEFAULT_POSITION_THRESHOLD = 0.6f;
//        protected static float POSITION_THRESHOLD_ALLOWABLE = 0.001f;

//        protected Paint mIndicatorPaint;
//        protected int mTabBackgroundResId;
//        protected int mTabOnScreenLimit;
//        protected int mTabMinWidth;
//        protected int mTabMaxWidth;
//        protected int mTabTextAppearance;
//        protected int mTabSelectedTextColor;
//        protected bool mTabSelectedTextColorSet;
//        protected int mTabPaddingStart;
//        protected int mTabPaddingTop;
//        protected int mTabPaddingEnd;
//        protected int mTabPaddingBottom;
//        protected int mIndicatorHeight;

//        protected LinearLayoutManager mLinearLayoutManager;
//        protected RecyclerOnScrollListener mRecyclerOnScrollListener;
//        protected ViewPager viewPager;
//        protected Adapter adapter;
//        protected int scrollState;
//        protected int indicatorPosition;
//        protected int mIndicatorGap;
//        protected int mIndicatorScroll;
//        private int mOldPosition;
//        private int mOldScrollOffSet;
//        protected float mOldPositionOffSet;
//        protected float mPositionThreshold;
//        protected bool mRequestScrollToTab;
//        protected bool mScrollEanbled;

//        public RecyclerTabLayout(Context context) : this(context, null)
//        {
//        }

//        public RecyclerTabLayout(Context context, IAttributeSet attrs) : this(context, attrs, 0)
//        {
//        }
//        class CustomLinearLayoutManager : LinearLayoutManager
//        {
//            bool mScrollEanbled;

//            public CustomLinearLayoutManager(Context context, ref bool mScrollEanbled) : base(context)
//            {
//                this.mScrollEanbled = mScrollEanbled;
//            }
//            public override bool CanScrollHorizontally()
//            {
//                return this.mScrollEanbled;
//            }
//        }

//        public RecyclerTabLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
//        {
//            SetWillNotDraw(false);
//            this.mIndicatorPaint = new Paint();
//            GetAttributes(context, attrs, defStyle);
//            this.mLinearLayoutManager = new CustomLinearLayoutManager(this.Context, ref this.mScrollEanbled)
//            {
//                Orientation = LinearLayoutManager.Horizontal
//            };
//            SetLayoutManager(this.mLinearLayoutManager);
//            SetItemAnimator(null);
//            this.mPositionThreshold = DEFAULT_POSITION_THRESHOLD;
//        }

//        private void GetAttributes(Context context, IAttributeSet attrs, int defStyle)
//        {
//            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.rtl_RecyclerTabLayout,
//                    defStyle, Resource.Style.rtl_RecyclerTabLayout);
//            SetIndicatorColor(a.GetColor(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabIndicatorColor, 0));
//            SetIndicatorHeight(a.GetDimensionPixelSize(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabIndicatorHeight, 0));

//            this.mTabTextAppearance = a.GetResourceId(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabTextAppearance,
//            Resource.Style.rtl_RecyclerTabLayout_Tab);

//            this.mTabPaddingStart = this.mTabPaddingTop = this.mTabPaddingEnd = this.mTabPaddingBottom = a
//            .GetDimensionPixelSize(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabPadding, 0);
//            this.mTabPaddingStart = a.GetDimensionPixelSize(
//            Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabPaddingStart, this.mTabPaddingStart);
//            this.mTabPaddingTop = a.GetDimensionPixelSize(
//            Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabPaddingTop, this.mTabPaddingTop);
//            this.mTabPaddingEnd = a.GetDimensionPixelSize(
//            Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabPaddingEnd, this.mTabPaddingEnd);
//            this.mTabPaddingBottom = a.GetDimensionPixelSize(
//            Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabPaddingBottom, this.mTabPaddingBottom);

//            if (a.HasValue(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabSelectedTextColor))
//            {
//                this.mTabSelectedTextColor = a
//                .GetColor(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabSelectedTextColor, 0);
//                this.mTabSelectedTextColorSet = true;
//            }

//            this.mTabOnScreenLimit = a.GetInteger(
//            Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabOnScreenLimit, 0);
//            if (this.mTabOnScreenLimit == 0)
//            {
//                this.mTabMinWidth = a.GetDimensionPixelSize(
//                Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabMinWidth, 0);
//                this.mTabMaxWidth = a.GetDimensionPixelSize(
//                Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabMaxWidth, 0);
//            }

//            this.mTabBackgroundResId = a
//            .GetResourceId(Resource.Styleable.rtl_RecyclerTabLayout_rtl_tabBackground, 0);
//            this.mScrollEanbled = a.GetBoolean(Resource.Styleable.rtl_RecyclerTabLayout_rtl_scrollEnabled, true);
//            a.Recycle();
//        }

//        protected override void OnDetachedFromWindow()
//        {
//            if (this.mRecyclerOnScrollListener != null)
//            {
//                RemoveOnScrollListener(this.mRecyclerOnScrollListener);
//                this.mRecyclerOnScrollListener = null;
//            }
//            base.OnDetachedFromWindow();
//        }


//        public void SetIndicatorColor(Color color)
//        {
//            this.mIndicatorPaint.Color = color;
//        }

//        public void SetIndicatorHeight(int indicatorHeight)
//        {
//            this.mIndicatorHeight = indicatorHeight;
//        }

//        public void SetAutoSelectionMode(bool autoSelect)
//        {
//            if (this.mRecyclerOnScrollListener != null)
//            {
//                RemoveOnScrollListener(this.mRecyclerOnScrollListener);
//                this.mRecyclerOnScrollListener = null;
//            }
//            if (autoSelect)
//            {
//                this.mRecyclerOnScrollListener = new RecyclerOnScrollListener(this, this.mLinearLayoutManager);
//                AddOnScrollListener(this.mRecyclerOnScrollListener);
//            }
//        }

//        public void SetPositionThreshold(float positionThreshold)
//        {
//            this.mPositionThreshold = positionThreshold;
//        }

//        public void SetUpWithViewPager(ViewPager viewPager)
//        {
//            var adapter = new DefaultAdapter(viewPager);
//            adapter.SetTabPadding(this.mTabPaddingStart, this.mTabPaddingTop, this.mTabPaddingEnd, this.mTabPaddingBottom);
//            adapter.SetTabTextAppearance(this.mTabTextAppearance);
//            adapter.SetTabSelectedTextColor(this.mTabSelectedTextColorSet, this.mTabSelectedTextColor);
//            adapter.SetTabMaxWidth(this.mTabMaxWidth);
//            adapter.SetTabMinWidth(this.mTabMinWidth);
//            adapter.SetTabBackgroundResId(this.mTabBackgroundResId);
//            adapter.SetTabOnScreenLimit(this.mTabOnScreenLimit);
//            SetUpWithAdapter(adapter);
//        }

//        public void SetUpWithAdapter(RecyclerTabLayout.Adapter adapter)
//        {
//            this.adapter = adapter;
//            this.viewPager = adapter.ViewPager;
//            if (this.viewPager.Adapter == null)
//            {
//                throw new ArgumentException("ViewPager does not have a PagerAdapter Set");
//            }
//            this.viewPager.PageSelected += (obj, arg) =>
//            {
//                if (this.scrollState == ViewPager.ScrollStateIdle && this.indicatorPosition != arg.Position)
//                {
//                    ScrollToTab(arg.Position);
//                }
//            };
//            this.viewPager.PageScrollStateChanged += (obj, arg) =>
//            {
//                this.scrollState = arg.State;
//            };
//            this.viewPager.PageScrolled += (obj, arg) =>
//            {
//                ScrollToTab(arg.Position, arg.PositionOffset, false);
//            };
//            SetAdapter(adapter);

//            ScrollToTab(this.viewPager.CurrentItem);
//        }

//        public void SetCurrentItem(int position, bool smoothScroll)
//        {
//            if (this.viewPager != null)
//            {
//                this.viewPager.SetCurrentItem(position, smoothScroll);
//                ScrollToTab(this.viewPager.CurrentItem);
//                return;
//            }

//            if (smoothScroll && position != this.indicatorPosition)
//            {
//                StartAnimation(position);

//            }
//            else
//            {
//                ScrollToTab(position);
//            }
//        }

//        protected void StartAnimation(int position)
//        {

//            float distance = 1;

//            var view = this.mLinearLayoutManager.FindViewByPosition(position);
//            if (view != null)
//            {
//                float currentX = view.GetX() + view.MeasuredWidth / 2f;
//                float centerX = this.MeasuredWidth / 2f;
//                distance = Math.Abs(centerX - currentX) / view.MeasuredWidth;
//            }

//            ValueAnimator animator;
//            if (position < this.indicatorPosition)
//            {
//                animator = ValueAnimator.OfFloat(distance, 0);
//            }
//            else
//            {
//                animator = ValueAnimator.OfFloat(-distance, 0);
//            }
//            animator.SetDuration(DEFAULT_SCROLL_DURATION);
//            animator.Update += (obj, arg) =>
//                {
//                    ScrollToTab(position, (float)arg.Animation.AnimatedValue, true);
//                };
//            animator.Start();
//        }

//        protected void ScrollToTab(int position)
//        {
//            ScrollToTab(position, 0, false);
//            this.adapter.SetCurrentIndicatorPosition(position);
//            this.adapter.NotifyDataSetChanged();
//        }

//        protected void ScrollToTab(int position, float positionOffSet, bool fitIndicator)
//        {
//            int scrollOffSet = 0;

//            var selectedView = this.mLinearLayoutManager.FindViewByPosition(position);
//            var nextView = this.mLinearLayoutManager.FindViewByPosition(position + 1);

//            if (selectedView != null)
//            {
//                int width = this.MeasuredWidth;
//                float sLeft = (position == 0) ? 0 : width / 2f - selectedView.MeasuredWidth / 2f; // left edge of selected tab
//                float sRight = sLeft + selectedView.MeasuredWidth; // right edge of selected tab

//                if (nextView != null)
//                {
//                    float nLeft = width / 2f - nextView.MeasuredWidth / 2f; // left edge of next tab
//                    float distance = sRight - nLeft; // total distance that is needed to distance to next tab
//                    float dx = distance * positionOffSet;
//                    scrollOffSet = (int)(sLeft - dx);

//                    if (position == 0)
//                    {
//                        float indicatorGap = (nextView.MeasuredWidth - selectedView.MeasuredWidth) / 2;
//                        this.mIndicatorGap = (int)(indicatorGap * positionOffSet);
//                        this.mIndicatorScroll = (int)((selectedView.MeasuredWidth + indicatorGap) * positionOffSet);

//                    }
//                    else
//                    {
//                        float indicatorGap = (nextView.MeasuredWidth - selectedView.MeasuredWidth) / 2;
//                        this.mIndicatorGap = (int)(indicatorGap * positionOffSet);
//                        this.mIndicatorScroll = (int)dx;
//                    }

//                }
//                else
//                {
//                    scrollOffSet = (int)sLeft;
//                    this.mIndicatorScroll = 0;
//                    this.mIndicatorGap = 0;
//                }
//                if (fitIndicator)
//                {
//                    this.mIndicatorScroll = 0;
//                    this.mIndicatorGap = 0;
//                }

//            }
//            else
//            {
//                if (this.MeasuredWidth > 0 && this.mTabMaxWidth > 0 && this.mTabMinWidth == this.mTabMaxWidth)
//                { //fixed size
//                    int width = this.mTabMinWidth;
//                    int offSet = (int)(positionOffSet * -width);
//                    int leftOffSet = (int)((this.MeasuredWidth - width) / 2f);
//                    scrollOffSet = offSet + leftOffSet;
//                }
//                this.mRequestScrollToTab = true;
//            }

//            UpdateCurrentIndicatorPosition(position, positionOffSet - this.mOldPositionOffSet, positionOffSet);
//            this.indicatorPosition = position;

//            StopScroll();

//            if (position != this.mOldPosition || scrollOffSet != this.mOldScrollOffSet)
//            {
//                this.mLinearLayoutManager.ScrollToPositionWithOffset(position, scrollOffSet);
//            }
//            if (this.mIndicatorHeight > 0)
//            {
//                Invalidate();
//            }

//            this.mOldPosition = position;
//            this.mOldScrollOffSet = scrollOffSet;
//            this.mOldPositionOffSet = positionOffSet;
//        }

//        protected void UpdateCurrentIndicatorPosition(int position, float dx, float positionOffSet)
//        {
//            if (this.adapter == null)
//            {
//                return;
//            }
//            int indicatorPosition = -1;
//            if (dx > 0 && positionOffSet >= this.mPositionThreshold - POSITION_THRESHOLD_ALLOWABLE)
//            {
//                indicatorPosition = position + 1;

//            }
//            else if (dx < 0 && positionOffSet <= 1 - this.mPositionThreshold + POSITION_THRESHOLD_ALLOWABLE)
//            {
//                indicatorPosition = position;
//            }
//            if (indicatorPosition >= 0 && indicatorPosition != this.adapter.GetCurrentIndicatorPosition())
//            {
//                this.adapter.SetCurrentIndicatorPosition(indicatorPosition);
//                this.adapter.NotifyDataSetChanged();
//            }
//        }

//        public override void OnDraw(Canvas canvas)
//        {
//            var view = this.mLinearLayoutManager.FindViewByPosition(this.indicatorPosition);
//            if (view == null)
//            {
//                if (this.mRequestScrollToTab)
//                {
//                    this.mRequestScrollToTab = false;
//                    ScrollToTab(this.viewPager.CurrentItem);
//                }
//                return;
//            }
//            this.mRequestScrollToTab = false;

//            int left;
//            int right;
//            if (IsLayoutRtl())
//            {
//                left = view.Left - this.mIndicatorScroll - this.mIndicatorGap;
//                right = view.Right - this.mIndicatorScroll + this.mIndicatorGap;
//            }
//            else
//            {
//                left = view.Left + this.mIndicatorScroll - this.mIndicatorGap;
//                right = view.Right + this.mIndicatorScroll + this.mIndicatorGap;
//            }

//            int top = this.Height - this.mIndicatorHeight;
//            int bottom = this.Height;

//            canvas.DrawRect(left, top, right, bottom, this.mIndicatorPaint);
//        }

//        protected bool IsLayoutRtl()
//        {
//            return ViewCompat.GetLayoutDirection(this) == ViewCompat.LayoutDirectionRtl;
//        }

//        protected class RecyclerOnScrollListener : OnScrollListener
//        {

//            protected RecyclerTabLayout mRecyclerTabLayout;
//            protected LinearLayoutManager mLinearLayoutManager;

//            public RecyclerOnScrollListener(RecyclerTabLayout recyclerTabLayout, LinearLayoutManager linearLayoutManager)
//            {
//                this.mRecyclerTabLayout = recyclerTabLayout;
//                this.mLinearLayoutManager = linearLayoutManager;
//            }

//            public int mDx;

//            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
//            {
//                this.mDx += dx;
//            }

//            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
//            {
//                switch (newState)
//                {
//                    case ScrollStateIdle:
//                        if (this.mDx > 0)
//                        {
//                            SelectCenterTabForRightScroll();
//                        }
//                        else
//                        {
//                            SelectCenterTabForLeftScroll();
//                        }
//                        this.mDx = 0;
//                        break;
//                    case ScrollStateDragging:
//                    case ScrollStateSettling:
//                        break;
//                }
//            }

//            protected void SelectCenterTabForRightScroll()
//            {
//                int first = this.mLinearLayoutManager.FindFirstVisibleItemPosition();
//                int last = this.mLinearLayoutManager.FindLastVisibleItemPosition();
//                int center = this.mRecyclerTabLayout.Width / 2;
//                for (int position = first; position <= last; position++)
//                {
//                    var view = this.mLinearLayoutManager.FindViewByPosition(position);
//                    if (view.Left + view.Width >= center)
//                    {
//                        this.mRecyclerTabLayout.SetCurrentItem(position, false);
//                        break;
//                    }
//                }
//            }

//            protected void SelectCenterTabForLeftScroll()
//            {
//                int first = this.mLinearLayoutManager.FindFirstVisibleItemPosition();
//                int last = this.mLinearLayoutManager.FindLastVisibleItemPosition();
//                int center = this.mRecyclerTabLayout.Width / 2;
//                for (int position = last; position >= first; position--)
//                {
//                    var view = this.mLinearLayoutManager.FindViewByPosition(position);
//                    if (view.Left <= center)
//                    {
//                        this.mRecyclerTabLayout.SetCurrentItem(position, false);
//                        break;
//                    }
//                }
//            }
//        }


//        public new abstract class Adapter : RecyclerView.Adapter
//        {
//            protected ViewPager viewPager;
//            protected int indicatorPosition;

//            public Adapter(ViewPager viewPager)
//            {
//                this.viewPager = viewPager;
//            }

//            public ViewPager ViewPager
//            {
//                get => this.viewPager;
//            }

//            public void SetCurrentIndicatorPosition(int indicatorPosition)
//            {
//                this.indicatorPosition = indicatorPosition;
//            }

//            public int GetCurrentIndicatorPosition()
//            {
//                return this.indicatorPosition;
//            }
//        }

//        public class DefaultAdapter : RecyclerTabLayout.Adapter
//        {

//            protected static int MaxTabTextLines = 2;

//            protected int mTabPaddingStart;
//            protected int mTabPaddingTop;
//            protected int mTabPaddingEnd;
//            protected int mTabPaddingBottom;
//            protected int mTabTextAppearance;
//            protected bool mTabSelectedTextColorSet;
//            protected int mTabSelectedTextColor;
//            private int mTabMaxWidth;
//            private int mTabMinWidth;
//            private int mTabBackgroundResId;
//            private int mTabOnScreenLimit;

//            public DefaultAdapter(ViewPager viewPager) : base(viewPager)
//            {
//            }

//            [Obsolete("deprecated")]
//            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
//            {
//                var tabTextView = new TabTextView(parent.Context);

//                if (this.mTabSelectedTextColorSet)
//                {
//                    tabTextView.SetTextColor(tabTextView.createColorStateList(
//                            tabTextView.CurrentTextColor, this.mTabSelectedTextColor));
//                }

//                ViewCompat.SetPaddingRelative(tabTextView, this.mTabPaddingStart, this.mTabPaddingTop,
//                        this.mTabPaddingEnd, this.mTabPaddingBottom);
//                tabTextView.SetTextAppearance(parent.Context, this.mTabTextAppearance);
//                tabTextView.Gravity = GravityFlags.Center;
//                tabTextView.SetMaxLines(MaxTabTextLines);
//                tabTextView.Ellipsize = TextUtils.TruncateAt.End;

//                if (this.mTabOnScreenLimit > 0)
//                {
//                    int width = parent.MeasuredWidth / this.mTabOnScreenLimit;
//                    tabTextView.SetMaxWidth(width);
//                    tabTextView.SetMinWidth(width);
//                }
//                else
//                {
//                    if (this.mTabMaxWidth > 0)
//                    {
//                        tabTextView.SetMaxWidth(this.mTabMaxWidth);
//                    }
//                    tabTextView.SetMinWidth(this.mTabMinWidth);
//                }

//                tabTextView.SetTextAppearance(tabTextView.Context, this.mTabTextAppearance);
//                if (this.mTabSelectedTextColorSet)
//                {
//                    tabTextView.SetTextColor(tabTextView.createColorStateList(
//                            tabTextView.CurrentTextColor, this.mTabSelectedTextColor));
//                }
//                if (this.mTabBackgroundResId != 0)
//                {
//                    tabTextView.SetBackgroundDrawable(
//                            AppCompatResources.GetDrawable(tabTextView.Context, this.mTabBackgroundResId));
//                }
//                tabTextView.LayoutParameters = CreateLayoutParamsForTabs();
//                return new ViewHolder(tabTextView, this);
//            }

//            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
//            {
//                var viewHolder = holder as DefaultAdapter.ViewHolder;
//                string title = this.ViewPager.Adapter.GetPageTitle(position);
//                viewHolder.Title.Text = title;
//                viewHolder.Title.Selected = GetCurrentIndicatorPosition() == position;
//            }

//            public override int ItemCount
//            {
//                get => this.ViewPager.Adapter.Count;
//            }

//            public void SetTabPadding(int tabPaddingStart, int tabPaddingTop, int tabPaddingEnd,
//                                      int tabPaddingBottom)
//            {
//                this.mTabPaddingStart = tabPaddingStart;
//                this.mTabPaddingTop = tabPaddingTop;
//                this.mTabPaddingEnd = tabPaddingEnd;
//                this.mTabPaddingBottom = tabPaddingBottom;
//            }

//            public void SetTabTextAppearance(int tabTextAppearance)
//            {
//                this.mTabTextAppearance = tabTextAppearance;
//            }

//            public void SetTabSelectedTextColor(bool tabSelectedTextColorSet,
//                                                int tabSelectedTextColor)
//            {
//                this.mTabSelectedTextColorSet = tabSelectedTextColorSet;
//                this.mTabSelectedTextColor = tabSelectedTextColor;
//            }

//            public void SetTabMaxWidth(int tabMaxWidth)
//            {
//                this.mTabMaxWidth = tabMaxWidth;
//            }

//            public void SetTabMinWidth(int tabMinWidth)
//            {
//                this.mTabMinWidth = tabMinWidth;
//            }

//            public void SetTabBackgroundResId(int tabBackgroundResId)
//            {
//                this.mTabBackgroundResId = tabBackgroundResId;
//            }

//            public void SetTabOnScreenLimit(int tabOnScreenLimit)
//            {
//                this.mTabOnScreenLimit = tabOnScreenLimit;
//            }

//            protected RecyclerView.LayoutParams CreateLayoutParamsForTabs()
//            {
//                return new RecyclerView.LayoutParams(
//                        ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
//            }

//            public class ViewHolder : RecyclerView.ViewHolder
//            {

//                public TextView Title;

//                public ViewHolder(View itemView, Adapter adapter) : base(itemView)
//                {
//                    this.Title = (TextView)itemView;
//                    itemView.Click += (obj, arg) =>
//                    {
//                        int pos = this.AdapterPosition;
//                        if (pos != NoPosition)
//                        {
//                            adapter.ViewPager.SetCurrentItem(pos, true);
//                        }
//                    };
//                }
//            }
//        }


//        public class TabTextView : AppCompatTextView
//        {
//            public TabTextView(Context context) : base(context)
//            {
//            }

//            public ColorStateList createColorStateList(int defaultColor, int selectedColor)
//            {
//                int[][] states = new int[2][];
//                int[] colors = new int[2];
//                states[0] = SelectedStateSet.ToArray();
//                colors[0] = selectedColor;
//                // Default enabled state
//                states[1] = EmptyStateSet.ToArray();
//                colors[1] = defaultColor;
//                return new ColorStateList(states, colors);
//            }
//        }
//    }
//}