import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useDnnnProject } from './dnnn-project-api';
import { DnnnProjectTabsContainer } from './dnnn-project-tabs-container';

interface EditPageProps {
  projectId: string;
}

// DnnnProjectEditPage — edit existing DNNN project. All 6 tabs active, data pre-filled.
export function DnnnProjectEditPage({ projectId }: EditPageProps) {
  const { data: project } = useDnnnProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? `Chỉnh sửa: ${project.projectName}` : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
      />
      <DnnnProjectTabsContainer projectId={projectId} mode="edit" />
    </div>
  );
}
