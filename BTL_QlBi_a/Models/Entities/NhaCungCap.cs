using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class NhaCungCap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNCC { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenNCC { get; set; }

        [MaxLength(15)]
        public string? SDT { get; set; }

        [MaxLength(255)]
        public string? DiaChi { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        // Navigation properties
        public virtual ICollection<MatHang> MatHangs { get; set; }
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
    }
}
