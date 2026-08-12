# Nearbly Frontend

Frontend híbrido em Astro + React para as páginas institucionais, páginas públicas de lojas e painel administrativo da Nearbly.

## Desenvolvimento

Configure as variáveis de [`.env.example`](./.env.example) e mantenha a API disponível em `http://localhost:5112`.
O frontend usa Astro 7 e requer Node.js `>=22.12.0`.

```bash
cp .env.example .env
yarn install --frozen-lockfile
yarn dev
```

O Astro fica disponível em `http://localhost:4321`. Em produção, o runtime Node deve encaminhar `/api/**` e `/r/**` para o backend antes das demais rotas para o Astro.

## Rotas

- `/`, `/solucoes` e `/como-funciona`: páginas institucionais prerenderizadas.
- `/:slug`: página pública SSR da loja.
- `/admin/login` e `/admin/lojas/**`: painel React com autenticação em memória e `sessionStorage`.

## Scripts

- `yarn dev`: servidor Astro em desenvolvimento.
- `yarn build`: valida tipos e gera o servidor Astro.
- `yarn lint`: ESLint.
- `yarn test`: testes unitários Vitest.
- `yarn e2e`: testes Playwright, quando houver servidor e API configurados.

## Decisões

- Dados da loja pública são buscados no servidor; a ilha React cuida apenas de abas e registro de visualização.
- O cliente HTTP centraliza Problem Details, autenticação Bearer e a origem da API.
- O JWT não é enviado a logs e expira conforme `expiresAtUtc`.
- Reordenação usa DnD Kit e refaz a consulta se alguma atualização parcial falhar.
- As transições Barba ficam restritas ao namespace institucional e respeitam `prefers-reduced-motion`.
