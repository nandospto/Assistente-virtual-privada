using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssistenteControllers : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public AssistenteControllers(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost] // Set Config
        public async Task<IActionResult> SetConfigs(Configuracoes configuracao)
        {
            _appDbContext.Configuracoes.Add(configuracao);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetConfigs), new { id = configuracao.Id }, configuracao);
        }
        [HttpGet] // Get Config
        public async Task<IActionResult> GetConfigs()
        {
            var result = await _appDbContext.Configuracoes.ToListAsync();

            return result == null ?  NotFound() : Ok(result);
        }
    }
}