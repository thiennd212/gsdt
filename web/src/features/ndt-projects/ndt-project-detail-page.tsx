import { Button } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useNdtProject } from './ndt-project-api';
import { NdtProjectTabsContainer } from './ndt-project-tabs-container';

interface DetailPageProps {
  projectId: string;
}

// NdtProjectDetailPage — readonly view of NĐT project. All tabs render with disabled inputs.
export function NdtProjectDetailPage({ projectId }: DetailPageProps) {
  const navigate = useNavigate();
  const { data: project } = useNdtProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? project.projectName : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
        actions={
          <Button
            icon={<EditOutlined />}
            onClick={() => navigate({ to: `/ndt-projects/${projectId}/edit` })}
          >
            Chỉnh sửa
          </Button>
        }
      />
      <NdtProjectTabsContainer projectId={projectId} mode="detail" />
    </div>
  );
}
