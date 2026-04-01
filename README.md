# Sistema de Gestão de Ativos — Multiempresa
### ASP.NET MVC 5 + Entity Framework 6 + .NET Framework 4.8

---

## Pré-requisitos

- Visual Studio 2022
- .NET Framework 4.8
- SQL Server LocalDB (incluso no VS2022)

---

## Passos para executar

### 1. Abrir o projeto
Abra o arquivo `SistemaAtivos.sln` no Visual Studio 2022.

### 2. Restaurar pacotes NuGet
No menu: **Tools → NuGet Package Manager → Package Manager Console**

```
Update-Package -reinstall
```

Ou clique com botão direito na Solution → **Restore NuGet Packages**.

### 3. Pacotes necessários (instalados automaticamente via packages.config)

| Pacote | Versão |
|---|---|
| EntityFramework | 6.5.1 |
| BCrypt.Net-Next | 4.0.3 |
| QRCoder | 1.4.3 |
| Microsoft.AspNet.Mvc | 5.2.9 |
| jQuery | 3.6.0 |
| Bootstrap | 3.4.1 |

Se precisar instalar manualmente via Package Manager Console:
```
Install-Package EntityFramework -Version 6.5.1
Install-Package BCrypt.Net-Next -Version 4.0.3
Install-Package QRCoder -Version 1.4.3
Install-Package Microsoft.AspNet.Mvc -Version 5.2.9
```

### 4. Banco de dados
O banco é criado automaticamente na primeira execução via `CreateDatabaseIfNotExists`.

**Usuário padrão criado automaticamente:**
- E-mail: `admin@sistema.com`
- Senha: `admin123`
- Tipo: Admin

### 5. Executar
Pressione **F5** ou clique em **IIS Express** para iniciar.

---

## Estrutura do projeto

```
SistemaAtivos/
├── App_Start/
│   ├── BundleConfig.cs
│   ├── FilterConfig.cs
│   └── RouteConfig.cs
├── Content/
│   └── Site.css              ← estilos principais
├── Controllers/
│   ├── AccountController.cs  ← login / logout
│   ├── AdminController.cs    ← dashboard, empresas
│   ├── AtivoController.cs    ← CRUD + QR Code
│   ├── CategoriaController.cs
│   ├── ColaboradorController.cs
│   └── ManutencaoController.cs
├── Filters/
│   └── EmpresaAuthorizeAttribute.cs  ← proteção de rotas
├── Migrations/
│   ├── Configuration.cs
│   └── 202400001_AdicionarMultiEmpresa.cs
├── Models/
│   ├── AtivosContext.cs      ← DbContext + Seed
│   ├── Ativo.cs
│   ├── Categoria.cs
│   ├── Colaborador.cs
│   ├── Empresa.cs
│   ├── Manutencao.cs
│   └── Usuario.cs
├── Scripts/
│   ├── jquery-3.6.0.min.js
│   └── bootstrap.bundle.min.js
└── Views/
    ├── Account/Login.cshtml
    ├── Admin/
    │   ├── Dashboard.cshtml  ← cards de empresas
    │   └── Empresa.cshtml    ← área interna com accordion
    ├── Ativo/
    │   ├── Index.cshtml
    │   ├── Create.cshtml
    │   ├── Edit.cshtml
    │   └── Detalhes.cshtml   ← exibe QR Code inline
    ├── Categoria/
    │   ├── Index.cshtml
    │   ├── Create.cshtml
    │   └── Edit.cshtml
    ├── Colaborador/
    │   ├── Index.cshtml
    │   ├── Create.cshtml
    │   └── Edit.cshtml
    ├── Manutencao/
    │   ├── Index.cshtml
    │   └── Create.cshtml
    └── Shared/_Layout.cshtml
```

---

## Controle de acesso

| Ação | Admin | Cliente |
|---|---|---|
| Ver todas as empresas | ✅ | ✗ |
| Ver apenas sua empresa | ✅ | ✅ |
| Criar/excluir empresa | ✅ | ✗ |
| CRUD de ativos próprios | ✅ | ✅ |
| CRUD de ativos de outras empresas | ✅ | ✗ |

---

## Bootstrap CSS / JS

Os arquivos `bootstrap.min.css` e `bootstrap.bundle.min.js` (v4.x) precisam ser
copiados manualmente para as pastas `Content/` e `Scripts/` respectivamente,
ou restaurados via NuGet (já declarado no packages.config).

Link direto para download:
- https://getbootstrap.com/docs/4.6/getting-started/download/

---

## QR Code

Cada ativo possui uma rota `/Ativo/QrCode/{id}` que retorna a imagem PNG do QR Code.
O QR aponta para `/Ativo/Detalhes/{id}`, permitindo escaneamento direto em campo.

---

## Senha inicial de usuários cliente

Ao criar uma empresa, o usuário cliente recebe a senha **`123456`**.
Recomenda-se implementar uma tela de troca de senha para produção.
