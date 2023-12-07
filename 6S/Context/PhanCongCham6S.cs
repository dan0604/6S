namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PhanCongCham6S
    {
        [Key]
        [StringLength(10)]
        public string Loai_BC { get; set; }

        [StringLength(50)]
        public string TenLoai { get; set; }

        [StringLength(10)]
        public string ID_PhongBan { get; set; }

        [StringLength(30)]
        public string TenPhongBan { get; set; }

        [StringLength(30)]
        public string Username { get; set; }

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }
    }
}
