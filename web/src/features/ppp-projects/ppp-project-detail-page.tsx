import { Button } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { PermissionGate } from '@/shared/components/permission-gate';
import { usePppProject } from './ppp-project-api';
import { PppProjectTabsContainer } from './ppp-project-tabs-container';

interface DetailPageProps {
  projectId: string;
}

// PppProjectDetailPage — readonly view of PPP project. All tabs render with disabled inputs.
export function PppProjectDetailPage({ projectId }: DetailPageProps) {
  const navigate = useNavigate();
  const { data: project } = usePppProject(projectId);

  return (
    <div>
      <AdminPageHeader
        title={project ? project.projectName : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
        actions={
          <PermissionGate permission="INV.PPP.WRITE">
            <Button
              icon={<EditOutlined />}
              onClick={() => navigate({ to: `/ppp-projects/${projectId}/edit` })}
            >
              Chỉnh sửa
            </Button>
          </PermissionGate>
        }
      />
      <PppProjectTabsContainer projectId={projectId} mode="detail" />
    </div>
  );
}
