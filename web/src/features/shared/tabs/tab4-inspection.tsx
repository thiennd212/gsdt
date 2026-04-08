import { Table, Card, Tabs } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { useDomesticProject } from '@/features/domestic-projects/domestic-project-api';
import type { InspectionDto, EvaluationDto, AuditRecordDto, ViolationDto } from '@/features/domestic-projects/domestic-project-types';

interface Tab4Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// Tab 4: Thanh tra / Kiểm tra / Kiểm toán — shared component for TN + ODA.
// 3 sub-sections as inner tabs: Inspections, Evaluations, Audits + Violations.
export function Tab4Inspection({ projectId }: Tab4Props) {
  const { data: project } = useDomesticProject(projectId);

  const inspectionCols: ColumnsType<InspectionDto> = [
    { title: 'Ngày', dataIndex: 'inspectionDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Cơ quan', dataIndex: 'inspectionAgency', key: 'agency', width: 180, ellipsis: true },
    { title: 'Nội dung', dataIndex: 'content', key: 'content', ellipsis: true },
    { title: 'Kết luận', dataIndex: 'conclusion', key: 'conclusion', width: 200, ellipsis: true, render: (v) => v ?? '—' },
  ];

  const evaluationCols: ColumnsType<EvaluationDto> = [
    { title: 'Ngày', dataIndex: 'evaluationDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Nội dung', dataIndex: 'content', key: 'content', ellipsis: true },
    { title: 'Kết quả', dataIndex: 'result', key: 'result', width: 200, ellipsis: true, render: (v) => v ?? '—' },
  ];

  const auditCols: ColumnsType<AuditRecordDto> = [
    { title: 'Ngày', dataIndex: 'auditDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Cơ quan', dataIndex: 'auditAgency', key: 'agency', width: 180, ellipsis: true },
    { title: 'Nội dung', dataIndex: 'content', key: 'content', ellipsis: true },
  ];

  const violationCols: ColumnsType<ViolationDto> = [
    { title: 'Ngày', dataIndex: 'violationDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Nội dung', dataIndex: 'content', key: 'content', ellipsis: true },
    { title: 'Phạt', dataIndex: 'penalty', key: 'penalty', width: 120, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
  ];

  const subTabs = [
    {
      key: 'inspections',
      label: `Kiểm tra (${project?.inspections?.length ?? 0})`,
      children: (
        <Table<InspectionDto> rowKey="id" columns={inspectionCols} dataSource={project?.inspections ?? []} size="small" pagination={{ pageSize: 5 }} />
      ),
    },
    {
      key: 'evaluations',
      label: `Theo dõi ĐG (${project?.evaluations?.length ?? 0})`,
      children: (
        <Table<EvaluationDto> rowKey="id" columns={evaluationCols} dataSource={project?.evaluations ?? []} size="small" pagination={{ pageSize: 5 }} />
      ),
    },
    {
      key: 'audits',
      label: `Kiểm toán (${project?.audits?.length ?? 0})`,
      children: (
        <Table<AuditRecordDto> rowKey="id" columns={auditCols} dataSource={project?.audits ?? []} size="small" pagination={{ pageSize: 5 }} />
      ),
    },
    {
      key: 'violations',
      label: `Vi phạm (${project?.violations?.length ?? 0})`,
      children: (
        <Table<ViolationDto> rowKey="id" columns={violationCols} dataSource={project?.violations ?? []} size="small" pagination={{ pageSize: 5 }} />
      ),
    },
  ];

  return (
    <Card size="small" title="Thanh tra / Kiểm tra / Kiểm toán">
      <Tabs items={subTabs} size="small" tabPosition="left" />
    </Card>
  );
}
