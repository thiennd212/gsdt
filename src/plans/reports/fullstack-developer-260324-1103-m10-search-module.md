# Phase Implementation Report

## Executed Phase
- Phase: M10 Search module — SavedQuery, UnifiedSearch facade, FacetedSearch, SqlServerFacetProvider
- Plan: none (direct implementation task)
- Status: completed

## Files Modified

### New — Domain
- `src/modules/search/GSDT.Search.Domain/Entities/SavedQuery.cs` (73 lines) — aggregate root
- `src/modules/search/GSDT.Search.Domain/Services/IFacetProvider.cs` (22 lines) — port + FacetResult/FacetBucket records
- `src/modules/search/GSDT.Search.Domain/Repositories/ISavedQueryRepository.cs` (15 lines)
- `src/modules/search/GSDT.Search.Domain/Enums/FacetType.cs` (8 lines)

### New — Application
- `DTOs/SearchItemDto.cs`, `SearchResultDto.cs`, `FacetResultDto.cs`, `SavedQueryDto.cs` (4 files)
- `Commands/CreateSavedQuery/` — Command + Handler + Validator (3 files)
- `Commands/UpdateSavedQuery/` — Command + Handler + Validator (3 files)
- `Commands/DeleteSavedQuery/` — Command + Handler + Validator (3 files)
- `Queries/UnifiedSearch/` — Query + Handler + UnifiedSearchDocument (3 files)
- `Queries/FacetedSearch/` — Query + Handler (2 files)
- `Queries/ExecuteSavedQuery/` — Query + Handler (2 files)
- `Queries/GetSavedQueries/` — Query + Handler (2 files)

### Modified — Infrastructure
- `Persistence/SearchDbContext.cs` — added `DbSet<SavedQuery>`, `OnModelCreating`, `SavedQueryConfiguration`

### New — Infrastructure
- `Configurations/SavedQueryConfiguration.cs` — EF config with indexes
- `Repositories/SavedQueryRepository.cs` — EF Core write-side
- `Providers/SqlServerFacetProvider.cs` — GROUP BY facets per entity type
- `Providers/ElasticsearchFacetProvider.cs` — stub, delegates to SQL fallback
- `SearchInfrastructureRegistration.cs` — DI registration with config switch

### New — Presentation
- `Controllers/SearchController.cs` — GET /api/v1/search, GET /api/v1/search/facets
- `Controllers/SavedQueriesController.cs` — full CRUD + execute

### New — Tests
- `tests/unit/GSDT.Search.Domain.Tests/GSDT.Search.Domain.Tests.csproj`
- `tests/unit/GSDT.Search.Domain.Tests/Entities/SavedQueryEntityTests.cs` (12 tests)
- `tests/unit/GSDT.Search.Application.Tests/GSDT.Search.Application.Tests.csproj`
- `tests/unit/GSDT.Search.Application.Tests/Commands/CreateSavedQueryCommandHandlerTests.cs` (3 tests)
- `tests/unit/GSDT.Search.Application.Tests/Commands/CreateSavedQueryCommandValidatorTests.cs` (8 tests)
- `tests/unit/GSDT.Search.Application.Tests/Queries/UnifiedSearchQueryHandlerTests.cs` (7 tests — includes page size clamping, facet delegation, failure propagation, index name mapping)

### Modified — Solution
- `src/GSDT.slnx` — added 2 test projects

## Tasks Completed
- [x] SavedQuery entity: AuditableEntity<Guid>, IAggregateRoot, ITenantScoped, Create/Update factory
- [x] IFacetProvider port + FacetResult/FacetBucket records
- [x] FacetType enum
- [x] ISavedQueryRepository interface
- [x] All 4 DTOs
- [x] CreateSavedQuery command + handler + validator
- [x] UpdateSavedQuery command + handler + validator
- [x] DeleteSavedQuery command + handler + validator
- [x] UnifiedSearch query + handler (wraps existing ISearchService)
- [x] FacetedSearch query + handler
- [x] ExecuteSavedQuery query + handler
- [x] GetSavedQueries query + handler
- [x] SearchDbContext updated with SavedQuery DbSet
- [x] SavedQueryConfiguration (EF)
- [x] SavedQueryRepository
- [x] SqlServerFacetProvider (GROUP BY per entity type)
- [x] ElasticsearchFacetProvider (stub/fallback)
- [x] SearchInfrastructureRegistration with config switch
- [x] SearchController (unified search + facets)
- [x] SavedQueriesController (CRUD + execute)
- [x] Domain tests: 12/12 pass
- [x] Application tests: 19/19 pass
- [x] Both test projects added to GSDT.slnx
- [x] Build: 0 errors, 0 warnings

## Tests Status
- Build: PASS (0 errors)
- Domain tests: 12/12 PASS
- Application tests: 19/19 PASS
- Total: 31/31 PASS

## Issues Encountered
1. `IAggregateRoot` requires `DomainEvents`, `AddDomainEvent`, `ClearDomainEvents` — SavedQuery doesn't raise events but must implement; added minimal no-op implementation with backing list.
2. No concrete `SearchDocument` subclass existed in codebase — created `UnifiedSearchDocument` in Application layer with `EntityType`, `Title`, `Snippet`, `Score` fields matching what SQL FTS adapter projects.

## Next Steps
- Register `AddSearchInfrastructure` + `AddSearchModule` in the host API startup (Phase 17 task)
- EF migration needed: `dotnet ef migrations add InitialSearch -p .../Search.Infrastructure -s .../Api`
- `UnifiedSearchDocument` fields (Title, Snippet, Score, EntityType) must be projected by the FTS SQL query — FtsQueryBuilder may need updating when actual FTS catalog exists
- SqlServerFacetProvider facet map is hardcoded; consider DB-driven config for new entity types
