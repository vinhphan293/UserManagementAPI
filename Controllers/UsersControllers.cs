using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo)
    {
        _repo = repo;
    }

    // GET: api/users
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        return Ok(_repo.GetAll());
    }

    // GET: api/users/{id}
    [HttpGet("{id:int}")]
    public ActionResult<User> GetUser(int id)
    {
        var user = _repo.GetById(id);
        return user is null ? NotFound() : Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public ActionResult<User> CreateUser(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");
        if (_repo.EmailExists(dto.Email))
            return Conflict("A user with the same email already exists.");

        var user = new User()
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Department = dto.Department.Trim(),
            IsActive = true
        };
        var created = _repo.Add(user);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }
    
    // PUT: api/users/{id}
    [HttpPut("{id:int}")]
    public ActionResult UpdateUser(int id, UpdateUserDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing is null) return NotFound();
        
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");
        
        if (_repo.EmailExists(dto.Email, id))
            return Conflict("A user with the same email already exists.");
        
        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();
        existing.Email = dto.Email.Trim();
        existing.Department = dto.Department.Trim();
        existing.IsActive = dto.IsActive;
        
        var updated = _repo.Update(existing);
        return updated ? NoContent() : NotFound();
    }
    
    // DELETE: api/users/{id}
    [HttpDelete("{id:int}")]
    public ActionResult DeleteUser(int id)
    {
        var deleted = _repo.Delete(id);
        return deleted ? NoContent() : NotFound();
    }
}