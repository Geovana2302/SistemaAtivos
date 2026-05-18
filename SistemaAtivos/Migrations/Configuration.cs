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
            var admin = context.Usuarios.FirstOrDefault(u => u.Email == "admin@sistema.com");
            if (admin == null)
            {
                context.Usuarios.Add(new SistemaAtivos.Models.Usuario
                {
                    Nome         = "Administrador",
                    Email        = "admin@sistema.com",
                    Senha        = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Perfil       = SistemaAtivos.Models.Perfil.Admin,
                    EmpresaId    = null,
                    IsSuperAdmin = true
                });
                context.SaveChanges();
            }
            else
            {
                if (!admin.Senha.StartsWith("$2"))
                    admin.Senha = BCrypt.Net.BCrypt.HashPassword("admin123");
                admin.IsSuperAdmin = true;
                context.SaveChanges();
            }
        }
    }
}
