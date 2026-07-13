namespace PayBridge.Api.Errors
{
    public sealed class ErrorDescriptor
    {
        public int Code { get; set; }
        public string Key{ get; set; }
        public string Message { get; set; }
        public string UserMessage { get; set; }
    }
}
