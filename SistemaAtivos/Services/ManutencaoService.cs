using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SistemaAtivos.Models;

namespace SistemaAtivos.Services
{
    public class ManutencaoService
    {
        private readonly AtivosContext _db;

        public ManutencaoService(AtivosContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Manutencao CriarChamado(Manutencao man)
        {
            if (man == null) throw new ArgumentNullException(nameof(man));
            man.Data = man.Data == default ? DateTime.Today : man.Data;
            man.Status = StatusManutencao.Aberto;
            _db.Manutencoes.Add(man);
            _db.SaveChanges();
            return man;
        }

        public bool AtualizarOrcamento(int manutencaoId, decimal valorOrcamento, string diagnostico, StatusManutencao novoStatus)
        {
            var man = _db.Manutencoes.FirstOrDefault(m => m.Id == manutencaoId);
            if (man == null) return false;
            man.ValorOrcamento = valorOrcamento;
            man.DiagnosticoTecnico = diagnostico;
            man.Status = novoStatus;
            _db.Entry(man).State = EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        public IEnumerable<Manutencao> ListarHistoricoPorAtivo(int ativoId, bool incluirEmpresaRestricao = true, int? empresaId = null, bool isAdmin = false)
        {
            var q = _db.Manutencoes.Include(m => m.Ativo).Where(m => m.AtivoId == ativoId).AsQueryable();
            if (!isAdmin && empresaId.HasValue)
                q = q.Where(m => m.EmpresaId == empresaId.Value);
            return q.OrderByDescending(m => m.Data).ToList();
        }

        public Manutencao GetById(int id)
        {
            return _db.Manutencoes.Include(m => m.Ativo).FirstOrDefault(m => m.Id == id);
        }

        public bool Excluir(int id)
        {
            var man = _db.Manutencoes.Find(id);
            if (man == null) return false;
            _db.Manutencoes.Remove(man);
            _db.SaveChanges();
            return true;
        }
    }
}
