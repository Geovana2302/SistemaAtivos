namespace SistemaAtivos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AdicionarMultiEmpresa : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Empresas",
                c => new
                {
                    Id   = c.Int(nullable: false, identity: true),
                    Nome = c.String(nullable: false, maxLength: 100),
                    Cor  = c.String(maxLength: 7),
                    Logo = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Usuarios",
                c => new
                {
                    Id        = c.Int(nullable: false, identity: true),
                    Nome      = c.String(nullable: false, maxLength: 100),
                    Email     = c.String(nullable: false, maxLength: 200),
                    Senha     = c.String(nullable: false, maxLength: 200),
                    Tipo      = c.Int(nullable: false),
                    EmpresaId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);

            CreateTable(
                "dbo.Categorias",
                c => new
                {
                    Id        = c.Int(nullable: false, identity: true),
                    Nome      = c.String(nullable: false, maxLength: 100),
                    Descricao = c.String(maxLength: 300),
                    EmpresaId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);

            CreateTable(
                "dbo.Colaboradores",
                c => new
                {
                    Id        = c.Int(nullable: false, identity: true),
                    Nome      = c.String(nullable: false, maxLength: 100),
                    Email     = c.String(maxLength: 200),
                    Telefone  = c.String(maxLength: 20),
                    Cargo     = c.String(maxLength: 100),
                    EmpresaId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);

            CreateTable(
                "dbo.Ativos",
                c => new
                {
                    Id             = c.Int(nullable: false, identity: true),
                    Nome           = c.String(nullable: false, maxLength: 100),
                    NumeroSerie    = c.String(maxLength: 100),
                    Marca          = c.String(maxLength: 100),
                    Modelo         = c.String(maxLength: 100),
                    DataAquisicao  = c.DateTime(),
                    Valor          = c.Decimal(precision: 18, scale: 2),
                    Status         = c.Int(nullable: false),
                    Observacoes    = c.String(maxLength: 500),
                    CategoriaId    = c.Int(),
                    ColaboradorId  = c.Int(),
                    EmpresaId      = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categorias",   t => t.CategoriaId)
                .ForeignKey("dbo.Colaboradores", t => t.ColaboradorId)
                .ForeignKey("dbo.Empresas",      t => t.EmpresaId)
                .Index(t => t.CategoriaId)
                .Index(t => t.ColaboradorId)
                .Index(t => t.EmpresaId);

            CreateTable(
                "dbo.Manutencoes",
                c => new
                {
                    Id                 = c.Int(nullable: false, identity: true),
                    Data               = c.DateTime(nullable: false),
                    Tipo               = c.Int(nullable: false),
                    Descricao          = c.String(nullable: false, maxLength: 500),
                    Custo              = c.Decimal(precision: 18, scale: 2),
                    Fornecedor         = c.String(maxLength: 200),
                    ProximaManutencao  = c.DateTime(),
                    AtivoId            = c.Int(),
                    EmpresaId          = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Ativos",   t => t.AtivoId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.AtivoId)
                .Index(t => t.EmpresaId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Manutencoes",  "EmpresaId",     "dbo.Empresas");
            DropForeignKey("dbo.Manutencoes",  "AtivoId",       "dbo.Ativos");
            DropForeignKey("dbo.Ativos",       "EmpresaId",     "dbo.Empresas");
            DropForeignKey("dbo.Ativos",       "ColaboradorId", "dbo.Colaboradores");
            DropForeignKey("dbo.Ativos",       "CategoriaId",   "dbo.Categorias");
            DropForeignKey("dbo.Colaboradores","EmpresaId",     "dbo.Empresas");
            DropForeignKey("dbo.Categorias",   "EmpresaId",     "dbo.Empresas");
            DropForeignKey("dbo.Usuarios",     "EmpresaId",     "dbo.Empresas");
            DropIndex("dbo.Manutencoes",  new[] { "EmpresaId" });
            DropIndex("dbo.Manutencoes",  new[] { "AtivoId" });
            DropIndex("dbo.Ativos",       new[] { "EmpresaId" });
            DropIndex("dbo.Ativos",       new[] { "ColaboradorId" });
            DropIndex("dbo.Ativos",       new[] { "CategoriaId" });
            DropIndex("dbo.Colaboradores",new[] { "EmpresaId" });
            DropIndex("dbo.Categorias",   new[] { "EmpresaId" });
            DropIndex("dbo.Usuarios",     new[] { "EmpresaId" });
            DropTable("dbo.Manutencoes");
            DropTable("dbo.Ativos");
            DropTable("dbo.Colaboradores");
            DropTable("dbo.Categorias");
            DropTable("dbo.Usuarios");
            DropTable("dbo.Empresas");
        }
    }
}
