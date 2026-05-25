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

                // Colaboradores
                var colaborador = new SistemaAtivos.Models.Colaborador
                {
                    Nome      = "Joao Silva",
                    Cargo     = "Analista de TI",
                    Email     = "joao@demo.com",
                    EmpresaId = empresa.Id
                };
                var colaborador2 = new SistemaAtivos.Models.Colaborador
                {
                    Nome      = "Maria Souza",
                    Cargo     = "Gestora Administrativa",
                    Email     = "maria@demo.com",
                    EmpresaId = empresa.Id
                };
                context.Colaboradores.Add(colaborador);
                context.Colaboradores.Add(colaborador2);
                context.SaveChanges();

                // Categorias separadas com icones
                var catNotebook  = new SistemaAtivos.Models.Categoria { Nome = "Notebooks",    Descricao = "Computadores portateis",              EmpresaId = empresa.Id, Icone = "bi-laptop" };
                var catMonitor   = new SistemaAtivos.Models.Categoria { Nome = "Monitores",    Descricao = "Telas e displays",                   EmpresaId = empresa.Id, Icone = "bi-display" };
                var catPerif     = new SistemaAtivos.Models.Categoria { Nome = "Perifericos",  Descricao = "Teclados, mouses e headsets",         EmpresaId = empresa.Id, Icone = "bi-keyboard" };
                var catRede      = new SistemaAtivos.Models.Categoria { Nome = "Rede",         Descricao = "Roteadores, switches e cabeamento",   EmpresaId = empresa.Id, Icone = "bi-wifi" };
                var catMoveis    = new SistemaAtivos.Models.Categoria { Nome = "Moveis",       Descricao = "Mesas, cadeiras e armarios",          EmpresaId = empresa.Id, Icone = "bi-lamp" };
                context.Categorias.Add(catNotebook);
                context.Categorias.Add(catMonitor);
                context.Categorias.Add(catPerif);
                context.Categorias.Add(catRede);
                context.Categorias.Add(catMoveis);
                context.SaveChanges();

                // Ativos distribuidos nas categorias
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Notebook Dell Inspiron",
                    NumeroSerie   = "SN-001",
                    Marca         = "Dell",
                    Modelo        = "Inspiron 15",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2023, 1, 10),
                    Valor         = 3500.00m,
                    CategoriaId   = catNotebook.Id,
                    ColaboradorId = colaborador.Id,
                    EmpresaId     = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Notebook Lenovo ThinkPad",
                    NumeroSerie   = "SN-002",
                    Marca         = "Lenovo",
                    Modelo        = "ThinkPad E14",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2023, 6, 15),
                    Valor         = 4200.00m,
                    CategoriaId   = catNotebook.Id,
                    ColaboradorId = colaborador2.Id,
                    EmpresaId     = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Monitor LG 24\"",
                    NumeroSerie   = "SN-003",
                    Marca         = "LG",
                    Modelo        = "24MP60G",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2023, 3, 5),
                    Valor         = 1200.00m,
                    CategoriaId   = catMonitor.Id,
                    ColaboradorId = colaborador.Id,
                    EmpresaId     = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Monitor Samsung 27\"",
                    NumeroSerie   = "SN-004",
                    Marca         = "Samsung",
                    Modelo        = "F27T450",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2022, 11, 20),
                    Valor         = 1800.00m,
                    CategoriaId   = catMonitor.Id,
                    ColaboradorId = colaborador2.Id,
                    EmpresaId     = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Teclado Mecanico Logitech",
                    NumeroSerie   = "SN-005",
                    Marca         = "Logitech",
                    Modelo        = "G413",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2023, 2, 1),
                    Valor         = 450.00m,
                    CategoriaId   = catPerif.Id,
                    ColaboradorId = colaborador.Id,
                    EmpresaId     = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Roteador TP-Link",
                    NumeroSerie   = "SN-006",
                    Marca         = "TP-Link",
                    Modelo        = "Archer AX73",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2022, 8, 10),
                    Valor         = 600.00m,
                    CategoriaId   = catRede.Id,
                    ColaboradorId = colaborador.Id,
                    EmpresaId     = empresa.Id
                });
                context.Ativos.Add(new SistemaAtivos.Models.Ativo
                {
                    Nome          = "Cadeira Ergonomica",
                    NumeroSerie   = "SN-007",
                    Marca         = "Flexform",
                    Modelo        = "Pro 500",
                    Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                    DataAquisicao = new DateTime(2022, 6, 20),
                    Valor         = 800.00m,
                    CategoriaId   = catMoveis.Id,
                    ColaboradorId = colaborador2.Id,
                    EmpresaId     = empresa.Id
                });
                context.SaveChanges();
            }
        }
    }
}
