using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class AtivoController : Controller
    {
        private readonly AtivosContext _db;

        public AtivoController(AtivosContext db) => _db = db;

        private bool IsAdmin() => HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");

        private IQueryable<Ativo> GetQuery()
        {
            var q = _db.Ativos
                .Include(a => a.Categoria)
                .Include(a => a.Colaborador)
                .Include(a => a.Empresa)
                .AsQueryable();

            if (!IsAdmin())
                q = q.Where(a => a.EmpresaId == GetEmpresaId());

            return q;
        }

        public IActionResult Index(int? empresaId, string? ordem)
        {
            var q = AplicarFiltroEOrdem(GetQuery().Where(a => a.Status == StatusAtivo.Ativo), empresaId, ordem);
            PopularViewBagLista(empresaId, ordem);
            return View(q.ToList());
        }

        public IActionResult Inativos(int? empresaId, string? ordem)
        {
            var q = AplicarFiltroEOrdem(GetQuery().Where(a => a.Status == StatusAtivo.Inativo), empresaId, ordem);
            PopularViewBagLista(empresaId, ordem);
            return View(q.ToList());
        }

        public IActionResult Detalhes(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return NotFound();
            return View(ativo);
        }

        public IActionResult Imprimir(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return NotFound();
            return View(ativo);
        }

        public IActionResult QrCode(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return NotFound();

            var url = Url.Action("Detalhes", "Ativo", new { id }, Request.Scheme)!;
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrData);
            return File(qrCode.GetGraphic(6), "image/png");
        }

        [HttpGet]
        public IActionResult Create(int? categoriaId, int? empresaId)
        {
            var ativo = new Ativo();
            if (categoriaId.HasValue) ativo.CategoriaId = categoriaId;
            if (empresaId.HasValue) ativo.EmpresaId = empresaId;
            else if (!IsAdmin()) ativo.EmpresaId = GetEmpresaId();
            PopularDropdowns(ativo);
            ViewBag.EmpresaPreSelecionada = empresaId.HasValue || !IsAdmin();
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ativo ativo)
        {
            if (!IsAdmin()) ativo.EmpresaId = GetEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Ativos.Add(ativo);
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Ativo cadastrado com sucesso.";
                    if (ativo.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = ativo.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao cadastrar ativo. Tente novamente."; }
            }

            PopularDropdowns(ativo);
            return View(ativo);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return NotFound();
            PopularDropdowns(ativo);
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Ativo ativo)
        {
            if (!IsAdmin()) ativo.EmpresaId = GetEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Entry(ativo).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Ativo atualizado.";
                    if (ativo.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = ativo.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao atualizar ativo. Tente novamente."; }
            }

            PopularDropdowns(ativo);
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null)
            {
                TempData["Erro"] = "Ativo não encontrado ou sem permissão.";
                return RedirectToAction("Index");
            }

            var empId = ativo.EmpresaId;
            try
            {
                _db.Ativos.Remove(ativo);
                _db.SaveChanges();
                TempData["Sucesso"] = "Ativo excluído com sucesso.";
            }
            catch { TempData["Erro"] = "Erro ao excluir ativo."; }

            if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }

        // ── helpers ──────────────────────────────────────────────────────

        private IQueryable<Ativo> AplicarFiltroEOrdem(IQueryable<Ativo> q, int? empresaId, string? ordem)
        {
            if (empresaId.HasValue) q = q.Where(a => a.EmpresaId == empresaId);
            return ordem switch
            {
                "antigo"  => q.OrderBy(a => a.Id),
                "empresa" => q.OrderBy(a => a.Empresa!.Nome).ThenByDescending(a => a.Id),
                _         => q.OrderByDescending(a => a.Id)
            };
        }

        private void PopularViewBagLista(int? empresaId, string? ordem)
        {
            ViewBag.Empresas     = _db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;
            ViewBag.OrdemAtual   = ordem ?? "recente";
        }

        private void PopularDropdowns(Ativo? ativo = null)
        {
            var empId = IsAdmin() ? ativo?.EmpresaId : GetEmpresaId();

            var cats = empId.HasValue
                ? _db.Categorias.Where(c => c.EmpresaId == empId).ToList()
                : _db.Categorias.ToList();

            var cols = empId.HasValue
                ? _db.Colaboradores.Where(c => c.EmpresaId == empId).ToList()
                : _db.Colaboradores.ToList();

            ViewBag.Categorias    = new SelectList(cats, "Id", "Nome", ativo?.CategoriaId);
            ViewBag.Colaboradores = new SelectList(cols, "Id", "Nome", ativo?.ColaboradorId);
            ViewBag.EmpresaSelectList = IsAdmin()
                ? new SelectList(_db.Empresas, "Id", "Nome", ativo?.EmpresaId)
                : new SelectList(_db.Empresas.Where(e => e.Id == empId).ToList(), "Id", "Nome", empId);
        }
    }
}
