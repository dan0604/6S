namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_MucTieu
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        
        public int? MucTieu { get; set; }

        [StringLength(20)]
        public string ThangNam { get; set; }

        [StringLength(10)]
        public string ID_PhongBan { get; set; }

        public int? Status { get; set; }
    }
}
