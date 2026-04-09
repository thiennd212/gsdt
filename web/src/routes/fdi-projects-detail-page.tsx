import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { FdiProjectDetailPage } from '@/features/fdi-projects';

// Route: /fdi-projects/$id — view FDI project detail (readonly)
export const fdiProjectDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/fdi-projects/$id',
  component: function FdiDetailRouteComponent() {
    const { id } = fdiProjectDetailRoute.useParams();
    return <FdiProjectDetailPage projectId={id} />;
  },
});
