using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public enum StatusAtivo { Ativo, Inativo, EmManutencao }

    // =====================================================================
    // REQUISITO 2 - MOVIMENTACAO DE DADOS (Relacionamento entre entidades)
    // Classe Ativo: representa um equipamento/patrimonio.
    // Relaciona-se com: Categoria, Colaborador (responsavel), Empresa e Manutencoes.
    //
    // REQUISITO 3 - VALIDACAO DE DADOS (Data Annotations)
    // Utiliza [Required], [StringLength], [DataType] para validar os campos.
    // =====================================================================
    public class Ativo
    {
        public int Id { get; set; }

        // REQUISITO 3 - [Required] com mensagem customizada garante nome obrigatorio
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

        // REQUISITO 2 - Relacionamento: Ativo pertence a uma Categoria
        public int? CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        // REQUISITO 2 - Relacionamento: Ativo tem um Colaborador responsavel
        public int? ColaboradorId { get; set; }

        [ForeignKey("ColaboradorId")]
        [Display(Name = "Responsável")]
        public virtual Colaborador Colaborador { get; set; }

        // REQUISITO 2 - Relacionamento: Ativo pertence a uma Empresa
        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        // REQUISITO 2 - Relacionamento 1:N - Um Ativo pode ter varias Manutencoes
        public virtual ICollection<Manutencao> Manutencoes { get; set; } = new List<Manutencao>();
    }
}
