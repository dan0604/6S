namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_BaoCao_Cham6S_H
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        [StringLength(20)]
        public string Ma_BC { get; set; }

        [StringLength(10)]
        public string Loai_BC { get; set; }

        [StringLength(30)]
        public string Username { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Ngay_BC { get; set; }

        public int? Error { get; set; }

        public int? Point { get; set; }

        [StringLength(10)]
        public string PhongBan { get; set; }

        [StringLength(250)]
        public string Duongluu { get; set; }

        public int? TongHinh { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgayTao_BC { get; set; }

        public int? Status { get; set; }
    }
}
