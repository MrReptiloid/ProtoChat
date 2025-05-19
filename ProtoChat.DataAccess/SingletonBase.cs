using System.Reflection;

namespace ProtoChat.Domain.Abstractions;

public abstract class SingletonBase<T> where T : SingletonBase<T>
{
    private static readonly Lazy<T> _instance = new(CreateInstance());
    public static T Instance => _instance.Value;

    protected SingletonBase()
    {
        if (_instance.IsValueCreated)
        {
            throw new InvalidOperationException($"An instance of {typeof(T)} already exists. Use {nameof(Instance)} to access it.");
        }
    }
    
    private static T CreateInstance()
    {
        ConstructorInfo? constructor = typeof(T).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null);

        if (constructor == null)
        {
            throw new InvalidOperationException($"{typeof(T)} must have a private or protected parameterless constructor.");
        }
        
        return (T)constructor.Invoke(null);
    }
}