using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public enum TipoManutencao { Preventiva, Corretiva, Preditiva }

    public class Manutencao
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        public TipoManutencao Tipo { get; set; } = TipoManutencao.Preventiva;

        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Custo { get; set; }

        [StringLength(200)]
        [Display(Name = "Fornecedor / Técnico")]
        public string Fornecedor { get; set; }

        [Display(Name = "Próxima Manutenção")]
        [DataType(DataType.Date)]
        public DateTime? ProximaManutencao { get; set; }

        public int? AtivoId { get; set; }

        [ForeignKey("AtivoId")]
        public virtual Ativo Ativo { get; set; }

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }
    }
}
