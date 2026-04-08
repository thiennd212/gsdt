import { createRouter } from '@tanstack/react-router';
import { RouteErrorFallback } from '@/shared/components/route-error-fallback';
import { rootRoute } from '@/routes/root-layout';
import { authenticatedRoute } from '@/routes/authenticated-layout';
import { adminRoute } from '@/routes/admin-layout';
import { loginRoute } from '@/routes/login-page';
import { callbackRoute } from '@/routes/callback-page';
import { auditLogsRoute } from '@/routes/audit-logs-page';
import { adminUsersRoute } from '@/routes/admin-users-page';
import { adminSystemParamsRoute } from '@/routes/admin-system-params-page';
import { adminMasterDataRoute } from '@/routes/admin-master-data-page';
import { adminOrganizationRoute } from '@/routes/admin-organization-page';
import { adminApiKeysRoute } from '@/routes/admin-api-keys-page';
import { adminHealthRoute } from '@/routes/admin-health-page';
import { adminWebhookDeliveriesRoute } from '@/routes/admin-webhook-deliveries-page';
import { adminAbacRulesRoute } from '@/routes/admin-abac-rules-page';
import { adminDelegationsRoute } from '@/routes/admin-delegations-page';
import { adminAccessReviewsRoute } from '@/routes/admin-access-reviews-page';
import { adminSessionsRoute } from '@/routes/admin-sessions-page';
import { adminBackupRoute } from '@/routes/admin-backup-page';
import { adminWorkflowRoute } from '@/routes/admin-workflow-page';
import { adminWorkflowDetailRoute } from '@/routes/admin-workflow-detail-page';
import { adminWorkflowAssignmentsRoute } from '@/routes/admin-workflow-assignments-page';
import { adminWorkflowInstancesRoute } from '@/routes/admin-workflow-instances-page';
import { adminWorkflowInstanceDetailRoute } from '@/routes/admin-workflow-instance-detail-page';
import { adminNotificationTemplatesRoute } from '@/routes/admin-notification-templates-page';
import { casesRoute } from '@/routes/cases-list-page';
import { caseDetailRoute } from '@/routes/case-detail-page';
import { workflowInboxRoute } from '@/routes/workflow-inbox-page';
import { dashboardRoute } from '@/routes/dashboard-page';
import { reportDefinitionsRoute } from '@/routes/report-definitions-page';
import { reportExecutionsRoute } from '@/routes/report-executions-page';
import { formsRoute } from '@/routes/forms-list-page';
import { formDetailRoute } from '@/routes/form-detail-page';
import { filesRoute } from '@/routes/files-list-page';
import { notificationsRoute } from '@/routes/notifications-list-page';
import { aiSearchRoute } from '@/routes/ai-search-page';
import { profileRoute } from '@/routes/profile-page';
import { rolesRoute } from '@/routes/roles-page';
import { notFoundRoute } from '@/routes/not-found-page';
import { adminRulesRoute } from '@/routes/admin-rules-page';
import { adminRulesDetailRoute } from '@/routes/admin-rules-detail-page';
import { adminTemplatesRoute } from '@/routes/admin-templates-page';
import { adminJobsRoute } from '@/routes/admin-jobs-page';
import { adminAiRoute } from '@/routes/admin-ai-page';
import { adminMenusRoute } from '@/routes/admin-menus-page';
import { searchRoute } from '@/routes/search-page';
import { copilotRoute } from '@/routes/copilot-page';
import { chatRoute } from '@/routes/chat-page';
import { signaturesRoute } from '@/routes/signatures-page';
import { integrationPartnersRoute } from '@/routes/integration-partners-page';
import { integrationContractsRoute } from '@/routes/integration-contracts-page';
import { integrationMessageLogsRoute } from '@/routes/integration-message-logs-page';
import { publicFormRoute } from '@/routes/public-form-page';
import { consentRoute } from '@/routes/consent-page';
import { adminRtbfRoute } from '@/routes/admin-rtbf-page';
import { adminCredentialPoliciesRoute } from '@/routes/admin-credential-policies-page';
import { adminExternalIdentitiesRoute } from '@/routes/admin-external-identities-page';
import { adminGroupsRoute } from '@/routes/admin-groups-page';
import { adminDataScopesRoute } from '@/routes/admin-data-scopes-page';
import { adminSodRulesRoute } from '@/routes/admin-sod-rules-page';
import { adminPolicyRulesRoute } from '@/routes/admin-policy-rules-page';
import { adminJitProviderConfigsRoute } from '@/routes/admin-jit-provider-configs-page';
import { adminDashboardRoute } from '@/routes/admin-dashboard-route';
import { adminCatalogsRoute } from '@/routes/admin-catalogs-page';
import { adminCatalogsKhlcntRoute } from '@/routes/admin-catalogs-khlcnt-page';
import { adminCatalogTypeRoute } from '@/routes/admin-catalogs-type-page';

// Route tree — public routes + authenticated subtree + admin guard subtree
const routeTree = rootRoute.addChildren([
  loginRoute,
  callbackRoute,
  publicFormRoute,
  authenticatedRoute.addChildren([
    dashboardRoute,
    rolesRoute,
    auditLogsRoute,
    casesRoute,
    caseDetailRoute,
    workflowInboxRoute,
    reportDefinitionsRoute,
    reportExecutionsRoute,
    formsRoute,
    formDetailRoute,
    filesRoute,
    notificationsRoute,
    aiSearchRoute,
    profileRoute,
    searchRoute,
    copilotRoute,
    chatRoute,
    signaturesRoute,
    integrationPartnersRoute,
    integrationContractsRoute,
    integrationMessageLogsRoute,
    consentRoute,
    // Admin routes — nested under adminRoute (enforces Admin/SystemAdmin role)
    adminRoute.addChildren([
      adminDashboardRoute,
      adminUsersRoute,
      adminSystemParamsRoute,
      adminMasterDataRoute,
      adminOrganizationRoute,
      adminApiKeysRoute,
      adminWebhookDeliveriesRoute,
      adminBackupRoute,
      adminAbacRulesRoute,
      adminDelegationsRoute,
      adminAccessReviewsRoute,
      adminSessionsRoute,
      adminHealthRoute,
      adminWorkflowRoute,
      adminWorkflowDetailRoute,
      adminWorkflowAssignmentsRoute,
      adminWorkflowInstancesRoute,
      adminWorkflowInstanceDetailRoute,
      adminNotificationTemplatesRoute,
      adminRulesRoute,
      adminRulesDetailRoute,
      adminTemplatesRoute,
      adminJobsRoute,
      adminAiRoute,
      adminMenusRoute,
      adminRtbfRoute,
      adminCredentialPoliciesRoute,
      adminExternalIdentitiesRoute,
      adminGroupsRoute,
      adminDataScopesRoute,
      adminSodRulesRoute,
      adminPolicyRulesRoute,
      adminJitProviderConfigsRoute,
      adminCatalogsRoute,
      adminCatalogsKhlcntRoute,
      adminCatalogTypeRoute,
    ]),
  ]),
  notFoundRoute,
]);

export const router = createRouter({
  routeTree,
  defaultPreload: 'intent',
  defaultErrorComponent: RouteErrorFallback,
});

// Register router for type safety across the app
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}
