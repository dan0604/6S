namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_PhanCong_L_View
    {
        [Key]
        [StringLength(10)]
        public string PhongBan { get; set; }

        [StringLength(30)]
        public string Username { get; set; }

        [StringLength(30)]
        public string Gio { get; set; }

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        [StringLength(10)]
        public string ThangNam { get; set; }

        public int? Status { get; set; }

        [StringLength(10)]
        public string Loai_BC { get; set; }

        [StringLength(50)]
        public string NVCaiTien { get; set; }

        [StringLength(10)]
        public string MaKhoi { get; set; }
    }
}
