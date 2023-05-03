namespace MyProj_Models.ViewModels
{
    public class DetailVM
    {
        public DetailVM()
        {
            Product = new Product();
        }
        public Product Product { get; set; }
        public bool ExistsInCart { get; set; }
    }
}
