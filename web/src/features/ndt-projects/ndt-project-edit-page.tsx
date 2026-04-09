import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useNdtProject } from './ndt-project-api';
import { NdtProjectTabsContainer } from './ndt-project-tabs-container';

interface EditPageProps {
  projectId: string;
}

// NdtProjectEditPage — edit existing NĐT project. All 6 tabs active, data pre-filled.
export function NdtProjectEditPage({ projectId }: EditPageProps) {
  const { data: project } = useNdtProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? `Chỉnh sửa: ${project.projectName}` : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
      />
      <NdtProjectTabsContainer projectId={projectId} mode="edit" />
    </div>
  );
}
