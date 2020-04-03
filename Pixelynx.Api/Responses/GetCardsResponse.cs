namespace Pixelynx.Api.Responses
{
    public class GetCardsResponse
    {
        public string Id { get; set; }
        public string Last4Digits { get; set; }
        public string Brand { get; set; }
        public long ExpiryMonth { get; set; }
        public long ExpiryYear { get; set; }
    }
}