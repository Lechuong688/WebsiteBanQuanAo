using System.ComponentModel.DataAnnotations;

namespace WebBanQuanAo.Areas.Admin.Models
{
    public class UpdateUserViewModel
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "UserName không được để trống")]
        public string? UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        [Phone]
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string? PhoneNumber { get; set; }

        public List<string?> SelectedRoles { get; set; } = new();
        public List<string?> AllRoles { get; set; } = new();
    }
}
