import { Button } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { useDnnnProject } from './dnnn-project-api';
import { DnnnProjectTabsContainer } from './dnnn-project-tabs-container';

interface DetailPageProps {
  projectId: string;
}

// DnnnProjectDetailPage — readonly view of DNNN project. All tabs render with disabled inputs.
export function DnnnProjectDetailPage({ projectId }: DetailPageProps) {
  const navigate = useNavigate();
  const { data: project } = useDnnnProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? project.projectName : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
        actions={
          <Button
            icon={<EditOutlined />}
            onClick={() => navigate({ to: `/dnnn-projects/${projectId}/edit` })}
          >
            Chỉnh sửa
          </Button>
        }
      />
      <DnnnProjectTabsContainer projectId={projectId} mode="detail" />
    </div>
  );
}
