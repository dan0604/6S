namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_BaoCao_Cham6S_L
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        [StringLength(20)]
        public string Ma_BC { get; set; }

        public int? STT { get; set; }

        [StringLength(50)]
        public string KhuVuc { get; set; }

        [StringLength(30)]
        public string Hangmuc_Chinh { get; set; }

        [StringLength(30)]
        public string HangMuc_Phu { get; set; }

        [StringLength(255)]
        public string MoTa { get; set; }

        [StringLength(100)]
        public string HinhAnh { get; set; }

        public int? Status { get; set; }
    }
}
