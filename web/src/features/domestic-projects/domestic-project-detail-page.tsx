import { Button } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { PermissionGate } from '@/shared/components/permission-gate';
import { useDomesticProject } from './domestic-project-api';
import { DomesticProjectTabsContainer } from './domestic-project-tabs-container';

interface DetailPageProps {
  projectId: string;
}

// DetailPage — readonly view of domestic project. All tabs render with disabled inputs.
export function DomesticProjectDetailPage({ projectId }: DetailPageProps) {
  const navigate = useNavigate();
  const { data: project } = useDomesticProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? project.projectName : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
        actions={
          <PermissionGate permission="INV.DOMESTIC.WRITE">
            <Button
              icon={<EditOutlined />}
              onClick={() => navigate({ to: `/domestic-projects/${projectId}/edit` })}
            >
              Chỉnh sửa
            </Button>
          </PermissionGate>
        }
      />
      <DomesticProjectTabsContainer projectId={projectId} mode="detail" />
    </div>
  );
}
