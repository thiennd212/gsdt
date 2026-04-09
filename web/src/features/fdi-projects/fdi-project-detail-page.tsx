import { Button } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useFdiProject } from './fdi-project-api';
import { FdiProjectTabsContainer } from './fdi-project-tabs-container';

interface DetailPageProps {
  projectId: string;
}

// FdiProjectDetailPage — readonly view of FDI project. All tabs render with disabled inputs.
export function FdiProjectDetailPage({ projectId }: DetailPageProps) {
  const navigate = useNavigate();
  const { data: project } = useFdiProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? project.projectName : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
        actions={
          <Button
            icon={<EditOutlined />}
            onClick={() => navigate({ to: `/fdi-projects/${projectId}/edit` })}
          >
            Chỉnh sửa
          </Button>
        }
      />
      <FdiProjectTabsContainer projectId={projectId} mode="detail" />
    </div>
  );
}
