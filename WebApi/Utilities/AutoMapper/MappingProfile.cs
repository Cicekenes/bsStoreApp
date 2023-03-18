using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace WebApi.Utilities.AutoMapper
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //Birinci parametre kaynak, ikinci parametre istenilen class
            CreateMap<BookDtoForUpdate,Book>().ReverseMap();
            CreateMap<BookDto,Book>().ReverseMap();
            CreateMap<BookDtoForInsertion,Book>().ReverseMap(); 
            CreateMap<User,UserForRegistrationDto>().ReverseMap();
        }
    }
}
