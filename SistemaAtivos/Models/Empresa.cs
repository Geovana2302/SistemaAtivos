using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaAtivos.Models
{
    public class Empresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome da Empresa")]
        public string Nome { get; set; }

        [StringLength(7)]
        public string Cor { get; set; } = "#534AB7";

        [StringLength(500)]
        public string Logo { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();
        public virtual ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
        public virtual ICollection<Manutencao> Manutencoes { get; set; } = new List<Manutencao>();
    }
}
