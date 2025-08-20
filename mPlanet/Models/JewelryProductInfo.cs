using System.Collections.Generic;

namespace mPlanet.Models
{
    public class JewelryProductInfo : IProductInfo
    {
        public string ProductId { get; set; }
        public string ProductType => "Jewelry";
        public string Photo { get; set; }
        public string BoxNumber { get; set; }
        public string JewelryType { get; set; }

        public JewelryProductInfo(string productId, string boxNumber, string jewelryType, string photo = null)
        {
            ProductId = productId;
            BoxNumber = boxNumber;
            JewelryType = jewelryType;
            Photo = photo;
        }

        public Dictionary<string, string> GetDisplayFields()
        {
            return new Dictionary<string, string>
            {
                ["Box"] = BoxNumber ?? "",
                ["Type"] = JewelryType ?? "",
                ["ID"] = ProductId ?? ""
            };
        }

        public string GetDisplayName()
        {
            return $"{JewelryType} - {ProductId}";
        }
    }
}