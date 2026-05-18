$dir = "C:\Users\geova\OneDrive\Área de Trabalho\SistemaAtivos\SistemaAtivos\Views\Usuario"

# CREATE.CSHTML
$create = @'
@model SistemaAtivos.Models.Usuario
@{
    ViewBag.Title = "Novo Usuario";
    var isAdmin = Session["UsuarioTipo"] != null && Session["UsuarioTipo"].ToString() == "Admin";
}

<div class="page-header">
    <h4 class="page-title">Novo Usuario</h4>
</div>

<div class="form-card">
    @using (Html.BeginForm())
    {
        @Html.AntiForgeryToken()
        @Html.ValidationSummary(true, "Por favor, corrija os campos indicados.", new { @class = "alert-custom alert-danger-custom" })

        <div class="form-row-2">
            <div class="field-group">
                @Html.LabelFor(m => m.Nome, new { @class = "field-label" })
                @Html.TextBoxFor(m => m.Nome, new { @class = "field-input", placeholder = "Nome completo" })
                @Html.ValidationMessageFor(m => m.Nome, "", new { @class = "field-error" })
            </div>
            <div class="field-group">
                @Html.LabelFor(m => m.Email, new { @class = "field-label" })
                @Html.TextBoxFor(m => m.Email, new { @class = "field-input", type = "email", placeholder = "email@empresa.com" })
                @Html.ValidationMessageFor(m => m.Email, "", new { @class = "field-error" })
            </div>
        </div>

        <div class="form-row-2">
            <div class="field-group">
                <label class="field-label">Senha</label>
                <input type="password" name="senha" class="field-input" placeholder="Minimo 6 caracteres" />
                <span class="field-error">@Html.ValidationMessage("senha")</span>
            </div>
            if (isAdmin)
            {
                <div class="field-group">
                    <label class="field-label">Perfil</label>
                    @Html.DropDownList("Perfil", (SelectList)ViewBag.Perfis, new { @class = "field-input" })
                </div>
            }
        </div>

        if (isAdmin)
        {
            <div class="field-group">
                <label class="field-label">Empresa</label>
                @Html.DropDownList("EmpresaId", (SelectList)ViewBag.EmpresaSelectList, "-- Nenhuma (Admin/Tecnico) --", new { @class = "field-input" })
            </div>
        }
        else
        {
            @Html.HiddenFor(m => m.EmpresaId)
        }

        <div class="form-actions">
            if (Model.EmpresaId.HasValue)
            {
                @Html.ActionLink("Cancelar", "Empresa", "Admin", new { id = Model.EmpresaId }, new { @class = "btn-cancel" })
            }
            else
            {
                @Html.ActionLink("Cancelar", "Index", null, new { @class = "btn-cancel" })
            }
            <button type="submit" class="btn-purple">Cadastrar Usuario</button>
        </div>
    }
</div>
'@

# EDIT.CSHTML
$edit = @'
@model SistemaAtivos.Models.Usuario
@{
    ViewBag.Title = "Editar Usuario";
    var isAdmin = Session["UsuarioTipo"] != null && Session["UsuarioTipo"].ToString() == "Admin";
}

<div class="page-header">
    <h4 class="page-title">Editar Usuario</h4>
</div>

<div class="form-card">
    @using (Html.BeginForm("Edit", "Usuario", new { id = Model.Id }, FormMethod.Post))
    {
        @Html.AntiForgeryToken()
        @Html.ValidationSummary(true, "Por favor, corrija os campos indicados.", new { @class = "alert-custom alert-danger-custom" })

        <div class="form-row-2">
            <div class="field-group">
                @Html.LabelFor(m => m.Nome, new { @class = "field-label" })
                <input type="text" name="Nome" value="@Model.Nome" class="field-input" placeholder="Nome completo" required />
            </div>
            <div class="field-group">
                @Html.LabelFor(m => m.Email, new { @class = "field-label" })
                <input type="email" name="Email" value="@Model.Email" class="field-input" placeholder="email@empresa.com" required />
                @Html.ValidationMessage("Email", "", new { @class = "field-error" })
            </div>
        </div>

        <div class="form-row-2">
            <div class="field-group">
                <label class="field-label">Nova Senha (deixe em branco para manter)</label>
                <input type="password" name="senha" class="field-input" placeholder="Nova senha" />
            </div>
            if (isAdmin)
            {
                <div class="field-group">
                    <label class="field-label">Perfil</label>
                    <select name="Perfil" class="field-input">
                        @foreach (var p in Enum.GetValues(typeof(SistemaAtivos.Models.Perfil)).Cast<SistemaAtivos.Models.Perfil>())
                        {
                            <option value="@p" @(Model.Perfil == p ? "selected" : "")>@p</option>
                        }
                    </select>
                </div>
            }
        </div>

        if (isAdmin)
        {
            <div class="field-group">
                <label class="field-label">Empresa</label>
                @Html.DropDownList("EmpresaId", (SelectList)ViewBag.EmpresaSelectList, "-- Nenhuma (Admin/Tecnico) --", new { @class = "field-input" })
            </div>
        }

        <div class="field-group" style="margin-top:1rem;border-top:1px solid #eee;padding-top:1rem">
            <label class="field-label">Senha do Administrador (obrigatoria para salvar)</label>
            <input type="password" name="senhaAdmin" class="field-input" placeholder="Sua senha de administrador" required />
            @Html.ValidationMessage("senhaAdmin", "", new { @class = "field-error" })
        </div>

        <div class="form-actions">
            if (Model.EmpresaId.HasValue)
            {
                @Html.ActionLink("Cancelar", "Empresa", "Admin", new { id = Model.EmpresaId }, new { @class = "btn-cancel" })
            }
            else
            {
                @Html.ActionLink("Cancelar", "Index", "Usuario", null, new { @class = "btn-cancel" })
            }
            <button type="submit" class="btn-purple">Salvar Alteracoes</button>
        </div>
    }
</div>
'@

# INDEX.CSHTML
$index = @'
@model IEnumerable<SistemaAtivos.Models.Usuario>
@{
    ViewBag.Title = "Usuarios";
    var isAdmin = Session["UsuarioTipo"]?.ToString() == "Admin";
    var usuarioLogadoId = (int)Session["UsuarioId"];
}

<div class="page-header">
    <h4 class="page-title">Usuarios</h4>
    @Html.ActionLink("+ Novo Usuario", "Create", null, new { @class = "btn-purple" })
</div>

@if (isAdmin && ViewBag.Empresas != null)
{
    <div style="margin-bottom:1rem;display:flex;gap:8px;flex-wrap:wrap">
        @Html.ActionLink("Todos", "Index", new { empresaId = (int?)null }, new { @class = ViewBag.EmpresaFiltro == null ? "btn-purple" : "btn-outline-sm" })
        @foreach (var emp in (IEnumerable<SistemaAtivos.Models.Empresa>)ViewBag.Empresas)
        {
            @Html.ActionLink(emp.Nome, "Index", new { empresaId = emp.Id }, new { @class = (int?)ViewBag.EmpresaFiltro == emp.Id ? "btn-purple" : "btn-outline-sm" })
        }
    </div>
}

@if (!Model.Any())
{
    <div class="empty-state">
        <p>Nenhum usuario cadastrado ainda.</p>
        @Html.ActionLink("Cadastrar primeiro usuario", "Create", null, new { @class = "btn-purple" })
    </div>
}
else
{
    <div class="table-responsive">
        <table class="table-custom">
            <thead>
                <tr>
                    <th>Nome</th>
                    <th>E-mail</th>
                    <th>Perfil</th>
                    @if (isAdmin) { <th>Empresa</th> }
                    <th style="text-align:right">Acoes</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var u in Model)
                {
                    <tr>
                        <td>@u.Nome @if (u.Id == usuarioLogadoId) { <span class="acc-badge">voce</span> }</td>
                        <td>@u.Email</td>
                        <td><span class="status-badge">@u.Perfil</span></td>
                        @if (isAdmin) { <td>@(u.Empresa != null ? u.Empresa.Nome : "-")</td> }
                        <td style="text-align:right">
                            <div style="display:flex;gap:4px;justify-content:flex-end">
                                @Html.ActionLink("Editar", "Edit", new { id = u.Id }, new { @class = "btn-xs-outline" })
                                @if (u.Id != usuarioLogadoId)
                                {
                                    using (Html.BeginForm("Delete", "Usuario", new { id = u.Id }, FormMethod.Post))
                                    {
                                        @Html.AntiForgeryToken()
                                        <button type="submit" class="btn-xs-danger" onclick="return confirm('Excluir este usuario?')">Excluir</button>
                                    }
                                }
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
'@

[System.IO.File]::WriteAllText("$dir\Create.cshtml", $create, [System.Text.Encoding]::UTF8)
[System.IO.File]::WriteAllText("$dir\Edit.cshtml",   $edit,   [System.Text.Encoding]::UTF8)
[System.IO.File]::WriteAllText("$dir\Index.cshtml",  $index,  [System.Text.Encoding]::UTF8)

Write-Host "Create: $((Get-Item "$dir\Create.cshtml").Length) bytes"
Write-Host "Edit:   $((Get-Item "$dir\Edit.cshtml").Length) bytes"
Write-Host "Index:  $((Get-Item "$dir\Index.cshtml").Length) bytes"
