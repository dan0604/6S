namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_User { get; set; }

        [Key]
        [StringLength(30)]
        public string Username { get; set; }

        [StringLength(50)]
        public string Pass { get; set; }

        [StringLength(50)]
        public string Fullname { get; set; }

        [StringLength(20)]
        public string Manv { get; set; }

        public int? NhomQuyen { get; set; }

        [StringLength(10)]
        public string ID_PhongBan { get; set; }

        public int? Status { get; set; }
    }
}
