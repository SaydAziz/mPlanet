using System;
using System.ComponentModel;

namespace mPlanet.Models
{
    public class TagInfo : IEquatable<TagInfo>, INotifyPropertyChanged
    {
        public string PC { get; set; }
        public string EPC { get; set; }
        public string RSSI { get; set; }
        public DateTime ScanTime { get; set; }
        public IProductInfo ProductInfo { get; set; }
        
        private string _status = "Ожидается";
        public string Status 
        { 
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        private string _notes = "";
        public string Notes 
        { 
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TagInfo(string pc, string epc, string rssi, IProductInfo productInfo = null)
        {
            PC = pc;
            EPC = epc;
            RSSI = rssi;
            ScanTime = DateTime.Now;
            ProductInfo = productInfo;
        }

        public bool Equals(TagInfo other)
        {
            if (other == null) return false;

            return EPC?.Equals(other.EPC, StringComparison.OrdinalIgnoreCase) == true;
        }

        public override bool Equals(object obj) => Equals(obj as TagInfo);

        public override int GetHashCode() => EPC?.GetHashCode() ?? 0;

        public override string ToString() => $"EPC: {EPC}, RSSI: {RSSI}";
    }
}
