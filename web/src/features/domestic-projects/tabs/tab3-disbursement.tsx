import { Table, Card, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { useDomesticProject } from '../domestic-project-api';
import type { DisbursementDto } from '../domestic-project-types';

const { Text } = Typography;

interface Tab3Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// Tab 3: Giải ngân — disbursement records table
export function Tab3Disbursement({ projectId }: Tab3Props) {
  const { data: project } = useDomesticProject(projectId);
  const disbursements = project?.disbursements ?? [];

  const columns: ColumnsType<DisbursementDto> = [
    { title: 'Đến ngày', dataIndex: 'reportDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Vốn ĐTC tháng', dataIndex: 'publicCapitalMonthly', key: 'monthly', width: 140, render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'Vốn ĐTC tháng trước', dataIndex: 'publicCapitalPreviousMonth', key: 'prev', width: 160, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
    { title: 'Lũy kế từ đầu năm', dataIndex: 'publicCapitalYtd', key: 'ytd', width: 160, render: (v: number) => <Text strong>{v?.toLocaleString('vi-VN')}</Text> },
    { title: 'Vốn khác', dataIndex: 'otherCapital', key: 'other', width: 120, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
  ];

  return (
    <Card size="small" title="Tình hình giải ngân">
      <Table<DisbursementDto>
        rowKey="id"
        columns={columns}
        dataSource={disbursements}
        size="small"
        pagination={{ pageSize: 10, showTotal: (t) => `${t} bản ghi` }}
      />
    </Card>
  );
}
