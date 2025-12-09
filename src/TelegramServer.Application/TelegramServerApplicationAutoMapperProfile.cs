using System.Collections.Generic;
using AutoMapper;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;

namespace TelegramServer;

public class TelegramServerApplicationAutoMapperProfile : Profile
{
    public TelegramServerApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<IDictionary<string, string>, TelegramAuthDataDto>()
            .ForMember(d => d.Id,
                m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterNameId, null)))
            .ForMember(d => d.UserName,
                m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterNameUserName, null)))
            .ForMember(d => d.AuthDate,
                m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterNameAuthDate, null)))
            .ForMember(d => d.FirstName,
                m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterNameFirstName, null)))
            .ForMember(d => d.LastName,
                m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterNameLastName, null)))
            .ForMember(d => d.Hash, m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterNameHash, null)))
            .ForMember(d => d.PhotoUrl,
                m => m.MapFrom(s => GetValue(s, CommonConstants.RequestParameterPhotoUrl, null)));
    }

    private static TV GetValue<TK, TV>(IDictionary<TK, TV> source, TK key, TV value = default)
    {
        if (source.IsNullOrEmpty())
        {
            return value;
        }

        if (!source.ContainsKey(key))
        {
            return value;
        }

        return source[key];
    }
}