import React, { Suspense } from 'react';
import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Lazy-load CaseDetailPage — split point for the case detail bundle
const CaseDetailPage = React.lazy(() =>
  import('@/features/cases/case-detail-page').then((m) => ({ default: m.CaseDetailPage })),
);

// Route: /cases/$caseId — case detail, requires auth
export const caseDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/cases/$caseId',
  component: function CaseDetailRoute() {
    const { caseId } = caseDetailRoute.useParams();
    return (
      <Suspense fallback={null}>
        <CaseDetailPage caseId={caseId} />
      </Suspense>
    );
  },
});
