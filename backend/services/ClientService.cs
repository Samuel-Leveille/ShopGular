using ShopGular.backend.Models;
using ShopGular.Backend.Models.Dtos;
namespace ShopGular.Backend.Services;
public class ClientService
{

    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    public ClientDto? GetClientById(long id)
    {
        Client? client = null;
        try
        {
            client = _context.Clients.Find(id);
            if (client == null)
            {
                Console.WriteLine($"User client avec id {id} introuvable");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'obtention d'un utilisateur client par son id : {ex.Message}");
        }
        return client != null ? Client.ToDto(client) : null;
    }

    public ClientDto? SignUp(SignUpClientDto dto)
    {
        Client? client = null;
        try
        {
            client = Client.SignUpDtoToEntity(dto);
            client.Password = PasswordHashing.HashPassword(dto.Password);
            _context.Clients.Add(client);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la tentative d'ajout d'un nouvel utilisateur client : {ex.Message}");
        }
        return client != null ? Client.ToDto(client) : null;
    }
}