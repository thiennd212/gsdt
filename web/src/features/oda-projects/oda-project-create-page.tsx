import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { OdaProjectTabsContainer } from './oda-project-tabs-container';

export function OdaProjectCreatePage() {
  return (
    <div>
      <AdminPageHeader title="Thêm mới dự án ODA" description="Nhập thông tin dự án sử dụng vốn ODA và vốn vay ưu đãi" />
      <OdaProjectTabsContainer projectId={null} mode="create" />
    </div>
  );
}
