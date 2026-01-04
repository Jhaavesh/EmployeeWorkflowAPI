using Microsoft.AspNetCore.Mvc;
using EmployeeWorkflow.API.Data;
using EmployeeWorkflow.API.DTOs;
using MySql.Data.MySqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeWorkflow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DbConnection _db;
        private readonly IConfiguration _config;

        public AuthController(DbConnection db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(
                    "INSERT INTO Users (Username, Password, Role) VALUES (@u, @p, @r)", conn);
                cmd.Parameters.AddWithValue("@u", dto.Username);
                cmd.Parameters.AddWithValue("@p", BCrypt.Net.BCrypt.HashPassword(dto.Password));
                cmd.Parameters.AddWithValue("@r", dto.Role);
                cmd.ExecuteNonQuery();

                return Ok(new { success = true, message = "User registered successfully" });
            }
            catch (MySqlException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(
                    "SELECT Id, Username, Password, Role FROM Users WHERE Username=@u", conn);
                cmd.Parameters.AddWithValue("@u", dto.Username);

                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return Unauthorized(new { success = false, message = "Invalid username or password" });

                var userId = reader["Id"].ToString();
                var username = reader["Username"].ToString();
                var hashedPassword = reader["Password"].ToString();
                var role = reader["Role"].ToString();

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, hashedPassword))
                    return Unauthorized(new { success = false, message = "Invalid username or password" });

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    _config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    claims,
                    expires: DateTime.Now.AddHours(8),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    success = true,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    user = new { username, role }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}