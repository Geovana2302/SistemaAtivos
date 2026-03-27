using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public enum TipoUsuario { Admin, Cliente }

    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Senha { get; set; }

        public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente;

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }
    }
}
