using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class KhuVuc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKhuVuc { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenKhuVuc { get; set; }

        [MaxLength(255)]
        public string? MoTa { get; set; }

        // Navigation properties
        public virtual ICollection<BanBia> BanBias { get; set; }
    }
}
