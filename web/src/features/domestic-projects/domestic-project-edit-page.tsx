import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useDomesticProject } from './domestic-project-api';
import { DomesticProjectTabsContainer } from './domestic-project-tabs-container';

interface EditPageProps {
  projectId: string;
}

// EditPage — edit existing domestic project. All 6 tabs active, data pre-filled.
export function DomesticProjectEditPage({ projectId }: EditPageProps) {
  const { data: project } = useDomesticProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? `Chỉnh sửa: ${project.projectName}` : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
      />
      <DomesticProjectTabsContainer projectId={projectId} mode="edit" />
    </div>
  );
}
