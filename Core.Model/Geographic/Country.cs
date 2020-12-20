using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Model.Geographic {
    [Serializable]
    public class Country : GeopoliticalLocation, INotifyPropertyChanged {
        [JsonIgnore]
        private string _alpha3;
        [JsonProperty("alpha_3")]
        public string Alpha3 {
            get => _alpha3;
            set {
                _alpha3 = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string _region;
        [JsonProperty("region")]
        public string Region {
            get => _region;
            set {
                _region = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string _subRegion;
        [JsonProperty("sub_region")]
        public string SubRegion {
            get => _subRegion;
            set {
                _subRegion = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string _intermediateRegion;
        [JsonProperty("intermediate_region")]
        public string IntermediateRegion {
            get => _intermediateRegion;
            set {
                _intermediateRegion = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int? _regionCode;
        [JsonProperty("region_code")]
        public int? RegionCode {
            get => _regionCode;
            set {
                _regionCode = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int? _subRegionCode;
        [JsonProperty("sub_region_code")]
        public int? SubRegionCode {
            get => _subRegionCode;
            set {
                _subRegionCode = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int? _intermediateRegionCode;
        [JsonProperty("intermediate_region_code")]
        public int? IntermediateRegionCode {
            get => _intermediateRegionCode;
            set {
                _intermediateRegionCode = value;
                OnPropertyChanged();
            }
        }
    }
}
