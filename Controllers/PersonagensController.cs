using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;

namespace RpgApi.Controllers
{
    [Authorize(Roles = "Jogador,Admin")]
    [ApiController]
    [Route("[controller]")]
    public class PersonagensController : ControllerBase
    {
        private readonly DataContext _context;//Declaração

        private readonly IHttpContextAccessor _httpContextAccessor;

        private int ObterUsuarioId()
        {
            return int.Parse(_httpContextAccessor.HttpContext.User
            .FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public PersonagensController(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context; //inicialização do atributo
            _httpContextAccessor = httpContextAccessor;

        }

        [HttpGet("{id}")] //Buscar pelo id
        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
                 Personagem p = await _context.Personagens
                 .Include(ar => ar.Arma)
                 .Include(us => us.Usuario)
                 .Include(ph => ph.PersonagemHabilidades)
                 .ThenInclude(h => h.Habilidade)
                        .FirstOrDefaultAsync(pBusca => pBusca.Id == id);
                 return Ok(p);
            }
            catch (System.Exception ex)
            {
                
              return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<Personagem> lista = await _context.Personagens.ToListAsync();
                return Ok(lista);
                 
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(Personagem novoPersonagem)
        {
            try
            {
                if(novoPersonagem.PontosVida > 100)
                {
                    throw new System.Exception("Ponto de vida não pode ser maior que  100");
                }

                //int usuarioId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                //novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == usuarioId);
                novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == ObterUsuarioId());

                await _context.Personagens.AddAsync(novoPersonagem);
                await _context.SaveChangesAsync();
                return Ok(novoPersonagem.Id);
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(Personagem novoPersonagem)
        {
            try
            {
                  if(novoPersonagem.PontosVida > 100)
                {
                    throw new System.Exception("Ponto de vida não pode ser maior que  100");
                }

                //int usuarioId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                //novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == usuarioId);
                novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == ObterUsuarioId());


                 _context.Personagens.Update(novoPersonagem);
                 int linhaAfetadas = await _context.SaveChangesAsync();

                 return Ok(linhaAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
                
            }
        }

        [HttpDelete("id")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                 Personagem pRemover = await _context.Personagens
                 .FirstOrDefaultAsync(p => p.Id == id);

                 _context.Personagens.Remove(pRemover);
                 int linhaAfetadas = await _context.SaveChangesAsync();
                 
                 return Ok(linhaAfetadas);
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetByUser")]

        public async Task<IActionResult> GetByUserAsync()
        {
            try
            {
                 int id = int.Parse(User.Claims
                 .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

                 List<Personagem> lista = await _context.Personagens
                 .Where(u => u.Usuario.Id == id).ToListAsync();
                 return Ok(lista);

            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        private string ObterPerfilUsuario()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
        }

        [HttpGet("GetByPerfil")]
        public async Task<IActionResult> GetByPerfilAsync()
        {
            try
            {
                 List<Personagem> lista = new List<Personagem>();

                 if(ObterPerfilUsuario() == "Admin")
                 
                     lista = await _context.Personagens.ToListAsync();
                 
                 else
                 {
                     lista = await _context.Personagens
                     .Where(p => p.Usuario.Id == ObterUsuarioId()).ToListAsync();
                 }
                 return Ok(lista);
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("RestaurarPontosVida")]
        public async Task<IActionResult> RestaurarPontosVidaAsync(Personagem p)
        {
            try
            {
                int linhaAfetadas = 0;
                Personagem pEncontrado =
                await _context.Personagens.FirstOrDefaultAsync(pBusca => pBusca.Id == p.Id);
                pEncontrado.PontosVida = 100;

                bool atualizou = await TryUpdateModelAsync<Personagem>(pEncontrado, "p", pAtualizar => pAtualizar.PontosVida);
                
                if (atualizou)
                    linhaAfetadas = await _context.SaveChangesAsync();
                
                return Ok(linhaAfetadas);
                 
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ZerarRanking")]
        public async Task<IActionResult> ZerarRankingAsync(Personagem p)
        {
            try
            {
                 Personagem pEncontrado =
                 await _context.Personagens.FirstOrDefaultAsync(pBusca => pBusca.Id == p.Id);

                 pEncontrado.Disputas = 0;
                 pEncontrado.Vitorias = 0;
                 pEncontrado.Derrotas = 0;
                 int linhaAfetadas = 0;

                 bool atualizou = await TryUpdateModelAsync<Personagem>(pEncontrado, "p",
                 pAtualizar => pAtualizar.Disputas,
                 pAtualizar => pAtualizar.Vitorias,
                 pAtualizar => pAtualizar.Derrotas);

                 if (atualizou)
                    linhaAfetadas = await _context.SaveChangesAsync();

                    return Ok(linhaAfetadas); 
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ZerarRankingRestaurarVidas")]
        public async Task<IActionResult> ZerarRankingRestaurarVidasAsync()
        {
            try
            {
                 List<Personagem> lista =
                 await _context.Personagens.ToListAsync();

                 foreach (Personagem p in lista)
                 {
                     await ZerarRankingAsync(p);
                     await RestaurarPontosVidaAsync(p);
                 }
                 return Ok();
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }


        
    }
}
//Identação: SHIFT + ALT + F