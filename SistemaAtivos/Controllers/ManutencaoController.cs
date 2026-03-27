using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class ManutencaoController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

        private IQueryable<Manutencao> GetQuery()
        {
            var q = db.Manutencoes
                .Include(m => m.Ativo)
                .Include(m => m.Empresa)
                .AsQueryable();
            if (!IsAdmin())
                q = q.Where(m => m.EmpresaId == GetEmpresaId());
            return q;
        }

        public ActionResult Index()
        {
            return View(GetQuery().OrderByDescending(m => m.Data).ToList());
        }

        [HttpGet]
        public ActionResult Create(int? ativoId)
        {
            PopularDropdowns(ativoId: ativoId);
            var man = new Manutencao { AtivoId = ativoId };
            if (!IsAdmin()) man.EmpresaId = GetEmpresaId();
            return View(man);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Manutencao manutencao)
        {
            if (!IsAdmin()) manutencao.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                db.Manutencoes.Add(manutencao);
                db.SaveChanges();
                TempData["Sucesso"] = "Manutenção registrada.";
                return RedirectToAction("Index");
            }
            PopularDropdowns(manutencao);
            return View(manutencao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var man = GetQuery().FirstOrDefault(m => m.Id == id);
            if (man == null) return HttpNotFound();
            db.Manutencoes.Remove(man);
            db.SaveChanges();
            TempData["Sucesso"] = "Manutenção excluída.";
            return RedirectToAction("Index");
        }

        private void PopularDropdowns(Manutencao man = null, int? ativoId = null)
        {
            var ativos = IsAdmin()
                ? db.Ativos.ToList()
                : db.Ativos.Where(a => a.EmpresaId == GetEmpresaId()).ToList();
            ViewBag.AtivoId = new SelectList(ativos, "Id", "Nome", man?.AtivoId ?? ativoId);

            if (IsAdmin())
                ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", man?.EmpresaId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
