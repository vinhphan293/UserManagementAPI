using UserManagementAPI.Models;
namespace UserManagementAPI.Repositories;

public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(int id);
    User Add(User user);
    bool Update(User user);
    bool Delete(int id);
    bool EmailExists(string email, int? excludingUserId = null);
}