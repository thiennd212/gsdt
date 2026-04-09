import { Button } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { PermissionGate } from '@/shared/components/permission-gate';
import { useOdaProject } from './oda-project-api';
import { OdaProjectTabsContainer } from './oda-project-tabs-container';

interface DetailPageProps { projectId: string; }

export function OdaProjectDetailPage({ projectId }: DetailPageProps) {
  const navigate = useNavigate();
  const { data: project } = useOdaProject(projectId);
  return (
    <div>
      <AdminPageHeader
        title={project ? project.projectName : 'Đang tải...'}
        description={project ? `Mã DA: ${project.projectCode}` : undefined}
        actions={
          <PermissionGate permission="INV.ODA.WRITE">
            <Button icon={<EditOutlined />} onClick={() => navigate({ to: `/oda-projects/${projectId}/edit` })}>Chỉnh sửa</Button>
          </PermissionGate>
        }
      />
      <OdaProjectTabsContainer projectId={projectId} mode="detail" />
    </div>
  );
}
