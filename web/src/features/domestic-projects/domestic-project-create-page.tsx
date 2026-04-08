import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { DomesticProjectTabsContainer } from './domestic-project-tabs-container';

// CreatePage — new domestic project. Only Tab 1 is active until project is created.
export function DomesticProjectCreatePage() {
  return (
    <div>
      <AdminPageHeader title="Thêm mới dự án trong nước" description="Nhập thông tin dự án đầu tư công trong nước" />
      <DomesticProjectTabsContainer projectId={null} mode="create" />
    </div>
  );
}
