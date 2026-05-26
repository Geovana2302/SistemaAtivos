using Microsoft.EntityFrameworkCore;
using SistemaAtivos.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AtivosContext>(options =>
    options.UseSqlite("Data Source=ativos.db"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ── Seed ─────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AtivosContext>();
    db.Database.EnsureCreated();

    // Admin principal
    var admin = db.Usuarios.FirstOrDefault(u => u.Email == "admin@sistema.com");
    if (admin == null)
    {
        db.Usuarios.Add(new Usuario
        {
            Nome         = "Administrador",
            Email        = "admin@sistema.com",
            Senha        = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Perfil       = Perfil.Admin,
            IsSuperAdmin = true
        });
        db.SaveChanges();
    }
    else
    {
        if (admin.Senha == null || !admin.Senha.StartsWith("$2"))
            admin.Senha = BCrypt.Net.BCrypt.HashPassword("admin123");
        admin.IsSuperAdmin = true;
        db.SaveChanges();
    }

    // Empresa de demonstração
    var empresa = db.Empresas.FirstOrDefault(e => e.Nome == "Empresa Demo");
    if (empresa == null)
    {
        empresa = new Empresa { Nome = "Empresa Demo", Cor = "#534AB7" };
        db.Empresas.Add(empresa);
        db.SaveChanges();
    }

    // Gestor
    if (!db.Usuarios.Any(u => u.Email == "gestor@demo.com"))
    {
        db.Usuarios.Add(new Usuario
        {
            Nome      = "Gestor Demo",
            Email     = "gestor@demo.com",
            Senha     = BCrypt.Net.BCrypt.HashPassword("gestor123"),
            Perfil    = Perfil.Gestor,
            EmpresaId = empresa.Id
        });
        db.SaveChanges();
    }

    // Colaboradores
    var col1 = db.Colaboradores.FirstOrDefault(c => c.Email == "joao@demo.com" && c.EmpresaId == empresa.Id);
    if (col1 == null)
    {
        col1 = new Colaborador { Nome = "João Silva", Cargo = "Analista de TI", Email = "joao@demo.com", EmpresaId = empresa.Id };
        db.Colaboradores.Add(col1);
        db.SaveChanges();
    }

    var col2 = db.Colaboradores.FirstOrDefault(c => c.Email == "maria@demo.com" && c.EmpresaId == empresa.Id);
    if (col2 == null)
    {
        col2 = new Colaborador { Nome = "Maria Souza", Cargo = "Gestora Administrativa", Email = "maria@demo.com", EmpresaId = empresa.Id };
        db.Colaboradores.Add(col2);
        db.SaveChanges();
    }

    // Categorias
    var defCats = new[]
    {
        new { Nome = "Notebooks",    Descricao = "Computadores portáteis",            Icone = "bi-laptop"   },
        new { Nome = "Monitores",    Descricao = "Telas e displays",                  Icone = "bi-display"  },
        new { Nome = "Periféricos",  Descricao = "Teclados, mouses e headsets",       Icone = "bi-keyboard" },
        new { Nome = "Rede",         Descricao = "Roteadores, switches e cabeamento", Icone = "bi-wifi"     },
        new { Nome = "Móveis",       Descricao = "Mesas, cadeiras e armários",        Icone = "bi-lamp"     },
    };

    foreach (var def in defCats)
    {
        var cat = db.Categorias.FirstOrDefault(c => c.EmpresaId == empresa.Id && c.Nome == def.Nome);
        if (cat == null)
            db.Categorias.Add(new Categoria { Nome = def.Nome, Descricao = def.Descricao, Icone = def.Icone, EmpresaId = empresa.Id });
        else
        { cat.Icone = def.Icone; cat.Descricao = def.Descricao; }
    }
    db.SaveChanges();

    var catNotebook = db.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Notebooks");
    var catMonitor  = db.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Monitores");
    var catPerif    = db.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Periféricos");
    var catRede     = db.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Rede");
    var catMoveis   = db.Categorias.First(c => c.EmpresaId == empresa.Id && c.Nome == "Móveis");

    // Ativos
    var defAtivos = new (string Nome, string NS, string Marca, string Modelo, decimal Valor, int Cat, int Col, DateTime Data)[]
    {
        ("Notebook Dell Inspiron",   "DEMO-001", "Dell",     "Inspiron 15",  3500m, catNotebook.Id, col1.Id, new DateTime(2023,1,10) ),
        ("Notebook Lenovo ThinkPad", "DEMO-002", "Lenovo",   "ThinkPad E14", 4200m, catNotebook.Id, col2.Id, new DateTime(2023,6,15) ),
        ("Monitor LG 24\"",          "DEMO-003", "LG",       "24MP60G",      1200m, catMonitor.Id,  col1.Id, new DateTime(2023,3,5)  ),
        ("Monitor Samsung 27\"",     "DEMO-004", "Samsung",  "F27T450",      1800m, catMonitor.Id,  col2.Id, new DateTime(2022,11,20)),
        ("Teclado Logitech G413",    "DEMO-005", "Logitech", "G413",          450m, catPerif.Id,    col1.Id, new DateTime(2023,2,1)  ),
        ("Mouse Logitech MX Master", "DEMO-006", "Logitech", "MX Master 3",   380m, catPerif.Id,    col2.Id, new DateTime(2023,2,1)  ),
        ("Roteador TP-Link AX73",    "DEMO-007", "TP-Link",  "Archer AX73",   600m, catRede.Id,     col1.Id, new DateTime(2022,8,10) ),
        ("Switch HP 24 Portas",      "DEMO-008", "HP",       "1820-24G",      950m, catRede.Id,     col1.Id, new DateTime(2022,5,15) ),
        ("Cadeira Ergonômica",       "DEMO-009", "Flexform", "Pro 500",       800m, catMoveis.Id,   col2.Id, new DateTime(2022,6,20) ),
        ("Mesa de Escritório",       "DEMO-010", "Tokstok",  "Stan 140cm",    650m, catMoveis.Id,   col2.Id, new DateTime(2021,3,10) ),
    };

    foreach (var a in defAtivos)
    {
        if (!db.Ativos.Any(x => x.NumeroSerie == a.NS && x.EmpresaId == empresa.Id))
        {
            db.Ativos.Add(new Ativo
            {
                Nome          = a.Nome,
                NumeroSerie   = a.NS,
                Marca         = a.Marca,
                Modelo        = a.Modelo,
                Status        = StatusAtivo.Ativo,
                DataAquisicao = a.Data,
                Valor         = a.Valor,
                CategoriaId   = a.Cat,
                ColaboradorId = a.Col,
                EmpresaId     = empresa.Id
            });
        }
    }
    db.SaveChanges();

    Console.WriteLine("===========================================");
    Console.WriteLine("  Admin  : admin@sistema.com / admin123");
    Console.WriteLine("  Gestor : gestor@demo.com  / gestor123");
    Console.WriteLine("===========================================");
}

app.Run();
