# Nearbly

## Produto

Nearbly transforma um identificador NFC ou QR Code em uma página pública de uma loja. A página apresenta links organizados em abas, o visitante acessa um link por um redirect rastreado e a área administrativa consulta visualizações e cliques agregados.

## Arquitetura

Este é um monorepo com dois aplicativos:

- `Nearbly.Backend/`: API .NET 10 e seus testes.
- `Nearbly.Frontend/`: aplicação React + TypeScript + Vite.

O backend é um monólito modular em .NET 10:

- `Nearbly.Backend/src/Nearbly.Domain`: entidades, enums, constantes e regras puras do domínio.
- `Nearbly.Backend/src/Nearbly.Application`: casos de uso, DTOs, validações e contratos de infraestrutura.
- `Nearbly.Backend/src/Nearbly.Infrastructure`: EF Core, PostgreSQL, Identity, JWT e implementações externas.
- `Nearbly.Backend/src/Nearbly.Api`: composição da aplicação, middleware e Minimal APIs.

As dependências seguem `Application -> Domain`, `Infrastructure -> Application + Domain` e `Api -> Application + Infrastructure`. Não adicionar repositórios genéricos, Unit of Work adicional, microserviços, filas ou abstrações sem necessidade concreta.

## Comandos

```bash
dotnet tool restore
dotnet restore Nearbly.Backend/Nearbly.sln
dotnet build Nearbly.Backend/Nearbly.sln
dotnet test Nearbly.Backend/Nearbly.sln
docker compose up -d postgres
dotnet ef database update --project Nearbly.Backend/src/Nearbly.Infrastructure --startup-project Nearbly.Backend/src/Nearbly.Api
dotnet run --project Nearbly.Backend/src/Nearbly.Api
```

Copie `.env.example` para `.env` e exporte as variáveis antes de executar a API. A API usa `ConnectionStrings__Default`, JWT e `BootstrapAdmin__*`. Swagger fica em `/swagger` durante Development ou quando `Swagger__Enabled=true`.

Para o frontend, execute `yarn install --frozen-lockfile` e `yarn dev` dentro de `Nearbly.Frontend`. O Vite usa `http://localhost:5173` por padrão.

O Swagger declara Bearer somente nas operações administrativas protegidas. Respostas de falha usam `application/problem+json`, incluindo erros de autenticação, autorização, rota inexistente, JSON inválido, rate limit e exceções de domínio. Operações administrativas geram logs estruturados com método, rota, ator, status e duração; nunca registre tokens, senhas ou URLs de analytics individuais.

`docs/API.md` é a referência versionada do contrato HTTP para frontend e integrações. Toda alteração de endpoint, DTO, status, enum, erro ou regra de compatibilidade deve atualizar essa documentação e seus testes.

## Regras

- Leia integralmente `AGENTS.md` e `MEMORIES.md` antes de modificar o código.
- Confira o Git e as migrations existentes no início do trabalho.
- Use `DateTimeOffset` em UTC e `TimeProvider` para horários testáveis.
- Não persista IP, User-Agent ou dados pessoais dos visitantes.
- Slugs são normalizados sem diacríticos, em minúsculas e com separadores consolidados.
- URLs públicas devem ser absolutas, `http`/`https`, com host e sem credenciais.
- Cores aceitam somente `#RRGGBB`; `SortOrder` não pode ser negativo.
- Exclusões administrativas são desativações lógicas; eventos históricos não são apagados.
- Endpoints admin, exceto login, exigem JWT. Nunca registre senha, token ou segredo.

## Contratos principais

- `POST /api/admin/auth/login`
- `GET|POST|PUT|DELETE /api/admin/stores`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/tabs`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/links`
- `GET /api/admin/stores/{storeId}/analytics`
- `GET /api/public/stores/{slug}` e `POST /api/public/stores/{slug}/views`
- `GET /r/{linkId}?src=nfc|qr_code|direct|unknown`

Respostas públicas nunca expõem a URL externa: links apontam para `/r/{linkId}`.

As datas de analytics (`from` e `to`) são inclusivas e devem estar no formato ISO `yyyy-MM-dd`. `CTR` é percentual e vale `0` quando não existem visualizações.

## Checklist

Antes: leia este arquivo e `MEMORIES.md`, verifique `git status` e entenda as migrations. Depois: atualize testes e documentação, rode build/testes e registre em `MEMORIES.md` apenas decisões e aprendizados duráveis, sem transformá-lo em diário.
