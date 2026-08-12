# Memórias Duráveis

## Decisões atuais

- **2026-08-12 | Arquitetura |** O backend usa um monólito modular em quatro projetos (`Domain`, `Application`, `Infrastructure`, `Api`) com Minimal APIs. **Motivo:** manter limites claros sem custo operacional de microserviços. **Impacto:** casos de uso ficam testáveis e a composição permanece em `Api`.
- **2026-08-12 | Persistência |** `INearblyDbContext` é o único contrato de acesso da aplicação; o `DbContext` implementa a unidade transacional de cada request. **Motivo:** evitar repositórios genéricos e abstrações redundantes. **Impacto:** queries específicas permanecem explícitas nos casos de uso.
- **2026-08-12 | Eventos |** Visualizações e cliques armazenam somente origem e horários UTC. **Motivo:** privacidade por padrão. **Impacto:** não é possível fazer analytics por IP ou navegador.
- **2026-08-12 | Identity |** O MVP usa `IdentityUserContext` em vez de `IdentityDbContext`, com tabelas `asp_net_*` e sem roles. **Motivo:** o produto não possui sistema de papéis nesta fase. **Impacto:** a migration inicial cria somente usuários, claims, logins e tokens necessários.
- **2026-08-12 | Testes |** O `WebApplicationFactory` de integração injeta a connection string do Testcontainers por variáveis de ambiente antes da criação do host. **Motivo:** a configuração do host pode prevalecer sobre fontes adicionadas tardiamente. **Impacto:** o teste usa PostgreSQL real e aplica a migration automaticamente.
- **2026-08-12 | Erros e operação |** O pipeline converte exceções de domínio, JSON inválido, autenticação, autorização, rotas inexistentes e rate limit em Problem Details; a auditoria administrativa ocorre depois da autenticação. **Motivo:** manter contratos HTTP uniformes e identificar o ator sem registrar credenciais. **Impacto:** clientes podem tratar falhas por `application/problem+json` e logs permanecem sem tokens ou senhas.
- **2026-08-12 | OpenAPI |** A segurança Bearer é aplicada por operação a partir dos metadados de autorização, em vez de ser declarada globalmente. **Motivo:** endpoints públicos não devem aparentar exigir autenticação. **Impacto:** Swagger diferencia corretamente os fluxos público e administrativo.

## Invariantes

- Slugs são únicos globalmente depois da normalização.
- Uma aba pertence à loja da rota e um link só pode apontar para uma aba da mesma loja.
- Loja, aba e link possuem desativação lógica independente.
- Links públicos ativos só aparecem quando a loja e a aba associada também estão ativas.
- O redirect valida loja, link e URL e grava o clique antes do `302`.

## Banco, testes e ambiente

- PostgreSQL local é iniciado por `docker compose up -d postgres`.
- Migrations ficam em `src/Nearbly.Infrastructure/Persistence/Migrations` e usam Infrastructure como projeto e Api como startup.
- Testes unitários não dependem de banco; testes de integração usam Testcontainers quando o Docker está disponível.

## Questões em aberto

- O frontend administrativo será um consumidor separado da API e não faz parte deste MVP.
- O MVP ainda não inclui refresh token, paginação, upload de mídia, cache, filas ou gerenciamento de administradores.
