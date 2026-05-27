using BankingAPI.Interfaces;
using BankingAPI.Models.DTOs;
using BankingAPI.Repositories;
using BankingAPI.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;
using BankingAPI.Contexts;
using System.Runtime;

namespace BankingAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepository<String, Account> _accountRepository;
        private readonly IRepository<int, Transaction> _transactionRepository;

        protected  BankingContext _context;
        public TransactionService(IRepository<String, Account> accountRepository, IRepository<int, Transaction> transactionRepository, BankingContext context)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _context = context;
        }

        public TransactionResponse TransferFunds(TransactionRequest request)
        {
            var fromAccount = _accountRepository.Get(request.FromAccountNumber);
            var toAccount = _accountRepository.Get(request.ToAccountNumber);
            var transaction = new Transaction
            {
                FromAccountNumber = request.FromAccountNumber,
                ToAccountNumber = request.ToAccountNumber,
            };
            if (fromAccount == null || toAccount == null)
            {
                transaction = new Transaction
                {
                    Amount = (decimal)request.Amount,
                    FromAccountNumber = request.FromAccountNumber,
                    ToAccountNumber = request.ToAccountNumber,
                    Status = "Cancelled"
                };
                _transactionRepository.Create(transaction);
                throw new Exception("One or both accounts not found.");
            }
            if (fromAccount.Balance < request.Amount)
            {
                transaction = new Transaction
                {
                    Amount = (decimal)request.Amount,
                    FromAccountNumber = request.FromAccountNumber,
                    ToAccountNumber = request.ToAccountNumber,
                    Status = "Cancelled due to insufficient funds"
                };

                _transactionRepository.Create(transaction);
                throw new Exception("Insufficient funds in the source account.");
            }

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;
            transaction = new Transaction
            {
                Amount = (decimal)request.Amount,
                FromAccountNumber = request.FromAccountNumber,
                ToAccountNumber = request.ToAccountNumber,
                Status = "Completed"
            };
            _transactionRepository.Create(transaction);
            _accountRepository.Update(request.FromAccountNumber, fromAccount);
            _accountRepository.Update(request.ToAccountNumber, toAccount);
            return new TransactionResponse { Amount = fromAccount.Balance};
        }
        public PagedResponse<Transaction> SearchTransactions(TransactionSearch request)
        {
            var query = _context.Transactions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(t =>
                    t.FromAccountNumber.Contains(request.Search) ||
                    t.ToAccountNumber.Contains(request.Search));
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(t => t.Status == request.Status);
            }

            if (request.MinAmount.HasValue)
            {
                query = query.Where(t =>
                    t.Amount >= request.MinAmount.Value);
            }

            if (request.MaxAmount.HasValue)
            {
                query = query.Where(t =>
                    t.Amount <= request.MaxAmount.Value);
            }
            if (request.StartDate.HasValue)
            {
                query = query.Where(t =>
                    t.TransactionDate >= request.StartDate.Value.Date);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date.AddDays(1);
                query = query.Where(t =>
                    t.TransactionDate < endDate);
            }
            query = request.SortBy.ToLower() switch
            {
                "amount" => request.SortOrder == "asc"
                    ? query.OrderBy(t => t.Amount)
                    : query.OrderByDescending(t => t.Amount),

                "status" => request.SortOrder == "asc"
                    ? query.OrderBy(t => t.Status)
                    : query.OrderByDescending(t => t.Status),

                _ => request.SortOrder == "asc"
                    ? query.OrderBy(t => t.TransactionDate)
                    : query.OrderByDescending(t => t.TransactionDate)
            };

            var totalRecords = query.Count();

            var data = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResponse<Transaction>
            {
                Data = data,
                TotalRecords = totalRecords,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
        public WithdrawalResponse WithdrawFunds(WithdrawalRequest request)
        {
            var fromAccount = _accountRepository.Get(request.FromAccountNumber);
            if (fromAccount == null)
            {
                throw new Exception("Account not found.");
            }
            if (fromAccount.Balance < request.Amount)
            {
                throw new Exception("Insufficient funds in the account.");
            }

            fromAccount.Balance -= request.Amount;
            _accountRepository.Update(request.FromAccountNumber, fromAccount);
            return new WithdrawalResponse { Amount = fromAccount.Balance };
        }
    }
}