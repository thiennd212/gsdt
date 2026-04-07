# GSDT — Post-Bootstrap Checklist

## Script results
- Build: FAIL — manual fix needed
- Checks: 4/5 passed
- Issues: 1 (BUILD_FAILED)

## Claude Kit (CK) — Run these skills IN ORDER

Open project in Claude Code:
```bash
cd C:/Aquitas/Project/GSDT/source
claude
```

Then run:
```
/ck:docs init          # Update docs for new codebase
/ck:scout              # Build codebase awareness
/ck:security-scan      # Baseline security audit
/ck:test               # Verify remaining tests pass
/ck:git                # Commit clean baseline
```

## Manual review (if needed)
- [ ] Review `.github/workflows/ci.yml` — update test paths, Docker registry
- [ ] Update `appsettings.json` — database name, app title
- [ ] Update `infra/docker/docker-compose.yml` — container names
- [ ] Update `web/package.json` — name, description

## Start development
- [ ] `/ck:plan` — Create implementation plan
- [ ] `/ck:cook` — End-to-end feature implementation
- [ ] `/ck:ask` — Technical consultation
