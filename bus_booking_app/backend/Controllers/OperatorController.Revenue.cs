using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class OperatorController
    {
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats()
        {
            var opId = GetOperatorId();
            var stats = await _operatorService.GetRevenueStatsAsync(opId);
            return Ok(stats);
        }

        [HttpPost("expenses")]
        public async Task<IActionResult> AddExpense(CreateExpenseRequest request)
        {
            var opId = GetOperatorId();
            var result = await _operatorService.AddExpenseAsync(opId, request);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }
    }
}
