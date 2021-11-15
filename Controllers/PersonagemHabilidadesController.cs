using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using RpgApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonagemHabilidadesController : ControllerBase

    {
        private readonly DataContext _context;

        public PersonagemHabilidadesController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]

        public async Task<IActionResult> AddPersonagemHabilidadeAsync(PersonagemHabilidade novoPersonagemHabilidade)
        {
            try
            {
               Personagem personagem = await _context.Personagens
            .Include(p => p.Arma).Include(p => p.PersonagemHabilidades).ThenInclude(ps => ps.Habilidade).FirstOrDefaultAsync(p => p.Id == novoPersonagemHabilidade.PersonagemId);

            if (personagem == null)
                throw new System.Exception("Personagem não encontrado para o Id Informado.");

            Habilidade habilidade = await _context.Habilidades.FirstOrDefaultAsync(h => h.Id == novoPersonagemHabilidade.HabilidadeId);
            if (habilidade == null)
                throw new System.Exception("Habilidade não encontrada.");

            PersonagemHabilidade ph = new PersonagemHabilidade();
            ph.Personagem = personagem;
            ph.Habilidade = habilidade;
            await _context.PersonagemHabilidades.AddAsync(ph);
            int linhasAfetadas = await _context.SaveChangesAsync();

            return Ok(linhasAfetadas);   
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        /*[HttpGet("{id}")]//busca pelo id

      public async Task<IActionResult> GetSingle(int id)
      {
          try
          {
               Personagem p = await _context.Personagens
               .Include(ar => ar.Arma)
               .Include(ph => ph.PersonagemHabilidades)
               .FirstOrDefaultAsync(pBusca => pBusca.Id == id);

               return Ok(p);
          }
          catch (System.Exception ex)
          {
              
              return BadRequest(ex.Message);
          }
      }*/

      [HttpPost("DeletarPersonagemHabilidade")]

      public async Task<IActionResult> DeleteAsync(PersonagemHabilidade ph)
      {
          try
          {
               PersonagemHabilidade phRemove = await _context.PersonagemHabilidades
               .FirstOrDefaultAsync(phBusca => phBusca.PersonagemId == ph.PersonagemId 
               && phBusca.HabilidadeId == ph.HabilidadeId);
               if (phRemove == null)
                throw new System.Exception("Personagem ou Habilidade não encontrados");
                _context.PersonagemHabilidades.Remove(phRemove);
                int linhasAfetadas = await _context.SaveChangesAsync();


               return Ok(linhasAfetadas);
          }
          catch (System.Exception ex)
          {
              
              return BadRequest(ex.Message);
          }
      }

      [HttpGet("personagemId")]

      public async Task<IActionResult> GetHabilidadesPersonagem(int personagemId)
      {
          try
          {
              List<PersonagemHabilidade> phLista = new List<PersonagemHabilidade>();
               
               phLista = await _context.PersonagemHabilidades
               .Include(p => p.Personagem)
               .Include(p => p.Habilidade)
               .Where(p => p.Personagem.Id == personagemId)
               .ToListAsync();


               return Ok(phLista);
          }
          catch (System.Exception ex)
          {
              
              return BadRequest(ex.Message);
          }
      }

      [HttpGet("GetHabilidades")]//<-- Se aqui estiver escrito GetHabilidade entao aqui em baixo estará tambem, se estiver Y em baixo tambem tem que ser Y.

      public async Task<IActionResult> GetHabilidades(int Habilidade)
      {
          try
          {
             List<Habilidade> habilidades = new List<Habilidade>();
             habilidades = await _context.Habilidades.ToListAsync();


               return Ok(habilidades);
          }
          catch (System.Exception ex)
          {
              
              return BadRequest(ex.Message);
          }
      }


        
    }

   
    

    

     
}