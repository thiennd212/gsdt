import { Table, Button, Space, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useDeletePppDecision, usePppProject } from '../ppp-project-api';
import { PppCtdtDecisionForm } from './ppp-tab1-ctdt-decision-form';
import { PppDtDecisionForm } from './ppp-tab1-dt-decision-form';
import type { PppInvestmentDecisionDto } from '../ppp-project-types';

interface DecisionsZoneProps {
  projectId: string;
  disabled?: boolean;
}

const DECISION_TYPE_CTDT = 0;
const DECISION_TYPE_DT = 1;

// PppTab1DecisionsZone — two sub-sections: QĐ CTĐT and QĐ ĐT with shared delete list.
// Forms are extracted to ppp-tab1-ctdt-decision-form and ppp-tab1-dt-decision-form.
export function PppTab1DecisionsZone({ projectId, disabled }: DecisionsZoneProps) {
  const { data: project } = usePppProject(projectId);
  const decisions = project?.decisions ?? [];
  const deleteMutation = useDeletePppDecision();

  const ctdtDecisions = decisions.filter((d) => d.decisionType === DECISION_TYPE_CTDT);
  const dtDecisions = decisions.filter((d) => d.decisionType === DECISION_TYPE_DT);

  function handleDelete(decisionId: string) {
    deleteMutation.mutate({ projectId, decisionId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const baseColumns: ColumnsType<PppInvestmentDecisionDto> = [
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 130 },
    { title: 'Ngày', dataIndex: 'decisionDate', key: 'date', width: 100,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Cơ quan', dataIndex: 'decisionAuthority', key: 'authority', ellipsis: true },
    { title: 'Người ký', dataIndex: 'signer', key: 'signer', width: 150,
      render: (v) => v ?? '—' },
    { title: 'Tổng TMĐT', dataIndex: 'totalInvestment', key: 'total', width: 130,
      render: (v: number) => v?.toLocaleString('vi-VN') },
    {
      title: '', key: 'actions', width: 50,
      render: (_, r) => !disabled && (
        <Popconfirm title="Xóa quyết định?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <Space direction="vertical" style={{ width: '100%' }} size={16}>
      {/* QĐ Chủ trương đầu tư */}
      <div>
        <div style={{ fontWeight: 600, marginBottom: 8 }}>QĐ Chủ trương đầu tư (CTĐT)</div>
        {!disabled && <PppCtdtDecisionForm projectId={projectId} />}
        <Table<PppInvestmentDecisionDto>
          rowKey="id" columns={baseColumns} dataSource={ctdtDecisions}
          size="small" pagination={false} locale={{ emptyText: 'Chưa có QĐ CTĐT' }}
        />
      </div>

      {/* QĐ Đầu tư */}
      <div>
        <div style={{ fontWeight: 600, marginBottom: 8 }}>QĐ Đầu tư (ĐT)</div>
        {!disabled && <PppDtDecisionForm projectId={projectId} />}
        <Table<PppInvestmentDecisionDto>
          rowKey="id" columns={baseColumns} dataSource={dtDecisions}
          size="small" pagination={false} locale={{ emptyText: 'Chưa có QĐ ĐT' }}
        />
      </div>
    </Space>
  );
}
