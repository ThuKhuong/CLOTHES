-- Script thêm d? li?u m?u cho b?ng KHACHHANG
-- Phù h?p v?i c?u trúc database th?c t?

-- Clear existing data
DELETE FROM KHACHHANG;

-- Thêm d? li?u m?u khách hàng ?a d?ng
INSERT INTO KHACHHANG (MAKH, HOTEN, GIOITINH, DCHI, SDT, EMAIL) VALUES
('KH001', N'Nguy?n V?n An', N'Nam', N'123 ???ng Lê L?i, Ph??ng B?n Thành, Qu?n 1, TP.HCM', '0987654321', 'nguyenvanan@gmail.com'),
('KH002', N'Tr?n Th? Bình', N'N?', N'456 ???ng Nguy?n Hu?, Ph??ng B?n Nghé, Qu?n 1, TP.HCM', '0976543210', 'tranthibinh@yahoo.com'),
('KH003', N'Lê Hoàng Nam', N'Nam', N'789 ???ng Pasteur, Ph??ng 6, Qu?n 3, TP.HCM', '0965432109', 'lehoangnam@hotmail.com'),
('KH004', N'Ph?m Minh Th?', N'N?', N'321 ???ng C?ng Qu?nh, Ph??ng Ph?m Ng? Lão, Qu?n 1, TP.HCM', '0954321098', 'phamminhthu@gmail.com'),
('KH005', N'Võ Thanh Tùng', N'Nam', N'654 ???ng Nam K? Kh?i Ngh?a, Ph??ng 7, Qu?n 3, TP.HCM', '0943210987', 'vothanhtung@outlook.com'),
('KH006', N'Hoàng Th? Mai', N'N?', N'987 ???ng Hai Bà Tr?ng, Ph??ng ?a Kao, Qu?n 1, TP.HCM', '0932109876', 'hoangthimai@gmail.com'),
('KH007', N'?? V?n Hùng', N'Nam', N'147 ???ng ?i?n Biên Ph?, Ph??ng ?a Kao, Qu?n 1, TP.HCM', '0921098765', 'dovanhung@yahoo.com'),
('KH008', N'Ngô Th? Lan', N'N?', N'258 ???ng Võ V?n T?n, Ph??ng 5, Qu?n 3, TP.HCM', '0910987654', 'ngothilan@gmail.com'),
('KH009', N'Bùi Minh ??c', N'Nam', N'369 ???ng Tr?n H?ng ??o, Ph??ng 2, Qu?n 5, TP.HCM', '0901876543', 'buiminhduc@hotmail.com'),
('KH010', N'Lý Th? Hoa', N'N?', N'741 ???ng Nguy?n Trãi, Ph??ng 11, Qu?n 5, TP.HCM', '0890765432', 'lythihoa@outlook.com'),
('KH011', N'Nguy?n Thanh Long', N'Nam', N'852 ???ng Lý Thái T?, Ph??ng 1, Qu?n 10, TP.HCM', '0889654321', 'nguyenthanhlong@gmail.com'),
('KH012', N'Tr?n Thúy H?ng', N'N?', N'963 ???ng Âu C?, Ph??ng 10, Qu?n Tân Bình, TP.HCM', '0878543210', 'tranthuyhang@yahoo.com'),
('KH013', N'Phan Minh Tu?n', N'Nam', N'159 ???ng Cách M?ng Tháng 8, Ph??ng 5, Qu?n Tân Bình, TP.HCM', '0867432109', 'phanminhtuan@gmail.com'),
('KH014', N'??ng Th? Thu', N'N?', N'357 ???ng Hoàng V?n Th?, Ph??ng 8, Qu?n Phú Nhu?n, TP.HCM', '0856321098', 'dangthithu@hotmail.com'),
('KH015', N'L?u ?ình Khoa', N'Nam', N'468 ???ng Phan ?ình Phùng, Ph??ng 1, Qu?n Phú Nhu?n, TP.HCM', '0845210987', 'luudinhkhoa@gmail.com');

-- Ki?m tra d? li?u v?a thêm
SELECT 
    MAKH as 'Mã KH',
    HOTEN as 'H? tên',
    GIOITINH as 'Gi?i tính',
    LEFT(DCHI, 30) + '...' as '??a ch?',
    SDT as 'S?T',
    EMAIL as 'Email'
FROM KHACHHANG 
ORDER BY MAKH;

-- Th?ng kê
SELECT 
    'T?ng s? khách hàng' as 'Th?ng kê',
    COUNT(*) as 'S? l??ng'
FROM KHACHHANG

UNION ALL

SELECT 
    'Khách hàng nam' as 'Th?ng kê',
    COUNT(*) as 'S? l??ng'
FROM KHACHHANG 
WHERE GIOITINH = N'Nam'

UNION ALL

SELECT 
    'Khách hàng n?' as 'Th?ng kê', 
    COUNT(*) as 'S? l??ng'
FROM KHACHHANG 
WHERE GIOITINH = N'N?'