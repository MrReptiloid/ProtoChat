namespace ProtoChat.DataAccess;

public interface IUserStore
{
    Task<bool> RegisterUserAsync(string userName, string password);
    Task<bool> ValidateUserAsync(string userName, string password);
}