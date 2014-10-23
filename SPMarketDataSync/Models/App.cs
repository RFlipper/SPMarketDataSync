using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Plumsail.SPMarketDataSync.Models
{
    public class App
    {
        [Key]
        public string AssetId { get; set; }

        public string Title { get; set; }
        public string Publisher { get; set; }
        public string PublisherUrl { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public DateTime ReleasedDate { get; set; }

        public string CategoryID { get; set; }
        public string ThumbnailUrl { get; set; }

        public string Price { get; set; }
        public Decimal PriceValue { get; set; }
        public int PriceType { get; set; }

        public virtual ICollection<History> HistoricalData { get; set; }

        public App()
        {
            HistoricalData = new List<History>();
        }
    }
}
