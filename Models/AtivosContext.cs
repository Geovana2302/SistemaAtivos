using Microsoft.EntityFrameworkCore;

namespace SistemaAtivos.Models
{
    public class AtivosContext : DbContext
    {
        public AtivosContext(DbContextOptions<AtivosContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Ativo> Ativos => Set<Ativo>();
        public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
        public DbSet<Empresa> Empresas => Set<Empresa>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ativo>()
                .HasOne(a => a.Colaborador)
                .WithMany(c => c.Ativos)
                .HasForeignKey(a => a.ColaboradorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ativo>()
                .HasOne(a => a.Categoria)
                .WithMany(c => c.Ativos)
                .HasForeignKey(a => a.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ativo>()
                .HasOne(a => a.Empresa)
                .WithMany(e => e.Ativos)
                .HasForeignKey(a => a.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Categorias)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Colaboradores)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
