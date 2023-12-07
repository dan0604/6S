namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_PhanCong_H
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        [StringLength(20)]
        public string ID_PhanCong { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Ngay_Tao { get; set; }

        [StringLength(30)]
        public string Nguoi_Tao { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Ngay_Ky { get; set; }

        [StringLength(30)]
        public string Nguoi_Ky { get; set; }

        [StringLength(250)]
        public string Duongluu { get; set; }

        public int? Status { get; set; }
    }
}
