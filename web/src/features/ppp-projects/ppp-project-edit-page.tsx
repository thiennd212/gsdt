import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { usePppProject } from './ppp-project-api';
import { PppProjectTabsContainer } from './ppp-project-tabs-container';

interface EditPageProps {
  projectId: string;
}

// PppProjectEditPage — edit existing PPP project. All 7 tabs active, data pre-filled.
export function PppProjectEditPage({ projectId }: EditPageProps) {
  const { data: project } = usePppProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? `Chỉnh sửa: ${project.projectName}` : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
      />
      <PppProjectTabsContainer projectId={projectId} mode="edit" />
    </div>
  );
}
