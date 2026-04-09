#!/usr/bin/env bash
# run-migrations.sh — Run EF Core database update for every module DbContext.
# Usage: ./infra/scripts/run-migrations.sh [Development|Staging|Production]
set -euo pipefail

ENVIRONMENT=${1:-Development}
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

export ASPNETCORE_ENVIRONMENT="$ENVIRONMENT"

echo "==> Running EF Core migrations (environment: $ENVIRONMENT)"
echo "    Repo root: $REPO_ROOT"

# Map each DbContext class name to the csproj directory that owns it.
# Format: "ContextName:relative/path/to/project"
declare -A CTX_PROJECT=(
  [AiDbContext]="src/modules/ai/GSDT.Ai.Infrastructure"
  [AuditDbContext]="src/modules/audit/GSDT.Audit.Infrastructure"
  [CasesDbContext]="src/modules/cases/GSDT.Cases.Infrastructure"
  [FilesDbContext]="src/modules/files/GSDT.Files.Infrastructure"
  [FormsDbContext]="src/modules/forms/GSDT.Forms.Infrastructure"
  [IdentityDbContext]="src/modules/identity/GSDT.Identity.Infrastructure"
  [MasterDataDbContext]="src/modules/masterdata/GSDT.MasterData"
  [NotificationsDbContext]="src/modules/notifications/GSDT.Notifications.Infrastructure"
  [OrgDbContext]="src/modules/organization/GSDT.Organization"
  [SystemParamsDbContext]="src/modules/systemparams/GSDT.SystemParams"
  [WorkflowDbContext]="src/modules/workflow/GSDT.Workflow.Infrastructure"
  [InvestmentProjectsDbContext]="src/modules/investment-projects/GSDT.InvestmentProjects.Infrastructure"
)

FAILED=()

for ctx in "${!CTX_PROJECT[@]}"; do
  project_dir="$REPO_ROOT/${CTX_PROJECT[$ctx]}"

  if [[ ! -d "$project_dir" ]]; then
    echo "  [WARN] Project directory not found for $ctx: $project_dir — skipping"
    FAILED+=("$ctx (directory missing)")
    continue
  fi

  echo "--> Migrating $ctx (project: ${CTX_PROJECT[$ctx]})"
  if dotnet ef database update --context "$ctx" --project "$project_dir" --no-build; then
    echo "    OK: $ctx"
  else
    echo "    FAILED: $ctx"
    FAILED+=("$ctx")
  fi
done

if [[ ${#FAILED[@]} -gt 0 ]]; then
  echo ""
  echo "==> Some migrations failed:"
  for f in "${FAILED[@]}"; do
    echo "    - $f"
  done
  exit 1
fi

echo ""
echo "==> All migrations completed successfully."
