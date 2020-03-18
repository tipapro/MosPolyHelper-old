namespace MosPolyHelper.Features.Addresses
{
    using Android.Content;
    using Android.OS;
    using Android.Util;
    using Android.Views;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.RecyclerView.Widget;
    using AndroidX.SwipeRefreshLayout.Widget;
    using MosPolyHelper.Adapters;
    using MosPolyHelper.Domains.AddressesDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Utilities;
    using System.ComponentModel;

    class AddressesView : FragmentBase
    {
        readonly AddressesVm viewModel;
        RecyclerView recyclerView;
        SwipeRefreshLayout swipeToRefresh;
        Toolbar toolbar;
        int accumulator;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.Addresses):
                    SetUpBuildings(this.viewModel.Addresses);
                    break;
                default:
                    break;
            }
        }

        void SetUpBuildings(Addresses buildings)
        {
            if (buildings != null)
            {
                this.recyclerView?.SetAdapter(new AddressesAdapter(buildings));
            }
            if (this.swipeToRefresh != null)
            {
                this.swipeToRefresh.Refreshing = false;
            }
        }

        public AddressesView() : base(Fragments.Buildings)
        {
            var loggerFactory = DependencyInjector.GetILoggerFactory();
            this.viewModel = new AddressesVm(loggerFactory, DependencyInjector.GetIMediator());
            this.viewModel.PropertyChanged += OnPropertyChanged;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            this.toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.toolbar.Title = GetString(Resource.String.addresses_title);
            (this.Activity as MainView)?.SetSupportActionBar(this.toolbar);
            //(this.Activity as MainView).SupportActionBar.SetDisplayShowTitleEnabled(false);
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this.Activity, drawer, this.toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            toggle.DrawerIndicatorEnabled = true;
            drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            if (this.recyclerView.GetAdapter() == null)
            {
                SetUpBuildings(this.viewModel.Addresses);
            }
        }

        //public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        //{
        //    base.OnCreateOptionsMenu(menu, inflater);
        //    inflater.Inflate(Resource.Menu.menu_buildings, menu);
        //}

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.HasOptionsMenu = true;
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            this.viewModel.SetUpAddresses();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_addresses, container, false);
            this.recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_addresses);
            this.recyclerView.SetLayoutManager(new LinearLayoutManager(container.Context));
            float scale = view.Context.Resources.DisplayMetrics.Density;
            this.recyclerView.AddItemDecoration(new AddressesAdapter.ItemDecoration((int)(8 * scale + 0.5f)));
            float dp8 = TypedValue.ApplyDimension(ComplexUnitType.Dip, 8f, container.Resources.DisplayMetrics);
            float dp32 = dp8 * 4;
            this.recyclerView.ScrollChange += (obj, arg) =>
            {
                if (this.recyclerView.CanScrollVertically(-1))
                {
                    this.accumulator -= arg.OldScrollY;
                    this.toolbar.Elevation =
                    this.accumulator > dp32 ? dp8 : this.accumulator / 4f;
                }
                else
                {
                    this.toolbar.Elevation = this.accumulator = 0;
                }
            };
            this.swipeToRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.addresses_update);
            this.swipeToRefresh.Refresh += (obj, arg) =>
            {
                this.viewModel.RefreshCommand.Execute(null);
            };
            return view;
        }


        public override void OnDestroy()
        {
            this.viewModel.PropertyChanged -= OnPropertyChanged;
            base.OnDestroy();
        }
    }
}