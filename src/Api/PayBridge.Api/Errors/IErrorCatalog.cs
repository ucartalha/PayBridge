namespace PayBridge.Api.Errors
{
    public interface IErrorCatalog
    {
        ErrorDescriptor GetByCode(int errorCode);
    }
}
