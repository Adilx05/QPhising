using AutoMapper;
using QPhising.Application.Common.Contracts.Exports;
using QPhising.Domain.Exports;

namespace QPhising.Application.Features.Exports.Common;

public sealed class ExportJobMappingProfile : Profile
{
    public ExportJobMappingProfile()
    {
        CreateMap<ExportJob, ExportJobContract>();
    }
}
