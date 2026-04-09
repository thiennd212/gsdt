import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { FdiProjectTabsContainer } from './fdi-project-tabs-container';

// FdiProjectCreatePage — new FDI project. Only Tab 1 active until project is created.
export function FdiProjectCreatePage() {
  return (
    <div>
      <AdminPageHeader
        title="Thêm mới dự án FDI"
        description="Nhập thông tin dự án đầu tư nhà đầu tư nước ngoài"
      />
      <FdiProjectTabsContainer projectId={null} mode="create" />
    </div>
  );
}
