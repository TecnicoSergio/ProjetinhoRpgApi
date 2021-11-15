using Microsoft.EntityFrameworkCore;
using RpgApi.Models;
using RpgApi.Models.Enums;

namespace RpgApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Personagem> Personagens { get; set; }
        public DbSet<Arma> Armas { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Habilidade> Habilidades { get; set; }

        public DbSet<PersonagemHabilidade> PersonagemHabilidades { get; set; }
         public DbSet<Disputa> Disputas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilde)
        {
            modelBuilde.Entity<Personagem>().HasData
            (
               new Personagem() { Id = 1 }, //Frodo Cavaleiro             
               new Personagem() { Id = 2, Nome = "Sam", PontosVida=100, Forca=15, Defesa=25, Inteligencia=30, Classe=ClasseEnum.Cavaleiro},     
               new Personagem() { Id = 3, Nome = "Galadriel", PontosVida=100, Forca=18, Defesa=21, Inteligencia=35, Classe=ClasseEnum.Clerigo },
               new Personagem() { Id = 4, Nome = "Gandalf", PontosVida=100, Forca=18, Defesa=18, Inteligencia=37, Classe=ClasseEnum.Mago },
               new Personagem() { Id = 5, Nome = "Hobbit", PontosVida=100, Forca=20, Defesa=17, Inteligencia=31, Classe=ClasseEnum.Cavaleiro },
               new Personagem() { Id = 6, Nome = "Celeborn", PontosVida=100, Forca=21, Defesa=13, Inteligencia=34, Classe=ClasseEnum.Clerigo },
               new Personagem() { Id = 7, Nome = "Radagast", PontosVida=100, Forca=25, Defesa=11, Inteligencia=35, Classe=ClasseEnum.Mago}
              
               
            );

           modelBuilde.Entity<Arma>().HasData
            (
               new Arma() {Id = 1, Nome= "Espada Ninja", Dano=35,PersonagemId=1 },
               new Arma() {Id = 2, Nome= "Chicote Perverso", Dano=36,PersonagemId=2 },
               new Arma() {Id = 3, Nome= "Laço Horizontal", Dano=37,PersonagemId=3 }
           );

           modelBuilde.Entity<PersonagemHabilidade>()
           .HasKey(ph => new { ph.PersonagemId, ph.HabilidadeId});

           modelBuilde.Entity<Habilidade>().HasData
           (
               new Habilidade() { Id = 1, Nome="Adormecer", Dano=39 },
               new Habilidade() { Id = 2, Nome="Congelar", Dano=41 },
               new Habilidade() { Id = 3, Nome="Hipnotizar", Dano=37 }
           );

           
           modelBuilde.Entity<Usuario>().Property(u => u.Perfil).HasDefaultValue("Jogador");

            
        }

        

        
    }
}