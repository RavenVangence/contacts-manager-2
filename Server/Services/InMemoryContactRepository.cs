using ContactsManager.Server.Models;

namespace ContactsManager.Server.Services;

public class InMemoryContactRepository : IContactRepository
{
    private readonly List<Contact> _contacts = new();
    private int _nextId = 1;

    public InMemoryContactRepository()
    {
        // Add some sample data
        Add(new Contact { FirstName = "John", LastName = "Doe", Phone = "555-1234", Used = true });
        Add(new Contact { FirstName = "Jane", LastName = "Smith", Phone = "555-5678", Used = false });
        Add(new Contact { FirstName = "Mike", LastName = "Johnson", Phone = "555-9012", Used = true });
        Add(new Contact { FirstName = "Sarah", LastName = "Williams", Phone = "555-3456", Used = false });
        Add(new Contact { FirstName = "David", LastName = "Brown", Phone = "555-7890", Used = true });
    }

    public Contact Add(Contact c)
    {
        c.Id = _nextId++;
        _contacts.Add(c);
        return c;
    }

    public bool Delete(int id)
    {
        var existing = Get(id);
        if (existing == null) return false;
        _contacts.Remove(existing);
        return true;
    }

    public Contact? Get(int id) => _contacts.FirstOrDefault(c => c.Id == id);

    public IEnumerable<Contact> GetAll() => _contacts;

    public bool Update(int id, Contact c)
    {
        var existing = Get(id);
        if (existing == null) return false;
        existing.FirstName = c.FirstName;
        existing.LastName = c.LastName;
        existing.Phone = c.Phone;
        existing.Used = c.Used;
        return true;
    }

    public void SaveAll(List<Contact> contacts)
    {
        // Replace all contacts with the provided list
        _contacts.Clear();
        _contacts.AddRange(contacts);

        // Update next ID to be one higher than the highest ID
        _nextId = contacts.Any() ? contacts.Max(c => c.Id) + 1 : 1;
    }
}
