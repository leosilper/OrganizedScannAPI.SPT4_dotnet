namespace OrganizedScannAPI.Application.Hateoas
{
    public class HateoasLink
    {
        public string Rel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
    }
}
