using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;

namespace ShopGular.Backend.Controllers;
[ApiController]
[Route("api/user/[controller]")]
public class ClientController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("{id}")]
    public IActionResult GetClientById(long id)
    {
        ClientDto? clientDto = _clientService.GetClientById(id);
        return Ok(clientDto);
    }

    [HttpPost("signup")]
    public IActionResult SignUp(SignUpClientDto dto)
    {
        ClientDto? clientDto = _clientService.SignUp(dto);
        return CreatedAtAction(nameof(GetClientById), new { id = clientDto?.Id }, clientDto);
    }

}