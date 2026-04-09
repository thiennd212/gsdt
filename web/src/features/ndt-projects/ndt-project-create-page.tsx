import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { NdtProjectTabsContainer } from './ndt-project-tabs-container';

// NdtProjectCreatePage — new NĐT project. Only Tab 1 active until project is created.
export function NdtProjectCreatePage() {
  return (
    <div>
      <AdminPageHeader
        title="Thêm mới dự án NĐT"
        description="Nhập thông tin dự án đầu tư nhà đầu tư trong nước"
      />
      <NdtProjectTabsContainer projectId={null} mode="create" />
    </div>
  );
}
