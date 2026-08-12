# Nearbly Backend

Backend do fluxo NFC/QR da Nearbly, com API administrativa autenticada, página pública, redirect rastreado e analytics.

## Requisitos

- .NET SDK `10.0.302`
- Docker, para PostgreSQL local

## Executar

```bash
cp .env.example .env
set -a; source .env; set +a
docker compose up -d postgres
dotnet tool restore
dotnet ef database update --project src/Nearbly.Infrastructure --startup-project src/Nearbly.Api
dotnet run --project src/Nearbly.Api
```

Por padrão, a API inicia em `http://localhost:5112` e o Swagger em `http://localhost:5112/swagger` em Development. O login bootstrap é criado apenas se não existir; a senha nunca é redefinida automaticamente.

Os testes unitários cobrem as regras puras do domínio. Os testes de integração sobem PostgreSQL com Testcontainers e cobrem autenticação, autorização, CRUD, conflitos, projeção pública, tracking, redirect, reativação, filtros e analytics.

Consulte `AGENTS.md` para contratos, convenções, segurança e operações.
