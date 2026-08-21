using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [MinLength(6)]
        public string? NewPassword { get; set; }

        public int UserRoleId { get; set; }

        [StringLength(250)]
        public string? ProfilePicturePath { get; set; }
    }   
}