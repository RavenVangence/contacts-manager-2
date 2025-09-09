using ContactsManager.Server.Models;

namespace ContactsManager.Server.Services;

public interface IContactRepository
{
    IEnumerable<Contact> GetAll();
    Contact? Get(int id);
    Contact Add(Contact c);
    bool Update(int id, Contact c);
    bool Delete(int id);
}
