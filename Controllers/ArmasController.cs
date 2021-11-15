using Microsoft.AspNetCore.Mvc;
using RpgApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpgApi.Data;
using Microsoft.EntityFrameworkCore;

namespace RpgApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ArmasController : ControllerBase
    {
        private readonly DataContext _context;
        public ArmasController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("getAll")]

        public async Task<IActionResult> Get()
        {            
            try
            {
                List<Arma> ListaArmas = await _context.Armas.ToListAsync();
                return Ok(ListaArmas);
                 
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
            
        }

        /*[HttpGet("{id}")] //Buscar pelo id
        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
                 Arma ar = await _context.Armas
                        .FirstOrDefaultAsync(arBusca => arBusca.Id == id);
                 return Ok(ar);
            }
            catch (System.Exception ex)
            {
                
              return BadRequest(ex.Message);
            }
        }*/

        /*[HttpPost]
        public async Task<IActionResult> Add(Arma novoArma)
        {
            try
            {
                if(novoArma.Dano > 50)
                {
                    throw new System.Exception("O Dano não pode ser maior que 50");
                }
                await _context.Armas.AddAsync(novoArma);
                await _context.SaveChangesAsync();
                
                return Ok(novoArma.Id);
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
      }*/


      [HttpPost]

      public async Task<IActionResult> Add(Arma novaArma)
      {
          try
          {
              if (novaArma.Dano == 0)
                throw new System.Exception("O dano da arma não pode ser 0");

            Personagem personagem = await _context.Personagens
              .FirstOrDefaultAsync(p => p.Id == novaArma.PersonagemId);

            if(personagem == null)
                throw new System.Exception("Seu usuario não contem personagens com o Id do Personagem informado.");

            Arma buscaArma = await _context.Armas
            .FirstOrDefaultAsync(a => a.PersonagemId == novaArma.PersonagemId);

            if (buscaArma != null)
                throw new System.Exception("O Personagem selecionado já contem uma arma atribuída a ele. ");

              await _context.Armas.AddAsync(novaArma);
              await _context.SaveChangesAsync();
              return Ok(novaArma.Id);  
               
          }
          catch (System.Exception ex)
          {
              
              return BadRequest(ex.Message);
          }
      }

      

      

     



        
    }

    
        

    
}   