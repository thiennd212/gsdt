import { Card, Descriptions, Tag } from 'antd';
import dayjs from 'dayjs';
import { useDomesticProject } from '@/features/domestic-projects/domestic-project-api';

interface Tab5Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// Tab 5: Khai thác / Vận hành — shared component for TN + ODA.
// Shows operation info as read-only descriptions (data comes from project detail).
export function Tab5Operation({ projectId }: Tab5Props) {
  const { data: project } = useDomesticProject(projectId);
  const op = project?.operation;

  if (!op) {
    return (
      <Card size="small" title="Khai thác / Vận hành">
        <Tag>Chưa có thông tin vận hành</Tag>
      </Card>
    );
  }

  return (
    <Card size="small" title="Khai thác / Vận hành">
      <Descriptions column={2} size="small" bordered>
        <Descriptions.Item label="Ngày đưa vào vận hành">
          {op.operationDate ? dayjs(op.operationDate).format('DD/MM/YYYY') : '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Chủ sử dụng">
          {op.operatingAgency ?? '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Doanh thu năm trước">
          {op.revenueLastYear != null ? op.revenueLastYear.toLocaleString('vi-VN') + ' triệu VNĐ' : '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Chi phí năm trước">
          {op.expenseLastYear != null ? op.expenseLastYear.toLocaleString('vi-VN') + ' triệu VNĐ' : '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Ghi chú" span={2}>
          {op.notes ?? '—'}
        </Descriptions.Item>
      </Descriptions>
    </Card>
  );
}
