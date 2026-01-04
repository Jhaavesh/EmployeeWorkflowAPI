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
    public class ApprovalController : ControllerBase
    {
        private readonly DbConnection _db;

        public ApprovalController(DbConnection db)
        {
            _db = db;
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("manager")]
        public IActionResult ManagerApproval(ApprovalDto dto)
        {
            return ProcessApproval(dto, "Manager", "Pending-Admin");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public IActionResult AdminApproval(ApprovalDto dto)
        {
            return ProcessApproval(dto, "Admin", dto.Action == "Approve" ? "Approved" : "Rejected");
        }

        private IActionResult ProcessApproval(ApprovalDto dto, string role, string nextStatus)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);

            using var conn = _db.GetConnection();
            conn.Open();

            var logCmd = new MySqlCommand(
                "INSERT INTO Approvals (RequestId, ApprovedBy, Role, Action) VALUES (@r,@u,@role,@a)",
                conn);

            logCmd.Parameters.AddWithValue("@r", dto.RequestId);
            logCmd.Parameters.AddWithValue("@u", username);
            logCmd.Parameters.AddWithValue("@role", role);
            logCmd.Parameters.AddWithValue("@a", dto.Action);

            logCmd.ExecuteNonQuery();

            var updateCmd = new MySqlCommand(
                "UPDATE Requests SET Status=@s WHERE RequestId=@r",
                conn);

            updateCmd.Parameters.AddWithValue("@s", nextStatus);
            updateCmd.Parameters.AddWithValue("@r", dto.RequestId);

            updateCmd.ExecuteNonQuery();

            return Ok($"Request {dto.Action} by {role} ✅");
        }
    }
}
