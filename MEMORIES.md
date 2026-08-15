# Memórias Duráveis

## Decisões atuais

- **2026-08-12 | Identidade do frontend |** As superfícies Nearbly usam tema escuro com preto `#06080F`, índigo `#2B22E0`, ciano e menta como acentos; os componentes animados vêm do registry React Bits e são adaptados aos tokens e a `prefers-reduced-motion`. **Motivo:** alinhar o produto ao design system oficial e manter uma linguagem visual coerente entre marketing e painel. **Impacto:** páginas públicas partem da mesma base escura, mas preservam as cores configuradas pela loja como personalização.
- **2026-08-12 | Frontend |** O frontend migrou de Vite SPA para Astro em modo server com React islands; páginas institucionais são prerenderizadas, páginas públicas resolvem a loja via SSR e o painel administrativo é uma ilha client-only por depender de `BrowserRouter`. **Motivo:** preservar SEO e primeira resposta da página pública sem misturar APIs de navegador no SSR do painel. **Impacto:** o deploy precisa de runtime Node e proxy reverso para `/api/**` e `/r/**`; o frontend usa `API_BASE_URL` no servidor e `PUBLIC_API_BASE_URL` no navegador.
- **2026-08-12 | Runtime |** O frontend usa Astro 7 com `@astrojs/node` 11 e requer Node.js `>=22.12.0`. **Motivo:** Astro 7 não suporta Node 20. **Impacto:** ambientes locais e de deploy precisam disponibilizar Node 22 ou superior.

- **2026-08-12 | Arquitetura |** O repositório é um monorepo com `Nearbly.Backend` e `Nearbly.Frontend`; o backend usa um monólito modular em quatro projetos (`Domain`, `Application`, `Infrastructure`, `Api`) com Minimal APIs. **Motivo:** manter os aplicativos separados e os limites internos claros sem custo operacional de microserviços. **Impacto:** casos de uso ficam testáveis e a composição permanece em `Api`.
- **2026-08-12 | Persistência |** `INearblyDbContext` é o único contrato de acesso da aplicação; o `DbContext` implementa a unidade transacional de cada request. **Motivo:** evitar repositórios genéricos e abstrações redundantes. **Impacto:** queries específicas permanecem explícitas nos casos de uso.
- **2026-08-12 | Eventos |** Visualizações e cliques armazenam somente origem e horários UTC. **Motivo:** privacidade por padrão. **Impacto:** não é possível fazer analytics por IP ou navegador.
- **2026-08-12 | Identity |** O MVP usa `IdentityUserContext` em vez de `IdentityDbContext`, com tabelas `asp_net_*` e sem roles. **Motivo:** o produto não possui sistema de papéis nesta fase. **Impacto:** a migration inicial cria somente usuários, claims, logins e tokens necessários.
- **2026-08-12 | Testes |** O `WebApplicationFactory` de integração injeta a connection string do Testcontainers por variáveis de ambiente antes da criação do host. **Motivo:** a configuração do host pode prevalecer sobre fontes adicionadas tardiamente. **Impacto:** o teste usa PostgreSQL real e aplica a migration automaticamente.
- **2026-08-12 | Erros e operação |** O pipeline converte exceções de domínio, JSON inválido, autenticação, autorização, rotas inexistentes e rate limit em Problem Details; a auditoria administrativa ocorre depois da autenticação. **Motivo:** manter contratos HTTP uniformes e identificar o ator sem registrar credenciais. **Impacto:** clientes podem tratar falhas por `application/problem+json` e logs permanecem sem tokens ou senhas.
- **2026-08-12 | OpenAPI |** A segurança Bearer é aplicada por operação a partir dos metadados de autorização, em vez de ser declarada globalmente. **Motivo:** endpoints públicos não devem aparentar exigir autenticação. **Impacto:** Swagger diferencia corretamente os fluxos público e administrativo.
- **2026-08-12 | Conteúdo |** Abas usam `ContentType` (`links`, `products`, `markdown`, `gallery`) e os conteúdos tipados mantêm `StoreId` e `StoreTabId` para validar ownership no caso de uso. **Motivo:** impedir referências cruzadas e permitir contratos públicos previsíveis com quatro coleções. **Impacto:** trocar o tipo de uma aba é bloqueado enquanto existir conteúdo, inclusive desativado.
- **2026-08-12 | Mídia |** Uploads são processados por ImageSharp para WebP, removem metadados e limitam a maior dimensão a 1600 px; `IObjectStorage` usa filesystem com volume no desenvolvimento e S3 compatível quando configurado. **Motivo:** manter o domínio independente do storage e evitar chaves privadas no contrato público. **Impacto:** produtos, galeria e logo referenciam `MediaAsset` por ID e mídia referenciada não pode ser desativada.
- **2026-08-15 | Cores da loja |** Na página pública a cor principal é ação (CTA, aba ativa, ícones) e a secundária é ambiente: `shade()` em `lib/format.ts` mantém matiz e saturação dela, mas força a luminosidade, gerando `--store-base`, `--store-surface`, `--store-surface-strong`, `--store-line` e `--store-glow` para fundo, cards, bordas e brilhos. **Motivo:** misturar a cor crua em superfícies grandes deixava a secundária invisível com o padrão escuro e quebrava o contraste quando a loja escolhia uma cor clara. **Impacto:** qualquer `#RRGGBB` tinge a página inteira sem perder legibilidade, e o padrão `#06080F` continua com a aparência atual.
- **2026-08-15 | Página pública |** A página pública elege um link de contato principal (`whatsapp` > `phone` > `email` > `map`/`location` > `website`) como CTA de conversão: botão destacado no perfil, atalhos para os outros links e barra fixa no rodapé em telas até 860px; produtos são cards com imagem grande e CTA que reaproveita esse mesmo link. **Motivo:** a página precisa gerar pedido, não apenas listar conteúdo, e o contrato público só expõe `/r/{linkId}`, sem número ou URL para montar mensagem pré-preenchida. **Impacto:** o CTA some quando a loja não tem nenhum link ativo e todo clique continua rastreado pelo redirect existente.
- **2026-08-15 | Identificador público |** Cada loja recebe `publicCode` aleatório e imutável, derivado do UUID da própria loja e separado do slug. **Motivo:** QR Codes e cartões físicos não podem quebrar quando a loja altera o slug. **Impacto:** páginas públicas e visualizações devem usar `publicCode`; slugs continuam aceitos apenas para URLs legadas.

## Invariantes

- Slugs são únicos globalmente depois da normalização.
- Uma aba pertence à loja da rota e um link só pode apontar para uma aba da mesma loja.
- Loja, aba e link possuem desativação lógica independente.
- Links públicos ativos só aparecem quando a loja e a aba associada também estão ativas.
- Conteúdo público só aparece quando a loja, aba, item e mídia associada estão ativos.
- Produtos e galeria não geram analytics; somente visualizações da página e cliques em links são registrados.
- O redirect valida loja, link e URL e grava o clique antes do `302`.

## Banco, testes e ambiente

- PostgreSQL local é iniciado por `docker compose up -d postgres`.
- Migrations ficam em `Nearbly.Backend/src/Nearbly.Infrastructure/Persistence/Migrations` e usam Infrastructure como projeto e Api como startup.
- Testes unitários não dependem de banco; testes de integração usam Testcontainers quando o Docker está disponível.

## Questões em aberto

- O frontend administrativo fica em `Nearbly.Frontend` e ainda é um consumidor inicial separado da API; a integração deve seguir `docs/API.md`.
- O MVP ainda não inclui refresh token, paginação, filas ou gerenciamento de administradores.
