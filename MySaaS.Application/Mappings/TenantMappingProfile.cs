using AutoMapper;
using MySaaS.Application.DTOs;
using MySaaS.Domain.Entities;

namespace MySaaS.Application.Mappings;

/// <summary>
/// AutoMapper profile for Tenant entity mappings.
/// </summary>
public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        // Entity -> Response DTO
        CreateMap<Tenant, TenantResponse>();
        CreateMap<Tenant, TenantBasicResponse>();

        // Request DTO -> Entity (for creation)
        CreateMap<CreateTenantRequest, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.SubscriptionExpiresAt, opt => opt.Ignore());
    }
}
