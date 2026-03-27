using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public enum StatusAtivo { Ativo, Inativo, EmManutencao }

    public class Ativo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(100)]
        [Display(Name = "Número de Série")]
        public string NumeroSerie { get; set; }

        [StringLength(100)]
        public string Marca { get; set; }

        [StringLength(100)]
        public string Modelo { get; set; }

        [Display(Name = "Data de Aquisição")]
        [DataType(DataType.Date)]
        public DateTime? DataAquisicao { get; set; }

        [Display(Name = "Valor")]
        [DataType(DataType.Currency)]
        public decimal? Valor { get; set; }

        public StatusAtivo Status { get; set; } = StatusAtivo.Ativo;

        [StringLength(500)]
        public string Observacoes { get; set; }

        public int? CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        public int? ColaboradorId { get; set; }

        [ForeignKey("ColaboradorId")]
        [Display(Name = "Responsável")]
        public virtual Colaborador Colaborador { get; set; }

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        public virtual ICollection<Manutencao> Manutencoes { get; set; } = new List<Manutencao>();
    }
}
