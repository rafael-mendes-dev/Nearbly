# Nearbly API

Referência versionada do contrato HTTP da Nearbly para o frontend administrativo, páginas públicas e integrações NFC/QR.

## 1. Visão geral

### Base URL

Em desenvolvimento:

~~~text
http://localhost:5112
~~~

Em produção, a URL depende do ambiente de hospedagem. A API não possui versionamento no path neste MVP. Alterações incompatíveis devem criar uma versão nova antes de remover o contrato atual.

### Formato

- Protocolo: HTTP/HTTPS.
- Requests e respostas JSON usam propriedades em camelCase.
- Request JSON: application/json.
- Resposta JSON: application/json; charset=utf-8.
- Erros: application/problem+json.
- IDs: UUID.
- Datas com horário: ISO 8601 UTC, por exemplo "2026-08-12T12:30:00Z".
- Datas sem horário: yyyy-MM-dd.
- Valores monetários usam BRL e são informativos; produtos não possuem checkout.

### Segurança

Endpoints administrativos, exceto o login, exigem:

~~~http
Authorization: Bearer <accessToken>
~~~

Endpoints públicos e redirect são anônimos. Não envie senha, token ou URL externa em logs do frontend.

### Swagger/OpenAPI

Em Development:

~~~text
http://localhost:5112/swagger
http://localhost:5112/swagger/v1/swagger.json
~~~

Fora de Development, habilite com Swagger__Enabled=true. O esquema Bearer aparece somente nas operações protegidas.

## 2. Padrões de contrato

### Status HTTP

| Status | Uso |
|---|---|
| 200 | Leitura ou atualização com corpo. |
| 201 | Criação administrativa. O header Location aponta para o recurso. |
| 204 | Desativação lógica ou registro de visualização, sem corpo. |
| 302 | Redirect rastreado para URL externa. |
| 400 | JSON, parâmetro ou payload inválido. |
| 401 | Token ausente/inválido ou credenciais inválidas. |
| 403 | Token válido sem permissão. |
| 404 | Loja, aba, link ou recurso público inexistente. |
| 409 | Slug/chave duplicado ou associação de aba inválida. |
| 429 | Limite de login excedido. |
| 500 | Falha inesperada sem detalhe interno. |

### Problem Details

Toda falha deve ser tratada pelo frontend como application/problem+json:

~~~json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not found",
  "status": 404,
  "detail": "Store not found.",
  "instance": "/api/public/stores/unknown",
  "traceId": "00-..."
}
~~~

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---:|---|
| type | string | não | URI de referência do problema, quando fornecida. |
| title | string | sim | Categoria curta do erro. |
| status | integer | sim | Mesmo status HTTP. |
| detail | string | sim | Mensagem segura para exibição/diagnóstico. |
| instance | string | não | Rota que recebeu a request. |
| traceId | string | não | Identificador de correlação. |

Falhas de validação podem combinar várias mensagens em detail. O contrato atual não retorna um mapa errors por campo; use detail como fallback.

### Ordenação e desativação

Listas administrativas e públicas usam sortOrder crescente e, em empate, id crescente. Não há paginação no MVP.

DELETE não remove dados. Ele define isActive como false e preserva eventos. Listagens administrativas retornam ativos e inativos. Para reativar, use PUT com isActive: true.

## 3. DTOs

### LoginRequest

Request de POST /api/admin/auth/login:

~~~json
{
  "email": "admin@nearbly.local",
  "password": "NearblyLocal123"
}
~~~

| Campo | Tipo | Obrigatório | Restrições |
|---|---|---:|---|
| email | string | sim | Email válido, até 320 caracteres. |
| password | string | sim | Não vazio, até 256 caracteres. |

### LoginResponse

~~~json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-08-12T13:30:00Z"
}
~~~

Não há refresh token. Remova o token quando expirar ou no logout.

### StoreResponse

~~~json
{
  "id": "9b7d6d9d-7d20-4c2e-ae60-7c3f4b9af100",
  "name": "Café Central",
  "slug": "cafe-central",
  "publicCode": "s_9b7d6d9d7d204c2eae607c3f4b9af100",
  "description": "Café no centro",
  "logoUrl": "https://cdn.example.com/logo.png",
  "logoMediaId": null,
  "primaryColor": "#112233",
  "secondaryColor": null,
  "isActive": true,
  "createdAtUtc": "2026-08-12T12:00:00Z",
  "updatedAtUtc": "2026-08-12T12:00:00Z"
}
~~~

| Campo | Tipo | Nullable | Restrições |
|---|---|---:|---|
| id | uuid | não | Identificador. |
| name | string | não | Até 160 caracteres. |
| slug | string | não | Normalizado, único globalmente, até 120 caracteres. |
| publicCode | string | não | Código aleatório e imutável da página pública; use-o em QR Codes e cartões. |
| description | string | sim | Até 500 caracteres. |
| logoUrl | string | sim | URL HTTP/HTTPS sem credenciais ou caminho interno `/media/{mediaId}` quando logoMediaId estiver preenchido. |
| logoMediaId | uuid | sim | Mídia otimizada da loja; quando preenchido, tem prioridade sobre logoUrl. |
| primaryColor | string | sim | #RRGGBB. |
| secondaryColor | string | sim | #RRGGBB. |
| isActive | boolean | não | Estado lógico. |
| createdAtUtc | datetime | não | UTC. |
| updatedAtUtc | datetime | não | UTC. |

### CreateStoreRequest e UpdateStoreRequest

~~~json
{
  "name": "Café Central",
  "slug": "Café Central",
  "description": "Café no centro",
  "logoUrl": "https://cdn.example.com/logo.png",
  "primaryColor": "#112233",
  "secondaryColor": "#DDEEFF",
  "isActive": true
}
~~~

isActive só existe no update e é opcional. Se omitido, o estado atual é preservado. O slug aceita maiúsculas, espaços e acentos; a API normaliza para minúsculas, remove diacríticos e consolida separadores. Exemplo: "São Paulo / Café" vira "sao-paulo-cafe".

### TabResponse

~~~json
{
  "id": "d1c95cc2-d5fb-41aa-97e0-4d4e1d6bb001",
  "storeId": "9b7d6d9d-7d20-4c2e-ae60-7c3f4b9af100",
  "key": "menu",
  "name": "Menu",
  "contentType": "links",
  "sortOrder": 0,
  "isActive": true,
  "createdAtUtc": "2026-08-12T12:00:00Z",
  "updatedAtUtc": "2026-08-12T12:00:00Z"
}
~~~

| Campo | Tipo | Restrições |
|---|---|---|
| id | uuid | Identificador da aba. |
| storeId | uuid | Loja proprietária. |
| key | string | Obrigatório, minúsculo, até 80 caracteres, único na loja. |
| name | string | Obrigatório, até 120 caracteres. |
| contentType | string | `links`, `products`, `markdown` ou `gallery`; ausente na entrada significa `links`. Só pode mudar enquanto a aba nunca teve conteúdo, inclusive inativo. |
| sortOrder | integer | Maior ou igual a zero. |
| isActive | boolean | Estado lógico. |
| createdAtUtc / updatedAtUtc | datetime | UTC. |

CreateTabRequest:

~~~json
{
  "key": "menu",
  "name": "Menu",
  "sortOrder": 0
}
~~~

UpdateTabRequest possui os mesmos campos e aceita também isActive.

`UpdateStoreRequest` aceita também `logoMediaId` para selecionar uma mídia já enviada à mesma loja.

### LinkResponse

Usado somente na área administrativa. A URL externa é exposta apenas para o administrador.

~~~json
{
  "id": "bf8e3504-249b-4e4d-a6bd-3b01b89c2002",
  "storeId": "9b7d6d9d-7d20-4c2e-ae60-7c3f4b9af100",
  "storeTabId": "d1c95cc2-d5fb-41aa-97e0-4d4e1d6bb001",
  "type": "instagram",
  "label": "Instagram",
  "icon": "instagram",
  "url": "https://instagram.com/example",
  "sortOrder": 0,
  "isActive": true,
  "createdAtUtc": "2026-08-12T12:00:00Z",
  "updatedAtUtc": "2026-08-12T12:00:00Z"
}
~~~

| Campo | Tipo | Nullable | Restrições |
|---|---|---:|---|
| id | uuid | não | Identificador. |
| storeId | uuid | não | Loja proprietária. |
| storeTabId | uuid | sim | Aba da mesma loja ou null para raiz. |
| type | string | não | Até 80 caracteres, minúsculo. Tipos conhecidos: instagram, facebook, whatsapp, website, email, phone. Novos tipos não exigem migration. |
| label | string | não | Até 160 caracteres. |
| icon | string | sim | Até 120 caracteres; identificador de ícone do frontend. |
| url | string | não | URL HTTP/HTTPS com host e sem credenciais; até 2048 caracteres. |
| sortOrder | integer | não | Maior ou igual a zero. |
| isActive | boolean | não | Estado lógico. |
| createdAtUtc / updatedAtUtc | datetime | não | UTC. |

CreateLinkRequest:

~~~json
{
  "type": "instagram",
  "label": "Instagram",
  "icon": "instagram",
  "url": "https://instagram.com/example",
  "sortOrder": 0,
  "storeTabId": null
}
~~~

UpdateLinkRequest possui os mesmos campos e aceita também isActive. Para mover o link para a raiz, envie storeTabId: null.

### DTOs públicos

PublicStoreResponse:

~~~json
{
  "id": "9b7d6d9d-7d20-4c2e-ae60-7c3f4b9af100",
  "name": "Café Central",
  "slug": "cafe-central",
  "publicCode": "s_9b7d6d9d7d204c2eae607c3f4b9af100",
  "description": "Café no centro",
  "logoUrl": "https://cdn.example.com/logo.png",
  "primaryColor": "#112233",
  "secondaryColor": null,
  "links": [
    {
      "id": "bf8e3504-249b-4e4d-a6bd-3b01b89c2002",
      "type": "website",
      "label": "Site",
      "icon": "globe",
      "href": "/r/bf8e3504-249b-4e4d-a6bd-3b01b89c2002"
    }
  ],
  "tabs": [
    {
      "id": "d1c95cc2-d5fb-41aa-97e0-4d4e1d6bb001",
      "key": "menu",
      "name": "Menu",
      "sortOrder": 0,
      "links": []
    }
  ]
}
~~~

A resposta pública contém somente loja, abas ativas e links ativos. Links sem aba ficam em links; links associados ficam em tabs[].links. href sempre é /r/{linkId}. A URL externa nunca aparece.

PublicLinkResponse possui id, type, label, icon nullable e href.

RegisterPageViewRequest:

~~~json
{
  "source": "Nfc"
}
~~~

source é opcional; omitido significa Direct. Valores JSON: Nfc, QrCode, Direct e Unknown.

### StoreAnalyticsResponse

~~~json
{
  "views": 120,
  "clicks": 87,
  "ctr": 72.5,
  "sources": {
    "Nfc": 50,
    "QrCode": 60,
    "Direct": 10,
    "Unknown": 0
  },
  "topLinks": [
    {
      "linkId": "bf8e3504-249b-4e4d-a6bd-3b01b89c2002",
      "label": "Instagram",
      "type": "instagram",
      "clicks": 48
    }
  ],
  "viewsByDay": [
    {
      "date": "2026-08-12",
      "views": 120
    }
  ]
}
~~~

ctr é percentual: clicks * 100 / views, arredondado para duas casas. Com views zero, ctr é 0. O valor não é limitado a 100.

## 4. Autenticação

### POST /api/admin/auth/login

Autentica o administrador bootstrap. Não exige token.

~~~bash
curl -i -X POST http://localhost:5112/api/admin/auth/login -H 'Content-Type: application/json' -d '{"email":"admin@nearbly.local","password":"NearblyLocal123"}'
~~~

Resposta 200: LoginResponse. Possíveis respostas: 200, 400, 401 e 429.

O limite inicial é de cinco tentativas por minuto por endereço remoto. Não existe refresh token.

## 5. Endpoints administrativos

Todos exigem Authorization: Bearer <accessToken>.

### Lojas

#### GET /api/admin/stores

Lista StoreResponse[], ativos e inativos. Respostas: 200, 401 e 403.

#### GET /api/admin/stores/{storeId}

Busca uma loja por UUID. Respostas: 200 StoreResponse, 401, 403 e 404.

#### POST /api/admin/stores

Cria uma loja usando CreateStoreRequest. Retorna 201 StoreResponse e Location: /api/admin/stores/{storeId}. Respostas: 201, 400, 401, 403 e 409. O conflito normalmente indica slug existente.

#### PUT /api/admin/stores/{storeId}

Atualiza a loja usando UpdateStoreRequest. Retorna 200 StoreResponse. Respostas: 200, 400, 401, 403, 404 e 409.

#### DELETE /api/admin/stores/{storeId}

Desativa logicamente, sem remover filhos ou eventos. Retorna 204 sem corpo. Respostas: 204, 401, 403 e 404.

### Abas

#### GET /api/admin/stores/{storeId}/tabs

Lista TabResponse[], ativos e inativos, ordenados por sortOrder e id. Respostas: 200, 401, 403 e 404.

#### GET /api/admin/stores/{storeId}/tabs/{tabId}

Busca uma aba pertencente à loja da rota. Retorna 200 TabResponse. Respostas: 200, 401, 403 e 404.

#### POST /api/admin/stores/{storeId}/tabs

Cria usando CreateTabRequest. Retorna 201 TabResponse e Location: /api/admin/stores/{storeId}/tabs/{tabId}. Respostas: 201, 400, 401, 403, 404 e 409.

#### PUT /api/admin/stores/{storeId}/tabs/{tabId}

Atualiza usando UpdateTabRequest e permite reativar com isActive: true. Retorna 200 TabResponse. Respostas: 200, 400, 401, 403 e 404.

#### DELETE /api/admin/stores/{storeId}/tabs/{tabId}

Desativa logicamente. Links da aba ficam persistidos, mas ocultos enquanto a aba estiver inativa. Retorna 204. Respostas: 204, 401, 403 e 404.

### Links

#### GET /api/admin/stores/{storeId}/links

Lista LinkResponse[], incluindo URL externa e itens inativos. Respostas: 200, 401, 403 e 404.

#### GET /api/admin/stores/{storeId}/links/{linkId}

Busca link pertencente à loja da rota. Retorna 200 LinkResponse. Respostas: 200, 401, 403 e 404.

#### POST /api/admin/stores/{storeId}/links

Cria usando CreateLinkRequest. storeTabId null coloca o link na raiz; um UUID preenchido precisa pertencer à mesma loja. Retorna 201 LinkResponse e Location: /api/admin/stores/{storeId}/links/{linkId}. Respostas: 201, 400, 401, 403, 404 e 409.

Conflitos comuns:

- The selected tab does not belong to this store.
- The link could not be saved.

#### PUT /api/admin/stores/{storeId}/links/{linkId}

Atualiza usando UpdateLinkRequest, incluindo aba e isActive. Retorna 200 LinkResponse. Respostas: 200, 400, 401, 403, 404 e 409.

#### DELETE /api/admin/stores/{storeId}/links/{linkId}

Desativa logicamente e preserva eventos. Retorna 204. Respostas: 204, 401, 403 e 404.

### Analytics

#### GET /api/admin/stores/{storeId}/analytics

Query params opcionais:

| Parâmetro | Tipo | Comportamento |
|---|---|---|
| from | yyyy-MM-dd | Início inclusivo à meia-noite UTC. |
| to | yyyy-MM-dd | Fim inclusivo até o final do dia UTC. |

Exemplos:

~~~text
/api/admin/stores/{storeId}/analytics
/api/admin/stores/{storeId}/analytics?from=2026-08-01
/api/admin/stores/{storeId}/analytics?to=2026-08-12
/api/admin/stores/{storeId}/analytics?from=2026-08-01&to=2026-08-12
~~~

Retorna 200 StoreAnalyticsResponse. Respostas: 200, 400 quando datas são inválidas ou from é posterior a to, 401, 403 e 404.

sources sempre contém Nfc, QrCode, Direct e Unknown. topLinks retorna no máximo dez links, ordenados por cliques decrescentes e depois linkId. viewsByDay só inclui dias com visualizações. Eventos não guardam IP, User-Agent ou dados pessoais.

## 6. Endpoints públicos

### GET /api/public/stores/{identifier}

Retorna a página pública sem token:

~~~bash
curl http://localhost:5112/api/public/stores/s_9b7d6d9d7d204c2eae607c3f4b9af100
~~~

Resposta 200: PublicStoreResponse. Respostas: 200 e 404.

Use `publicCode` como identificador permanente em QR Codes e cartões. Slugs existentes continuam aceitos para compatibilidade, mas deixam de resolver após uma alteração de slug.

Filtros:

- A loja precisa estar ativa.
- Abas inativas são omitidas.
- Links inativos são omitidos.
- Link de aba inativa é omitido.
- Link sem aba aparece em links.
- Link de aba ativa aparece em tabs[].links.
- URL externa nunca é exposta.

### POST /api/public/stores/{identifier}/views

Registra uma visualização sem token:

~~~bash
curl -i -X POST http://localhost:5112/api/public/stores/s_9b7d6d9d7d204c2eae607c3f4b9af100/views -H 'Content-Type: application/json' -d '{"source":"QrCode"}'
~~~

Request opcional: RegisterPageViewRequest. source omitido significa Direct. Retorna 204 sem corpo. Respostas: 204, 400 e 404.

## 7. Redirect rastreado

### GET /r/{linkId}

Registra o clique antes do redirect. Query param src:

| Valor | Origem |
|---|---|
| nfc | Nfc |
| qr, qr_code, qrcode | QrCode |
| direct ou ausente | Direct |
| qualquer outro | Unknown |

~~~bash
curl -i 'http://localhost:5112/r/bf8e3504-249b-4e4d-a6bd-3b01b89c2002?src=qr_code'
~~~

Resposta 302 com header Location contendo a URL externa. Respostas: 302, 404 para link inexistente/inativo e 409 para URL persistida inválida.

Regras:

- Link precisa estar ativo.
- Loja precisa estar ativa.
- Aba inativa não impede o redirect.
- Clique é persistido antes do 302.
- URL aceita somente HTTP/HTTPS sem credenciais.

## 8. Fluxo recomendado

### Frontend administrativo

1. Faça login.
2. Armazene accessToken e use expiresAtUtc.
3. Envie Authorization Bearer nas requests administrativas.
4. Liste lojas.
5. Crie/atualize abas e links com o storeId retornado.
6. Use isActive para reativar registros.
7. Consulte analytics com filtros yyyy-MM-dd.
8. Em 401, descarte o token e volte ao login; em 409, mostre detail para correção; em 400, mostre a validação.

### Página pública

1. Obtenha o `publicCode` da loja.
2. Faça GET /api/public/stores/{identifier} usando esse código.
3. Renderize links e tabs.
4. Use href exatamente como retornado, resolvendo contra a origem da API.
5. Registre uma visualização por carregamento real.
6. Nunca espere URL externa no DTO público.

### NFC e QR

- NFC: visualização com source Nfc quando aplicável.
- QR: visualização com source QrCode.
- Links devem usar href /r/{linkId}; acrescente src=nfc ou src=qr_code quando necessário.

## 9. Tipos TypeScript

~~~typescript
export type TrafficSource = "Nfc" | "QrCode" | "Direct" | "Unknown";

export interface LoginRequest { email: string; password: string; }
export interface LoginResponse {
  accessToken: string;
  tokenType: "Bearer";
  expiresAtUtc: string;
}

export interface StoreResponse {
  id: string;
  name: string;
  slug: string;
  publicCode: string;
  description: string | null;
  logoUrl: string | null;
  logoMediaId: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface TabResponse {
  id: string;
  storeId: string;
  key: string;
  name: string;
  contentType: 'links' | 'products' | 'markdown' | 'gallery';
  sortOrder: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface AdminLinkResponse {
  id: string;
  storeId: string;
  storeTabId: string | null;
  type: string;
  label: string;
  icon: string | null;
  url: string;
  sortOrder: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PublicLinkResponse {
  id: string;
  type: string;
  label: string;
  icon: string | null;
  href: string;
}

export interface PublicTabResponse {
  id: string;
  key: string;
  name: string;
  contentType: 'links' | 'products' | 'markdown' | 'gallery';
  sortOrder: number;
  links: PublicLinkResponse[];
  products: PublicProductResponse[];
  markdownBlocks: PublicMarkdownBlockResponse[];
  galleryItems: PublicGalleryItemResponse[];
}

export interface PublicProductResponse { id: string; name: string; description: string | null; imageUrl: string; price: number | null; isAvailable: boolean; sortOrder: number; }
export interface PublicMarkdownBlockResponse { id: string; title: string | null; markdown: string; sortOrder: number; }
export interface PublicGalleryItemResponse { id: string; imageUrl: string; altText: string; caption: string | null; sortOrder: number; }

export interface PublicStoreResponse {
  id: string;
  name: string;
  slug: string;
  publicCode: string;
  description: string | null;
  logoUrl: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  links: PublicLinkResponse[];
  tabs: PublicTabResponse[];
}

export interface StoreAnalyticsResponse {
  views: number;
  clicks: number;
  ctr: number;
  sources: Record<TrafficSource, number>;
  topLinks: Array<{ linkId: string; label: string; type: string; clicks: number; }>;
  viewsByDay: Array<{ date: string; views: number; }>;
}
~~~

## 10. Configuração local

O arquivo .env local não é versionado:

| Variável | Obrigatória | Uso |
|---|---:|---|
| ConnectionStrings__Default | sim | Connection string PostgreSQL. |
| POSTGRES_DB | sim no Compose | Nome do banco. |
| POSTGRES_USER | sim no Compose | Usuário PostgreSQL. |
| POSTGRES_PASSWORD | sim no Compose | Senha PostgreSQL; deve coincidir com a connection string. |
| DATABASE_PORT | não | Porta publicada; padrão 5432. |
| Jwt__Issuer | sim | Emissor JWT. |
| Jwt__Audience | sim | Audiência JWT. |
| Jwt__SigningKey | sim | Mínimo de 32 bytes. |
| Jwt__ExpirationMinutes | não | Padrão 60. |
| BootstrapAdmin__Email | sim para criar admin | Email inicial. |
| BootstrapAdmin__Password | sim para criar admin | Senha inicial, não redefinida depois. |
| BootstrapAdmin__DisplayName | não | Configuração reservada para identificação futura. |
| Cors__AllowedOrigins__0 | não | Origem do frontend, por exemplo http://localhost:4321. |
| Swagger__Enabled | não | Necessário somente fora de Development. |
| Media__Provider | não | `filesystem` (padrão) ou `s3`. |
| Media__RootPath | não | Diretório do storage local. No Compose, use `/var/lib/nearbly/media`. |
| Media__S3__Endpoint | quando S3 | Endpoint S3 compatível. |
| Media__S3__Bucket | quando S3 | Bucket privado. |
| Media__S3__AccessKey / Media__S3__SecretKey | quando S3 | Credenciais do storage; nunca versionar. |

Comandos:

~~~bash
cd /Users/rafael/conductor/workspaces/Nearbly/porto
set -a; source .env; set +a
docker compose up -d postgres
dotnet ef database update --project Nearbly.Backend/src/Nearbly.Infrastructure --startup-project Nearbly.Backend/src/Nearbly.Api
dotnet run --project Nearbly.Backend/src/Nearbly.Api
~~~

## 11. Compatibilidade e evolução

- Adicione propriedades opcionais antes de torná-las obrigatórias.
- Não renomeie propriedades sem uma versão de contrato.
- Novos tipos de link podem ser strings sem migration.
- Novas origens exigem atualização coordenada de enum, parser e frontend.
- Preserve id, slug, href, createdAtUtc e updatedAtUtc.
- Nunca use LinkResponse.url para montar páginas públicas.
- Mudanças persistidas devem incluir migration e testes de integração.
- Mudanças em status, erros, enums ou campos devem atualizar este arquivo, Swagger e testes.

## 12. Conteúdo variado e mídia

### Tipos de aba

`TabResponse.contentType` é sempre retornado como `links`, `products`, `markdown` ou `gallery`. Clientes antigos podem omitir o campo ao criar ou atualizar uma aba; nesse caso a API mantém ou assume `links`. Uma aba só pode trocar de tipo quando não possui nenhum conteúdo associado, mesmo que esteja desativado.

Links associados a uma aba precisam apontar para uma aba `links`. A API valida a loja da rota, a loja da aba e a loja da mídia antes de criar qualquer conteúdo.

### Upload de mídia

`POST /api/admin/stores/{storeId}/media` exige JWT e `multipart/form-data` com o campo `file`. JPEG, PNG e WebP são aceitos até 5 MB. A API valida a assinatura real do arquivo, remove metadados, limita a maior dimensão a 1600 px e armazena uma versão WebP otimizada.

Resposta `201 MediaResponse`:

~~~json
{
  "id": "f1c8b6d0-4c13-4fd4-86f1-0f05cb700001",
  "url": "/media/f1c8b6d0-4c13-4fd4-86f1-0f05cb700001",
  "mimeType": "image/webp",
  "sizeBytes": 28412,
  "width": 1200,
  "height": 800,
  "isActive": true,
  "createdAtUtc": "2026-08-12T12:00:00Z"
}
~~~

`GET /media/{mediaId}` é anônimo, não expõe a chave privada do storage e retorna cache público. `DELETE /api/admin/stores/{storeId}/media/{mediaId}` só desativa mídia sem referência. Mídias usadas por logo, produto ou galeria retornam `409`.

Para usar a mídia como logo, envie `logoMediaId` em `PUT /api/admin/stores/{storeId}`. `logoUrl` continua aceitando URLs externas para compatibilidade; a mídia interna tem prioridade.

### CRUD de conteúdo

Todos os endpoints abaixo exigem JWT, usam desativação lógica e retornam ativos e inativos nas listagens administrativas:

| Método | Rota | Corpo de criação |
|---|---|---|
| GET/POST | `/api/admin/stores/{storeId}/tabs/{tabId}/products` | `name`, `description`, `mediaAssetId`, `price`, `isAvailable`, `sortOrder` |
| GET/PUT/DELETE | `/api/admin/stores/{storeId}/tabs/{tabId}/products/{id}` | mesmos campos; update aceita `isActive` |
| GET/POST | `/api/admin/stores/{storeId}/tabs/{tabId}/markdown-blocks` | `title`, `markdown`, `sortOrder` |
| GET/PUT/DELETE | `/api/admin/stores/{storeId}/tabs/{tabId}/markdown-blocks/{id}` | mesmos campos; update aceita `isActive` |
| GET/POST | `/api/admin/stores/{storeId}/tabs/{tabId}/gallery-items` | `mediaAssetId`, `altText`, `caption`, `sortOrder` |
| GET/PUT/DELETE | `/api/admin/stores/{storeId}/tabs/{tabId}/gallery-items/{id}` | mesmos campos; update aceita `isActive` |

Produtos exigem imagem e aceitam preço BRL opcional maior ou igual a zero. `altText` é obrigatório para itens de galeria. `sortOrder` nunca pode ser negativo. Conteúdo criado no endpoint de tipo diferente da aba retorna `409`.

### Resposta pública

Cada `PublicTabResponse` contém sempre as quatro coleções, com as não aplicáveis vazias:

~~~json
{
  "id": "d1c95cc2-d5fb-41aa-97e0-4d4e1d6bb001",
  "key": "cardapio",
  "name": "Cardápio",
  "contentType": "products",
  "sortOrder": 0,
  "links": [],
  "products": [{
    "id": "bf8e3504-249b-4e4d-a6bd-3b01b89c2002",
    "name": "Café coado",
    "description": "250 ml",
    "imageUrl": "/media/f1c8b6d0-4c13-4fd4-86f1-0f05cb700001",
    "price": 8.5,
    "isAvailable": true,
    "sortOrder": 0
  }],
  "markdownBlocks": [],
  "galleryItems": []
}
~~~

As abas públicas ativas são ordenadas por `sortOrder` e `id`; itens de cada coleção seguem a mesma regra. Produtos e galeria não geram cliques ou redirects. Markdown é retornado como texto e deve ser renderizado com sanitização no cliente; HTML arbitrário não faz parte do contrato. `PublicStoreResponse.links` continua presente para links legados sem aba.

## 13. Fora do MVP

Não fazem parte do contrato atual: refresh token, paginação administrativa, gerenciamento de administradores, roles, filas, webhooks e analytics por IP/User-Agent.
