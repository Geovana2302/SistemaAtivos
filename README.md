# 📦 Sistema de Gestão de Ativos

Sistema web **multiempresa** para gerenciamento de ativos patrimoniais e controle completo de manutenções, desenvolvido com **ASP.NET MVC 5**, **Entity Framework 6** e **.NET Framework 4.8**.

---

## 🚀 Funcionalidades

### 🔐 Autenticação e Controle de Acesso (Requisito 1)
- Login com validação de credenciais via hash **SHA-256** (senha nunca armazenada em texto puro)
- Controle de **Sessão** para proteger todas as áreas restritas
- Dois perfis de acesso: **Admin** e **Cliente**
- Filtro `[EmpresaAuthorize]` aplicado em todos os controllers sensíveis
- Logout com `Session.Clear()` + `Session.Abandon()`

### 🏢 Gestão de Empresas
- Cadastro e gerenciamento de empresas clientes
- Dashboard administrativo com visão geral de todas as empresas
- Página individual por empresa com seus ativos, colaboradores e manutenções

### 🖥️ Gestão de Ativos
- Cadastro completo de equipamentos/patrimônios
- Vinculação a **Empresa**, **Categoria** e **Colaborador** responsável
- Controle de status: **Ativo**, **Inativo**, **Em Manutenção**
- Geração de **QR Code** por ativo (rota `/Ativo/QrCode/{id}`) para acesso via dispositivo móvel
- Filtros por empresa e ordenação dinâmica
- Histórico de manutenções por ativo

### 🔧 Gestão de Manutenções (Requisito 2 — Movimentação de Dados)
- Abertura de chamados vinculados a ativos e empresas (relacionamento entre múltiplas entidades)
- **Máquina de estados** completa:

```
Aberto → Em Orçamento → Aguardando Aprovação → Aprovado → Em Conserto → Concluído
                                                                       ↘ Cancelado
```

- **Quadro Técnico (Kanban)** para movimentação de chamados com suporte a **AJAX**
- Envio de orçamento com diagnóstico técnico e valor pelo técnico (Admin)
- Aprovação de orçamento pelo cliente com registro de data e nome do responsável
- **Histórico** com filtro por empresa e intervalo de datas + cálculo de faturamento total

### 👥 Colaboradores
- Cadastro de colaboradores por empresa
- Associação como responsável por ativos

### 🗂️ Categorias
- Gerenciamento de categorias de ativos por empresa

---

## 🛠️ Tecnologias Utilizadas

| Tecnologia | Versão |
|---|---|
| ASP.NET MVC | 5.2.9 |
| .NET Framework | 4.8 |
| Entity Framework | 6.5.1 (Code First + Migrations) |
| Bootstrap | 5 |
| Bootstrap Icons | Ícones da interface |
| jQuery | 3.6.0 |
| QRCoder | 1.4.3 |
| SQL Server / LocalDB | Banco de dados |

---

## 🏗️ Estrutura do Projeto

```
SistemaAtivos/
├── Controllers/
│   ├── AccountController.cs       ← Login, logout e controle de sessão
│   ├── AdminController.cs         ← Dashboard e gestão de empresas
│   ├── AtivoController.cs         ← CRUD de ativos + QR Code
│   ├── ManutencaoController.cs    ← Fluxo completo de manutenções (Kanban, orçamento, aprovação)
│   ├── CategoriaController.cs     ← CRUD de categorias
│   └── ColaboradorController.cs   ← CRUD de colaboradores
├── Models/
│   ├── Ativo.cs                   ← Entidade ativo com Data Annotations
│   ├── Manutencao.cs              ← Entidade manutenção + máquina de estados
│   ├── Empresa.cs                 ← Entidade empresa
│   ├── Usuario.cs                 ← Entidade usuário (Admin/Cliente)
│   ├── Categoria.cs               ← Entidade categoria
│   ├── Colaborador.cs             ← Entidade colaborador
│   └── AtivosContext.cs           ← DbContext + configurações EF + Migrations automáticas
├── Services/
│   └── ManutencaoService.cs       ← Regras de negócio de manutenção
├── Filters/
│   └── EmpresaAuthorizeAttribute.cs  ← Filtro de autorização baseado em sessão
├── Helpers/
│   └── CriptoHelper.cs            ← Hash SHA-256 para senhas
├── Migrations/                    ← Migrations do Entity Framework
├── Content/
│   └── Site.css                   ← Estilos customizados
└── Views/
    ├── Account/Login.cshtml
    ├── Admin/
    │   ├── Dashboard.cshtml        ← Cards de empresas
    │   └── Empresa.cshtml          ← Área interna da empresa
    ├── Ativo/
    │   ├── Index.cshtml
    │   ├── Create.cshtml
    │   ├── Edit.cshtml
    │   ├── Detalhes.cshtml         ← Histórico + QR Code
    │   └── Inativos.cshtml
    ├── Manutencao/
    │   ├── Index.cshtml            ← Visão do cliente
    │   ├── Create.cshtml
    │   ├── Edit.cshtml
    │   ├── QuadroTecnico.cshtml    ← Kanban (Admin)
    │   ├── Historico.cshtml        ← Histórico com faturamento
    │   ├── _ManutencaoCard.cshtml  ← Partial card do Kanban
    │   └── _OrcamentoModal.cshtml  ← Modal de orçamento
    ├── Categoria/ e Colaborador/
    └── Shared/_Layout.cshtml
```

---

## 🗄️ Modelo de Dados

```
Empresa  (1) ──── (N) Usuario
Empresa  (1) ──── (N) Ativo
Empresa  (1) ──── (N) Colaborador
Empresa  (1) ──── (N) Categoria
Empresa  (1) ──── (N) Manutencao
Ativo    (1) ──── (N) Manutencao
Ativo    (N) ──── (1) Categoria
Ativo    (N) ──── (1) Colaborador
```

---

## ⚙️ Como Executar

### Pré-requisitos
- Visual Studio 2019 ou superior
- .NET Framework 4.8
- SQL Server ou LocalDB (incluso no Visual Studio)

### Passos

**1. Clone o repositório**
```bash
git clone https://github.com/Geovana2302/SistemaAtivos.git
```

**2. Abra a solução** no Visual Studio
```
SistemaAtivos.sln
```

**3. Restaure os pacotes NuGet**
```
Tools → NuGet Package Manager → Restore NuGet Packages
```

**4. Verifique a string de conexão** em `web.config`:
```xml
<connectionStrings>
  <add name="AtivosContext"
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=SistemaAtivos;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**5. Execute o projeto** — as Migrations são aplicadas automaticamente (`MigrateDatabaseToLatestVersion`)

**6. Acesse** via browser:
```
https://localhost:{porta}/Account/Login
```

### Credenciais padrão (criadas no Seed)
| Campo | Valor |
|---|---|
| E-mail | `admin@sistema.com` |
| Senha | `admin123` |
| Tipo | Admin |

> Usuários clientes são criados junto com a empresa e recebem a senha padrão `123456`.

---

## 🔒 Segurança

- Senhas armazenadas com **hash SHA-256** — nunca em texto puro
- Todas as rotas protegidas pelo filtro `[EmpresaAuthorize]`
- Proteção contra **CSRF** com `@Html.AntiForgeryToken()` em todos os formulários POST
- **Isolamento de dados por empresa** — clientes só visualizam seus próprios registros

---

## 🔑 Controle de Acesso

| Funcionalidade | Admin | Cliente |
|---|---|---|
| Ver todas as empresas | ✅ | ✗ |
| Ver apenas sua empresa | ✅ | ✅ |
| Criar / excluir empresa | ✅ | ✗ |
| CRUD de ativos | ✅ | ✅ (só os seus) |
| Quadro Técnico (Kanban) | ✅ | ✗ |
| Enviar orçamento | ✅ | ✗ |
| Aprovar orçamento | ✅ | ✅ |
| Histórico de faturamento | ✅ | ✗ |
| Ver manutenções | ✅ | ✅ (só as suas) |

---

## 📋 Requisitos do Projeto Atendidos

| # | Requisito | Como foi implementado | Status |
|---|---|---|---|
| 1 | **Autenticação e Sessão** | `AccountController` com hash SHA-256, `Session`, filtro `[EmpresaAuthorize]` | ✅ |
| 2 | **Movimentação de Dados** | Fluxo de manutenção com relacionamento entre `Manutencao`, `Ativo` e `Empresa` | ✅ |
| 3 | **Validação com Data Annotations** | `[Required]`, `[StringLength]`, `[DataType]` em todos os Models + `ModelState.IsValid` nos Controllers | ✅ |
| 4 | **Lógica Operacional** | Actions `MoverStatus`, `Aprovar`, `Orcamento`, `QuadroTecnico`, `Historico` com estado em tempo real | ✅ |
| 5 | **Estabilidade (try-catch)** | Blocos `try-catch` em todas as operações críticas de escrita no banco | ✅ |

---

## 👩‍💻 Autora

**Geovana** — [@Geovana2302](https://github.com/Geovana2302)
