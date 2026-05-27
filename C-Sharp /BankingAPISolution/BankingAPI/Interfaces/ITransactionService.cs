using BankingAPI.Models.DTOs;
using BankingAPI.Models;

namespace BankingAPI.Interfaces
{
    public interface ITransactionService
    {
        public TransactionResponse TransferFunds(TransactionRequest request);
        public PagedResponse<Transaction> SearchTransactions(TransactionSearch request);
        public WithdrawalResponse WithdrawFunds(WithdrawalRequest request);

    }
}