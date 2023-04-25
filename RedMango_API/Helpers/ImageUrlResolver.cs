using AutoMapper;
using RedMango_API.Models;
using RedMango_API.Models.Dto;

namespace RedMango_API.Helpers
{
    public class ImageUrlResolver : IValueResolver<MenuItem, MenuItemDto, string>
    {
        private readonly IConfiguration _config;

        public ImageUrlResolver(IConfiguration config)
        {
            _config = config;
        }

        public string Resolve(MenuItem source, MenuItemDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.Image))
            {
                 return _config["ApiUrl"] + source.Image;
            }
            return null;
        }
    }
}
