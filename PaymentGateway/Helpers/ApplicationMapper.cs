using System.Transactions;
using AutoMapper;
using PaymentGateway.Models;
using PaymentGateway.Data;
using Transaction = PaymentGateway.Models.Transaction;

namespace PaymentGateway.Helpers
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<Transaction, TransactionViewModel>().ReverseMap();
        }
    }
}
