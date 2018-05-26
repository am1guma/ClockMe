using System.ComponentModel.DataAnnotations;

namespace ClockMe.Models
{
    public class ResetPassword
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}