using System;

namespace Plumsail.SPMarketDataSync.Models
{
    public class History
    {
        public string AssetId { get; set; }
        public virtual App Product { get; set; }
        public DateTime Date { get; set; }

        public int Downloads { get; set; }
        public int Rating { get; set; }
        public int Votes { get; set; }

        public int ID { get; set; }
    }
}
