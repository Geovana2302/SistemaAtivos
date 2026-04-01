namespace SistemaAtivos.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SistemaAtivos.Models.AtivosContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "SistemaAtivos.Models.AtivosContext";
        }

        protected override void Seed(SistemaAtivos.Models.AtivosContext context)
        {
            // Seed dos usuários admin se não existirem
            var adminExists = context.Usuarios.Any(u => u.Email == "admin@sistema.com");
            if (!adminExists)
            {
                context.Usuarios.Add(new SistemaAtivos.Models.Usuario
                {
                    Nome = "Administrador",
                    Email = "admin@sistema.com",
                    Senha = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Tipo = SistemaAtivos.Models.TipoUsuario.Admin,
                    EmpresaId = null
                });
                context.SaveChanges();
            }
        }
    }
}
