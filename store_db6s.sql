---truy vấn dữ liệu---
delete from dbo.Tbl_BaoCao_Cham6S_H where Ma_BC =''
delete from dbo.Tbl_BaoCao_Cham6S_L where Ma_BC =''
delete Tbl_BaoCao_Cham6S_L where Ma_BC = 'CH202310007'
update dbo.Tbl_PhanCong_H set Status = '1' , Ngay_Ky =  null, Nguoi_Ky = null where ID_PhanCong = 'PC202312001'
update dbo.Tbl_PhanCong_L set Status = '1' where ID_PhanCong = 'PC202312001'
select * from dbo.Tbl_PhongBan
select * from dbo.Tbl_PhanCong_H
select * from dbo.Tbl_PhanCong_L where ID_PhanCong = 'PC202312002' and Status = 0
update dbo.Tbl_PhanCong_L set Status = 1 where ID_PhanCong = 'PC202312002' and Username ='thaonhi928@gmail.com' and PhongBan ='PB9' and Status = 0
select * from dbo.Tbl_PhanCong_L where ID_PhanCong = 'PC202312002' and Username ='thaonhi928@gmail.com' and PhongBan ='PB9' and Status = 1
TRUNCATE TABLE dbo.Tbl_PhanCong_L
TRUNCATE TABLE dbo.Tbl_PhanCong_H
---gọi store---
sp_helptext sp_Insert_PhanCong_H
---test---
select * from PhanCongCham6S where Username = 'hoan.nx@gmail.com' and DAY(GETDATE()) >= DAY(TuNgay) and DAY(GETDATE()) <= DAY(DenNgay)
update Tbl_PhanCong_H set Status = '2' where ID_PhanCong = 'PC202311001'
update Tbl_PhanCong_L set Status = '2' where ID_PhanCong = 'PC202311001'
---backup---
CREATE VIEW [v_BC6S] AS  
select DISTINCT A1.Ma_Khoi,A2.ID_PhongBan,A5.MaLoai, A1.TenKhoi,A2.TenPhongBan,A3.MucTieu,A5.TenLoai,  
A4.Point, A4.TongHinh,A3.ThangNam,A4.Ngay_BC  
  
from Tbl_Khoi as A1 left join Tbl_PhongBan as A2 on A1.Ma_Khoi = A2.Ma_Khoi      
inner join Tbl_MucTieu A3 on A2.ID_PhongBan = A3.ID_PhongBan      
inner join Tbl_BaoCao_Cham6S_H A4 on A3.ID_PhongBan = A4.PhongBan      
inner join Tbl_LoaiBC A5 on A4.Loai_BC = A5.MaLoai  
  
--ORDER BY A3.ThangNam, A1.Ma_Khoi,A2.ID_PhongBan  
where A4.Status=1
SELECT A.ID_NhomQuyen, STRING_AGG(B.ID_Quyen, ', ') AS Grouped_ID_Quyen, STRING_AGG(B.TenQuyen, ', ') AS Grouped_TenQuyen
FROM Tbl_NhomQuyen AS A 
INNER JOIN Tbl_DMQuyen AS B ON A.IDMenu = B.ID_Quyen 
GROUP BY A.ID_NhomQuyen


select distinct B.ID_NhomQuyen,B.Quyen,B.TenQuyen,STRING_AGG(C.TenQuyen, ', ') AS Grouped_TenQuyen
from Tbl_NhomQuyen A inner join Tbl_MoTaNhomQuyen B
on A.ID_NhomQuyen = B.ID_NhomQuyen left join Tbl_DMQuyen C
on A.IDMenu = C.ID_Quyen
GROUP BY B.ID_NhomQuyen,B.Quyen,B.TenQuyen