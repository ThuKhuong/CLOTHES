# CLOTHES

## Tổng quan dự án
CLOTHES là một dự án .NET 10 trong kho mã nguồn này. Mục tiêu của dự án là xây dựng ứng dụng quản lý/kinh doanh thời trang (có thể tùy chỉnh theo yêu cầu của bạn).

## Mục tiêu dự án
- Quản lý thông tin sản phẩm, danh mục và tồn kho.
- Theo dõi đơn hàng, khách hàng và doanh thu.
- Hỗ trợ thao tác nhanh, dễ sử dụng và mở rộng.

## Mô tả chức năng
- Quản lý sản phẩm: thêm/sửa/xóa, cập nhật giá, hình ảnh, số lượng.
- Quản lý danh mục: phân loại sản phẩm theo nhóm.
- Quản lý khách hàng: lưu thông tin liên hệ, lịch sử mua hàng.
- Quản lý đơn hàng (bán hàng): tạo đơn bán, cập nhật trạng thái, thống kê.
- Quản lý nhập hàng: tạo phiếu nhập, cập nhật tồn kho khi nhập hàng mới.
- Báo cáo cơ bản: doanh thu theo ngày/tháng, sản phẩm bán chạy, nhập hàng.

## Yêu cầu hệ thống
- Windows 10/11 hoặc hệ điều hành có hỗ trợ .NET 10.
- .NET 10 SDK.
- Visual Studio Community 2026 (khuyến nghị).
- Git (để clone dự án).

## Cài đặt và thiết lập
1. Cài đặt .NET 10 SDK từ trang chủ .NET.
2. Cài đặt Visual Studio Community 2026 và chọn workload "ASP.NET and web development" hoặc ".NET desktop development" (tùy loại dự án).
3. Clone mã nguồn:

```pwsh
git clone https://github.com/ThuKhuong/CLOTHES
```

4. Mở thư mục dự án bằng Visual Studio:
   - File > Open > Project/Solution
   - Chọn file solution (.sln) nếu có.
5. Restore NuGet packages (nếu chưa tự chạy):

```pwsh
dotnet restore
```

## Chạy dự án
### Chạy bằng Visual Studio
1. Chọn project cần chạy trong Solution Explorer.
2. Nhấn F5 để chạy có debug hoặc Ctrl+F5 để chạy không debug.

### Chạy bằng dòng lệnh
1. Mở terminal tại thư mục dự án.
2. Chạy lệnh:

```pwsh
dotnet run
```

## Ví dụ sử dụng
- Sau khi chạy, ứng dụng sẽ hiển thị theo cấu hình (console/web/desktop).
- Nếu là web app, mở trình duyệt tới địa chỉ được hiển thị trong terminal (thường là https://localhost:xxxx).

## Đóng góp
1. Tạo nhánh mới từ main.
2. Thực hiện thay đổi và commit với mô tả rõ ràng.
3. Tạo pull request để được review.

## License
Vui lòng cập nhật thông tin license phù hợp cho dự án của bạn (ví dụ: MIT, Apache-2.0, GPL, ...).
