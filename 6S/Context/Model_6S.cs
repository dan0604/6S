using _6S.Context;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace _6S.Context
{
    public partial class Model_6S : DbContext
    {
        public Model_6S()
            : base("name=Model_6S")
        {
        }

        public virtual DbSet<Tbl_BaoCao_Cham6S_H> Tbl_BaoCao_Cham6S_H { get; set; }
        public virtual DbSet<Tbl_BaoCao_Cham6S_L> Tbl_BaoCao_Cham6S_L { get; set; }
        public virtual DbSet<Tbl_DMQuyen> Tbl_DMQuyen { get; set; }
        public virtual DbSet<Tbl_Khoi> Tbl_Khoi { get; set; }
        public virtual DbSet<Tbl_LoaiBC> Tbl_LoaiBC { get; set; }
        public virtual DbSet<Tbl_MoTaNhomQuyen> Tbl_MoTaNhomQuyen { get; set; }
        public virtual DbSet<Tbl_MucTieu> Tbl_MucTieu { get; set; }
        public virtual DbSet<Tbl_NhanVienCaiTien> Tbl_NhanVienCaiTien { get; set; }
        public virtual DbSet<Tbl_NhomQuyen> Tbl_NhomQuyen { get; set; }
        public virtual DbSet<Tbl_PhanCong_H> Tbl_PhanCong_H { get; set; }
        public virtual DbSet<Tbl_PhanCong_L> Tbl_PhanCong_L { get; set; }
        public virtual DbSet<Tbl_PhongBan> Tbl_PhongBan { get; set; }
        public virtual DbSet<Tbl_User> Tbl_User { get; set; }
        public virtual DbSet<Tbl_PhanCong_L_View> Tbl_PhanCong_L_View { get; set; }
        public virtual DbSet<PhanCongCham6S> PhanCongCham6S { get; set; }
        public virtual DbSet<BC6SView> BC6SView { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tbl_BaoCao_Cham6S_H>()
                .Property(e => e.Ma_BC)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_BaoCao_Cham6S_H>()
                .Property(e => e.Loai_BC)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_BaoCao_Cham6S_H>()
                .Property(e => e.Username)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_BaoCao_Cham6S_H>()
                .Property(e => e.PhongBan)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_BaoCao_Cham6S_H>()
                .Property(e => e.Duongluu)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_BaoCao_Cham6S_L>()
                .Property(e => e.Ma_BC)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_BaoCao_Cham6S_L>()
                .Property(e => e.HinhAnh)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_DMQuyen>()
                .Property(e => e.ID_Quyen)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_Khoi>()
                .Property(e => e.Ma_Khoi)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_LoaiBC>()
                .Property(e => e.MaLoai)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_MoTaNhomQuyen>()
                .Property(e => e.Quyen)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_MucTieu>()
                .Property(e => e.ThangNam)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_MucTieu>()
                .Property(e => e.ID_PhongBan)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_NhomQuyen>()
                .Property(e => e.IDMenu)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhanCong_L>()
                .Property(e => e.ID_PhanCong)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhanCong_L>()
                .Property(e => e.PhongBan)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhanCong_L>()
                .Property(e => e.Username)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhanCong_L>()
                .Property(e => e.ThangNam)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhanCong_L>()
                .Property(e => e.Loai_BC)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhanCong_L>()
                .Property(e => e.MaKhoi)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhongBan>()
                .Property(e => e.ID_PhongBan)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_PhongBan>()
                .Property(e => e.Ma_Khoi)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_User>()
                .Property(e => e.Username)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_User>()
                .Property(e => e.Pass)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_User>()
                .Property(e => e.Fullname)
                .IsUnicode(false);

            modelBuilder.Entity<Tbl_User>()
                .Property(e => e.ID_PhongBan)
                .IsUnicode(false);
        }
    }
}
