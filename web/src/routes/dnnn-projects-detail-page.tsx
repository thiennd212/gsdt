import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { DnnnProjectDetailPage } from '@/features/dnnn-projects';

// Route: /dnnn-projects/$id — view DNNN project detail (readonly)
export const dnnnProjectDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/dnnn-projects/$id',
  component: function DnnnDetailRouteComponent() {
    const { id } = dnnnProjectDetailRoute.useParams();
    return <DnnnProjectDetailPage projectId={id} />;
  },
});
