using UserManagementAPI.Models;
using System.Collections.Concurrent;
using System.Threading;
namespace UserManagementAPI.Repositories;

public class InMemoryUserRepository: IUserRepository
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _idCounter = 0;

    public InMemoryUserRepository()
    {
        Add(new User { FirstName = "Ava", LastName = "Nguyen", Email = "ava.nguyen@techhive.local", Department = "HR" });
        Add(new User {FirstName = "Keanu", LastName = "Lee", Email = "keanulee600@gmail.com", Department = "IT" });
    }
    public IEnumerable<User> GetAll() => _users.Values.OrderBy(u => u.Id);

    public User? GetById(int id)
    {
        _users.TryGetValue(id, out var user);
        return user;
    }
    
    public User Add(User user)
    {
        var newId = Interlocked.Increment(ref _idCounter);
        user.Id = newId;
        _users[newId] = user;
        return user;
    }
    
    public bool Delete(int id)
    {
        return _users.TryRemove(id, out _);
    }
    
    public bool Update(User user)
    {
        if (!_users.ContainsKey(user.Id))
            return false;

        _users[user.Id] = user;
        return true;
    }
    
    public bool EmailExists(string email, int? ignoreUserId = null)
    {
        return _users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) 
                                      && (!ignoreUserId.HasValue || u.Id != ignoreUserId.Value));
    }
}