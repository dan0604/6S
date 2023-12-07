namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BC6SView
    {
        [Key]
        [StringLength(50)]
        public string Tenkhoi { get; set; }

        [StringLength(50)]
        public string TenPhongBan { get; set; }

        public int? MucTieu { get; set; }

        public int? DiemChamNoiBo { get; set; }

        public int? TongHinhChamNoiBo { get; set; }

        public int? DiemChamCheo { get; set; }

        public int? TongHinhChamCheo { get; set; }

        public int? DiemChamHieuChinh { get; set; }

        public int? TongHinhChamHieuChinh { get; set; }

        //[Column(TypeName = "date")]
        //public DateTime? NgayNB { get; set; }

        //[Column(TypeName = "date")]
        //public DateTime? NCH { get; set; }

        //[Column(TypeName = "date")]
        //public DateTime? NHC { get; set; }
    }
}
