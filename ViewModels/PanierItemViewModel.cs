namespace WebApplication1.ViewModels
{
    public class PanierItemViewModel
    {
        public int ProduitId { get; set; }
        public string Nom { get; set; } = "";
        public decimal Prix { get; set; }
        public int Quantite { get; set; } = 1;

        public decimal Total => Prix * Quantite;
    }
}