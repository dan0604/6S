namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_MoTaNhomQuyen
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        public int? ID_NhomQuyen { get; set; }

        [StringLength(250)]
        public string Quyen { get; set; }

        [StringLength(30)]
        public string TenQuyen { get; set; }

        public int? Status { get; set; }
    }
}
