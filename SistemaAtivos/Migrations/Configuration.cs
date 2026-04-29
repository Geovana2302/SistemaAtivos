namespace SistemaAtivos.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SistemaAtivos.Helpers;

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
            // REQUISITO 1 - SEGURANCA DE SENHAS
            // A senha original era "admin123". Vamos salvar o HASH dela.
            // Assim, o banco de dados nunca armazena a senha em texto puro.
            string senhaCriptografada = CriptoHelper.HashSHA256("admin123");

            context.Usuarios.AddOrUpdate(u => u.Email,
                new SistemaAtivos.Models.Usuario
                {
                    Nome = "Administrador",
                    Email = "admin@sistema.com",
                    Senha = senhaCriptografada, // Salvando a senha embaralhada!
                    Tipo = SistemaAtivos.Models.TipoUsuario.Admin,
                    EmpresaId = null
                }
            );
            context.SaveChanges();
        }
    }
}
