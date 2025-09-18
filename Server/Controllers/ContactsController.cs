using ContactsManager.Server.Models;
using ContactsManager.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactRepository _repo;
    public ContactsController(IContactRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
        => _repo.Get(id) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public IActionResult Create(Contact c)
    {
        if (string.IsNullOrWhiteSpace(c.FirstName) || string.IsNullOrWhiteSpace(c.LastName) || string.IsNullOrWhiteSpace(c.Phone))
            return BadRequest("FirstName, LastName and Phone are required");
        var created = _repo.Add(c);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Contact c)
        => _repo.Update(id, c) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
        => _repo.Delete(id) ? NoContent() : NotFound();

    [HttpPost("save-all")]
    public IActionResult SaveAll([FromBody] List<Contact> contacts)
    {
        try
        {
            _repo.SaveAll(contacts);
            return Ok(new { message = "All contacts saved successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error saving contacts: {ex.Message}" });
        }
    }
}
