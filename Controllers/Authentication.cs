using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.Security;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthControllers : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IWebHostEnvironment _environment;

        private static string HashRawForSave(string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                throw new System.ArgumentException("Password is required.", nameof(rawPassword));
            }

            return PasswordHasher.Hash(rawPassword);
        }

        public AuthControllers(IUserRepository userRepository, IJwtTokenService jwtTokenService, IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _environment = environment;
        }

        /// <summary>
        /// Public self-registration — anyone can sign up as a Customer (role 3).
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var existingUser = await _userRepository.GetByUserNameAsync(dto.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            // Public registration always creates a Customer account (role 3)
            var user = new User
            {
                UserName = dto.Username,
                PasswordHash = HashRawForSave(dto.Password),
                RoleId = AppRoles.User,
            };

            await _userRepository.AddAsync(user);
            var createdUser = await _userRepository.GetByUserNameAsync(dto.Username);

            if (createdUser != null)
            {
                string token = _jwtTokenService.GenerateToken(createdUser);

                return Ok(new
                {
                    message = "User registered successfully.",
                    token,
                    userId = createdUser.UserId,
                    username = createdUser.UserName,
                    roleId = (int)createdUser.RoleId,
                });
            }

            return Ok(new { message = "User registered successfully." });
        }

        /// <summary>
        /// Admin-only registration — creates Admin or Staff accounts with any role.
        /// </summary>
        [HttpPost("register-admin")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            if (dto.RoleId < 1)
            {
                return BadRequest("RoleId is required.");
            }

            var existingUser = await _userRepository.GetByUserNameAsync(dto.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            var user = new User
            {
                UserName = dto.Username,
                PasswordHash = HashRawForSave(dto.Password),
                RoleId = dto.RoleId,
            };

            await _userRepository.AddAsync(user);
            var createdUser = await _userRepository.GetByUserNameAsync(dto.Username);

            if (createdUser != null)
            {
                return Ok(new
                {
                    message = "Account created successfully.",
                    userId = createdUser.UserId,
                    username = createdUser.UserName,
                    roleId = (int)createdUser.RoleId,
                });
            }

            return Ok(new { message = "Account created successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _userRepository.GetByUserNameAsync(dto.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            bool isValid = PasswordHasher.Verify(user.PasswordHash, dto.Password);
            if (!isValid)
            {
                return Unauthorized("Invalid username or password.");
            }

            string token = _jwtTokenService.GenerateToken(user);
            return Ok(new
            {
                message = "Login successful.",
                token,
                userId = user.UserId,
                username = user.UserName,
                roleId = (int)user.RoleId,
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("Username is required.");
            }

            string message = "If the account exists, a reset token has been generated.";
            var user = await _userRepository.GetByUserNameAsync(dto.Username);
            if (user == null)
            {   
                return Ok(new { message });
            }

            string resetToken = _jwtTokenService.GeneratePasswordResetToken(user.UserId.ToString());
            if (_environment.IsDevelopment())
            {
                return Ok(new { message, resetToken });
            }

            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("Token and new password are required.");
            }

            if (!_jwtTokenService.TryValidatePasswordResetToken(dto.Token, out string userIdText))
            {
                return BadRequest("Invalid or expired reset token.");
            }

            if (!int.TryParse(userIdText, out var userId))
            {
                return BadRequest("Invalid reset token format.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid or expired reset token.");
            }

            user.PasswordHash = HashRawForSave(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}