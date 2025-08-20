using System.Collections.Generic;

namespace mPlanet.Models
{
    public interface IProductInfo
    {
        string ProductId { get; }
        string ProductType { get; }
        string Photo { get; set; }
        Dictionary<string, string> GetDisplayFields();
        string GetDisplayName();
    }
}