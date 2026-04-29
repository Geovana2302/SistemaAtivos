using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAtivos.Models
{
    public enum TipoManutencao { Preventiva, Corretiva, Preditiva }

    // REQUISITO 2 - MOVIMENTACAO DE DADOS
    // Enum que define a maquina de estados do fluxo de manutencao.
    // O chamado percorre: Aberto -> EmOrcamento -> AguardandoAprovacao -> Aprovado -> EmConserto -> Concluido
    // Este fluxo envolve o relacionamento entre multiplas entidades (Manutencao, Ativo, Empresa)
    public enum StatusManutencao 
    { 
        Aberto,                // Chamado recebido
        EmOrcamento,           // Tecnico analisando e preparando orcamento
        AguardandoAprovacao,   // Orcamento enviado ao cliente para aprovacao
        Aprovado,              // Cliente aprovou o orcamento
        EmConserto,            // Tecnico executando o servico
        Concluido,             // Servico finalizado
        Cancelado              // Chamado cancelado
    }

    // =====================================================================
    // REQUISITO 2 - MOVIMENTACAO DE DADOS (Relacionamento entre entidades)
    // Classe Manutencao: entidade central do fluxo de movimentacao.
    // Relaciona-se com Ativo (equipamento) e Empresa (cliente).
    //
    // REQUISITO 3 - VALIDACAO DE DADOS (Data Annotations)
    // Utiliza Data Annotations para garantir que apenas dados validos
    // sejam processados durante o fluxo de movimentacao.
    // =====================================================================
    public class Manutencao
    {
        public int Id { get; set; }

        // REQUISITO 3 - [Required] garante que a data e obrigatoria
        // [DataType(DataType.Date)] define o formato esperado
        [Required]
        [Display(Name = "Data de Abertura")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        public TipoManutencao Tipo { get; set; } = TipoManutencao.Corretiva;

        // REQUISITO 3 - [Required] com mensagem de erro customizada
        // [StringLength(500)] limita o tamanho maximo do texto
        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(500)]
        [Display(Name = "Descrição do Problema")]
        public string Descricao { get; set; }

        // REQUISITO 3 - [DataType(DataType.Currency)] valida formato monetario
        [DataType(DataType.Currency)]
        [Display(Name = "Custo Real")]
        public decimal? Custo { get; set; }

        // REQUISITO 3 - [StringLength(200)] limita o tamanho do campo
        [StringLength(200)]
        [Display(Name = "Fornecedor / Técnico")]
        public string Fornecedor { get; set; }

        [Display(Name = "Próxima Manutenção")]
        [DataType(DataType.Date)]
        public DateTime? ProximaManutencao { get; set; }

        // REQUISITO 2 - MOVIMENTACAO DE DADOS (Relacionamento entre entidades)
        // Chave estrangeira que vincula a Manutencao ao Ativo (equipamento)
        public int? AtivoId { get; set; }

        [ForeignKey("AtivoId")]
        public virtual Ativo Ativo { get; set; }

        // REQUISITO 2 - Chave estrangeira que vincula a Manutencao a Empresa (cliente)
        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        // REQUISITO 2 - Campo que controla o estado atual do fluxo de movimentacao
        // A maquina de estados e gerenciada pelos Controllers (MoverStatus, Aprovar, Orcamento)
        public StatusManutencao Status { get; set; } = StatusManutencao.Aberto;

        [Display(Name = "Valor do Orçamento")]
        [DataType(DataType.Currency)]
        public decimal? ValorOrcamento { get; set; }

        [StringLength(1000)]
        [Display(Name = "Diagnóstico Técnico")]
        public string DiagnosticoTecnico { get; set; }

        [StringLength(1000)]
        [Display(Name = "Observação Interna (Admin)")]
        public string ObservacaoInterna { get; set; }

        [Display(Name = "Data da Aprovação")]
        [DataType(DataType.DateTime)]
        public DateTime? DataAprovacao { get; set; }

        [Display(Name = "Data de Conclusão")]
        [DataType(DataType.DateTime)]
        public DateTime? DataConclusao { get; set; }

        [Display(Name = "Aprovado por")]
        [StringLength(100)]
        public string AprovadoPor { get; set; }

        // Propriedade calculada para tempo de espera
        [NotMapped]
        public int DiasEmAberto
        {
            get
            {
                if (Status == StatusManutencao.Concluido && DataConclusao.HasValue)
                    return (DataConclusao.Value - Data).Days;
                return (DateTime.Now - Data).Days;
            }
        }

        [NotMapped]
        public string TempoEsperaFormatado
        {
            get
            {
                var dias = DiasEmAberto;
                if (dias == 0) return "Hoje";
                if (dias == 1) return "1 dia";
                return $"{dias} dias";
            }
        }
    }
}
