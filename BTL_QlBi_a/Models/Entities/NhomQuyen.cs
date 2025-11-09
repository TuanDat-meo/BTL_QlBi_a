using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class NhomQuyen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNhom { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenNhom { get; set; }

        // Navigation properties
        public virtual ICollection<NhanVien> NhanViens { get; set; }
        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
    }
}
