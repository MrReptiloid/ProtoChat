using System.Collections.Concurrent;
using ProtoChat.Domain.Abstractions;

namespace ProtoChat.DataAccess;

public sealed class InMemoryUserStore : SingletonBase<InMemoryUserStore>
{
    private readonly ConcurrentDictionary<string, string> _users = new();
    private static readonly Lazy<InMemoryUserStore> _instance = new(() => new InMemoryUserStore());
    public static InMemoryUserStore Instance => _instance.Value;
    
    private InMemoryUserStore() { }
    
    public Task<bool> RegisterUserAsync(string username, string password)
    {
        lock (_users)
        {
            if (_users.ContainsKey(username))
            {
                return Task.FromResult(false);
            }

            _users[username] = password;
            return Task.FromResult(true);
        }
    }

    public Task<bool> ValidateUserAsync(string username, string password)
    {
        lock (_users)
        {
            if (_users.TryGetValue(username, out var storedPassword))
            {
                return Task.FromResult(storedPassword == password);
            }

            return Task.FromResult(false);
        }
    }
}