using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaAtivos.Models
{
    // REQUISITO 3 - VALIDACAO DE DADOS
    // Enum que define o perfil de acesso do usuario no sistema single-tenant.
    public enum Perfil { Admin, Gestor }

    // REQUISITO 2 - MOVIMENTACAO DE DADOS
    // Entidade central de autenticacao e controle de acesso.
    public class Usuario
    {
        public int Id { get; set; }

        // REQUISITO 3 - [Required] garante campo obrigatorio
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [StringLength(200)]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [StringLength(200)]
        public string Senha { get; set; }

        [Display(Name = "Perfil")]
        public Perfil Perfil { get; set; } = Perfil.Gestor;

        public int? EmpresaId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        public bool IsSuperAdmin { get; set; } = false;
    }
}
