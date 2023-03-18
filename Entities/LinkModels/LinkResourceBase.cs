namespace Entities.LinkModels
{
    public class LinkResourceBase
    {
        //Link kaynağını tüketecek sınıftır.Bütün linklerin organizasyonu
        public LinkResourceBase()
        {
            
        }

        public List<Link> Links { get; set; } = new List<Link>();

    }

}
