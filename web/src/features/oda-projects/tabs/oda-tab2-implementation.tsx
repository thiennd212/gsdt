import { useState } from 'react';
import { Card, Table, Button, Space, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined, EyeOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useOdaProject, useAddOdaBidPackage, useDeleteOdaBidPackage } from '../oda-project-api';
import type { BidPackageDto, CapitalPlanDto } from '../oda-project-types';
import { BidPackageFormModal } from '@/features/domestic-projects/tabs/bid-package-form-modal';

interface OdaTab2Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// ODA Tab 2: Tình hình TH — reuses bid package modal from P05, ODA capital plan has 5 fields
export function OdaTab2Implementation({ projectId, mode }: OdaTab2Props) {
  const { data: project } = useOdaProject(projectId);
  const isReadonly = mode === 'detail';
  const capitalPlans = project?.capitalPlans ?? [];
  const bidPackages = project?.bidPackages ?? [];

  const [bidModalOpen, setBidModalOpen] = useState(false);
  const addBid = useAddOdaBidPackage();
  const deleteBid = useDeleteOdaBidPackage();

  function handleAddBid(values: Record<string, unknown>) {
    addBid.mutate({ projectId, ...values }, {
      onSuccess: () => { message.success('Thêm gói thầu thành công'); setBidModalOpen(false); },
      onError: () => message.error('Thêm thất bại'),
    });
  }

  // ODA capital plan columns — 5 fields (no AllocationRound like TN)
  const capitalCols: ColumnsType<CapitalPlanDto> = [
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 120 },
    { title: 'Ngày', dataIndex: 'decisionDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Tổng', dataIndex: 'totalAmount', key: 'total', width: 130, render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'NSTW', dataIndex: 'centralBudget', key: 'central', width: 120, render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'NSĐP', dataIndex: 'localBudget', key: 'local', width: 120, render: (v: number) => v?.toLocaleString('vi-VN') },
  ];

  const bidCols: ColumnsType<BidPackageDto> = [
    { title: 'Tên gói thầu', dataIndex: 'name', key: 'name', ellipsis: true },
    { title: 'Giá dự toán', dataIndex: 'estimatedPrice', key: 'est', width: 130, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
    { title: 'Giá trúng', dataIndex: 'winningPrice', key: 'win', width: 130, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
    { title: 'HĐ', key: 'contracts', width: 60, render: (_, r) => r.contracts?.length ?? 0 },
    {
      title: '', key: 'actions', width: 80,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EyeOutlined />} />
          {!isReadonly && (
            <Popconfirm title="Xóa gói thầu?" onConfirm={() => deleteBid.mutate({ projectId, bidPackageId: record.id })} okText="Xóa" cancelText="Hủy">
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card size="small" title="Kế hoạch giao vốn ODA" style={{ marginBottom: 16 }}>
        <Table<CapitalPlanDto> rowKey="id" columns={capitalCols} dataSource={capitalPlans} size="small" pagination={{ pageSize: 5 }} />
      </Card>
      <Card size="small" title="Gói thầu" extra={!isReadonly && <Button size="small" icon={<PlusOutlined />} onClick={() => setBidModalOpen(true)}>Thêm gói thầu</Button>}>
        <Table<BidPackageDto> rowKey="id" columns={bidCols} dataSource={bidPackages} size="small" pagination={{ pageSize: 5 }} />
      </Card>
      <BidPackageFormModal open={bidModalOpen} onCancel={() => setBidModalOpen(false)} onSubmit={handleAddBid} saving={addBid.isPending} />
    </div>
  );
}
