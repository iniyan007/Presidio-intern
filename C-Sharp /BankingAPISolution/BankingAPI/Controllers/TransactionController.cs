using BankingAPI.Interfaces;
using BankingAPI.Models;
using BankingAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace BankingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        [HttpPost]
        public ActionResult<TransactionResponse> TransferFunds(TransactionRequest request)
        {
            try
            {
                var response = _transactionService.TransferFunds(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("search")]
        public IActionResult SearchTransactions([FromBody] TransactionSearch request)
        {
            var result = _transactionService.SearchTransactions(request);
            return Ok(result);
        }
        [HttpPost("withdraw")]
        public ActionResult<WithdrawalResponse> WithdrawFunds(WithdrawalRequest request)
        {
            try
            {
                var response = _transactionService.WithdrawFunds(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}