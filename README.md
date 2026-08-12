# Nearbly

Monorepo do fluxo NFC/QR da Nearbly, com API administrativa autenticada, página pública, redirect rastreado, analytics e frontend React.

## Estrutura

- `Nearbly.Backend/`: solução .NET 10 com Domain, Application, Infrastructure, API e testes.
- `Nearbly.Frontend/`: aplicação React + TypeScript + Vite.
- `docs/API.md`: contrato HTTP versionado para integração com o frontend.
- `docker-compose.yml`: PostgreSQL local.

## Requisitos

- .NET SDK `10.0.302`
- Docker, para PostgreSQL local

## Executar

```bash
cp .env.example .env
set -a; source .env; set +a
docker compose up -d postgres
dotnet tool restore
dotnet restore Nearbly.Backend/Nearbly.sln
dotnet ef database update --project Nearbly.Backend/src/Nearbly.Infrastructure --startup-project Nearbly.Backend/src/Nearbly.Api
dotnet run --project Nearbly.Backend/src/Nearbly.Api
```

Por padrão, a API inicia em `http://localhost:5112` e o Swagger em `http://localhost:5112/swagger` em Development. O login bootstrap é criado apenas se não existir; a senha nunca é redefinida automaticamente.

Em outro terminal, rode o frontend:

```bash
cd Nearbly.Frontend
yarn install --frozen-lockfile
yarn dev
```

O frontend Vite fica disponível em `http://localhost:5173`.

Preencha obrigatoriamente `ConnectionStrings__Default`, `Jwt__SigningKey` com pelo menos 32 caracteres, `BootstrapAdmin__Email` e `BootstrapAdmin__Password`. `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` e `DATABASE_PORT` configuram o PostgreSQL do Compose e devem corresponder à connection string. `Cors__AllowedOrigins__0` é opcional e `Swagger__Enabled` só é necessário fora de Development.

Os testes unitários cobrem as regras puras do domínio. Os testes de integração sobem PostgreSQL com Testcontainers e cobrem autenticação, autorização, CRUD, conflitos, projeção pública, tracking, redirect, reativação, filtros e analytics.

Consulte `AGENTS.md` para contratos, convenções, segurança e operações.

Para integração com o frontend, consulte [`docs/API.md`](docs/API.md), que documenta endpoints, DTOs, respostas, erros, exemplos, analytics e regras de evolução do contrato.
