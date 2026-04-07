# GSDT Documentation Index

> Complete documentation for the GSDT template — production-ready .NET 10 backend for Vietnamese government projects.

## Quick Navigation

### For New Developers

**Start here if you're joining the project:**

1. **[Project Overview & PDR](./project-overview-pdr.md)** — What are we building? What's done? What's the roadmap?
2. **[System Architecture](./system-architecture.md)** — How is the code organized? What are the layers?
3. **[Code Standards](./code-standards.md)** — How do we write code? Naming conventions? Patterns?
4. **[Codebase Summary](./codebase-summary.md)** — Where are the modules? What's implemented? Code examples?

**Time estimate:** 30-45 minutes to understand the full picture.

---

### For Feature Implementation

**Implementing a new feature? Follow this path:**

1. **[System Architecture → Repository Pattern Enforcement](./system-architecture.md#repository-pattern-enforcement)** — Template for new repositories (domain interface → infrastructure implementation → application handlers)
2. **[Code Standards → CQRS Patterns](./code-standards.md#cqrs-patterns)** — Command/Query naming, handler pattern, validator pattern
3. **[Codebase Summary → CQRS & Handler Patterns](./codebase-summary.md#cqrs--handler-patterns)** — Code examples of CreateCaseCommand and ListCasesQuery
4. **[Code Standards → Testing Conventions](./code-standards.md#testing-conventions)** — Write unit and integration tests following established patterns

**Time estimate:** 2-4 hours per feature, depending on complexity.

---

### For Testing

**Writing tests? Reference these:**

1. **[Code Standards → Testing Conventions](./code-standards.md#testing-conventions)** — Full testing guide with global setup, unit, integration, and architecture test patterns
2. **[Codebase Summary → Testing Infrastructure](./codebase-summary.md#testing-infrastructure)** — Test project organization and current coverage (37 tests, 80%+ architecture)

**Copy-paste ready:** WebAppFixture, SqlServerFixture, TestAuthHandler patterns are documented with code examples.

---

### For Architecture Decisions

**Evaluating system changes or new modules? Check:**

1. **[System Architecture](./system-architecture.md)** — Layer model, module map, CQRS split, security architecture, phase 2 microservices path
2. **[Code Standards → Repository Pattern](./code-standards.md#repository-pattern)** — Generic base + module-specific extensions
3. **[Project Overview → Technical Constraints](./project-overview-pdr.md#technical-constraints)** — .NET 10, SQL Server 2022, OpenIddict, Clean Architecture enforcement

---

### For Project Status & Roadmap

**Need to know what's done, what's planned, what's at risk?**

1. **[Project Overview & PDR](./project-overview-pdr.md)** — Phase breakdown (12 phases), success metrics, risks, roadmap (Q4 2025 - Q3 2026)
2. **[Codebase Summary → Next Steps](./codebase-summary.md#next-steps)** — Immediate priorities (complete remaining modules, implement real VNeID, add integration tests)

---

### For Compliance & Security

**Need to verify compliance or understand security controls?**

1. **[Codebase Summary → Security Highlights](./codebase-summary.md#security-highlights)** — Compliance table (TLS, headers, auth, ABAC, encryption, audit, secrets)
2. **[Codebase Summary → Compliance References](./codebase-summary.md#compliance-references)** — Mapping to standards (Law 91, NĐ53, NĐ59, NĐ68, NĐ85/TT12, QĐ742)
3. **[Project Overview → Non-Functional Requirements](./project-overview-pdr.md#non-functional-requirements)** — Detailed NFR mapping to implementation status
4. **[Compliance Evidence Checklist](./compliance-evidence-checklist.md)** — Pre-deployment audit checklist

---

## Document Map

| Document | Purpose | Audience | Pages |
|---|---|---|---|
| **[project-overview-pdr.md](./project-overview-pdr.md)** | Functional & non-functional requirements, phase breakdown, roadmap, risks, metrics | Project Managers, Architects, Stakeholders | 10 |
| **[system-architecture.md](./system-architecture.md)** | Architecture deep-dive: layers, modules, CQRS, authentication, security, microservices path | Architects, Senior Devs, Tech Leads | 9 |
| **[code-standards.md](./code-standards.md)** | Coding conventions, CQRS patterns, domain entities, repository pattern, error handling, testing | All Developers, QA Engineers | 10 |
| **[codebase-summary.md](./codebase-summary.md)** | Module inventory, implementation status, testing infrastructure, code examples, compliance mapping | New Developers, QA, Integration Teams | 12 |
| **[v2.40-patterns.md](./v2.40-patterns.md)** | v2.40 security & audit remediation patterns: Dapper RLS, SignalR auth, open redirect prevention, client-side search, outbox interceptor, async patterns | Developers, Security Teams | 4 |
| **[tech-stack.md](./tech-stack.md)** | Technology decisions, versions, rationale, alternatives | Architects, DevOps, Tech Leads | 5 |
| **[deployment-guide.md](./deployment-guide.md)** | Docker Compose dev setup, Kubernetes production, CI/CD pipeline, secrets management | DevOps, SREs, Ops Teams | 8 |
| **[module-creation-guide.md](./module-creation-guide.md)** | Step-by-step guide to create a new module following template | Developers | 5 |
| **[developer-onboarding-guide.md](./developer-onboarding-guide.md)** | First-day setup, local environment, running tests, making changes | New Team Members | 4 |
| **[api-design-standards.md](./api-design-standards.md)** | REST conventions, versioning, error codes, pagination, filtering | API Developers, Frontend Teams | 6 |
| **[authorization-module-gov-reference.md](./authorization-module-gov-reference.md)** | RBAC + ABAC implementation, government-specific patterns | Authorization Teams | 5 |
| **[compliance-evidence-checklist.md](./compliance-evidence-checklist.md)** | Pre-deployment compliance checklist (Law 91, NĐ53, NĐ59, NĐ68, NĐ85/TT12, QĐ742) | Compliance Officers, Auditors | 4 |
| **[admin-module-blueprint.md](./admin-module-blueprint.md)** | Admin features checklist (users, roles, audit logs, system parameters) | Product Owners, Feature Designers | 3 |

---

## Key Concepts Explained

### Clean Architecture Layers
```
Presentation (HTTP controllers) → Application (MediatR handlers)
  → Domain (pure business logic) ← Infrastructure (databases, external services)
                    ↑
              SharedKernel (interfaces, base classes)
```
**Rule:** Dependency arrows point inward only. Domain has zero outbound dependencies.

### CQRS Pattern
- **Writes:** Commands → EF Core aggregate repositories → Result<T>
- **Reads:** Queries → Dapper (optimized SQL) + Redis cache → Result<T>

### Repository Pattern (Identity Module Template)
- **Domain Layer:** Repository interfaces (IConsentRepository, IDelegationRepository, IAttributeRuleRepository)
- **Infrastructure Layer:** EF Core implementations (ConsentRepository, DelegationRepository, AttributeRuleRepository)
- **Application Layer:** Handlers depend on interfaces only, not Infrastructure concrete classes

### Testing Infrastructure
- **Global Setup:** `tests/Directory.Build.props` provides shared using statements
- **Unit Tests:** NSubstitute mocks, no database, no HTTP
- **Integration Tests:** WebAppFixture + SqlServerFixture (Testcontainers) + TestAuthHandler (JWT bypass)
- **Architecture Tests:** NetArchTest.Rules enforce Clean Architecture boundaries

---

## Current Project Status — ALL BACKEND MODULES COMPLETE (2026-03-18)

| Module | Status | Key Features |
|--------|--------|-------------|
| Identity | ✓ COMPLETE | OpenIddict OIDC, RBAC+ABAC, MFA, Consent, Delegation, VNeID |
| Cases | ✓ COMPLETE | DDD+CQRS, workflow transitions, PDF/QR export |
| Files | ✓ COMPLETE | MinIO+ClamAV, digital signature, encryption |
| Notifications | ✓ COMPLETE | Email/SMS/In-app (SignalR), 11 templates |
| Audit | ✓ COMPLETE | HMAC-chained logs, RTBF anonymization |
| Reporting | ✓ COMPLETE | KPI dashboard, Excel/PDF export, Hangfire async |
| Forms | ✓ COMPLETE | Dynamic DDL, dual-mode storage, complex fields |
| Workflow | ✓ COMPLETE | WorkflowEngine<S,A>, SLA breach checker |
| MasterData | ✓ COMPLETE | Province/District/Ward, cached lookups |
| Organization | ✓ COMPLETE | OrgUnits hierarchy |
| SystemParams | ✓ COMPLETE | Key-value config, cached |
| AI | ✓ COMPLETE | NLQ chat, query catalog (SSE) |
| Integration | ✓ COMPLETE | YARP gateway, webhooks, API Key M2M |

**Tests:** 224 passing (104 unit + 18 contract + 102 integration)
**Compliance:** Law 91/Decree 356, NĐ53, NĐ59, NĐ68, NĐ85/TT12, QĐ742
**Infrastructure:** CI/CD (GitHub Actions), Docker, Helm, Vault, k6, Prometheus

---

## Common Tasks

### I want to...

**...create a new module following Identity module pattern**
→ Read [Module Creation Guide](./module-creation-guide.md), then reference [System Architecture → Repository Pattern](./system-architecture.md#repository-pattern-enforcement)

**...implement a new feature (command + handler)**
→ Follow [Code Standards → CQRS Patterns](./code-standards.md#cqrs-patterns), see [Codebase Summary → Code Examples](./codebase-summary.md#cqrs--handler-patterns)

**...write unit tests for a handler**
→ Read [Code Standards → Testing Conventions → Unit Test Pattern](./code-standards.md#unit-test-pattern)

**...write integration tests for an API endpoint**
→ Read [Code Standards → Testing Conventions → Integration Test Pattern](./code-standards.md#integration-test-pattern) with WebAppFixture example

**...understand the architecture**
→ Start with [System Architecture → Layer Model](./system-architecture.md#layer-model) and [System Architecture → CQRS Split](./system-architecture.md#cqrs-split)

**...verify compliance with government standards**
→ Check [Compliance Evidence Checklist](./compliance-evidence-checklist.md) and [Codebase Summary → Compliance References](./codebase-summary.md#compliance-references)

**...deploy to production**
→ Follow [Deployment Guide](./deployment-guide.md) and run [Compliance Checklist](./compliance-evidence-checklist.md)

**...check project status and roadmap**
→ See [Project Overview → Phase Breakdown & Status](./project-overview-pdr.md#phase-breakdown--status) and [Project Overview → Development Roadmap](./project-overview-pdr.md#development-roadmap)

---

## Contributing to Documentation

When updating docs:
1. Keep files **under 800 LOC** (split into directories if needed)
2. Use **clear headers and tables** over walls of text
3. **Link to related docs** (internal consistency)
4. **Verify code references** against actual codebase (grep for function names, repository interfaces, etc.)
5. **Include Vietnamese compliance references** where relevant
6. **Update this README** if adding a new documentation file

Last updated: **2026-03-14** (Phase 04 & Phase 10 completion)

---

## Report: Documentation Update

See [Documentation Update Report](../plans/reports/docs-manager-260314-0732-phase-04-phase-10-completion.md) for detailed changes made during Phase 04 and Phase 10 completion.

**Changes summary:**
- Updated `system-architecture.md` with Repository Pattern Enforcement section (Identity module template)
- Expanded `code-standards.md` Testing Conventions with fixtures, patterns, and code examples
- Created `codebase-summary.md` with module inventory, implementation status, and compliance mapping
- Created `project-overview-pdr.md` with FRs, NFRs, phase breakdown, metrics, and roadmap
