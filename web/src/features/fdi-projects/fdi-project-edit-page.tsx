import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useFdiProject } from './fdi-project-api';
import { FdiProjectTabsContainer } from './fdi-project-tabs-container';

interface EditPageProps {
  projectId: string;
}

// FdiProjectEditPage — edit existing FDI project. All 6 tabs active, data pre-filled.
export function FdiProjectEditPage({ projectId }: EditPageProps) {
  const { data: project } = useFdiProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? `Chỉnh sửa: ${project.projectName}` : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
      />
      <FdiProjectTabsContainer projectId={projectId} mode="edit" />
    </div>
  );
}
