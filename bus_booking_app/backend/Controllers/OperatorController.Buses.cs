using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class OperatorController
    {
        [HttpPost("buses")]
        public async Task<IActionResult> AddBus(CreateBusRequest request)
        {
            var opId = GetOperatorId();
            var result = await _operatorService.AddBusAsync(opId, request);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message, bus = result.Bus });
        }

        [HttpGet("buses")]
        public async Task<IActionResult> GetMyBuses()
        {
            var opId = GetOperatorId();
            var buses = await _operatorService.GetMyBusesAsync(opId);
            return Ok(buses);
        }
    }
}
