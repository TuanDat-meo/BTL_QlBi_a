using System.ComponentModel.DataAnnotations;

namespace BTL_QlBi_a.Models.Entities
{
    public class ChucNang
    {
        [Key]
        [MaxLength(50)]
        public string MaCN { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenCN { get; set; }

        [MaxLength(255)]
        public string? MoTa { get; set; }

        // Navigation properties
        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
    }
}
