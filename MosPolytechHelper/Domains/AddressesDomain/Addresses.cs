namespace MosPolyHelper.Domains.AddressesDomain
{
    using Newtonsoft.Json;

    class Addresses
    {
        [JsonProperty(PropertyName = "addresses")]
        readonly string[] addresses;
        [JsonProperty(PropertyName = "versions")]
        public int Version { get; set; }

        [JsonIgnore]
        public int Count => this.addresses.Length;

        public Addresses(int version, string[] building)
        {
            this.Version = version;
            this.addresses = building;
        }

        public string this[int position]
        {
            get => this.addresses[position];
            set => this.addresses[position] = value;
        }

        public string[] GetArray()
        {
            return this.addresses;
        }
    }
}