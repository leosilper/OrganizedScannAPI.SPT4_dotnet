using System.Collections.Generic;

namespace OrganizedScannAPI.Application.Hateoas
{
    public class HateoasResponse<T>
    {
        public T Data { get; set; }
        public List<HateoasLink> Links { get; set; } = new();

        public HateoasResponse(T data)
        {
            Data = data;
        }
    }
}
