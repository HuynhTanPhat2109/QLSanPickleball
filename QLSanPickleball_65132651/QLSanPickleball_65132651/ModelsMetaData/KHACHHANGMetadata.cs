using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLSanPickleball_65132651.Models
{
    [MetadataType(typeof(KHACHHANGMetadata))]
    public partial class KHACHHANG
    {
        // Để trống vì đây là partial class dùng để "dính" Metadata vào Class chính
    }

    // 2. Nơi định nghĩa các ràng buộc dữ liệu
    public class KHACHHANGMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
        [Display(Name = "Họ và tên")]
        public string HOTENKH { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [Display(Name = "Ngày sinh")]
        public string NGAYSINH { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public string GIOITINH { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải đúng 10 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string SODIENTHOAIKH { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string EMAILKH { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
        public string MATKHAUKH { get; set; }
    }
}