using Microsoft.AspNetCore.Mvc;
using AssistentVirtualPrivada.Models;

namespace AssistentVirtualPrivada.Controllers;

[ApiController]
[Route("api/[controller]")] // A rota será: api/chat
public class ChatController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatMessage request)
    {

    if (string.IsNullOrEmpty(request.Texto))
    {
        return BadRequest("A mensagem não pode estar vazia.");
    }
        novaMensagem = new ChatMessage {Texto = request.Texto};
        _context.ChatMessages.Add(novaMensagem);
        await _conext.SaveChangesAsync();
        return Ok(new { $"Você disse: {novaMensagem}" });
    }
}