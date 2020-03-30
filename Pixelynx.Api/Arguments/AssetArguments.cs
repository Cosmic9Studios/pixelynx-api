namespace Pixelynx.Api.Arguments
{
    public class AssetArguments
    {
        public string Filter { get; set; } = "";
        public string Type { get; set; } = "";
        public string ParentId { get; set; } = "";
        public bool? Random { get; set; } = false;
    }
}