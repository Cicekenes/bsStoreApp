namespace Entities.RequestFeatures
{
    public class BookParameters : RequestParameters
    {
        //Fiyat ifadesi negatif olamayacağı için uint kullanıldı
        public uint MinPrice { get; set; }
        public uint MaxPrice { get; set; } = 1000;
        public bool ValidPriceRange => MaxPrice > MinPrice;
        public string? SearchTerm { get; set; }
        public BookParameters()
        {
            OrderBy = "id";
        }
    }
}
