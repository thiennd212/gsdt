## Shared Infrastructure

### SharedKernel (GSDT.SharedKernel)
- **Base classes:** Entity<TId>, AuditableEntity, AggregateRoot, ValueObject
- **Common interfaces:**
  - `IRepository<T, TId>` — Generic repository contract
  - `IReadDbConnection` — Dapper-based read-side access
  - `ICacheService` — IMemoryCache (dev) / Redis (prod)
  - `IMessageBus` — MassTransit event publishing
  - `IBackgroundJobService` — Hangfire job scheduling
  - `IWebhookService` — External HTTP callbacks
  - `ICurrentUser`, `ITenantContext` — Request-scoped user/tenant info
  - `IConnectionStringProvider`, `IDbProvider` — Database abstraction
- **Domain primitives:** WorkflowEngine<TState, TAction>, DomainEvent, Error, Result<T>
- **Security:** IEncryptionService, Always Encrypted column support
- **Observability:** Serilog structure logging, OpenTelemetry instrumentation

### Infrastructure (GSDT.Infrastructure)
- **Database:** EF Core 10 DbContext base, Dapper IReadDbConnection implementations
- **Cache:** Redis integration, distributed cache invalidation
- **Messaging:** MassTransit RabbitMQ transport, domain event dispatcher
- **Background jobs:** Hangfire configuration, job definitions
- **Secrets:** HashiCorp Vault client (no plaintext secrets in config)
- **Observability:** Serilog sinks (Seq in dev, cloud in prod), OpenTelemetry exporters
- **Middleware:** SecurityHeaders, CorrelationId, RateLimiting, TenantAware behavior

### Backend Refactoring (2026-03-20)
Code quality improvements across 5 oversized C# files (>200 LOC):
- **Modularized files:** Extract utility functions, service classes, domain logic into focused modules
- **Impact:** Improved code maintainability, reduced cyclomatic complexity, better test isolation
- **Verification:** All 533 unit tests + 102 integration tests passing post-refactor

---

## Design System (Institutional Modern - 2026-03-20)

### Frontend UI Enhancements
**Color Tokens (CSS Custom Properties):**
- Navy primary: `#1B3A5C` (sidebar, buttons, links)
- Red accent: `#C8102E` (errors, destructive actions)
- Gold warning: `#F2A900` (badges, non-text only — WCAG AA compliance)
- Light/dark variants for 2-tone theme support

**Typography (Be Vietnam Pro + Inter):**
- Heading scales: H1 38px, H2 30px, H3 24px, H4 20px
- Body: 14px/1.57 line height
- Secondary text: #4A5568 (5.2:1 contrast on light bg — AA compliant)

**Components:**
- **Sidebar:** Gradient background, collapsible hamburger menu (768px breakpoint)
- **Topbar:** Breadcrumb navigation, language switcher, user avatar
- **Buttons:** Navy primary, red danger, ghost default (no inline style overrides)
- **Tables:** Row striping, hover states, responsive scrolling
- **Cards:** Elevation + shadow, 24px padding (Ant Design defaults)

**Dark Mode:**
- Reactive React context (Zustand) + localStorage persistence
- High-contrast variants: light navy #2C5AA0, light red #E63946
- All pages tested: light + dark modes pass WCAG AA

**Motion & Accessibility:**
- Smooth transitions (fade, slide) — respects `prefers-reduced-motion`
- Skip-to-content link on all pages
- Keyboard navigation: Tab/Shift+Tab, Enter/Space, Escape
- ARIA labels + roles on interactive elements
- Screen reader tested (NVDA)

