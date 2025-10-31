# Smoke Express

## Visão Geral
Smoke Express é uma plataforma de e-commerce premium especializada em produtos de tabacaria, desenvolvida com foco absoluto em crescimento orgânico. O projeto foi concebido como parte de um trabalho acadêmico e utiliza **Blazor Server** sobre **.NET 9** para garantir segurança, sigilo e controle de sessão no servidor. Toda a base foi construída respeitando os requisitos de marketing orgânico através de um blog interno e de um programa nativo de indicações.

Autores: **Bruno Bueno** e **Matheus Esposto**

## Principais Funcionalidades
- Autenticação com **ASP.NET Core Identity** e hashing de senha via **BCrypt**.
- Catálogo completo de produtos e categorias para administração interna.
- Estrutura de pedidos (orders, order items) preparada para futuras integrações de checkout.
- Módulo de conteúdo (blog) voltado a marketing orgânico, com suporte a Slug.
- Programa "Indique um Amigo" para retenção e expansão orgânica.
- Painel administrativo protegido por role `Admin`.

## Stack Tecnológica
- **.NET 9** / **Blazor Server**
- **SQL Server** (LocalDB por padrão, configurável via `appsettings.json`)
- **Entity Framework Core**
- **ASP.NET Core Identity** (cookies HttpOnly + BCrypt)

## Pré-requisitos
- SDK do .NET 9.0 ou superior instalado
- SQL Server (pode ser LocalDB, SQL Express ou instância remota)
- Certificado HTTPS de desenvolvimento (pode ser configurado via `dotnet dev-certs https --trust`)

## Estrutura da Solução
```
SmokeExpress.sln
└─ SmokeExpress.Web/                 # Projeto Blazor Server
   ├─ Data/                          # DbContext, migrações e seeds futuros
   ├─ Models/                        # Entidades de domínio
   ├─ Pages/                         # Páginas Blazor (Admin, Blog, Account, etc.)
   ├─ Services/                      # Serviços de domínio (ex.: ProductService)
   └─ Security/                      # Implementações de segurança (BCrypt password hasher)
```

## Configuração e Execução
1. **Clonar o repositório**
   ```powershell
   git clone <url-do-repositorio>
   cd SmokeExpress
   ```

2. **Restaurar pacotes e compilar**
   ```powershell
   dotnet restore
   dotnet build SmokeExpress.sln
   ```

3. **Configurar conexão com o banco**
   - Editar `SmokeExpress.Web/appsettings.json` (ou `appsettings.Development.json`) para apontar para o seu SQL Server.
   - Por padrão, o projeto usa `LocalDB` com o nome `SmokeExpressDb`.

4. **Aplicar migrações do Entity Framework**
   ```powershell
   dotnet ef database update --project SmokeExpress.Web --startup-project SmokeExpress.Web
   ```
   > Caso esteja rodando a primeira vez e não exista a migração inicial, utilize `dotnet ef migrations add InitialCreate --project SmokeExpress.Web --startup-project SmokeExpress.Web --output-dir Data/Migrations` antes do comando acima.

5. **Executar a aplicação**
   ```powershell
   dotnet run --project SmokeExpress.Web
   ```
   Acesse `https://localhost:5001` (ou a porta informada no console) para usar o site.

### Acesso inicial
- Usuário administrador padrão: `admin@smokeexpress.com`
- Senha inicial: `Admin@123`
- Roles pré-criadas: `Admin` e `User` (o administrador já está vinculado a ambas)

## Próximos Passos Recomendados
- Criar seeds adicionais para categorias, produtos e conteúdos iniciais do blog.
- Implementar páginas de manutenção (CRUD) para produtos, posts de blog e programa de indicações.
- Configurar pipelines de CI/CD e cobertura de testes automatizados.
- Ajustar layout/branding conforme identidade visual da Smoke Express.

## Licença
Projeto acadêmico — ajustes sob demanda dos autores.
