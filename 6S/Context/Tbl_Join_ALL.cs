namespace _6S.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class Tbl_Join_ALL
    {
        public Tbl_BaoCao_Cham6S_H baocao_H { get; set; }
        public Tbl_BaoCao_Cham6S_L baocao_L { get; set; }
        public Tbl_NhomQuyen NhomQuyen { get; set; }
        public Tbl_User User { get; set; }
        public Tbl_PhongBan PhongBan { get; set; }
        public Tbl_Khoi Khoi { get; set; }
        public Tbl_MucTieu MucTieu { get; set; }
        public Tbl_PhanCong_L PhanCong_L { get; set; }
        public Tbl_PhanCong_H PhanCong_H { get; set; }
        public Tbl_NhanVienCaiTien NhanVienCaiTien { get; set; }
    }
}
