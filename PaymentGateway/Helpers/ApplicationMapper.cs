
using AutoMapper;
using PaymentGateway.Models;
using PaymentGateway.Data;

namespace PaymentGateway.Helpers
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {

            CreateMap<Transaction, TransactionViewModel>().ReverseMap();

            CreateMap<Sender, TransactionViewModel>().ReverseMap();
            CreateMap<Receiver, TransactionViewModel>().ReverseMap();


    //        CreateMap<TransactionViewModel, Transaction>()
    //.ForMember(dest => dest.Sender, opt => opt.MapFrom(src => new Sender
    //{
    //    Name = src.SenderName,
    //    ID_NO = src.SenderID_NO,
    //    Phone_No = src.SenderPhone_No,
    //    Src_Account = src.SenderSrc_Account

    //}));

            //CreateMap<TransactionViewModel, Transaction>()
            //     .ForMember(dest => dest.Receiver, opt => opt.MapFrom(src => new Receiver
            //     {
            //         Name = src.ReceiverName,
            //         ID_NO = src.ReceiverID_NO,
            //         Phone_No = src.ReceiverPhone_No,
            //         Dst_Account = src.ReceiverDst_Account

            //     }));
        }
    }
}