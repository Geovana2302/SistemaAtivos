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

            // ── Empresa 2: TechNova Solucoes ───────────────────────────────
            var empresa2 = context.Empresas.FirstOrDefault(e => e.Nome == "TechNova Soluções");
            if (empresa2 == null)
            {
                empresa2 = new SistemaAtivos.Models.Empresa { Nome = "TechNova Soluções", Cor = "#0EA5E9" };
                context.Empresas.Add(empresa2);
                context.SaveChanges();
            }

            if (!context.Usuarios.Any(u => u.Email == "gestor@technova.com"))
            {
                context.Usuarios.Add(new SistemaAtivos.Models.Usuario
                {
                    Nome      = "Gestor TechNova",
                    Email     = "gestor@technova.com",
                    Senha     = BCrypt.Net.BCrypt.HashPassword("gestor123"),
                    Perfil    = SistemaAtivos.Models.Perfil.Gestor,
                    EmpresaId = empresa2.Id
                });
                context.SaveChanges();
            }

            var colTN1 = context.Colaboradores.FirstOrDefault(c => c.Email == "carlos@technova.com");
            if (colTN1 == null)
            {
                colTN1 = new SistemaAtivos.Models.Colaborador { Nome = "Carlos Mendes", Cargo = "Desenvolvedor Sênior", Email = "carlos@technova.com", EmpresaId = empresa2.Id };
                context.Colaboradores.Add(colTN1);
                context.SaveChanges();
            }

            var colTN2 = context.Colaboradores.FirstOrDefault(c => c.Email == "ana@technova.com");
            if (colTN2 == null)
            {
                colTN2 = new SistemaAtivos.Models.Colaborador { Nome = "Ana Beatriz Lima", Cargo = "UX Designer", Email = "ana@technova.com", EmpresaId = empresa2.Id };
                context.Colaboradores.Add(colTN2);
                context.SaveChanges();
            }

            var colTN3 = context.Colaboradores.FirstOrDefault(c => c.Email == "rafael@technova.com");
            if (colTN3 == null)
            {
                colTN3 = new SistemaAtivos.Models.Colaborador { Nome = "Rafael Costa", Cargo = "DevOps Engineer", Email = "rafael@technova.com", EmpresaId = empresa2.Id };
                context.Colaboradores.Add(colTN3);
                context.SaveChanges();
            }

            var defCatsTN = new[]
            {
                new { Nome = "Notebooks",    Descricao = "Computadores portáteis",           Icone = "bi-laptop"        },
                new { Nome = "Monitores",    Descricao = "Telas e displays",                 Icone = "bi-display"       },
                new { Nome = "Servidores",   Descricao = "Servidores e equipamentos rack",   Icone = "bi-server"        },
                new { Nome = "Periféricos",  Descricao = "Teclados, mouses e headsets",      Icone = "bi-keyboard"      },
                new { Nome = "Rede",         Descricao = "Roteadores, switches e cabeamento",Icone = "bi-wifi"          },
            };

            foreach (var def in defCatsTN)
            {
                if (context.Categorias.FirstOrDefault(c => c.EmpresaId == empresa2.Id && c.Nome == def.Nome) == null)
                    context.Categorias.Add(new SistemaAtivos.Models.Categoria { Nome = def.Nome, Descricao = def.Descricao, Icone = def.Icone, EmpresaId = empresa2.Id });
            }
            context.SaveChanges();

            var catTN_Note = context.Categorias.First(c => c.EmpresaId == empresa2.Id && c.Nome == "Notebooks");
            var catTN_Mon  = context.Categorias.First(c => c.EmpresaId == empresa2.Id && c.Nome == "Monitores");
            var catTN_Srv  = context.Categorias.First(c => c.EmpresaId == empresa2.Id && c.Nome == "Servidores");
            var catTN_Peri = context.Categorias.First(c => c.EmpresaId == empresa2.Id && c.Nome == "Periféricos");
            var catTN_Rede = context.Categorias.First(c => c.EmpresaId == empresa2.Id && c.Nome == "Rede");

            var ativosTN = new[]
            {
                // Ativos
                new { Nome = "MacBook Pro 14\"",          NS = "TN-001", Marca = "Apple",    Modelo = "MacBook Pro M3",    Valor = 14900m, Cat = catTN_Note.Id, Col = colTN1.Id,  Data = new DateTime(2024,1,15),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "MacBook Air 13\"",           NS = "TN-002", Marca = "Apple",    Modelo = "MacBook Air M2",    Valor = 10500m, Cat = catTN_Note.Id, Col = colTN2.Id,  Data = new DateTime(2024,3,10),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Dell XPS 15",                NS = "TN-003", Marca = "Dell",     Modelo = "XPS 15 9530",       Valor = 12800m, Cat = catTN_Note.Id, Col = colTN3.Id,  Data = new DateTime(2023,11,5),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Monitor LG UltraWide 34\"", NS = "TN-004", Marca = "LG",       Modelo = "34WP65C",           Valor = 2900m,  Cat = catTN_Mon.Id,  Col = colTN1.Id,  Data = new DateTime(2024,2,20),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Monitor Dell 27\" 4K",      NS = "TN-005", Marca = "Dell",     Modelo = "U2723QE",           Valor = 3400m,  Cat = catTN_Mon.Id,  Col = colTN2.Id,  Data = new DateTime(2023,8,18),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Switch Cisco SG350",        NS = "TN-006", Marca = "Cisco",    Modelo = "SG350-10",          Valor = 2100m,  Cat = catTN_Rede.Id, Col = colTN3.Id,  Data = new DateTime(2023,5,12),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Headset Sony WH-1000XM5",   NS = "TN-007", Marca = "Sony",     Modelo = "WH-1000XM5",        Valor = 1800m,  Cat = catTN_Peri.Id, Col = colTN2.Id,  Data = new DateTime(2024,4,8),   Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                // Em manutenção
                new { Nome = "Servidor Dell PowerEdge",   NS = "TN-008", Marca = "Dell",     Modelo = "PowerEdge R740",    Valor = 28000m, Cat = catTN_Srv.Id,  Col = colTN3.Id,  Data = new DateTime(2021,6,1),   Status = SistemaAtivos.Models.StatusAtivo.EmManutencao },
                new { Nome = "Notebook Lenovo ThinkPad",  NS = "TN-009", Marca = "Lenovo",   Modelo = "ThinkPad X1 Carbon",Valor = 9200m,  Cat = catTN_Note.Id, Col = colTN1.Id,  Data = new DateTime(2022,3,20),  Status = SistemaAtivos.Models.StatusAtivo.EmManutencao },
                // Inativos
                new { Nome = "MacBook Pro 13\" (2019)",   NS = "TN-010", Marca = "Apple",    Modelo = "MacBook Pro 2019",  Valor = 7000m,  Cat = catTN_Note.Id, Col = colTN2.Id,  Data = new DateTime(2019,9,10),  Status = SistemaAtivos.Models.StatusAtivo.Inativo      },
                new { Nome = "Monitor Dell 24\" Full HD", NS = "TN-011", Marca = "Dell",     Modelo = "P2419H",            Valor = 900m,   Cat = catTN_Mon.Id,  Col = colTN1.Id,  Data = new DateTime(2020,2,14),  Status = SistemaAtivos.Models.StatusAtivo.Inativo      },
                new { Nome = "Roteador Asus RT-AC68U",    NS = "TN-012", Marca = "Asus",     Modelo = "RT-AC68U",          Valor = 750m,   Cat = catTN_Rede.Id, Col = colTN3.Id,  Data = new DateTime(2020,7,30),  Status = SistemaAtivos.Models.StatusAtivo.Inativo      },
            };

            foreach (var a in ativosTN)
            {
                if (!context.Ativos.Any(x => x.NumeroSerie == a.NS && x.EmpresaId == empresa2.Id))
                {
                    context.Ativos.Add(new SistemaAtivos.Models.Ativo
                    {
                        Nome          = a.Nome,
                        NumeroSerie   = a.NS,
                        Marca         = a.Marca,
                        Modelo        = a.Modelo,
                        Status        = a.Status,
                        DataAquisicao = a.Data,
                        Valor         = a.Valor,
                        CategoriaId   = a.Cat,
                        ColaboradorId = a.Col,
                        EmpresaId     = empresa2.Id
                    });
                }
            }
            context.SaveChanges();

            // ── Empresa 3: Conecta RH ──────────────────────────────────────
            var empresa3 = context.Empresas.FirstOrDefault(e => e.Nome == "Conecta RH");
            if (empresa3 == null)
            {
                empresa3 = new SistemaAtivos.Models.Empresa { Nome = "Conecta RH", Cor = "#10B981" };
                context.Empresas.Add(empresa3);
                context.SaveChanges();
            }

            if (!context.Usuarios.Any(u => u.Email == "gestor@conectarh.com"))
            {
                context.Usuarios.Add(new SistemaAtivos.Models.Usuario
                {
                    Nome      = "Gestor Conecta RH",
                    Email     = "gestor@conectarh.com",
                    Senha     = BCrypt.Net.BCrypt.HashPassword("gestor123"),
                    Perfil    = SistemaAtivos.Models.Perfil.Gestor,
                    EmpresaId = empresa3.Id
                });
                context.SaveChanges();
            }

            var colCR1 = context.Colaboradores.FirstOrDefault(c => c.Email == "patricia@conectarh.com");
            if (colCR1 == null)
            {
                colCR1 = new SistemaAtivos.Models.Colaborador { Nome = "Patrícia Oliveira", Cargo = "Analista de RH", Email = "patricia@conectarh.com", EmpresaId = empresa3.Id };
                context.Colaboradores.Add(colCR1);
                context.SaveChanges();
            }

            var colCR2 = context.Colaboradores.FirstOrDefault(c => c.Email == "thiago@conectarh.com");
            if (colCR2 == null)
            {
                colCR2 = new SistemaAtivos.Models.Colaborador { Nome = "Thiago Fernandes", Cargo = "Coordenador de Recrutamento", Email = "thiago@conectarh.com", EmpresaId = empresa3.Id };
                context.Colaboradores.Add(colCR2);
                context.SaveChanges();
            }

            var colCR3 = context.Colaboradores.FirstOrDefault(c => c.Email == "julia@conectarh.com");
            if (colCR3 == null)
            {
                colCR3 = new SistemaAtivos.Models.Colaborador { Nome = "Júlia Ramos", Cargo = "Diretora de Pessoas", Email = "julia@conectarh.com", EmpresaId = empresa3.Id };
                context.Colaboradores.Add(colCR3);
                context.SaveChanges();
            }

            var defCatsCR = new[]
            {
                new { Nome = "Notebooks",    Descricao = "Computadores portáteis",           Icone = "bi-laptop"        },
                new { Nome = "Monitores",    Descricao = "Telas e displays",                 Icone = "bi-display"       },
                new { Nome = "Periféricos",  Descricao = "Teclados, mouses e headsets",      Icone = "bi-keyboard"      },
                new { Nome = "Móveis",       Descricao = "Mesas, cadeiras e armários",       Icone = "bi-lamp"          },
                new { Nome = "Telefonia",    Descricao = "Celulares e telefones IP",         Icone = "bi-phone"         },
            };

            foreach (var def in defCatsCR)
            {
                if (context.Categorias.FirstOrDefault(c => c.EmpresaId == empresa3.Id && c.Nome == def.Nome) == null)
                    context.Categorias.Add(new SistemaAtivos.Models.Categoria { Nome = def.Nome, Descricao = def.Descricao, Icone = def.Icone, EmpresaId = empresa3.Id });
            }
            context.SaveChanges();

            var catCR_Note = context.Categorias.First(c => c.EmpresaId == empresa3.Id && c.Nome == "Notebooks");
            var catCR_Mon  = context.Categorias.First(c => c.EmpresaId == empresa3.Id && c.Nome == "Monitores");
            var catCR_Peri = context.Categorias.First(c => c.EmpresaId == empresa3.Id && c.Nome == "Periféricos");
            var catCR_Mov  = context.Categorias.First(c => c.EmpresaId == empresa3.Id && c.Nome == "Móveis");
            var catCR_Tel  = context.Categorias.First(c => c.EmpresaId == empresa3.Id && c.Nome == "Telefonia");

            var ativosCR = new[]
            {
                // Ativos
                new { Nome = "Notebook HP EliteBook 840",  NS = "CR-001", Marca = "HP",        Modelo = "EliteBook 840 G10", Valor = 7800m,  Cat = catCR_Note.Id, Col = colCR1.Id,  Data = new DateTime(2024,2,5),   Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Notebook Asus VivoBook 15",  NS = "CR-002", Marca = "Asus",      Modelo = "VivoBook 15 X1502", Valor = 3900m,  Cat = catCR_Note.Id, Col = colCR2.Id,  Data = new DateTime(2023,10,22), Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Monitor Samsung 32\" Curvo", NS = "CR-003", Marca = "Samsung",   Modelo = "C32F391",           Valor = 2200m,  Cat = catCR_Mon.Id,  Col = colCR3.Id,  Data = new DateTime(2024,1,8),   Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Cadeira President Luxo",     NS = "CR-004", Marca = "Presidente",Modelo = "President V2",      Valor = 1400m,  Cat = catCR_Mov.Id,  Col = colCR3.Id,  Data = new DateTime(2023,4,17),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Mesa Escritório L 160cm",    NS = "CR-005", Marca = "Arquivo",   Modelo = "Mesa L Premium",    Valor = 980m,   Cat = catCR_Mov.Id,  Col = colCR2.Id,  Data = new DateTime(2023,4,17),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "iPhone 14 Pro",              NS = "CR-006", Marca = "Apple",     Modelo = "iPhone 14 Pro",     Valor = 6800m,  Cat = catCR_Tel.Id,  Col = colCR3.Id,  Data = new DateTime(2023,12,1),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                new { Nome = "Headset Jabra Evolve2 55",   NS = "CR-007", Marca = "Jabra",     Modelo = "Evolve2 55",        Valor = 2400m,  Cat = catCR_Peri.Id, Col = colCR1.Id,  Data = new DateTime(2024,3,25),  Status = SistemaAtivos.Models.StatusAtivo.Ativo        },
                // Em manutenção
                new { Nome = "Notebook Dell Vostro 3510",  NS = "CR-008", Marca = "Dell",      Modelo = "Vostro 3510",       Valor = 3200m,  Cat = catCR_Note.Id, Col = colCR2.Id,  Data = new DateTime(2022,5,11),  Status = SistemaAtivos.Models.StatusAtivo.EmManutencao },
                new { Nome = "Telefone IP Yealink T54W",   NS = "CR-009", Marca = "Yealink",   Modelo = "T54W",              Valor = 900m,   Cat = catCR_Tel.Id,  Col = colCR1.Id,  Data = new DateTime(2021,9,3),   Status = SistemaAtivos.Models.StatusAtivo.EmManutencao },
                // Inativos
                new { Nome = "Notebook Samsung Book E30",  NS = "CR-010", Marca = "Samsung",   Modelo = "Book E30",          Valor = 2100m,  Cat = catCR_Note.Id, Col = colCR1.Id,  Data = new DateTime(2020,6,14),  Status = SistemaAtivos.Models.StatusAtivo.Inativo      },
                new { Nome = "Monitor LG 22\" Full HD",    NS = "CR-011", Marca = "LG",        Modelo = "22MK430H",          Valor = 700m,   Cat = catCR_Mon.Id,  Col = colCR2.Id,  Data = new DateTime(2019,11,20), Status = SistemaAtivos.Models.StatusAtivo.Inativo      },
                new { Nome = "Samsung Galaxy S21",         NS = "CR-012", Marca = "Samsung",   Modelo = "Galaxy S21",        Valor = 3500m,  Cat = catCR_Tel.Id,  Col = colCR3.Id,  Data = new DateTime(2021,3,8),   Status = SistemaAtivos.Models.StatusAtivo.Inativo      },
            };

            foreach (var a in ativosCR)
            {
                if (!context.Ativos.Any(x => x.NumeroSerie == a.NS && x.EmpresaId == empresa3.Id))
                {
                    context.Ativos.Add(new SistemaAtivos.Models.Ativo
                    {
                        Nome          = a.Nome,
                        NumeroSerie   = a.NS,
                        Marca         = a.Marca,
                        Modelo        = a.Modelo,
                        Status        = a.Status,
                        DataAquisicao = a.Data,
                        Valor         = a.Valor,
                        CategoriaId   = a.Cat,
                        ColaboradorId = a.Col,
                        EmpresaId     = empresa3.Id
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
