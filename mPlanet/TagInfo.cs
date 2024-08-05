using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mPlanet
{
    internal struct TagInfo
    {
        public string pc { get; set; }
        public string epc { get; set; }
        public string rssi { get; set; }
        public DateTime ScanTime { get; set; }

        public TagInfo(string pc, string epc, string rssi)
        {
            this.pc = pc;
            this.epc = epc;
            this.rssi = rssi;
            ScanTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"EPC: {epc}, RSSI: {rssi}";
        }
    }
}
