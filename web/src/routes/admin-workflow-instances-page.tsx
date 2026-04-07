import { createRoute } from '@tanstack/react-router';
import { adminRoute } from './admin-layout';
import { InstanceMonitoringDashboard } from '@/features/workflow';

// Route: /admin/workflow/instances — monitoring dashboard for all workflow instances
export const adminWorkflowInstancesRoute = createRoute({
  getParentRoute: () => adminRoute,
  path: '/admin/workflow/instances',
  component: InstanceMonitoringDashboard,
});
