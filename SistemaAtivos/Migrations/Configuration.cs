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
            var empresa = context.Empresas.FirstOrDefault(e => e.Nome == "Empresa Demo");
            if (empresa == null)
            {
                empresa = new SistemaAtivos.Models.Empresa { Nome = "Empresa Demo", Cor = "#534AB7" };
                context.Empresas.Add(empresa);
                context.SaveChanges();
            }

            // Gestor
            if (!context.Usuarios.Any(u => u.Email == "gestor@demo.com"))
            {
                context.Usuarios.Add(new SistemaAtivos.Models.Usuario
                {
                    Nome      = "Gestor Demo",
                    Email     = "gestor@demo.com",
                    Senha     = BCrypt.Net.BCrypt.HashPassword("gestor123"),
                    Perfil    = SistemaAtivos.Models.Perfil.Gestor,
                    EmpresaId = empresa.Id
                });
                context.SaveChanges();
            }

            // Colaboradores
            var colaborador = context.Colaboradores.FirstOrDefault(c => c.Email == "joao@demo.com" && c.EmpresaId == empresa.Id);
            if (colaborador == null)
            {
                colaborador = new SistemaAtivos.Models.Colaborador { Nome = "Joao Silva", Cargo = "Analista de TI", Email = "joao@demo.com", EmpresaId = empresa.Id };
                context.Colaboradores.Add(colaborador);
                context.SaveChanges();
            }

            var colaborador2 = context.Colaboradores.FirstOrDefault(c => c.Email == "maria@demo.com" && c.EmpresaId == empresa.Id);
            if (colaborador2 == null)
            {
                colaborador2 = new SistemaAtivos.Models.Colaborador { Nome = "Maria Souza", Cargo = "Gestora Administrativa", Email = "maria@demo.com", EmpresaId = empresa.Id };
                context.Colaboradores.Add(colaborador2);
                context.SaveChanges();
            }

            // Categorias com icones — cria ou atualiza
            var defCats = new[]
            {
                new { Nome = "Notebooks",   Descricao = "Computadores portateis",            Icone = "bi-laptop"   },
                new { Nome = "Monitores",   Descricao = "Telas e displays",                  Icone = "bi-display"  },
                new { Nome = "Perifericos", Descricao = "Teclados, mouses e headsets",        Icone = "bi-keyboard" },
                new { Nome = "Rede",        Descricao = "Roteadores, switches e cabeamento",  Icone = "bi-wifi"     },
                new { Nome = "Moveis",      Descricao = "Mesas, cadeiras e armarios",         Icone = "bi-lamp"     },
            };

            // Remove categorias antigas genericas (Informatica) sem ativos vinculados
            var catGenerica = context.Categorias.FirstOrDefault(c => c.EmpresaId == empresa.Id && c.Nome == "Informatica");
            if (catGenerica != null && !context.Ativos.Any(a => a.CategoriaId == catGenerica.Id))
            {
                context.Categorias.Remove(catGenerica);
                context.SaveChanges();
            }

            foreach (var def in defCats)
            {
                var cat = context.Categorias.FirstOrDefault(c => c.EmpresaId == empresa.Id && c.Nome == def.Nome);
                if (cat == null)
                {
                    context.Categorias.Add(new SistemaAtivos.Models.Categoria
                    {
                        Nome      = def.Nome,
                        Descricao = def.Descricao,
                        Icone     = def.Icone,
                        EmpresaId = empresa.Id
                    });
                }
                else
                {
                    cat.Icone     = def.Icone;
                    cat.Descricao = def.Descricao;
                }
            }
            context.SaveChanges();

            // Referencia rapida as categorias
            var catNotebook = context.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Notebooks");
            var catMonitor  = context.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Monitores");
            var catPerif    = context.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Perifericos");
            var catRede     = context.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Rede");
            var catMoveis   = context.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Moveis");

            // Ativos — insere apenas se nao existir pelo numero de serie
            var defAtivos = new[]
            {
                new { Nome = "Notebook Dell Inspiron",    NS = "DEMO-001", Marca = "Dell",     Modelo = "Inspiron 15",   Valor = 3500m,  Cat = catNotebook.Id, Col = colaborador.Id,  Data = new DateTime(2023,1,10)  },
                new { Nome = "Notebook Lenovo ThinkPad",  NS = "DEMO-002", Marca = "Lenovo",   Modelo = "ThinkPad E14",  Valor = 4200m,  Cat = catNotebook.Id, Col = colaborador2.Id, Data = new DateTime(2023,6,15)  },
                new { Nome = "Monitor LG 24\"",           NS = "DEMO-003", Marca = "LG",       Modelo = "24MP60G",       Valor = 1200m,  Cat = catMonitor.Id,  Col = colaborador.Id,  Data = new DateTime(2023,3,5)   },
                new { Nome = "Monitor Samsung 27\"",      NS = "DEMO-004", Marca = "Samsung",  Modelo = "F27T450",       Valor = 1800m,  Cat = catMonitor.Id,  Col = colaborador2.Id, Data = new DateTime(2022,11,20) },
                new { Nome = "Teclado Logitech G413",     NS = "DEMO-005", Marca = "Logitech", Modelo = "G413",          Valor = 450m,   Cat = catPerif.Id,    Col = colaborador.Id,  Data = new DateTime(2023,2,1)   },
                new { Nome = "Mouse Logitech MX Master",  NS = "DEMO-006", Marca = "Logitech", Modelo = "MX Master 3",   Valor = 380m,   Cat = catPerif.Id,    Col = colaborador2.Id, Data = new DateTime(2023,2,1)   },
                new { Nome = "Roteador TP-Link AX73",     NS = "DEMO-007", Marca = "TP-Link",  Modelo = "Archer AX73",   Valor = 600m,   Cat = catRede.Id,     Col = colaborador.Id,  Data = new DateTime(2022,8,10)  },
                new { Nome = "Switch HP 24 Portas",       NS = "DEMO-008", Marca = "HP",       Modelo = "1820-24G",      Valor = 950m,   Cat = catRede.Id,     Col = colaborador.Id,  Data = new DateTime(2022,5,15)  },
                new { Nome = "Cadeira Ergonomica",        NS = "DEMO-009", Marca = "Flexform", Modelo = "Pro 500",       Valor = 800m,   Cat = catMoveis.Id,   Col = colaborador2.Id, Data = new DateTime(2022,6,20)  },
                new { Nome = "Mesa de Escritorio",        NS = "DEMO-010", Marca = "Tokstok",  Modelo = "Stan 140cm",    Valor = 650m,   Cat = catMoveis.Id,   Col = colaborador2.Id, Data = new DateTime(2021,3,10)  },
            };

            foreach (var a in defAtivos)
            {
                if (!context.Ativos.Any(x => x.NumeroSerie == a.NS && x.EmpresaId == empresa.Id))
                {
                    context.Ativos.Add(new SistemaAtivos.Models.Ativo
                    {
                        Nome          = a.Nome,
                        NumeroSerie   = a.NS,
                        Marca         = a.Marca,
                        Modelo        = a.Modelo,
                        Status        = SistemaAtivos.Models.StatusAtivo.Ativo,
                        DataAquisicao = a.Data,
                        Valor         = a.Valor,
                        CategoriaId   = a.Cat,
                        ColaboradorId = a.Col,
                        EmpresaId     = empresa.Id
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
