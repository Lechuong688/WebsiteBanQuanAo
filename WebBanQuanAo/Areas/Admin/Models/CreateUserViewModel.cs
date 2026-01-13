using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBanQuanAo.Areas.Admin.Models
{
    public class CreateUserViewModel
    {
        //public string Id { get; set; }
        [DisplayName("Họ và tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Name { get; set; }
        [DisplayName("Tên đăng nhập")]
        [Required(ErrorMessage = "UserName không được để trống")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu không hợp lệ")]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không đúng")]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
        [Phone]
        [DisplayName("Số điện thoại")]
        [Required(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }
    }
}
