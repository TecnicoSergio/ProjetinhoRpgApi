using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;

namespace RpgApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        
        public UsuariosController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //Metodo que gera o Hash e Salt de uma senha
        private void CriarPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<bool> UsuarioExistente(string username)
        {
            if(await _context.Usuarios.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
            {
                return true;
            }
            return false;
        }
        
        [AllowAnonymous]
        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario(Usuario user)
        {
            try
            {
                 if(await UsuarioExistente(user.Username))
                   throw new System.Exception("Nome de usuario já existe");

                 CriarPasswordHash(user.PasswordString, out byte[] hash, out byte[] salt);
                 user.PasswordString = string.Empty;
                 user.PasswordHash = hash;
                 user.PasswordSalt = salt;

                 await _context.Usuarios.AddAsync(user);
                 await _context.SaveChangesAsync();
                 return Ok(user.Id);
            }
            catch (System.Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
            
        }
        private bool VerificarPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
       
       
        [AllowAnonymous]
        [HttpPost("Autenticar")]

        public async Task<IActionResult> AutenticarUsuario(Usuario credenciaisUsuario)
        {
            try
            {
                Usuario usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Username .ToLower()
                .Equals(credenciaisUsuario.Username.ToLower()));

                if(usuario == null)
                
                    throw new System.Exception("Usuario não encontra");
                
                else if(!VerificarPasswordHash(credenciaisUsuario.PasswordString, usuario.PasswordHash, usuario.PasswordSalt))
                
                    throw new System.Exception("Senha incorreta.");
                
                else
                {
                    usuario.DataAcesso = System.DateTime.Now;
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();

                    //return Ok(usuario.Id);
                    return Ok(CriarToken(usuario));
                }
                    
                 
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
                
                
            }
        }

    private string CriarToken(Usuario usuario)
    {
       List<Claim> claims = new List<Claim>
       {
           new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
           new Claim(ClaimTypes.Name, usuario.Username),
           new Claim(ClaimTypes.Role, usuario.Perfil)

       };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
        .GetBytes(_configuration.GetSection("ConfiguracaoToken:Chave").Value));

        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
           Subject = new ClaimsIdentity(claims),
           Expires = DateTime.Now.AddDays(1),
           SigningCredentials = creds
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
        
    }
}