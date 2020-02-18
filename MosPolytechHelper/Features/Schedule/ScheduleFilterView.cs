namespace MosPolyHelper.Features.Schedule
{
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Views;
    using AndroidX.Fragment.App;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Adapters;

    class ScheduleFilterView : DialogFragment
    {
        AdvancedSearchAdapter adapter;

        public void SetAdapter(AdvancedSearchAdapter adapter)
        {
            this.adapter = adapter;
            var recyclerView = this.Dialog?.FindViewById<RecyclerView>(Resource.Id.recycler_advanced_search);
            if (recyclerView != null)
            {
                var searchView = this.Dialog.FindViewById<Android.Widget.SearchView>(Resource.Id.searchView1);
                if (searchView != null)
                {
                    searchView.QueryTextChange += (obj, arg) => adapter.UpdateTemplate(arg.NewText);
                }
                recyclerView.SetAdapter(adapter);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.Dialog.Window.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.Top);
            return inflater.Inflate(Resource.Layout.fragment_schedule_filter, container);
        }

        public override void OnStart()
        {
            base.OnStart();

            if (this.Dialog != null)
            {
                this.Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                this.Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                var recyclerView = this.Dialog.FindViewById<RecyclerView>(Resource.Id.recycler_advanced_search);
                if (recyclerView != null)
                {
                    var layoutManager = new LinearLayoutManager(recyclerView.Context);
                    var divider = new DividerItemDecoration(recyclerView.Context, layoutManager.Orientation)
                    {
                        Drawable = this.Context.GetDrawable(Resource.Drawable.all_divider)
                    };
                    recyclerView.AddItemDecoration(divider);
                    recyclerView.SetLayoutManager(layoutManager);
                    recyclerView.SetAdapter(this.adapter);
                    recyclerView.GetAdapter().NotifyDataSetChanged();

                    var searchView = this.Dialog.FindViewById<Android.Widget.SearchView>(Resource.Id.searchView1);
                    if (searchView != null)
                    {
                        searchView.QueryTextChange += (obj, arg) => this.adapter.UpdateTemplate(arg.NewText);
                    }
                }
            }
        }

        public static ScheduleFilterView NewInstance()
        {
            var fragment = new ScheduleFilterView();
            return fragment;
        }
    }
}