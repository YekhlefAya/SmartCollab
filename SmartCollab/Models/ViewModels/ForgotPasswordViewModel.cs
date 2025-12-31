using System.ComponentModel.DataAnnotations;

namespace SmartCollab.Models
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
