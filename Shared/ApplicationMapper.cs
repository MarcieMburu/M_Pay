using AutoMapper;

namespace Shared
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {

            CreateMap<Transaction, TransactionViewModel>().ReverseMap();

            CreateMap<Sender, TransactionViewModel>().ReverseMap();
            CreateMap<Receiver, TransactionViewModel>().ReverseMap();


        }
    }
}