using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class AdminController
    {
        [HttpGet("pending-operators")]
        public async Task<IActionResult> GetPendingOperators()
        {
            var operators = await _adminService.GetPendingOperatorsAsync();
            return Ok(operators);
        }

        [HttpPost("operators/{id}/approve")]
        public async Task<IActionResult> ApproveOperator(int id)
        {
            var result = await _adminService.ApproveOperatorAsync(id);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpDelete("operators/{id}/reject")]
        public async Task<IActionResult> RejectOperator(int id)
        {
            var result = await _adminService.RejectOperatorAsync(id);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetAllOperators()
        {
            var operators = await _adminService.GetAllOperatorsAsync();
            return Ok(operators);
        }
    }
}
