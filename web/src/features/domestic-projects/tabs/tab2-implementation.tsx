import { useState } from 'react';
import { Card, Table, Button, Space, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined, EyeOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useDomesticProject, useAddBidPackage, useDeleteBidPackage } from '../domestic-project-api';
import type { BidPackageDto, CapitalPlanDto } from '../domestic-project-types';
import { BidPackageFormModal } from './bid-package-form-modal';

interface Tab2Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// Tab 2: Tình hình thực hiện — Capital plans table + Bid packages table with popup modal
export function Tab2Implementation({ projectId, mode }: Tab2Props) {
  const { data: project } = useDomesticProject(projectId);
  const isReadonly = mode === 'detail';

  const capitalPlans = project?.capitalPlans ?? [];
  const bidPackages = project?.bidPackages ?? [];

  const [bidModalOpen, setBidModalOpen] = useState(false);
  const addBidPackage = useAddBidPackage();
  const deleteBidPackage = useDeleteBidPackage();

  function handleAddBid(values: Record<string, unknown>) {
    addBidPackage.mutate({ projectId, ...values }, {
      onSuccess: () => { message.success('Thêm gói thầu thành công'); setBidModalOpen(false); },
      onError: () => message.error('Thêm thất bại'),
    });
  }

  function handleDeleteBid(bidPackageId: string) {
    deleteBidPackage.mutate({ projectId, bidPackageId }, {
      onSuccess: () => message.success('Đã xóa gói thầu'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  // Capital plan columns
  const capitalPlanCols: ColumnsType<CapitalPlanDto> = [
    { title: 'Lần', dataIndex: 'allocationRound', key: 'round', width: 60 },
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 120 },
    { title: 'Ngày', dataIndex: 'decisionDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Tổng vốn', dataIndex: 'totalAmount', key: 'total', width: 130, render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'NSTW', dataIndex: 'centralBudget', key: 'central', width: 120, render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'NSĐP', dataIndex: 'localBudget', key: 'local', width: 120, render: (v: number) => v?.toLocaleString('vi-VN') },
  ];

  // Bid package columns
  const bidPackageCols: ColumnsType<BidPackageDto> = [
    { title: 'Tên gói thầu', dataIndex: 'name', key: 'name', ellipsis: true },
    { title: 'Giá dự toán', dataIndex: 'estimatedPrice', key: 'estimated', width: 130, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
    { title: 'Giá trúng', dataIndex: 'winningPrice', key: 'winning', width: 130, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
    { title: 'Số HĐ', key: 'contracts', width: 70, render: (_, r) => r.contracts?.length ?? 0 },
    {
      title: 'Thao tác', key: 'actions', width: 80,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EyeOutlined />} />
          {!isReadonly && (
            <Popconfirm title="Xóa gói thầu?" onConfirm={() => handleDeleteBid(record.id)} okText="Xóa" cancelText="Hủy">
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      {/* KH giao vốn */}
      <Card size="small" title="Kế hoạch giao vốn" style={{ marginBottom: 16 }}>
        <Table<CapitalPlanDto>
          rowKey="id" columns={capitalPlanCols} dataSource={capitalPlans}
          size="small" pagination={{ pageSize: 5, showTotal: (t) => `${t} bản ghi` }}
        />
      </Card>

      {/* Gói thầu */}
      <Card
        size="small"
        title="Gói thầu"
        extra={!isReadonly && (
          <Button size="small" icon={<PlusOutlined />} onClick={() => setBidModalOpen(true)}>Thêm gói thầu</Button>
        )}
      >
        <Table<BidPackageDto>
          rowKey="id" columns={bidPackageCols} dataSource={bidPackages}
          size="small" pagination={{ pageSize: 5, showTotal: (t) => `${t} gói thầu` }}
        />
      </Card>

      <BidPackageFormModal
        open={bidModalOpen}
        onCancel={() => setBidModalOpen(false)}
        onSubmit={handleAddBid}
        saving={addBidPackage.isPending}
      />
    </div>
  );
}
