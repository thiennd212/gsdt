import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { DnnnProjectTabsContainer } from './dnnn-project-tabs-container';

// DnnnProjectCreatePage — new DNNN project. Only Tab 1 active until project is created.
export function DnnnProjectCreatePage() {
  return (
    <div>
      <AdminPageHeader
        title="Thêm mới dự án DNNN"
        description="Nhập thông tin dự án đầu tư doanh nghiệp nhà nước"
      />
      <DnnnProjectTabsContainer projectId={null} mode="create" />
    </div>
  );
}
