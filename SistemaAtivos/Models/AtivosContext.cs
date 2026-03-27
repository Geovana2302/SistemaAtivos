using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace SistemaAtivos.Models
{
    public class AtivosContext : DbContext
    {
        public AtivosContext() : base("name=AtivosContext")
        {
            // Desabilita LazyLoadingEnabled para evitar problemas
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;

            // Usa MigrateDatabaseToLatestVersion para aplicar migrations automaticamente
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AtivosContext, Migrations.Configuration>(true));
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<Manutencao> Manutencoes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ativo>()
                .HasOptional(a => a.Colaborador)
                .WithMany(c => c.Ativos)
                .HasForeignKey(a => a.ColaboradorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Ativo>()
                .HasOptional(a => a.Categoria)
                .WithMany(c => c.Ativos)
                .HasForeignKey(a => a.CategoriaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Manutencao>()
                .HasOptional(m => m.Ativo)
                .WithMany(a => a.Manutencoes)
                .HasForeignKey(m => m.AtivoId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
