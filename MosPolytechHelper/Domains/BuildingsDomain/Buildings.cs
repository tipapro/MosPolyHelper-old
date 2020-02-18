namespace MosPolyHelper.Domains.BuildingsDomain
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    class Buildings
    {
        [JsonProperty(PropertyName = "buildings")]
        readonly string[] building;
        [JsonProperty(PropertyName = "versions")]
        public int Version { get; set; }

        [JsonIgnore]
        public int Count => this.building.Length;

        public Buildings(int version, string[] building)
        {
            this.Version = version;
            this.building = building;
        }

        public string this[int position]
        {
            get => this.building[position];
            set => this.building[position] = value;
        }

        public string[] GetArray()
        {
            return this.building;
        }
    }
}