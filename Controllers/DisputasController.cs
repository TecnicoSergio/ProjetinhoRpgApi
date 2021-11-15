using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisputasController : ControllerBase
    {
        //Construtores e métodos aqui.
        private readonly DataContext _context;

        public DisputasController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("Arma")]

        public async Task<IActionResult> AtaqueComArmasAsync(Disputa d)
        {
            try
            {
                Personagem atacante = await _context.Personagens
                .Include(p => p.Arma)
                .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem oponente = await _context.Personagens
                .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                int dano = atacante.Arma.Dano + (new Random().Next(atacante.Forca));
                dano = dano - new Random().Next(oponente.Defesa);

                if(dano > 0)
                    oponente.PontosVida = oponente.PontosVida - dano;
                if(oponente.PontosVida <= 0)
                    d.Narracao = $"{oponente.Nome} foi derrotado";

                    _context.Personagens.Update(oponente);
                    await _context.SaveChangesAsync();

                    StringBuilder dados = new StringBuilder();
                    dados.AppendFormat(" Atacante: {0}. ", atacante.Nome);
                    dados.AppendFormat(" Oponente: {0}. ", atacante.Nome);
                    dados.AppendFormat(" Pontos de vida do atacante: {0}. ", atacante.PontosVida);
                    dados.AppendFormat(" Pontos de vida do atacante: {0}. ", oponente.PontosVida);
                    dados.AppendFormat(" Dano: {0}. ", dano);

                    d.Narracao += dados.ToString();
                    _context.Disputas.Add(d);
                    _context.SaveChanges();
                    return Ok(d);

                 
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost("Habilidade")]
        public async Task<IActionResult> AtaqueComHabilidadeAsync(Disputa d)
        {
            try
            {
                Personagem atacante = await _context.Personagens
                .Include(p => p.PersonagemHabilidades)
                .ThenInclude(ph => ph.Habilidade).FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem oponente = await _context.Personagens
                .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                PersonagemHabilidade ph = await _context.PersonagemHabilidades
                .Include(p => p.Habilidade)
                .FirstOrDefaultAsync(phBusca => phBusca.HabilidadeId == d.HabilidadeId);
                //&& phBusca.PersonagemId == d.AtacanteId
                

                if(ph == null)
                {
                     d.Narracao = $"{atacante.Nome} não possui esta habilidade";

                }
                   
                else
                {
                    int dano = ph.Habilidade.Dano + (new Random().Next(atacante.Inteligencia));
                    dano = dano - new Random().Next(oponente.Defesa);

                    if(dano > 0)
                    {
                        oponente.PontosVida = oponente.PontosVida - (int)dano;
                        
                    }
                    if(oponente.PontosVida <= 0)
                    {
                        d.Narracao += $"{oponente.Nome} foi derrotado ";
                    }

                    _context.Personagens.Update(oponente);
                    await _context.SaveChangesAsync();

                     StringBuilder dados = new StringBuilder();
                    dados.AppendFormat(" Atacante: {0}. ", atacante.Nome);
                    dados.AppendFormat(" Oponente: {0}. ", atacante.Nome);
                    dados.AppendFormat(" Pontos de vida do atacante: {0}. ", atacante.PontosVida);
                    dados.AppendFormat(" Pontos de vida do atacante: {0}. ", oponente.PontosVida);
                    dados.AppendFormat(" Dano: {0}. ", dano);

                    d.Narracao += dados.ToString();
                    _context.Disputas.Add(d);
                    _context.SaveChanges();


                }
                return Ok(d);
                 
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("PersonagemRandom")]
        public async Task<IActionResult> Sorteio()
        {
            List<Personagem> personagens =
                 await _context.Personagens.ToListAsync();
            //Sorteio com numero da quantidade de personagens - 1
            int sorteio = new Random().Next(personagens.Count);
            //busca na lista pelo indice sorteado (Não é o ID)
            Personagem p = personagens[sorteio];
            string msg = 
                string.Format("Nº Sorteado {0}. Personagem: {1}", sorteio, p.Nome);
            return Ok(msg);
        }

         [HttpPost("DisputaEmGrupo")]

         public async Task<IActionResult> DisputaEmGrupoAsync(Disputa d)
         {
             try
             {
                 List<Personagem> personagens = await _context.Personagens
                 .Include(p => p.Arma)
                 .Include(p => p.PersonagemHabilidades).ThenInclude(ph => ph.Habilidade)
                 .Where(p => d.ListaIdPersonagens.Contains(p.Id)).ToListAsync();

                 int qtdPersonagensVivos = personagens.FindAll(p => p.PontosVida > 0).Count;

                 while (qtdPersonagensVivos > 1)
                 {
                     List<Personagem> atacantes = personagens.Where(p => p.PontosVida > 0).ToList();
                     Personagem atacante = atacantes[new Random().Next(atacantes.Count)];
                     d.AtacanteId = atacante.Id;

                     List<Personagem> oponentes = personagens.Where(p => p.Id != atacante.Id && p.PontosVida > 0).ToList();
                     Personagem oponente = oponentes[new Random().Next(oponentes.Count)];
                     d.OponenteId = oponente.Id;

                     int dano = 0;
                     string ataqueUsado = string.Empty;
                     string resultado = string.Empty;

                     bool ataqueUsaArma = (new Random().Next(1) == 0);

                     if (ataqueUsaArma && atacante.Arma != null)
                     {
                         dano = atacante.Arma.Dano + (new Random().Next(atacante.Forca));
                         dano = dano - new Random().Next(oponente.Defesa);
                         ataqueUsado = atacante.Arma.Nome;

                         if (dano > 0)
                            oponente.PontosVida = oponente.PontosVida - (int)dano;

                        resultado =
                            string.Format("{0} atacou {1} usando {2} com o dano {3}.", atacante.Nome, oponente.Nome, ataqueUsado, dano);
                        d.Narracao += resultado;
                        d.Resultados.Add(resultado);

                     }
                     else if (atacante.PersonagemHabilidades.Count != 0)
                     {
                         int sorteioHabilidadeId = new Random().Next(atacante.PersonagemHabilidades.Count);
                         Habilidade habilidadeEscolhida = atacante.PersonagemHabilidades[sorteioHabilidadeId].Habilidade;
                         ataqueUsado = habilidadeEscolhida.Nome;

                         dano = habilidadeEscolhida.Dano + (new Random().Next(atacante.Inteligencia));
                         dano = dano - new Random().Next(oponente.Defesa);

                         if (dano > 0)
                            oponente.PontosVida = oponente.PontosVida - (int)dano;
                        resultado =
                            string.Format("{0} atacou {1} usando {2} com o dano {3}.", atacante.Nome, oponente.Nome, ataqueUsado, dano);
                        d.Narracao += resultado;
                        d.Resultados.Add(resultado);

                         
                     }
                     if (!string.IsNullOrEmpty(ataqueUsado))
                     {
                         atacante.Vitorias++;
                         oponente.Derrotas++;
                         atacante.Disputas++;
                         oponente.Disputas++;

                         d.Id = 0;
                         d.DataDisputa = DateTime.Now;
                         _context.Disputas.Add(d);
                         await _context.SaveChangesAsync();
                     }

                     qtdPersonagensVivos = personagens.FindAll(p => p.PontosVida > 0).Count;

                     if (qtdPersonagensVivos == 1)
                     {
                         string resultadoFinal =
                            $"{atacante.Nome.ToUpper()} é CAMPEÃO COM {atacante.PontosVida} pontos de vida restantes";

                        d.Narracao += resultadoFinal;
                        d.Resultados.Add(resultadoFinal);

                        break;
                     }


                      
                    }

                 _context.Personagens.UpdateRange(personagens);
                 await _context.SaveChangesAsync();

                 return Ok(d);
                  
             }
             catch (System.Exception ex)
             {
                 
                 return BadRequest(ex.Message);
             }
         }

         [HttpDelete("ApagarDisputas")]
         public async Task<IActionResult> DeleteAsync()
         {
             try
             {
                  List<Disputa> disputas = await _context.Disputas.ToListAsync();

                  _context.Disputas.RemoveRange(disputas);
                  await _context.SaveChangesAsync();

                  return Ok("Disputas apagadas");
             }
             catch (System.Exception ex)
             {
                 
                 return BadRequest(ex.Message);
             }
         }
    
    
      


    }

    
   
}