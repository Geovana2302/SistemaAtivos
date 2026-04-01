using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public class Colaborador
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [StringLength(15, MinimumLength = 14, ErrorMessage = "Telefone deve estar no formato (99) 9999-9999 ou (99) 99999-9999")]
        [RegularExpression(@"^\(\d{2}\)\s\d{4,5}-\d{4}$", ErrorMessage = "Telefone deve estar no formato (99) 9999-9999 ou (99) 99999-9999")]
        public string Telefone { get; set; }

        [StringLength(100)]
        public string Cargo { get; set; }

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();
    }
}
