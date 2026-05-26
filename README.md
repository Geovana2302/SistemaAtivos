# Sistema de Gestão de Ativos

Sistema web para controle de patrimônio empresarial — cadastro de ativos, categorias, colaboradores e gestão multi-empresa.

---

## Pré-requisitos

Você precisa ter instalado apenas uma coisa:

**[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**

Clique no link, baixe o instalador para o seu sistema operacional (Windows, macOS ou Linux) e instale normalmente. Não é necessário instalar banco de dados, servidor web ou qualquer outra dependência.

---

## Como rodar

### 1. Baixe o projeto

Clone o repositório ou baixe o ZIP pelo GitHub:

```bash
git clone https://github.com/seu-usuario/SistemaAtivos.git
```

Ou clique em **Code → Download ZIP** no GitHub, extraia a pasta e continue.

### 2. Abra o terminal na pasta do projeto

- **Windows:** clique com o botão direito dentro da pasta `SistemaAtivos` e selecione "Abrir no Terminal" (ou "Abrir janela do PowerShell aqui")
- **macOS:** clique com o botão direito na pasta → "Novo Terminal na Pasta"
- **Linux:** clique com o botão direito → "Abrir Terminal"

### 3. Execute o projeto

```bash
dotnet run
```

Na primeira execução o sistema vai baixar as dependências automaticamente, criar o banco de dados e popular com os dados de demonstração. Você verá uma mensagem assim no terminal:

```
===========================================
  Admin  : admin@sistema.com / admin123
  Gestor : gestor@demo.com  / gestor123
===========================================
```

### 4. Acesse no navegador

Abra o navegador e entre em:

```
http://localhost:5000
```

---

## Credenciais de acesso

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | admin@sistema.com | admin123 |
| Gestor de Empresa | gestor@demo.com | gestor123 |

O **Administrador** gerencia todas as empresas do sistema.
O **Gestor** acessa apenas a empresa à qual está vinculado.

---

## Dados de demonstração

Na primeira execução o sistema cria automaticamente:

- 1 empresa: **Empresa Demo**
- 2 colaboradores: João Silva e Maria Souza
- 5 categorias: Notebooks, Monitores, Periféricos, Rede e Móveis
- 10 ativos cadastrados com número de série, marca, modelo e valor

---

## Para encerrar o sistema

No terminal onde o projeto está rodando, pressione:

```
Ctrl + C
```

---

## Observações

- O banco de dados é um arquivo chamado `ativos.db` criado automaticamente na pasta do projeto. Não é necessário instalar nenhum banco de dados separado.
- O sistema roda localmente — para acessar de outro computador na mesma rede, substitua `localhost` pelo IP da máquina que está rodando o projeto.
- Ao rodar `dotnet run` novamente, os dados já cadastrados são preservados.
