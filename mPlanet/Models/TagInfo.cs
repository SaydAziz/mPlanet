using System;

namespace mPlanet.Models
{
    public class TagInfo : IEquatable<TagInfo>
    {
        public string PC { get; set; }
        public string EPC { get; set; }
        public string RSSI { get; set; }
        public DateTime ScanTime { get; set; }

        public TagInfo(string pc, string epc, string rssi)
        {
            PC = pc;
            EPC = epc;
            RSSI = rssi;
            ScanTime = DateTime.Now;
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
