import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { DnnnProjectEditPage } from '@/features/dnnn-projects';

// Route: /dnnn-projects/$id/edit — edit DNNN project
export const dnnnProjectEditRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/dnnn-projects/$id/edit',
  component: function DnnnEditRouteComponent() {
    const { id } = dnnnProjectEditRoute.useParams();
    return <DnnnProjectEditPage projectId={id} />;
  },
});
