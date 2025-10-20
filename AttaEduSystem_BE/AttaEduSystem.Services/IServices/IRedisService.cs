namespace AttaEduSystem.Services.IServices
{
    public interface IRedisService
    {
        Task<bool> StoreString(string key, string value);
        Task<string> RetrieveString(string key);
        Task<bool> DeleteString(string key);
    }
}
