using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopGular.Backend.Models.Dtos;
using ShopGular.backend.Models;

namespace ShopGular.Backend.Models;
[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; private set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public User(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = password;
    }

    public static UserDto ToDto(User user)
    {
        string? first = null;
        if (user is Client c)
        {
            first = c.FirstName;
        }
        return new UserDto(user.Id, user.Name, first, user.Email, null);
    }
}