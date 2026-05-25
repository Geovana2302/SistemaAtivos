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
            // ── Admin principal ────────────────────────────────────────────
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

            // ── Empresa de demonstracao ────────────────────────────────────
            if (!context.Empresas.Any(e => e.Nome == "Empresa Demo"))
            {
                var empresa = new SistemaAtivos.Models.Empresa
                {
                    Nome = "Empresa Demo",
                    Cor  = "#534AB7"
                };
                context.Empresas.Add(empresa);
                context.SaveChanges();

                // Gestor da empresa
                context.Usuarios.Add(new SistemaAtivos.Models.Usuario
                {
                    Nome      = "Gestor Demo",
                    Email     = "gestor@demo.com",
                    Senha     = BCrypt.Net.BCrypt.HashPassword("gestor123"),
                    Perfil    = SistemaAtivos.Models.Perfil.Gestor,
                    EmpresaId = empresa.Id
                });
                context.SaveChanges();

                // Colaborador
                var colaborador = new SistemaAtivos.Models.Colaborador
                {
                    Nome      = "Joao Silva",
                    Cargo     = "Analista de TI",
                    Email     = "joao@demo.com",
                    EmpresaId = empresa.Id
                };
                context.Colaboradores.Add(colaborador);
                context.SaveChanges();

                // Categorias
                var catTI  = new SistemaAtivos.Models.Categoria { Nome = "Informatica", EmpresaId = empresa.Id, Icone = "bi-laptop" };
                var catMov = new SistemaAtivos.Models.Categoria { Nome = "Moveis",      EmpresaId = empresa.Id, Icone = "bi-chair" };
                context.Categorias.Add(catTI);
                context.Categorias.Add(catMov);
                context.SaveChanges();

                // Ativos
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome           = "Notebook Dell",
                    NumeroSerie    = "SN-001",
                    Marca          = "Dell",
                    Modelo         = "Inspiron 15",
                    Status         = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao  = new DateTime(2023, 1, 10),
                    Valor          = 3500.00m,
                    CategoriaId    = catTI.Id,
                    ColaboradorId  = colaborador.Id,
                    EmpresaId      = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome           = "Monitor LG 24\"",
                    NumeroSerie    = "SN-002",
                    Marca          = "LG",
                    Modelo         = "24MP60G",
                    Status         = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao  = new DateTime(2023, 3, 5),
                    Valor          = 1200.00m,
                    CategoriaId    = catTI.Id,
                    ColaboradorId  = colaborador.Id,
                    EmpresaId      = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome           = "Cadeira Ergonomica",
                    NumeroSerie    = "SN-003",
                    Marca          = "Flexform",
                    Modelo         = "Pro 500",
                    Status         = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao  = new DateTime(2022, 6, 20),
                    Valor          = 800.00m,
                    CategoriaId    = catMov.Id,
                    ColaboradorId  = colaborador.Id,
                    EmpresaId      = empresa.Id
                });
                context.SaveChanges();
            }
        }
    }
}
