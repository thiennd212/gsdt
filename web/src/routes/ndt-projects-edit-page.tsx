import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { NdtProjectEditPage } from '@/features/ndt-projects';

// Route: /ndt-projects/$id/edit — edit NĐT project
export const ndtProjectEditRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ndt-projects/$id/edit',
  component: function NdtEditRouteComponent() {
    const { id } = ndtProjectEditRoute.useParams();
    return <NdtProjectEditPage projectId={id} />;
  },
});
