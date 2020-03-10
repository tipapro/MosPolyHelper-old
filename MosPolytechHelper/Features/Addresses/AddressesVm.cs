namespace MosPolyHelper.Features.Addresses
{
    using MosPolyHelper.Domains.BuildingsDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;

    class AddressesVm : ViewModelBase
    {
        ILogger logger;
        AddressesModel model;
        Buildings buildings;

        public Buildings Buildings
        {
            get => this.buildings;
            set => SetValue(ref this.buildings, value);
        }
        public ICommand RefreshCommand { get; }
        public ICommand InfoCommand { get; }

        public AddressesVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator) :
            base(mediator, ViewModels.Buildings)
        {
            this.logger = loggerFactory.Create<AddressesVm>();
            this.model = new AddressesModel();
            this.RefreshCommand = new Command(Refresh);
            this.InfoCommand = new Command(GetInfo);
        }

        async void Refresh()
        {
            this.Buildings = await this.model.GetBuildingsAsync(true);
        }

        public async void SetUpBuildings()
        {
            this.Buildings = await this.model.GetBuildingsAsync(false);
        }

        void GetInfo()
        {

        }
    }
}