# Compliance Evidence Checklist

Maps each Vietnamese regulation to its technical control in GSDT and the evidence artifact produced at runtime. Use this during audits to locate evidence quickly.

---

## Regulation → Technical Control → Evidence

| Regulation | Requirement | Technical Control | Evidence Artifact | Location |
|---|---|---|---|---|
| **Law 91/2025/QH15** | Personal data consent | `ConsentRecord` entity + consent API | `identity.consent_records` table | Identity module |
| **Law 91/2025/QH15** | Right to erasure (RTBF) | `RtbfRequestHandler` → anonymise PII columns | Audit log entry + anonymised rows | Audit module |
| **Law 91/2025/QH15** | Data minimisation | `[PersonalData]` attribute on IdentityUser properties | Code review artefact | Identity domain |
| **NĐ53** | Cybersecurity audit log | HMAC-chained `AuditLog` (append-only) | `audit.audit_logs` table | Audit module |
| **NĐ53** | Log tamper detection | `ChainVerificationService` — validates HMAC chain | Chain verification report | Audit module |
| **NĐ53** | Log retention | `ArchiveAuditLogsJob` (Hangfire, yearly) | Archived log files in MinIO | Audit + Files |
| **NĐ59** | VNeID integration | `IVneIdConnector` + federation in OpenIddict | Auth log entries with VNeID subject | Identity Infrastructure |
| **NĐ59** | Citizen identity verification | VNeID mock (`vneid-mock` Docker service in dev) | Auth token with `vneid_verified` claim | AuthServer |
| **NĐ68** | Digital signature on documents | `IDigitalSignatureService` on file upload/export | Signature manifest in `files.file_signatures` | Files module |
| **NĐ68** | Signature verification | `VerifySignatureCommandHandler` | Verification result in audit log | Files + Audit |
| **NĐ85 / TT12** | ATTT security posture | SAST: SonarQube in CI stage 1 | SonarQube quality gate report | Jenkins CI |
| **NĐ85 / TT12** | DAST scan | OWASP ZAP in CI stage 4 | ZAP HTML report, exit code assertion | Jenkins CI |
| **NĐ85 / TT12** | SCA / SBOM | `dotnet CycloneDX` in CI stage 5 | `sbom.xml` CycloneDX artifact | Jenkins CI |
| **NĐ85 / TT12** | Vulnerability disclosure | Nexus IQ scan on published packages | Nexus IQ policy report | CI/CD |
| **QĐ742** | Password minimum length ≥ 12 chars | `IdentityOptions.Password.RequiredLength = 12` | Identity config + unit test | Identity Infrastructure |
| **QĐ742** | Password complexity | `RequireDigit`, `RequireUppercase`, `RequireNonAlphanumeric` | Identity options config | Identity Infrastructure |
| **QĐ742** | Password expiry enforcement | `PasswordExpiresAt` on `ApplicationUser` + `PasswordExpiryService` | `identity.users.password_expires_at` column | Identity module |
| **QĐ742** | Account lockout | `MaxFailedAccessAttempts = 5`, `LockoutDuration = 15min` | `identity.users.lockout_end` + audit log | Identity Infrastructure |
| **QĐ742** | MFA for privileged accounts | TOTP via `Otp.NET` + `IsTwoFactorEnabled` flag | `identity.users.two_factor_enabled` column | Identity module |
| **QĐ742** | Access review | `AccessReviewSchedulerJob` (quarterly Hangfire job) | `identity.access_review_records` table | Identity module |
| **QĐ742** | Role separation | RBAC roles: `Citizen`, `GovOfficer`, `Admin`, `SystemAdmin` | `identity.roles` table | Identity module |
| **QĐ742** | Data classification | `ClassificationLevel` enum on every `AuditableEntity` | Entity `classification_level` column | SharedKernel |
| **QĐ742** | Clearance-based access | ABAC `ClassificationLevelHandler` — compares `ClearanceLevel` vs resource level | HTTP 403 on mismatch | Identity + SharedKernel |
| **QĐ742** | Session management | `ISessionRepository` + sliding token expiry in OpenIddict | `identity.sessions` table | Identity Infrastructure |

---

## CI/CD Evidence Pipeline

```
Stage 1: SAST (SonarQube)
    └─ Artefact: sonar-report.json, quality gate PASSED/FAILED

Stage 2: Build + Unit Tests
    └─ Artefact: test-results.xml (xUnit), coverage.xml (coverlet)

Stage 3: Integration Tests
    └─ Artefact: integration-test-results.xml (Testcontainers)

Stage 4: DAST (OWASP ZAP)
    └─ Artefact: zap-report.html (must have 0 HIGH findings)

Stage 5: SCA / SBOM
    └─ Artefact: sbom.xml (CycloneDX), Nexus IQ policy report

Stage 6: Container Signing
    └─ Artefact: cosign signature manifest

Stage 7: Deploy
    └─ Artefact: Helm release manifest, deployment timestamp
```

All artefacts are archived in Jenkins and retained for **3 years** per NĐ53.

---

## Audit Log Entry Schema

Every state-changing operation writes to `audit.audit_logs`:

| Column | Description |
|---|---|
| `id` | UUID — primary key |
| `tenant_id` | Tenant scope |
| `actor_id` | User who performed action |
| `entity_type` | e.g. `Case`, `ApplicationUser` |
| `entity_id` | UUID of affected entity |
| `action` | e.g. `Created`, `Submitted`, `Approved` |
| `old_value` | JSON snapshot before change |
| `new_value` | JSON snapshot after change |
| `ip_address` | Client IP |
| `correlation_id` | Request correlation ID |
| `created_at` | UTC timestamp |
| `chain_hash` | HMAC-SHA256 of (prev_hash + this row) |
| `prev_chain_hash` | Hash of previous entry — enables tamper detection |

Chain breaks indicate tampering and must trigger an immediate security incident.

---

## Evidence Collection for Annual Audit

1. Export `audit.audit_logs` for the audit period → verify chain with `ChainVerificationService`
2. Pull SonarQube quality gate history → confirm no HIGH/CRITICAL findings unresolved > 30 days
3. Export `identity.access_review_records` → confirm quarterly reviews were completed
4. Pull `identity.consent_records` → confirm RTBF requests were processed ≤ 72 hours
5. Pull ZAP reports from Jenkins → confirm 0 HIGH DAST findings in each release
6. Export `files.file_signatures` for decision documents → verify digital signatures
