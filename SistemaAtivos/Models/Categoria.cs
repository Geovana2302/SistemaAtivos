using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Categoria")]
        public string Nome { get; set; }

        [StringLength(300)]
        public string Descricao { get; set; }

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();
    }
}
