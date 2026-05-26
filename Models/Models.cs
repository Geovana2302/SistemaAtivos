using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public enum StatusAtivo { Ativo, Inativo, EmManutencao }
    public enum Perfil { Admin, Gestor }

    // ── Validação customizada ────────────────────────────────────────────
    public class DataNaoFuturaAttribute : ValidationAttribute
    {
        public DataNaoFuturaAttribute() => ErrorMessage = "A data de aquisição não pode ser uma data futura.";
        public override bool IsValid(object? value) => value == null || (DateTime)value <= DateTime.Today;
    }

    // ── Empresa ──────────────────────────────────────────────────────────
    public class Empresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome da Empresa")]
        public string Nome { get; set; } = "";

        [StringLength(7)]
        public string Cor { get; set; } = "#534AB7";

        [StringLength(500)]
        public string? Logo { get; set; }

        public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();
        public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public virtual ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }

    // ── Categoria ────────────────────────────────────────────────────────
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Categoria")]
        public string Nome { get; set; } = "";

        [StringLength(300)]
        public string? Descricao { get; set; }

        [StringLength(100)]
        public string? Icone { get; set; }

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();
    }

    // ── Colaborador ──────────────────────────────────────────────────────
    public class Colaborador
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }

        [StringLength(15, MinimumLength = 14, ErrorMessage = "Telefone deve estar no formato (99) 9999-9999 ou (99) 99999-9999")]
        [RegularExpression(@"^\(\d{2}\)\s\d{4,5}-\d{4}$", ErrorMessage = "Telefone deve estar no formato (99) 9999-9999 ou (99) 99999-9999")]
        public string? Telefone { get; set; }

        [StringLength(100)]
        public string? Cargo { get; set; }

        public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();
    }

    // ── Ativo ────────────────────────────────────────────────────────────
    public class Ativo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [StringLength(100)]
        [Display(Name = "Número de Série")]
        public string? NumeroSerie { get; set; }

        [StringLength(100)]
        public string? Marca { get; set; }

        [StringLength(100)]
        public string? Modelo { get; set; }

        [Display(Name = "Data de Aquisição")]
        [DataType(DataType.Date)]
        [DataNaoFutura]
        public DateTime? DataAquisicao { get; set; }

        [Display(Name = "Valor")]
        [DataType(DataType.Currency)]
        [Range(0, 999999999.99, ErrorMessage = "Valor deve ser maior ou igual a zero.")]
        public decimal? Valor { get; set; }

        public StatusAtivo Status { get; set; } = StatusAtivo.Ativo;

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public int? CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        public int? ColaboradorId { get; set; }
        [ForeignKey("ColaboradorId")]
        [Display(Name = "Responsável")]
        public virtual Colaborador? Colaborador { get; set; }

        public int? EmpresaId { get; set; }
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }
    }

    // ── Usuario ──────────────────────────────────────────────────────────
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [StringLength(200)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = "";

        [StringLength(200)]
        public string? Senha { get; set; }

        [Display(Name = "Perfil")]
        public Perfil Perfil { get; set; } = Perfil.Gestor;

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        public bool IsSuperAdmin { get; set; } = false;
    }
}
