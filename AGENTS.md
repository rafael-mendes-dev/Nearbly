# Nearbly

## Produto

Nearbly transforma um identificador NFC ou QR Code em uma página pública de uma loja. A página apresenta identidade visual, abas e conteúdo; links externos passam por um redirect rastreado; a área administrativa gerencia lojas e consulta visualizações e cliques agregados.

O MVP suporta quatro tipos de conteúdo por aba:

- `links`: links rastreados para destinos externos.
- `products`: produtos com imagem, descrição, preço opcional e disponibilidade.
- `markdown`: blocos de conteúdo textual sanitizados no frontend.
- `gallery`: galeria de imagens com texto alternativo e legenda opcional.

## Arquitetura

Este é um monorepo com dois aplicativos:

- `Nearbly.Backend/`: API .NET 10 e testes.
- `Nearbly.Frontend/`: Astro 7 + React 19 + TypeScript em modo server.

O backend é um monólito modular em .NET 10:

- `Nearbly.Backend/src/Nearbly.Domain`: entidades, enums e regras puras do domínio.
- `Nearbly.Backend/src/Nearbly.Application`: casos de uso, DTOs, validações e contratos de infraestrutura.
- `Nearbly.Backend/src/Nearbly.Infrastructure`: EF Core, PostgreSQL, Identity, JWT e storage de mídia.
- `Nearbly.Backend/src/Nearbly.Api`: composição da aplicação, Minimal APIs e middleware HTTP.

As dependências seguem `Application -> Domain`, `Infrastructure -> Application + Domain` e `Api -> Application + Infrastructure`. Não adicionar repositórios genéricos, Unit of Work adicional, microserviços, filas ou abstrações sem necessidade concreta.

O frontend possui três comportamentos distintos:

- `/`, `/solucoes` e `/como-funciona` são páginas institucionais prerenderizadas.
- `/:slug` resolve a loja no servidor Astro e hidrata apenas a interação pública.
- `/admin/**` é uma ilha React `client:only` porque o painel usa `BrowserRouter`, `sessionStorage` e APIs do navegador.

Em produção, o proxy reverso deve encaminhar `/api/**`, `/r/**` e `/media/**` para a API antes das demais rotas do Astro.

## Runtime e dependências

- .NET SDK `10.0.302`, conforme `global.json`.
- Node.js `>=22.12.0`, exigido pelo Astro 7.
- Yarn Classic `1.x`, habilitado pelo Corepack.
- Docker com Docker Compose.
- PostgreSQL 17 para desenvolvimento e testes de integração.

## Comandos

Na raiz, configure o ambiente antes de executar a API:

```bash
cp .env.example .env
set -a
source .env
set +a
```

Os valores de `.env.example` são somente locais. Nunca use os placeholders em um ambiente compartilhado ou público.

### Backend

```bash
docker compose up -d postgres
dotnet tool restore
dotnet restore Nearbly.Backend/Nearbly.sln
dotnet build Nearbly.Backend/Nearbly.sln
dotnet ef database update \
  --project Nearbly.Backend/src/Nearbly.Infrastructure \
  --startup-project Nearbly.Backend/src/Nearbly.Api
dotnet run --project Nearbly.Backend/src/Nearbly.Api
```

A API inicia em `http://localhost:5112`. Swagger fica em `/swagger` durante Development ou quando `Swagger__Enabled=true`. A aplicação aplica migrations e tenta criar o administrador bootstrap na inicialização, exceto no ambiente `Testing`.

### Frontend

```bash
cd Nearbly.Frontend
cp .env.example .env
yarn install --frozen-lockfile
yarn dev
```

O Astro inicia em `http://localhost:4321`. Scripts disponíveis:

- `yarn dev`: desenvolvimento.
- `yarn build`: `astro check` e build SSR.
- `yarn lint`: ESLint.
- `yarn test`: testes unitários Vitest.
- `yarn e2e`: testes Playwright quando API e servidor estiverem configurados.

### Docker completo

Com `.env` configurado, `docker compose up --build` inicia PostgreSQL e API. O frontend continua sendo executado separadamente com Yarn.

## Configuração

A API exige `ConnectionStrings__Default` e `Jwt__SigningKey`; a chave JWT deve ter pelo menos 32 bytes. O administrador bootstrap só é criado quando `BootstrapAdmin__Email` e `BootstrapAdmin__Password` estão preenchidos.

Variáveis importantes:

- `ConnectionStrings__Default`: connection string do PostgreSQL.
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SigningKey`, `Jwt__ExpirationMinutes`: emissão e validação do JWT.
- `BootstrapAdmin__Email`, `BootstrapAdmin__Password`, `BootstrapAdmin__DisplayName`: usuário inicial.
- `Cors__AllowedOrigins__*`: origens permitidas para o frontend.
- `Media__Provider`: `filesystem` ou `s3`.
- `Media__RootPath`: diretório do storage local.
- `Media__S3__Endpoint`, `Media__S3__Bucket`, `Media__S3__AccessKey`, `Media__S3__SecretKey`: storage S3 compatível.
- `Swagger__Enabled`: habilita Swagger fora de Development.

No frontend, `API_BASE_URL` é usado pelo servidor Astro e `PUBLIC_API_BASE_URL` pelo navegador. Variáveis `PUBLIC_*` nunca podem conter segredos.

## Contrato HTTP

[`docs/API.md`](docs/API.md) é a referência versionada do contrato para frontend e integrações. Toda alteração de endpoint, DTO, status, enum, erro ou regra de compatibilidade deve atualizar essa documentação e os testes.

### Autenticação

`POST /api/admin/auth/login` é o único endpoint administrativo anônimo. Todos os demais endpoints `/api/admin/**` exigem `Authorization: Bearer <accessToken>` e o login possui rate limit de 5 tentativas por minuto e endereço de origem.

### Endpoints principais

- `GET|POST|PUT|DELETE /api/admin/stores`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/tabs`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/links`
- `GET /api/admin/stores/{storeId}/analytics`
- `POST /api/admin/stores/{storeId}/media` e `DELETE /api/admin/stores/{storeId}/media/{mediaId}`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/tabs/{tabId}/products`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/tabs/{tabId}/markdown-blocks`
- `GET|POST|PUT|DELETE /api/admin/stores/{storeId}/tabs/{tabId}/gallery-items`
- `GET /api/public/stores/{slug}`
- `POST /api/public/stores/{slug}/views`
- `GET /media/{mediaId}`
- `GET /r/{linkId}?src=nfc|qr_code|direct|unknown`

Respostas públicas nunca expõem a URL externa: links apontam para `/r/{linkId}`. `/media/{mediaId}` é anônimo e serve somente mídia ativa, com cache público.

Falhas usam `application/problem+json`, incluindo validação, autenticação, autorização, rota inexistente, JSON inválido, conflitos, rate limit e exceções inesperadas. O contrato não deve vazar stack trace, senha, token ou segredo.

Analytics recebe `from` e `to` inclusivos em `yyyy-MM-dd`. `CTR` é percentual e vale `0` quando não existem visualizações.

## Regras do domínio

- Use `DateTimeOffset` em UTC e `TimeProvider` para horários testáveis.
- Não persista IP, User-Agent ou dados pessoais dos visitantes.
- Slugs são normalizados sem diacríticos, em minúsculas e com separadores consolidados.
- URLs externas devem ser absolutas, usar `http` ou `https`, possuir host e não conter credenciais.
- Cores aceitam somente `#RRGGBB`; `SortOrder` não pode ser negativo.
- Uma aba pertence à loja da rota e conteúdo só pode referenciar a própria loja e aba.
- Uma aba possui `contentType`; trocar o tipo é bloqueado quando ela já teve conteúdo, inclusive inativo.
- Produtos e itens de galeria referenciam mídia da mesma loja; mídia referenciada não pode ser desativada.
- Uploads aceitam JPEG, PNG e WebP até 5 MB; o processador remove metadados, limita a maior dimensão a 1600 px e grava WebP otimizado.
- Exclusões administrativas são desativações lógicas; eventos históricos não são apagados.
- A página pública mostra somente loja, abas, conteúdo e links ativos.
- Produtos, Markdown e galeria não geram analytics de clique; apenas visualizações da página e cliques em links são registrados.
- O redirect valida loja, link e URL e registra o clique antes do `302`.

## Segurança e operação

- Nunca registre senha, token, chave, credencial S3 ou URL individual de analytics.
- Logs administrativos devem conter método, rota, ator, status e duração, sem dados sensíveis.
- Gere uma chave JWT diferente por ambiente e troque a senha bootstrap antes de qualquer exposição externa.
- Não exponha PostgreSQL diretamente à internet.
- Use HTTPS em produção e configure CORS somente para origens conhecidas.
- Mantenha Swagger desabilitado fora de ambientes controlados, salvo necessidade explícita.
- `.env`, builds, dependências instaladas e dados locais devem permanecer ignorados pelo Git.
- `LocalObjectStorage` é apropriado para desenvolvimento; produção deve usar storage persistente e seguro, como S3 compatível.

## Persistência e migrations

- `INearblyDbContext` é o único contrato de acesso da aplicação; não criar repositórios genéricos.
- O `DbContext` representa a unidade transacional de cada request.
- Migrations ficam em `Nearbly.Backend/src/Nearbly.Infrastructure/Persistence/Migrations`.
- O banco local é iniciado com `docker compose up -d postgres`.
- Testes de integração usam PostgreSQL real via Testcontainers quando Docker está disponível.

## Checklist de alteração

Antes de modificar código:

1. Leia este arquivo e `MEMORIES.md` integralmente.
2. Verifique `git status` e não reverta alterações de terceiros.
3. Confira as migrations e o contrato em `docs/API.md`.
4. Identifique se a mudança afeta Domain, Application, Infrastructure, API ou frontend.

Depois de modificar código:

1. Atualize testes e `docs/API.md` quando houver mudança de contrato.
2. Rode `dotnet build Nearbly.Backend/Nearbly.sln` e `dotnet test Nearbly.Backend/Nearbly.sln`.
3. Rode `yarn build`, `yarn lint` e `yarn test` em `Nearbly.Frontend`.
4. Rode `git diff --check` e confirme que nenhum segredo foi adicionado.
5. Registre em `MEMORIES.md` somente decisões e aprendizados duráveis, nunca um diário de execução.
