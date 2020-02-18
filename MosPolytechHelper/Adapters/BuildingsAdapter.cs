namespace MosPolyHelper.Adapters
{
    using Android.Graphics;
    using Android.Text;
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Domains.BuildingsDomain;

    class BuildingsAdapter : RecyclerView.Adapter
    {
        Buildings buildings;

        public override int ItemCount => this.buildings.Count;

        public BuildingsAdapter(Buildings buildings)
        {
            this.buildings = buildings;
        }

        public void Update(Buildings buildings)
        {
            this.buildings = buildings;
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_building, parent, false);
            return new ViewHolder(view);
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ViewHolder;
            var spanned = Html.FromHtml(this.buildings[position], FromHtmlOptions.ModeLegacy);
            viewHolder.Text.SetText(spanned, TextView.BufferType.Normal);
        }

        public class ViewHolder : RecyclerView.ViewHolder
        {
            public TextView Text { get; }

            public ViewHolder(View v) : base(v)
            {
                this.Text = v.FindViewById<TextView>(Resource.Id.text);
            }
        }

        public class ItemDecoration : RecyclerView.ItemDecoration
        {
            readonly int offset;

            public ItemDecoration(int offset) : base()
            {
                this.offset = offset;
            }

            public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
            {
                outRect.Top = outRect.Bottom = this.offset;
            }
        }
    }
}