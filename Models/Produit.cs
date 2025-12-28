using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models  // ← Changé !
{
    public class Produit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Prix { get; set; }

        public int Quantite { get; set; }

        [StringLength(50)]
        public string Categorie { get; set; }

        public string ImageUrl { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}