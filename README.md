# Gestao de Ativos

Sistema web **multi-empresa** para gerenciamento de ativos patrimoniais, desenvolvido com **ASP.NET MVC 5**, **Entity Framework 6** e **.NET Framework 4.8**. Conta com controle de acesso por perfil, geracao de QR Code e impressao de etiquetas para os ativos.

---

## Funcionalidades

### Administrador
- Dashboard com visao geral de todas as empresas cadastradas
- Cadastro de empresas com cor de identificacao (limite de 25 empresas)
- Edicao e exclusao de empresas com confirmacao de senha
- Cadastro e gerenciamento de administradores do sistema
- Acesso completo a todos os modulos

### Multi-Empresa
- Cada empresa possui seus proprios ativos, categorias, responsaveis e gestores
- Gestores acessam apenas os dados da sua propria empresa
- Isolamento total de dados entre empresas

### Ativos
- Cadastro completo: nome, numero de serie, marca, modelo, categoria, responsavel, data de aquisicao, valor, status e observacoes
- Status: **Ativo**, **Inativo**, **Em Manutencao**
- Listagem de ativos ativos e inativos separadamente
- Pagina de detalhes com todas as informacoes do ativo
- **Geracao de QR Code** com link direto para os detalhes do ativo
- **Impressao de etiqueta** com QR Code para colagem nos equipamentos

### Categorias
- Cadastro de categorias por empresa
- Vinculacao de ativos a categorias
- Protecao contra exclusao de categorias com ativos vinculados

### Responsaveis (Colaboradores)
- Cadastro de responsaveis por empresa
- Vinculacao de ativos a responsaveis
- Protecao contra exclusao de responsaveis com ativos vinculados

### Usuarios / Gestores
- Cadastro de gestores por empresa
- Edicao e exclusao com confirmacao de senha
- Controle de perfil: **Admin** e **Gestor**

### Seguranca
- Autenticacao por sessao
- Senhas armazenadas com hash **BCrypt**
- Filtro de autorizacao [EmpresaAuthorize] protegendo todos os controllers
- Gestores nao conseguem acessar dados de outras empresas
- Protecao CSRF em todos os formularios POST

---

## Tecnologias

| Tecnologia | Versao |
|---|---|
| ASP.NET MVC | 5 |
| .NET Framework | 4.8 |
| Entity Framework | 6 Code First |
| SQL Server | LocalDB |
| Bootstrap | 3 |
| jQuery | 3.6 |
| BCrypt.Net | - |
| QRCoder | - |

---

## Como Executar

### Pre-requisitos
- Visual Studio 2019 ou superior
- .NET Framework 4.8
- SQL Server LocalDB (incluido no Visual Studio)

### Passos

1. Clone o repositorio
`ash
git clone https://github.com/Geovana2302/SistemaAtivos.git
``r

2. Abra SistemaAtivos.sln no Visual Studio

3. Restaure os pacotes NuGet
``r
Tools > NuGet Package Manager > Restore NuGet Packages
``r

4. Execute as Migrations no Package Manager Console
``r
Update-Database
``r

5. Execute o projeto com **F5**

---

## Estrutura do Projeto

``r
SistemaAtivos/
├── Controllers/
│   ├── AccountController.cs       # Login e logout
│   ├── AdminController.cs         # Gestao de empresas e admins
│   ├── AtivoController.cs         # CRUD de ativos + QR Code + Etiqueta
│   ├── CategoriaController.cs     # CRUD de categorias
│   ├── ColaboradorController.cs   # CRUD de responsaveis
│   └── UsuarioController.cs       # CRUD de usuarios/gestores
├── Models/
│   ├── Ativo.cs
│   ├── Categoria.cs
│   ├── Colaborador.cs
│   ├── Empresa.cs
│   ├── Usuario.cs
│   └── AtivosContext.cs
├── Views/
│   ├── Admin/
│   ├── Ativo/           # CRUD + Detalhes + Imprimir etiqueta
│   ├── Categoria/
│   ├── Colaborador/
│   ├── Usuario/
│   └── Shared/_Layout.cshtml
├── Filters/
│   └── EmpresaAuthorizeAttribute.cs
└── Migrations/
`
ust

---

## Etiqueta com QR Code

Cada ativo possui pagina de impressao dedicada (/Ativo/Imprimir/{id}) que exibe:
- Nome da empresa
- Nome do ativo
- QR Code 180x180px com link direto para os detalhes
- Numero de serie, modelo, categoria e responsavel
- URL completa do ativo

Ao acessar, o dialogo de impressao abre automaticamente. Possivel imprimir ou salvar como PDF.

---

## Autora

**Geovana Bicalho**
GitHub: https://github.com/Geovana2302