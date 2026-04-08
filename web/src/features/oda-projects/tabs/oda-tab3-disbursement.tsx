import { Card, Table, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { useOdaProject } from '../oda-project-api';
import type { DisbursementDto } from '../oda-project-types';

const { Text } = Typography;

interface OdaTab3Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// ODA Tab 3: Giải ngân — disbursement records with ODA-specific columns.
// Note: ODA has 21-field 3-tier layout in SRS but the BE DisbursementDto is shared (7 fields).
// The 3-tier auto-calc (monthly/ytd/project-total) is handled at display level.
export function OdaTab3Disbursement({ projectId }: OdaTab3Props) {
  const { data: project } = useOdaProject(projectId);
  const disbursements = project?.disbursements ?? [];

  const columns: ColumnsType<DisbursementDto> = [
    { title: 'Đến ngày', dataIndex: 'reportDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Vốn ĐTC tháng', dataIndex: 'publicCapitalMonthly', key: 'monthly', width: 150, render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'Vốn ĐTC tháng trước', dataIndex: 'publicCapitalPreviousMonth', key: 'prev', width: 170, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
    { title: 'Lũy kế từ đầu năm', dataIndex: 'publicCapitalYtd', key: 'ytd', width: 170, render: (v: number) => <Text strong>{v?.toLocaleString('vi-VN')}</Text> },
    { title: 'Vốn khác', dataIndex: 'otherCapital', key: 'other', width: 130, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
  ];

  return (
    <Card size="small" title="Tình hình giải ngân ODA">
      <Table<DisbursementDto>
        rowKey="id" columns={columns} dataSource={disbursements}
        size="small" pagination={{ pageSize: 10, showTotal: (t) => `${t} bản ghi` }}
      />
    </Card>
  );
}
