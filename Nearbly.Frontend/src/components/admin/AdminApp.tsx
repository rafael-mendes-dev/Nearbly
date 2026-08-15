import { useEffect, useState } from "react";
import {
  QueryClient,
  QueryClientProvider,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import {
  DndContext,
  type DragEndEvent,
  PointerSensor,
  closestCenter,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  SortableContext,
  arrayMove,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, type Control, useForm, useWatch } from "react-hook-form";
import {
  BrowserRouter,
  Link,
  Navigate,
  NavLink,
  Route,
  Routes,
  useLocation,
  useNavigate,
  useParams,
} from "react-router-dom";
import {
  ArrowLeft,
  BarChart3,
  Camera,
  Check,
  ChevronDown,
  ExternalLink,
  Globe2,
  GripVertical,
  LayoutDashboard,
  Link2,
  LogOut,
  Mail,
  MapPin,
  Menu,
  Phone,
  Plus,
  Settings,
  Store,
  Tags,
  Trash2,
  Upload,
  X,
} from "lucide-react";
import { z } from "zod";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { api, API_BASE_URL } from "../../lib/api/client";
import { ApiError, problemMessage } from "../../lib/api/problem";
import {
  clearSession,
  readSession,
  saveSession,
  type Session,
} from "../../lib/auth/session";
import {
  fillDateGaps,
  formatNumber,
  formatPercent,
  isSafeColor,
} from "../../lib/format";
import type {
  AdminLinkResponse,
  LinkInput,
  LinkUpdate,
  StoreInput,
  StoreResponse,
  StoreUpdate,
  TabInput,
  TabResponse,
  TabUpdate,
} from "../../lib/api/types";
import ContentPage from "./ContentPage";
import { SpotlightCard } from "../react-bits/SpotlightCard";
import "./admin.css";

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
});

function useSessionState() {
  const [session, setSession] = useState<Session | null>(() => readSession());
  useEffect(() => {
    const handleExpired = () => setSession(null);
    window.addEventListener("nearbly:session-expired", handleExpired);
    return () => window.removeEventListener("nearbly:session-expired", handleExpired);
  }, []);
  return {
    session,
    login: (value: Session) => {
      saveSession(value);
      setSession(value);
    },
    logout: () => {
      clearSession();
      setSession(null);
    },
  };
}

const storeSchema = z.object({
  name: z.string().trim().min(2, "Informe o nome da loja.").max(160),
  slug: z.string().trim().min(2, "Informe o slug.").max(120),
  description: z.string().max(500).optional(),
  logoUrl: z
    .union([z.url("Use uma URL válida."), z.literal("")])
    .optional(),
  primaryColor: z
    .string()
    .regex(/^#[0-9A-Fa-f]{6}$/, "Use #RRGGBB.")
    .optional()
    .or(z.literal("")),
  secondaryColor: z
    .string()
    .regex(/^#[0-9A-Fa-f]{6}$/, "Use #RRGGBB.")
    .optional()
    .or(z.literal("")),
});
const loginSchema = z.object({
  email: z.email("Informe um email válido."),
  password: z.string().min(1, "Informe sua senha."),
});
const tabSchema = z.object({
  key: z.string().trim().min(1).max(80),
  name: z.string().trim().min(1).max(120),
  sortOrder: z.number().int().min(0),
});
const linkSchema = z.object({
  type: z.string().trim().min(1).max(80),
  label: z.string().trim().min(1).max(160),
  icon: z.string().max(120).optional(),
  url: z
    .url("Use uma URL válida.")
    .refine(
      (value) => ["http:", "https:"].includes(new URL(value).protocol),
      "Use http ou https.",
    ),
  sortOrder: z.number().int().min(0),
  storeTabId: z.string().optional(),
});

type StoreFormValues = z.infer<typeof storeSchema>;
type TabFormValues = z.infer<typeof tabSchema>;
type LinkFormValues = z.infer<typeof linkSchema>;

const linkTypeOptions = [
  {
    value: "website",
    label: "Site",
    description: "Página principal, catálogo ou loja virtual.",
    placeholder: "https://seusite.com.br",
  },
  {
    value: "instagram",
    label: "Instagram",
    description: "Perfil, publicação ou conteúdo no Instagram.",
    placeholder: "https://instagram.com/seuperfil",
  },
  {
    value: "facebook",
    label: "Facebook",
    description: "Página ou publicação no Facebook.",
    placeholder: "https://facebook.com/suapagina",
  },
  {
    value: "whatsapp",
    label: "WhatsApp",
    description: "Conversa direta pelo WhatsApp.",
    placeholder: "https://wa.me/5541999999999",
  },
  {
    value: "email",
    label: "E-mail",
    description: "Página de contato ou formulário de atendimento.",
    placeholder: "https://seusite.com.br/contato",
  },
  {
    value: "phone",
    label: "Telefone",
    description: "Página com telefone e canais de atendimento.",
    placeholder: "https://seusite.com.br/atendimento",
  },
  {
    value: "location",
    label: "Localização",
    description: "Endereço ou rota no Google Maps.",
    placeholder: "https://maps.google.com/...",
  },
] as const;

const linkIconOptions = [
  { value: "", label: "Automático", description: "Usa o ícone do tipo escolhido." },
  { value: "globe", label: "Site", description: "Globo" },
  { value: "instagram", label: "Instagram", description: "Câmera" },
  { value: "whatsapp", label: "WhatsApp", description: "Contato" },
  { value: "phone", label: "Telefone", description: "Telefone" },
  { value: "email", label: "E-mail", description: "Envelope" },
  { value: "location", label: "Localização", description: "Marcador de mapa" },
  { value: "external", label: "Link externo", description: "Seta externa" },
] as const;

const linkIcon = (value: string | null | undefined, size = 18) => {
  const common = { size, strokeWidth: 1.8 };
  if (value === "instagram" || value === "camera") return <Camera {...common} />;
  if (value === "email" || value === "mail") return <Mail {...common} />;
  if (value === "phone" || value === "whatsapp") return <Phone {...common} />;
  if (value === "website" || value === "globe" || value === "facebook") return <Globe2 {...common} />;
  if (value === "map" || value === "location") return <MapPin {...common} />;
  return <ExternalLink {...common} />;
};

const linkTypeLabel = (value: string) =>
  linkTypeOptions.find((option) => option.value === value)?.label ?? value;

const fieldError = (message?: string) =>
  message ? <small className="field-error">{message}</small> : null;

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <label className="field">
      <span>{label}</span>
      {children}
      {fieldError(error)}
    </label>
  );
}

function ColorField({
  control,
  name,
  label,
  fallback,
  error,
}: {
  control: Control<StoreFormValues>;
  name: "primaryColor" | "secondaryColor";
  label: string;
  fallback: `#${string}`;
  error?: string;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field }) => {
        const color = isSafeColor(field.value) ? field.value : fallback;

        return (
          <fieldset className="field color-field">
            <legend>{label}</legend>
            <div className="color-picker-control">
              <input
                className="color-picker-input"
                type="color"
                value={color}
                onChange={(event) => field.onChange(event.target.value.toUpperCase())}
                aria-label={`Selecionar ${label.toLocaleLowerCase("pt-BR")}`}
                title={`Selecionar ${label.toLocaleLowerCase("pt-BR")}`}
              />
              <input
                ref={field.ref}
                className="color-hex-input"
                name={field.name}
                value={field.value ?? ""}
                onBlur={field.onBlur}
                onChange={(event) => field.onChange(event.target.value.toUpperCase())}
                placeholder={fallback}
                inputMode="text"
                maxLength={7}
                autoComplete="off"
                aria-label={`${label} em hexadecimal`}
              />
              <button
                className="color-clear"
                type="button"
                onClick={() => field.onChange("")}
                disabled={!field.value}
                aria-label={`Usar a ${label.toLocaleLowerCase("pt-BR")} padrão`}
                title="Usar cor padrão"
              >
                <X size={16} />
              </button>
            </div>
            {fieldError(error)}
          </fieldset>
        );
      }}
    />
  );
}

function AdminShell({
  session,
  logout,
}: {
  session: Session;
  logout: () => void;
}) {
  const [mobileNav, setMobileNav] = useState(false);
  const location = useLocation();
  const storeId = location.pathname.split("/")[2];
  const storeQuery = useQuery({
    queryKey: ["store", storeId],
    queryFn: () => api.store(storeId ?? "", session.accessToken),
    enabled: Boolean(storeId),
  });
  const navItems = storeId
    ? [
        {
          to: `/lojas/${storeId}/visao-geral`,
          label: "Visão geral",
          icon: LayoutDashboard,
        },
        { to: `/lojas/${storeId}/conteudo`, label: "Conteúdo", icon: Tags },
        {
          to: `/lojas/${storeId}/configuracoes`,
          label: "Configurações",
          icon: Settings,
        },
      ]
    : [];
  return (
    <div className="admin-frame">
      <aside className={`admin-sidebar ${mobileNav ? "is-open" : ""}`}>
        <div className="admin-brand">
          <img className="brand-mark" src="/brand/logo-mark-white.svg" alt="" />
          <span>Nearbly</span>
          <button
            className="button-icon admin-close"
            onClick={() => setMobileNav(false)}
            aria-label="Fechar menu"
          >
            <X size={18} />
          </button>
        </div>
        <div className="admin-sidebar-label">Workspace</div>
        <NavLink
          className="admin-main-link"
          to="/lojas"
          onClick={() => setMobileNav(false)}
        >
          <Store size={18} /> Lojas
        </NavLink>
        {storeQuery.data && (
          <div className="admin-context">
            <span>Loja ativa</span>
            <strong>{storeQuery.data.name}</strong>
          </div>
        )}
        <nav className="admin-nav" aria-label="Navegação da loja">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              className={({ isActive }) => (isActive ? "is-active" : "")}
              to={to}
              onClick={() => setMobileNav(false)}
            >
              <Icon size={17} />
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="admin-sidebar-bottom">
          <a
            className="admin-public-link"
            href={storeQuery.data ? `/${storeQuery.data.publicCode}` : "/"}
            target="_blank"
            rel="noreferrer"
          >
            <ExternalLink size={16} /> Ver página pública
          </a>
          <button className="admin-logout" type="button" onClick={logout}>
            <LogOut size={16} /> Sair
          </button>
        </div>
      </aside>
      <div className="admin-main">
        <header className="admin-topbar">
          <button
            className="button-icon admin-menu"
            onClick={() => setMobileNav(true)}
            aria-label="Abrir menu"
          >
            <Menu size={19} />
          </button>
          <div>
            <span className="admin-breadcrumb">Nearbly / Administração</span>
            <strong>
              {location.pathname.includes("lojas")
                ? "Conteúdo da loja"
                : "Lojas"}
            </strong>
          </div>
          <span className="admin-session">{session.tokenType}</span>
        </header>
        <div className="admin-content">
          <Routes>
            <Route
              path="/lojas"
              element={<StoreListPage token={session.accessToken} />}
            />
            <Route
              path="/lojas/:storeId/visao-geral"
              element={<OverviewPage token={session.accessToken} />}
            />
            <Route
              path="/lojas/:storeId/links"
              element={<LegacyContentRedirect />}
            />
            <Route
              path="/lojas/:storeId/abas"
              element={<LegacyContentRedirect />}
            />
            <Route
              path="/lojas/:storeId/conteudo"
              element={<ContentPage token={session.accessToken} />}
            />
            <Route
              path="/lojas/:storeId/configuracoes"
              element={<SettingsPage token={session.accessToken} />}
            />
            <Route path="*" element={<Navigate to="/lojas" replace />} />
          </Routes>
        </div>
      </div>
    </div>
  );
}

function LoginPage({ onLogin }: { onLogin: (session: Session) => void }) {
  const navigate = useNavigate();
  const [error, setError] = useState("");
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<{ email: string; password: string }>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });
  const submit = handleSubmit(async (values) => {
    setError("");
    try {
      const response = await api.login(values);
      onLogin(response);
      navigate("/lojas");
    } catch (cause) {
      setError(problemMessage(cause, "Email ou senha inválidos."));
    }
  });
  return (
    <main className="login-page">
      <div className="login-aside">
        <a className="brand" href="/">
          <img className="brand-mark" src="/brand/logo-mark-white.svg" alt="" />
          <span>Nearbly</span>
        </a>
        <div>
          <span className="eyebrow">
            Área administrativa
          </span>
          <h1>Seu negócio, em perspectiva.</h1>
          <p>
            Organize links, acompanhe acessos e mantenha sua página pública
            sempre atualizada.
          </p>
        </div>
        <span className="login-aside-meta">Nearbly para negócios locais</span>
      </div>
      <section className="login-panel">
        <div className="login-form-wrap">
          <a className="login-back" href="/">
            <ArrowLeft size={16} /> Voltar ao site
          </a>
          <span className="eyebrow">Entrar no painel</span>
          <h2>Bem-vindo de volta.</h2>
          <p className="login-intro">
            Use as credenciais administrativas da sua conta.
          </p>
          <form onSubmit={submit}>
            <Field label="Email" error={errors.email?.message}>
              <input
                {...register("email")}
                type="email"
                autoComplete="username"
                placeholder="voce@negocio.com"
              />
            </Field>
            <Field label="Senha" error={errors.password?.message}>
              <input
                {...register("password")}
                type="password"
                autoComplete="current-password"
                placeholder="Sua senha"
              />
            </Field>
            {error && (
              <div className="alert alert-error" role="alert">
                {error}
              </div>
            )}
            <button
              className="button button-dark"
              disabled={isSubmitting}
              type="submit"
            >
              {isSubmitting ? "Entrando…" : "Entrar"}
              <ChevronDown size={17} />
            </button>
          </form>
        </div>
      </section>
    </main>
  );
}

function StoreListPage({ token }: { token: string }) {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState<StoreResponse | null | undefined>(
    undefined,
  );
  const query = useQuery({
    queryKey: ["stores"],
    queryFn: () => api.stores(token),
  });
  const mutation = useMutation({
    mutationFn: (input: StoreInput | { id: string; input: StoreUpdate }) =>
      "id" in input
        ? api.updateStore(input.id, input.input, token)
        : api.createStore(input, token),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["stores"] });
      setEditing(undefined);
    },
  });
  const deactivate = useMutation({
    mutationFn: (id: string) => api.deactivateStore(id, token),
    onSuccess: () =>
      void queryClient.invalidateQueries({ queryKey: ["stores"] }),
  });
  return (
    <section className="admin-section">
      <div className="section-top">
        <div>
          <span className="eyebrow">Workspace</span>
          <h1>Lojas</h1>
          <p>Escolha um espaço para administrar ou crie uma nova página.</p>
        </div>
        <button className="button button-dark" onClick={() => setEditing(null)}>
          <Plus size={17} /> Nova loja
        </button>
      </div>
      {query.isLoading && <Loading />}{" "}
      {query.error && <ErrorState error={query.error} />}
      {editing !== undefined && (
        <StoreForm
          store={editing}
          saving={mutation.isPending}
          error={mutation.error}
          onCancel={() => setEditing(undefined)}
          onSave={(input) =>
            editing
              ? mutation.mutate({
                  id: editing.id,
                  input: { ...input, isActive: editing.isActive },
                })
              : mutation.mutate(input)
          }
        />
      )}
      {query.data && (
        <div className="store-grid">
          {query.data.map((store) => (
            <SpotlightCard
              className={`store-card ${store.isActive ? "" : "is-inactive"}`}
              key={store.id}
            >
              <div className="store-card-top">
                <div
                  className="store-avatar"
                  style={{
                    background: isSafeColor(store.primaryColor)
                      ? store.primaryColor
                      : "var(--primary)",
                  }}
                >
                  {store.name.slice(0, 2).toUpperCase()}
                </div>
                <span
                  className={`status ${store.isActive ? "status-active" : "status-inactive"}`}
                >
                  {store.isActive ? "Ativa" : "Inativa"}
                </span>
              </div>
              <h2>{store.name}</h2>
              <p>{store.description || "Sem descrição cadastrada."}</p>
              <span className="store-slug">/{store.slug}</span>
              <div className="store-card-actions">
                <Link
                  className="button button-dark"
                  to={`/lojas/${store.id}/visao-geral`}
                >
                  Abrir painel
                </Link>
                <button
                  className="button-icon"
                  onClick={() => setEditing(store)}
                  aria-label={`Editar ${store.name}`}
                >
                  <Settings size={17} />
                </button>
                {store.isActive && (
                  <button
                    className="button-icon"
                    onClick={() =>
                      window.confirm(`Desativar ${store.name}?`) &&
                      deactivate.mutate(store.id)
                    }
                    aria-label={`Desativar ${store.name}`}
                  >
                    <Trash2 size={17} />
                  </button>
                )}
              </div>
            </SpotlightCard>
          ))}
        </div>
      )}
    </section>
  );
}

function StoreForm({
  store,
  saving,
  error,
  onCancel,
  onSave,
}: {
  store: StoreResponse | null;
  saving: boolean;
  error: unknown;
  onCancel: () => void;
  onSave: (input: StoreInput) => void;
}) {
  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<StoreFormValues>({
    resolver: zodResolver(storeSchema),
    defaultValues: store
      ? {
          name: store.name,
          slug: store.slug,
          description: store.description ?? "",
          logoUrl: store.logoMediaId ? "" : store.logoUrl ?? "",
          primaryColor: store.primaryColor ?? "",
          secondaryColor: store.secondaryColor ?? "",
        }
      : {
          name: "",
          slug: "",
          description: "",
          logoUrl: "",
          primaryColor: "#2B22E0",
          secondaryColor: "#06080F",
        },
  });
  return (
    <div className="form-sheet">
      <div className="sheet-heading">
        <div>
          <span className="eyebrow">{store ? "Editar loja" : "Nova loja"}</span>
          <h2>{store ? store.name : "Cadastrar espaço"}</h2>
        </div>
        <button
          className="button-icon"
          onClick={onCancel}
          aria-label="Fechar formulário"
        >
          <X size={18} />
        </button>
      </div>
      <form className="form-grid" onSubmit={handleSubmit(onSave)}>
        <Field label="Nome" error={errors.name?.message}>
          <input {...register("name")} placeholder="Café Central" />
        </Field>
        <Field label="Slug público" error={errors.slug?.message}>
          <input {...register("slug")} placeholder="cafe-central" />
        </Field>
        <Field label="Descrição" error={errors.description?.message}>
          <textarea
            {...register("description")}
            rows={3}
            placeholder="Uma frase sobre o negócio"
          />
        </Field>
        <Field label="Logo URL" error={errors.logoUrl?.message}>
          <input {...register("logoUrl")} placeholder="https://..." />
        </Field>
        <ColorField
          control={control}
          name="primaryColor"
          label="Cor principal"
          fallback="#2B22E0"
          error={errors.primaryColor?.message}
        />
        <ColorField
          control={control}
          name="secondaryColor"
          label="Cor secundária"
          fallback="#06080F"
          error={errors.secondaryColor?.message}
        />
        {error ? (
          <div className="alert alert-error">{problemMessage(error)}</div>
        ) : null}
        <div className="sheet-actions">
          <button
            className="button button-quiet"
            type="button"
            onClick={onCancel}
          >
            Cancelar
          </button>
          <button
            className="button button-dark"
            type="submit"
            disabled={saving}
          >
            {saving ? "Salvando…" : "Salvar loja"}
          </button>
        </div>
      </form>
    </div>
  );
}

function WorkspaceHeader({
  store,
  eyebrow,
  title,
  description,
}: {
  store: StoreResponse | undefined;
  eyebrow: string;
  title: string;
  description: string;
}) {
  return (
    <div className="section-top">
      <div>
        <span className="eyebrow">
          {eyebrow}
          {store ? ` / ${store.name}` : ""}
        </span>
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
      {store && (
        <span
          className={`status ${store.isActive ? "status-active" : "status-inactive"}`}
        >
          {store.isActive ? "Ativa" : "Inativa"}
        </span>
      )}
    </div>
  );
}

function useWorkspace(token: string) {
  const storeId = useParams<{ storeId: string }>().storeId ?? "";
  const store = useQuery({
    queryKey: ["store", storeId],
    queryFn: () => api.store(storeId, token),
    enabled: Boolean(storeId),
  });
  return { storeId, store };
}

function OverviewPage({ token }: { token: string }) {
  const { storeId, store } = useWorkspace(token);
  const [from, setFrom] = useState(() =>
    new Date(Date.now() - 29 * 86400000).toISOString().slice(0, 10),
  );
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const analytics = useQuery({
    queryKey: ["analytics", storeId, from, to],
    queryFn: () => api.analytics(storeId, token, from, to),
    enabled: Boolean(storeId && from && to),
  });
  const series = fillDateGaps(analytics.data?.viewsByDay ?? [], from, to);
  return (
    <section className="admin-section">
      <WorkspaceHeader
        store={store.data}
        eyebrow="Analytics"
        title="Visão geral"
        description="Acompanhe o que acontece depois que alguém encontra sua página."
      />
      <div className="filter-bar">
        <div>
          <label htmlFor="from">De</label>
          <input
            id="from"
            type="date"
            value={from}
            onChange={(event) => setFrom(event.target.value)}
          />
        </div>
        <div>
          <label htmlFor="to">Até</label>
          <input
            id="to"
            type="date"
            value={to}
            onChange={(event) => setTo(event.target.value)}
          />
        </div>
        {from > to && (
          <span className="field-error">
            O início deve ser anterior ao fim.
          </span>
        )}
      </div>
      {analytics.error && <ErrorState error={analytics.error} />}
      {analytics.isLoading ? (
        <Loading />
      ) : (
        analytics.data && (
          <>
            <div className="metric-grid">
              <Metric
                label="Visualizações"
                value={formatNumber(analytics.data.views)}
                icon={<BarChart3 />}
              />
              <Metric
                label="Cliques"
                value={formatNumber(analytics.data.clicks)}
                icon={<Link2 />}
              />
              <Metric
                label="CTR"
                value={formatPercent(analytics.data.ctr)}
                icon={<Check />}
              />
            </div>
            <div className="analytics-grid">
              <section className="panel">
                <div className="panel-heading">
                  <div>
                    <span className="eyebrow">Ritmo de acessos</span>
                    <h2>Visualizações por dia</h2>
                  </div>
                  <span className="panel-note">{series.length} dias</span>
                </div>
                <div className="chart analytics-chart" aria-label="Gráfico de visualizações por dia">
                  {series.length ? (
                    <ResponsiveContainer width="100%" height={220}>
                      <LineChart data={series} margin={{ top: 8, right: 8, left: -24, bottom: 0 }}>
                        <CartesianGrid stroke="#212b42" strokeDasharray="3 3" vertical={false} />
                        <XAxis dataKey="date" tickFormatter={(date: string) => date.slice(8)} tick={{ fill: '#9aa6bd', fontSize: 11 }} tickLine={false} axisLine={false} />
                        <YAxis allowDecimals={false} tick={{ fill: '#9aa6bd', fontSize: 11 }} tickLine={false} axisLine={false} />
                        <Tooltip labelFormatter={(date) => `Dia ${String(date).slice(8)}`} formatter={(value) => [value, 'visualizações']} />
                        <Line type="monotone" dataKey="views" stroke="#2b22e0" strokeWidth={3} dot={{ fill: '#13bbef', stroke: '#2b22e0', strokeWidth: 2, r: 4 }} activeDot={{ r: 5 }} />
                      </LineChart>
                    </ResponsiveContainer>
                  ) : <p className="empty-state">Sem visualizações no intervalo.</p>}
                </div>
              </section>
              <section className="panel">
                <div className="panel-heading">
                  <div>
                    <span className="eyebrow">Origem</span>
                    <h2>Como chegaram</h2>
                  </div>
                </div>
                <div className="source-list">
                  {Object.entries(analytics.data.sources).map(
                    ([name, value]) => (
                      <div className="source-row" key={name}>
                        <span>
                          {name === "QrCode"
                            ? "QR Code"
                            : name === "Nfc"
                              ? "NFC"
                              : name === "Direct"
                                ? "Direto"
                                : "Desconhecido"}
                        </span>
                        <strong>{formatNumber(value)}</strong>
                      </div>
                    ),
                  )}
                </div>
              </section>
            </div>
            <section className="panel">
              <div className="panel-heading">
                <div>
                  <span className="eyebrow">Links</span>
                  <h2>Mais acessados</h2>
                </div>
                <Link to={`/lojas/${storeId}/links`} className="text-link">
                  Gerenciar links →
                </Link>
              </div>
              {analytics.data.topLinks.length ? (
                <div className="top-links">
                  {analytics.data.topLinks.map((link, index) => (
                    <div className="top-link" key={link.linkId}>
                      <span className="top-link-rank">0{index + 1}</span>
                      <span>
                        {link.label}
                        <small>{link.type}</small>
                      </span>
                      <strong>{formatNumber(link.clicks)}</strong>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="empty-state">Ainda não há cliques registrados.</p>
              )}
            </section>
          </>
        )
      )}
    </section>
  );
}

function Metric({
  label,
  value,
  icon,
}: {
  label: string;
  value: string;
  icon: React.ReactNode;
}) {
  return (
    <SpotlightCard className="metric" spotlightColor="rgba(43, 34, 224, 0.2)">
      <span className="metric-icon">{icon}</span>
      <span>{label}</span>
      <strong>{value}</strong>
    </SpotlightCard>
  );
}

// Kept temporarily as a compatibility implementation for existing deep links.
// eslint-disable-next-line @typescript-eslint/no-unused-vars
function LinksPage({ token }: { token: string }) {
  const { storeId, store } = useWorkspace(token);
  const queryClient = useQueryClient();
  const linksQuery = useQuery({
    queryKey: ["links", storeId],
    queryFn: () => api.links(storeId, token),
    enabled: Boolean(storeId),
  });
  const tabsQuery = useQuery({
    queryKey: ["tabs", storeId],
    queryFn: () => api.tabs(storeId, token),
    enabled: Boolean(storeId),
  });
  const [editing, setEditing] = useState<AdminLinkResponse | null | undefined>(
    undefined,
  );
  const save = useMutation({
    mutationFn: (input: LinkInput | { id: string; input: LinkUpdate }) =>
      "id" in input
        ? api.updateLink(storeId, input.id, input.input, token)
        : api.createLink(storeId, input, token),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["links", storeId] });
      setEditing(undefined);
    },
  });
  const deactivate = useMutation({
    mutationFn: (id: string) => api.deactivateLink(storeId, id, token),
    onSuccess: () =>
      void queryClient.invalidateQueries({ queryKey: ["links", storeId] }),
  });
  const reorder = async (event: DragEndEvent) => {
    if (!linksQuery.data || event.active.id === event.over?.id) return;
    const oldIndex = linksQuery.data.findIndex(
      (link) => link.id === event.active.id,
    );
    const newIndex = linksQuery.data.findIndex(
      (link) => link.id === event.over?.id,
    );
    const ordered = arrayMove(linksQuery.data, oldIndex, newIndex);
    queryClient.setQueryData(
      ["links", storeId],
      ordered.map((link, index) => ({ ...link, sortOrder: index })),
    );
    try {
      await Promise.all(
        ordered.map((link, index) =>
          api.updateLink(
            storeId,
            link.id,
            {
              type: link.type,
              label: link.label,
              icon: link.icon,
              url: link.url,
              storeTabId: link.storeTabId,
              sortOrder: index,
              isActive: link.isActive,
            },
            token,
          ),
        ),
      );
    } catch {
      void queryClient.invalidateQueries({ queryKey: ["links", storeId] });
    }
  };
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
  );
  return (
    <section className="admin-section">
      <WorkspaceHeader
        store={store.data}
        eyebrow="Conteúdo"
        title="Links"
        description="Mantenha as ações mais importantes acessíveis e na ordem certa."
      />
      <div className="section-toolbar">
        <span>{linksQuery.data?.length ?? 0} links cadastrados</span>
        <button className="button button-dark" onClick={() => setEditing(null)}>
          <Plus size={17} /> Novo link
        </button>
      </div>
      {editing !== undefined && (
        <LinkForm
          link={editing}
          tabs={tabsQuery.data ?? []}
          saving={save.isPending}
          error={save.error}
          onCancel={() => setEditing(undefined)}
          onSave={(input) =>
            editing
              ? save.mutate({
                  id: editing.id,
                  input: { ...input, isActive: editing.isActive },
                })
              : save.mutate(input)
          }
        />
      )}
      {linksQuery.error ? (
        <ErrorState error={linksQuery.error} />
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={reorder}
        >
          <SortableContext
            items={(linksQuery.data ?? []).map((link) => link.id)}
            strategy={verticalListSortingStrategy}
          >
            <div className="sortable-list">
              {(linksQuery.data ?? []).map((link) => (
                <SortableLink
                  key={link.id}
                  link={link}
                  onEdit={() => setEditing(link)}
                  onDeactivate={() =>
                    window.confirm(`Desativar ${link.label}?`) &&
                    deactivate.mutate(link.id)
                  }
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}
    </section>
  );
}

function SortableLink({
  link,
  onEdit,
  onDeactivate,
}: {
  link: AdminLinkResponse;
  onEdit: () => void;
  onDeactivate: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform, transition } =
    useSortable({ id: link.id });
  return (
    <article
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition }}
      className={`sortable-item ${link.isActive ? "" : "is-inactive"}`}
    >
      <button
        className="drag-handle"
        {...attributes}
        {...listeners}
        aria-label={`Reordenar ${link.label}`}
      >
        <GripVertical size={18} />
      </button>
      <span className="sortable-icon">{linkIcon(link.icon || link.type)}</span>
      <span className="sortable-copy">
        <strong>{link.label}</strong>
        <small>
          {linkTypeLabel(link.type)} · {link.storeTabId ? "aba" : "raiz"}
        </small>
      </span>
      <span
        className={`status ${link.isActive ? "status-active" : "status-inactive"}`}
      >
        {link.isActive ? "Ativo" : "Inativo"}
      </span>
      <button
        className="button-icon"
        onClick={onEdit}
        aria-label={`Editar ${link.label}`}
      >
        <Settings size={16} />
      </button>
      {link.isActive && (
        <button
          className="button-icon"
          onClick={onDeactivate}
          aria-label={`Desativar ${link.label}`}
        >
          <Trash2 size={16} />
        </button>
      )}
    </article>
  );
}

function LinkForm({
  link,
  tabs,
  saving,
  error,
  onCancel,
  onSave,
}: {
  link: AdminLinkResponse | null;
  tabs: TabResponse[];
  saving: boolean;
  error: unknown;
  onCancel: () => void;
  onSave: (input: LinkInput) => void;
}) {
  const {
    control,
    register,
    setValue,
    handleSubmit,
    formState: { errors },
  } = useForm<LinkFormValues>({
    resolver: zodResolver(linkSchema),
    defaultValues: link
      ? {
          type: link.type,
          label: link.label,
          icon: link.icon ?? "",
          url: link.url,
          sortOrder: link.sortOrder,
          storeTabId: link.storeTabId ?? "",
        }
      : {
          type: "website",
          label: "",
          icon: "",
          url: "",
          sortOrder: 0,
          storeTabId: "",
        },
  });
  const selectedType = useWatch({ control, name: "type" });
  const selectedIcon = useWatch({ control, name: "icon" });
  const selectedTypeOption = linkTypeOptions.find(
    (option) => option.value === selectedType,
  );
  const selectedIconOption = linkIconOptions.find(
    (option) => option.value === selectedIcon,
  );
  const hasCustomType = Boolean(
    selectedType && !linkTypeOptions.some((option) => option.value === selectedType),
  );
  const hasCustomIcon = Boolean(
    selectedIcon && !linkIconOptions.some((option) => option.value === selectedIcon),
  );
  return (
    <div className="form-sheet">
      <div className="sheet-heading">
        <div>
          <span className="eyebrow">{link ? "Editar link" : "Novo link"}</span>
          <h2>{link ? link.label : "Adicionar ação"}</h2>
        </div>
        <button
          className="button-icon"
          onClick={onCancel}
          aria-label="Fechar formulário"
        >
          <X size={18} />
        </button>
      </div>
      <form
        className="form-grid"
        onSubmit={handleSubmit((values) =>
          onSave({
            ...values,
            icon: values.icon || null,
            storeTabId: values.storeTabId || null,
          }),
        )}
      >
        <Field label="Tipo de destino" error={errors.type?.message}>
          <select
            {...register("type", {
              onChange: () =>
                setValue("icon", "", {
                  shouldDirty: true,
                  shouldValidate: true,
                }),
            })}
          >
            {hasCustomType && (
              <option value={selectedType}>{selectedType} — tipo já cadastrado</option>
            )}
            {linkTypeOptions.map((option) => (
              <option value={option.value} key={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          <small className="field-hint">
            {selectedTypeOption?.description ?? "Tipo personalizado já cadastrado."}
          </small>
        </Field>
        <Field label="Texto do link" error={errors.label?.message}>
          <input {...register("label")} placeholder="Ex.: Fale conosco" />
        </Field>
        <Field label="Ícone" error={errors.icon?.message}>
          <div className="icon-select-control">
            <span className="icon-select-preview" aria-hidden="true">
              {linkIcon(selectedIcon || selectedType, 20)}
            </span>
            <select {...register("icon")}>
              {hasCustomIcon && (
                <option value={selectedIcon}>{selectedIcon} — ícone já cadastrado</option>
              )}
              {linkIconOptions.map((option) => (
                <option value={option.value} key={option.value || "automatic"}>
                  {option.label}{option.value ? "" : " (recomendado)"}
                </option>
              ))}
            </select>
          </div>
          <small className="field-hint">
            {selectedIconOption?.description ?? "Ícone personalizado já cadastrado."}
          </small>
        </Field>
        <Field label="URL externa" error={errors.url?.message}>
          <input
            {...register("url")}
            type="url"
            placeholder={selectedTypeOption?.placeholder ?? "https://..."}
          />
        </Field>
        <Field label="Aba" error={errors.storeTabId?.message}>
          <select {...register("storeTabId")}>
            <option value="">Raiz da página</option>
            {tabs
              .filter((tab) => tab.isActive)
              .map((tab) => (
                <option value={tab.id} key={tab.id}>
                  {tab.name}
                </option>
              ))}
          </select>
        </Field>
        <Field label="Ordem" error={errors.sortOrder?.message}>
          <input {...register("sortOrder", { valueAsNumber: true })} type="number" min="0" />
        </Field>
        {error ? <div className="alert alert-error">{problemMessage(error)}</div> : null}
        <div className="sheet-actions">
          <button
            className="button button-quiet"
            type="button"
            onClick={onCancel}
          >
            Cancelar
          </button>
          <button
            className="button button-dark"
            type="submit"
            disabled={saving}
          >
            {saving ? "Salvando…" : "Salvar link"}
          </button>
        </div>
      </form>
    </div>
  );
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
function TabsPage({ token }: { token: string }) {
  const { storeId, store } = useWorkspace(token);
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: ["tabs", storeId],
    queryFn: () => api.tabs(storeId, token),
    enabled: Boolean(storeId),
  });
  const [editing, setEditing] = useState<TabResponse | null | undefined>(
    undefined,
  );
  const save = useMutation({
    mutationFn: (input: TabInput | { id: string; input: TabUpdate }) =>
      "id" in input
        ? api.updateTab(storeId, input.id, input.input, token)
        : api.createTab(storeId, input, token),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["tabs", storeId] });
      setEditing(undefined);
    },
  });
  const deactivate = useMutation({
    mutationFn: (id: string) => api.deactivateTab(storeId, id, token),
    onSuccess: () =>
      void queryClient.invalidateQueries({ queryKey: ["tabs", storeId] }),
  });
  return (
    <section className="admin-section">
      <WorkspaceHeader
        store={store.data}
        eyebrow="Conteúdo"
        title="Abas"
        description="Agrupe os links para que cada visita encontre o próximo passo."
      />
      <div className="section-toolbar">
        <span>{query.data?.length ?? 0} abas cadastradas</span>
        <button className="button button-dark" onClick={() => setEditing(null)}>
          <Plus size={17} /> Nova aba
        </button>
      </div>
      {editing !== undefined && (
        <TabForm
          tab={editing}
          saving={save.isPending}
          error={save.error}
          onCancel={() => setEditing(undefined)}
          onSave={(input) =>
            editing
              ? save.mutate({
                  id: editing.id,
                  input: { ...input, isActive: editing.isActive },
                })
              : save.mutate(input)
          }
        />
      )}
      {query.error ? (
        <ErrorState error={query.error} />
      ) : (
        <div className="tab-list">
          {(query.data ?? []).map((tab) => (
            <article
              className={`tab-row ${tab.isActive ? "" : "is-inactive"}`}
              key={tab.id}
            >
              <span className="tab-order">
                {String(tab.sortOrder).padStart(2, "0")}
              </span>
              <span>
                <strong>{tab.name}</strong>
                <small>{tab.key}</small>
              </span>
              <span
                className={`status ${tab.isActive ? "status-active" : "status-inactive"}`}
              >
                {tab.isActive ? "Ativa" : "Inativa"}
              </span>
              <button
                className="button-icon"
                onClick={() => setEditing(tab)}
                aria-label={`Editar ${tab.name}`}
              >
                <Settings size={16} />
              </button>
              {tab.isActive && (
                <button
                  className="button-icon"
                  onClick={() =>
                    window.confirm(`Desativar ${tab.name}?`) &&
                    deactivate.mutate(tab.id)
                  }
                  aria-label={`Desativar ${tab.name}`}
                >
                  <Trash2 size={16} />
                </button>
              )}
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

function TabForm({
  tab,
  saving,
  error,
  onCancel,
  onSave,
}: {
  tab: TabResponse | null;
  saving: boolean;
  error: unknown;
  onCancel: () => void;
  onSave: (input: TabInput) => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TabFormValues>({
    resolver: zodResolver(tabSchema),
    defaultValues: tab
      ? { key: tab.key, name: tab.name, sortOrder: tab.sortOrder }
      : { key: "", name: "", sortOrder: 0 },
  });
  return (
    <div className="form-sheet">
      <div className="sheet-heading">
        <div>
          <span className="eyebrow">{tab ? "Editar aba" : "Nova aba"}</span>
          <h2>{tab ? tab.name : "Organizar conteúdo"}</h2>
        </div>
        <button
          className="button-icon"
          onClick={onCancel}
          aria-label="Fechar formulário"
        >
          <X size={18} />
        </button>
      </div>
      <form className="form-grid" onSubmit={handleSubmit(onSave)}>
        <Field label="Chave" error={errors.key?.message}>
          <input {...register("key")} placeholder="menu" />
        </Field>
        <Field label="Nome exibido" error={errors.name?.message}>
          <input {...register("name")} placeholder="Menu" />
        </Field>
        <Field label="Ordem" error={errors.sortOrder?.message}>
          <input {...register("sortOrder", { valueAsNumber: true })} type="number" min="0" />
        </Field>
        {error ? <div className="alert alert-error">{problemMessage(error)}</div> : null}
        <div className="sheet-actions">
          <button
            className="button button-quiet"
            type="button"
            onClick={onCancel}
          >
            Cancelar
          </button>
          <button
            className="button button-dark"
            type="submit"
            disabled={saving}
          >
            {saving ? "Salvando…" : "Salvar aba"}
          </button>
        </div>
      </form>
    </div>
  );
}

function SettingsPage({ token }: { token: string }) {
  const { storeId, store } = useWorkspace(token);
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: (input: StoreUpdate) => api.updateStore(storeId, input, token),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["store", storeId] });
      void queryClient.invalidateQueries({ queryKey: ["stores"] });
    },
  });
  return (
    <section className="admin-section">
      <WorkspaceHeader
        store={store.data}
        eyebrow="Workspace"
        title="Configurações"
        description="A identidade que aparece na página pública da sua loja."
      />
      {store.data && (
        <>
          <StoreForm
            store={store.data}
            saving={mutation.isPending}
            error={mutation.error}
            onCancel={() => undefined}
            onSave={(input) =>
              mutation.mutate({ ...input, isActive: store.data?.isActive })
            }
          />
          <LogoUploader store={store.data} token={token} />
        </>
      )}
    </section>
  );
}

function LogoUploader({ store, token }: { store: StoreResponse; token: string }) {
  const queryClient = useQueryClient();
  const [file, setFile] = useState<File | null>(null);
  const upload = useMutation({
    mutationFn: async () => {
      if (!file) throw new Error("Selecione uma imagem.");
      const media = await api.uploadMedia(store.id, file, token);
      return api.updateStore(store.id, { name: store.name, slug: store.slug, description: store.description, logoUrl: store.logoMediaId ? null : store.logoUrl, primaryColor: store.primaryColor, secondaryColor: store.secondaryColor, logoMediaId: media.id, isActive: store.isActive }, token);
    },
    onSuccess: () => { setFile(null); void queryClient.invalidateQueries({ queryKey: ["store", store.id] }); void queryClient.invalidateQueries({ queryKey: ["stores"] }); },
  });
  return <section className="form-sheet logo-upload-sheet"><div className="sheet-heading"><div><span className="eyebrow">Imagem da loja</span><h2>Logo pública</h2></div>{store.logoUrl && <img className="logo-upload-preview" src={store.logoUrl.startsWith("/media/") ? `${API_BASE_URL || window.location.origin}${store.logoUrl}` : store.logoUrl} alt={`Logo atual de ${store.name}`} />}</div><div className="logo-upload-row"><label className="media-drop"><input type="file" accept="image/jpeg,image/png,image/webp" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /><Upload size={20} /><span>{file?.name ?? "Escolher nova logo"}</span><small>JPEG, PNG ou WebP até 5 MB</small></label><button className="button button-dark" type="button" disabled={!file || upload.isPending} onClick={() => upload.mutate()}>{upload.isPending ? "Enviando…" : "Usar como logo"}</button></div>{upload.error && <div className="alert alert-error">{problemMessage(upload.error)}</div>}</section>;
}

function Loading() {
  return (
    <div className="loading-state" aria-busy="true">
      <span className="loader" /> Carregando dados…
    </div>
  );
}
function ErrorState({ error }: { error: unknown }) {
  return (
    <div className="alert alert-error" role="alert">
      {error instanceof ApiError && error.problem.status === 401
        ? "Sua sessão expirou. Entre novamente."
        : problemMessage(error)}
    </div>
  );
}

function ProtectedAdmin({
  session,
  logout,
}: {
  session: Session | null;
  logout: () => void;
}) {
  return session ? (
    <AdminShell session={session} logout={logout} />
  ) : (
    <Navigate to="/login" replace />
  );
}

function LegacyContentRedirect() {
  const { storeId } = useParams<{ storeId: string }>();
  return <Navigate to={`/lojas/${storeId}/conteudo`} replace />;
}

export default function AdminApp() {
  const auth = useSessionState();
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter basename="/admin">
        <Routes>
          <Route
            path="/login"
            element={
              auth.session ? (
                <Navigate to="/lojas" replace />
              ) : (
                <LoginPage onLogin={auth.login} />
              )
            }
          />
          <Route
            path="/*"
            element={
              <ProtectedAdmin session={auth.session} logout={auth.logout} />
            }
          />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
