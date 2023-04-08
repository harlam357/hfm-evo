using AutoMapper;

namespace HFM.Core.Artifacts.SlotMarkup;

public class SlotXmlBuilderProfile : Profile
{
    public SlotXmlBuilderProfile()
    {
        CreateMap<Log.LogLine, LogLine>()
            .ForMember(dest => dest.Raw, opt => opt.MapFrom(src => LogLine.RemoveInvalidXmlChars(src.Raw)));
        CreateMap<Proteins.Protein, Protein>()
            .ForMember(dest => dest.ServerIP, opt => opt.MapFrom(src => src.ServerIP ?? String.Empty))
            .ForMember(dest => dest.WorkUnitName, opt => opt.MapFrom(src => src.WorkUnitName ?? String.Empty))
            .ForMember(dest => dest.Core, opt => opt.MapFrom(src => src.Core ?? String.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? String.Empty))
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact ?? String.Empty));
    }
}
