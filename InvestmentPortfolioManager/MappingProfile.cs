using AutoMapper;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Globalization;

namespace InvestmentPortfolioManager
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatusEnum.Ok))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => Enum.Parse<CurrencyEnum>(src.Currency, true)))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => ToUpperFirst(src.FirstName)))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => ToUpperFirst(src.LastName)));

            CreateMap<CreateWalletDto, Wallet>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedDate)))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedDate)));
        }

        private string ToUpperFirst(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }

            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
