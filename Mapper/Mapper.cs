using AutoMapper;
using RealtimeLeaderboardAPI.Domain;

namespace RealtimeLeaderboardAPI.Application;

public class MappingProfile : Profile {
    public MappingProfile() {
        CreateMap<CreateSeasonDto, Season>();
        CreateMap<UpdateSeasonDto, Season>();
      
        CreateMap<RegisterUserDto, User>();
    }
}