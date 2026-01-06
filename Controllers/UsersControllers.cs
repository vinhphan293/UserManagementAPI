using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository repo, ILogger<UsersController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET: api/users
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        try
        {
            return Ok(_repo.GetAll());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users");
            return StatusCode(500, "An error occurred while retrieving users.");
        }
    }

    // GET: api/users/{id}
    [HttpGet("{id:int}")]
    public ActionResult<User> GetUser(int id)
    {
        if (id <= 0) return BadRequest("Invalid user id.");
        try
        {
            var user = _repo.GetById(id);
            return user is null ? NotFound() : Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user with id {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the user.");
        }
    }

    // POST: api/users
    [HttpPost]
    public ActionResult<User> CreateUser(CreateUserDto dto)
    {
        if (dto is null) return BadRequest("Request body is required.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(dto.Email))
            return BadRequest("Email is not a valid email address.");

        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with email {Email}", dto?.Email);
            return StatusCode(500, "An error occurred while creating the user.");
        }
    }
    
    // PUT: api/users/{id}
    [HttpPut("{id:int}")]
    public ActionResult UpdateUser(int id, UpdateUserDto dto)
    {
        if (id <= 0) return BadRequest("Invalid user id.");
        if (dto is null) return BadRequest("Request body is required.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(dto.Email))
            return BadRequest("Email is not a valid email address.");

        try
        {
            var existing = _repo.GetById(id);
            if (existing is null) return NotFound();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user with id {Id}", id);
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }
    
    // DELETE: api/users/{id}
    [HttpDelete("{id:int}")]
    public ActionResult DeleteUser(int id)
    {
        if (id <= 0) return BadRequest("Invalid user id.");
        try
        {
            var deleted = _repo.Delete(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user with id {Id}", id);
            return StatusCode(500, "An error occurred while deleting the user.");
        }
    }
}