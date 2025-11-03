using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class PhanQuyen
    {
        [Required]
        public int MaNhom { get; set; }

        [Required]
        [MaxLength(50)]
        public string MaCN { get; set; }

        // Navigation properties
        [ForeignKey("MaNhom")]
        public virtual NhomQuyen NhomQuyen { get; set; }

        [ForeignKey("MaCN")]
        public virtual ChucNang ChucNang { get; set; }
    }
}
