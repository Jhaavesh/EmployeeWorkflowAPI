using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkflow.API.Data;
using EmployeeWorkflow.API.DTOs;
using MySql.Data.MySqlClient;

namespace EmployeeWorkflow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : ControllerBase
    {
        private readonly DbConnection _db;

        public EmployeesController(DbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Employees", conn);
            var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    Id = r["Id"],
                    Name = r["Name"],
                    Email = r["Email"],
                    Department = r["Department"]
                });
            }

            return Ok(new { success = true, data = list });
        }

        [HttpPost]
        public IActionResult Add(EmployeeDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO Employees (Name,Email,Department) VALUES (@n,@e,@d)", conn);

            cmd.Parameters.AddWithValue("@n", dto.Name);
            cmd.Parameters.AddWithValue("@e", dto.Email);
            cmd.Parameters.AddWithValue("@d", dto.Department);

            cmd.ExecuteNonQuery();
            return Ok(new { success = true });
        }
    }
}
