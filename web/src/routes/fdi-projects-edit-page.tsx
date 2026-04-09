import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { FdiProjectEditPage } from '@/features/fdi-projects';

// Route: /fdi-projects/$id/edit — edit FDI project
export const fdiProjectEditRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/fdi-projects/$id/edit',
  component: function FdiEditRouteComponent() {
    const { id } = fdiProjectEditRoute.useParams();
    return <FdiProjectEditPage projectId={id} />;
  },
});
