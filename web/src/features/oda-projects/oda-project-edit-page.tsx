import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useOdaProject } from './oda-project-api';
import { OdaProjectTabsContainer } from './oda-project-tabs-container';

interface EditPageProps { projectId: string; }

export function OdaProjectEditPage({ projectId }: EditPageProps) {
  const { data: project } = useOdaProject(projectId);
  return (
    <div>
      <AdminPageHeader
        title={project ? `Chỉnh sửa: ${project.projectName}` : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
      />
      <OdaProjectTabsContainer projectId={projectId} mode="edit" />
    </div>
  );
}
