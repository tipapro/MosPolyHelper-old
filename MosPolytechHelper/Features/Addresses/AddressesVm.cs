namespace MosPolyHelper.Features.Addresses
{
    using MosPolyHelper.Domains.AddressesDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;

    class AddressesVm : ViewModelBase
    {
        ILogger logger;
        readonly AddressesModel model;
        Addresses addresses;

        public Addresses Addresses
        {
            get => this.addresses;
            set => SetValue(ref this.addresses, value);
        }
        public ICommand RefreshCommand { get; }
        public ICommand InfoCommand { get; }

        public AddressesVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator) :
            base(mediator, ViewModels.Addresses)
        {
            this.logger = loggerFactory.Create<AddressesVm>();
            this.model = new AddressesModel();
            this.RefreshCommand = new Command(Refresh);
            this.InfoCommand = new Command(GetInfo);
        }

        async void Refresh()
        {
            this.Addresses = await this.model.GetAddressesAsync(true);
        }

        public async void SetUpAddresses()
        {
            this.Addresses = await this.model.GetAddressesAsync(false);
        }

        void GetInfo()
        {

        }
    }
}