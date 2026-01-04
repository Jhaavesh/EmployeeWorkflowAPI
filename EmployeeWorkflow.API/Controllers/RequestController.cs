using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkflow.API.Data;
using EmployeeWorkflow.API.DTOs;
using MySql.Data.MySqlClient;
using System.Security.Claims;

namespace EmployeeWorkflow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly DbConnection _db;

        public RequestsController(DbConnection db)
        {
            _db = db;
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Create(RequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO Requests (Title,Description,Status,CreatedBy) VALUES (@t,@d,'Pending',@u)", conn);

            cmd.Parameters.AddWithValue("@t", dto.Title);
            cmd.Parameters.AddWithValue("@d", dto.Description);
            cmd.Parameters.AddWithValue("@u", userId);

            cmd.ExecuteNonQuery();
            return Ok(new { success = true });
        }
    }
}
