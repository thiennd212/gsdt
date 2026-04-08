import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { PppProjectTabsContainer } from './ppp-project-tabs-container';

// PppProjectCreatePage — new PPP project. Only Tab 1 active until project is created.
export function PppProjectCreatePage() {
  return (
    <div>
      <AdminPageHeader
        title="Thêm mới dự án PPP"
        description="Nhập thông tin dự án đầu tư theo phương thức đối tác công tư"
      />
      <PppProjectTabsContainer projectId={null} mode="create" />
    </div>
  );
}
