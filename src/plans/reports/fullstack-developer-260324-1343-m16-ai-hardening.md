# Phase Implementation Report

## Executed Phase
- Phase: M16 AI Hardening
- Plan: none (direct task)
- Status: completed

## Files Modified

### Domain Layer — NEW
- `src/modules/ai/GSDT.Ai.Domain/Enums/AiProvider.cs` (8 lines)
- `src/modules/ai/GSDT.Ai.Domain/Enums/PromptCategory.cs` (8 lines)
- `src/modules/ai/GSDT.Ai.Domain/Entities/AiModelProfile.cs` (85 lines)
- `src/modules/ai/GSDT.Ai.Domain/Entities/AiPromptTemplate.cs` (78 lines)
- `src/modules/ai/GSDT.Ai.Domain/Repositories/IAiModelProfileRepository.cs` (14 lines)
- `src/modules/ai/GSDT.Ai.Domain/Repositories/IAiPromptTemplateRepository.cs` (14 lines)

### Application Layer — NEW
- `src/modules/ai/GSDT.Ai.Application/DTOs/AiModelProfileDto.cs` (14 lines)
- `src/modules/ai/GSDT.Ai.Application/DTOs/AiPromptTemplateDto.cs` (14 lines)
- `src/modules/ai/GSDT.Ai.Application/Commands/CreateModelProfile/CreateModelProfileCommand.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/CreateModelProfile/CreateModelProfileCommandValidator.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/CreateModelProfile/CreateModelProfileCommandHandler.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/UpdateModelProfile/UpdateModelProfileCommand.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/UpdateModelProfile/UpdateModelProfileCommandValidator.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/UpdateModelProfile/UpdateModelProfileCommandHandler.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/CreatePromptTemplate/CreatePromptTemplateCommand.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/CreatePromptTemplate/CreatePromptTemplateCommandValidator.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/CreatePromptTemplate/CreatePromptTemplateCommandHandler.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/UpdatePromptTemplate/UpdatePromptTemplateCommand.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/UpdatePromptTemplate/UpdatePromptTemplateCommandValidator.cs`
- `src/modules/ai/GSDT.Ai.Application/Commands/UpdatePromptTemplate/UpdatePromptTemplateCommandHandler.cs`
- `src/modules/ai/GSDT.Ai.Application/Queries/GetModelProfiles/GetModelProfilesQuery.cs`
- `src/modules/ai/GSDT.Ai.Application/Queries/GetModelProfiles/GetModelProfilesQueryHandler.cs`
- `src/modules/ai/GSDT.Ai.Application/Queries/GetPromptTemplates/GetPromptTemplatesQuery.cs`
- `src/modules/ai/GSDT.Ai.Application/Queries/GetPromptTemplates/GetPromptTemplatesQueryHandler.cs`

### Infrastructure Layer — NEW
- `src/modules/ai/GSDT.Ai.Infrastructure/Persistence/Configurations/AiModelProfileConfiguration.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Persistence/Configurations/AiPromptTemplateConfiguration.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Repositories/AiModelProfileRepository.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Repositories/AiPromptTemplateRepository.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Services/AzureOpenAiChatService.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Services/AzureOpenAiStreamingChatService.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Services/AiModelRouter.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Services/TokenBudgetTracker.cs`
- `src/modules/ai/GSDT.Ai.Infrastructure/Services/InProcessAiModelProfileProvider.cs`

### Infrastructure Layer — MODIFIED
- `src/modules/ai/GSDT.Ai.Infrastructure/Persistence/AiDbContext.cs` — added AiModelProfiles + AiPromptTemplates DbSets
- `src/modules/ai/GSDT.Ai.Infrastructure/AiInfrastructureRegistration.cs` — registered repositories, Azure services, TokenBudgetTracker, AiModelRouter
- `src/modules/ai/GSDT.Ai.Infrastructure/GSDT.Ai.Infrastructure.csproj` — added Azure.AI.OpenAI, Microsoft.Extensions.AI.OpenAI, StackExchange.Redis

### Presentation Layer — NEW
- `src/modules/ai/GSDT.Ai.Presentation/Controllers/AiModelProfilesController.cs`
- `src/modules/ai/GSDT.Ai.Presentation/Controllers/AiPromptTemplatesController.cs`

### Package Versions — MODIFIED
- `src/Directory.Packages.props` — Azure.AI.OpenAI 2.1.0 (stable), Microsoft.Extensions.AI.OpenAI 10.4.1

### Tests — NEW
- `tests/unit/GSDT.Ai.Application.Tests/AiModelProfileEntityTests.cs` (8 tests)
- `tests/unit/GSDT.Ai.Application.Tests/AiPromptTemplateEntityTests.cs` (6 tests)
- `tests/unit/GSDT.Ai.Application.Tests/CreateModelProfileCommandValidatorTests.cs` (10 tests)
- `tests/unit/GSDT.Ai.Application.Tests/CreatePromptTemplateCommandValidatorTests.cs` (8 tests)

## Tasks Completed

- [x] AiProvider + PromptCategory enums
- [x] AiModelProfile entity (Create, SetAsDefault, ClearDefault, Deactivate, UpdateConfig)
- [x] AiPromptTemplate entity (Create, IncrementVersion, Deactivate, UpdatePrompts)
- [x] IAiModelProfileRepository + IAiPromptTemplateRepository domain interfaces
- [x] AiModelProfileDto + AiPromptTemplateDto
- [x] CreateModelProfileCommand + Handler + Validator (single-default invariant enforced)
- [x] UpdateModelProfileCommand + Handler + Validator
- [x] CreatePromptTemplateCommand + Handler + Validator
- [x] UpdatePromptTemplateCommand + Handler + Validator (version auto-increment)
- [x] GetModelProfilesQuery + Handler
- [x] GetPromptTemplatesQuery + Handler (optional category filter)
- [x] AiModelProfileConfiguration + AiPromptTemplateConfiguration (EF)
- [x] AiModelProfileRepository + AiPromptTemplateRepository (EF implementations)
- [x] AiDbContext — added 2 DbSets
- [x] AzureOpenAiChatService (graceful config-missing handling)
- [x] AzureOpenAiStreamingChatService (SSE streaming, graceful fallback)
- [x] AiModelRouter — profile-aware routing, replaces AiRoutingService when Ollama enabled
- [x] TokenBudgetTracker + ITokenBudgetTracker (Redis-backed, fail-open)
- [x] InProcessAiModelProfileProvider (implements IAiModelProfileProvider)
- [x] AiModelProfilesController (GET + POST + PUT /api/v1/ai/model-profiles)
- [x] AiPromptTemplatesController (GET + POST + PUT /api/v1/ai/prompt-templates)
- [x] DI registration updated (repositories, Azure services, router, budget tracker, profile provider)
- [x] Package versions fixed (Azure.AI.OpenAI 2.1.0, Microsoft.Extensions.AI.OpenAI 10.4.1)

## Tests Status
- Build: pass (0 errors)
- Unit tests (Application): 56/56 pass (32 new + 24 pre-existing)
- Unit tests (Infrastructure): 16/16 pass (all pre-existing)

## Issues Encountered
1. Azure.AI.OpenAI 2.2.0 not stable — latest stable is 2.1.0; updated Directory.Packages.props
2. IAggregateRoot requires DomainEvents/AddDomainEvent/ClearDomainEvents — added to both entities
3. Application layer has no reference to Infrastructure (clean arch) — introduced domain repository interfaces; handlers use repositories not AiDbContext directly
4. AiModelRouter registered as IAiRoutingService (replaces AiRoutingService) when Ollama enabled

## Architecture Notes
- AiModelRouter replaces AiRoutingService in the Ollama-enabled path; stub path unchanged
- Handlers use IAiModelProfileRepository/IAiPromptTemplateRepository (domain interfaces) — no infra leakage into Application layer
- Azure services fail open with log warning when config missing (no exception at startup)
- TokenBudgetTracker fails open when Redis unavailable (allows request, logs warning)
- InProcessAiModelProfileProvider queries AiDbContext directly (monolith pattern, matches existing InProcess clients)
