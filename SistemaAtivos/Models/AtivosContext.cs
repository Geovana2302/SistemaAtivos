using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace SistemaAtivos.Models
{
    public class AtivosContext : DbContext
    {
        public AtivosContext() : base("name=AtivosContext")
        {
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AtivosContext, Migrations.Configuration>(true));
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<Empresa> Empresas { get; set; }

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

            modelBuilder.Entity<Ativo>()
                .HasOptional(a => a.Empresa)
                .WithMany(e => e.Ativos)
                .HasForeignKey(a => a.EmpresaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Categoria>()
                .HasOptional(c => c.Empresa)
                .WithMany(e => e.Categorias)
                .HasForeignKey(c => c.EmpresaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Colaborador>()
                .HasOptional(c => c.Empresa)
                .WithMany(e => e.Colaboradores)
                .HasForeignKey(c => c.EmpresaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .HasOptional(u => u.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmpresaId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
