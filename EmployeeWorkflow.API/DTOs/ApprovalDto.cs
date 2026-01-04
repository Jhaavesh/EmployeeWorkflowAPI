namespace EmployeeWorkflow.API.DTOs
{
    public class ApprovalDto
    {
        public int RequestId { get; set; }
        public string Action { get; set; } = string.Empty;
    }
}
