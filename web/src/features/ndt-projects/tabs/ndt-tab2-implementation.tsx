import { useState } from 'react';
import { Card, Table, Button, Space, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useAddNdtBidPackage, useDeleteNdtBidPackage, useNdtProject } from '../ndt-project-api';
import { BidPackageFormModal } from '@/features/domestic-projects/tabs/bid-package-form-modal';
import type { NdtBidPackageDto } from '../ndt-project-types';

interface Tab2Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// NdtTab2Implementation — Tình hình TH for NĐT projects.
// Simpler than PPP Tab3: bid packages section only, no capital plans.
export function NdtTab2Implementation({ projectId, mode }: Tab2Props) {
  const isReadonly = mode === 'detail';
  const { data: project } = useNdtProject(projectId);
  const bidPackages = project?.bidPackages ?? [];

  const [bidModalOpen, setBidModalOpen] = useState(false);
  const addBid = useAddNdtBidPackage();
  const deleteBid = useDeleteNdtBidPackage();

  function handleAddBid(values: Record<string, unknown>) {
    addBid.mutate(
      { projectId, ...values },
      {
        onSuccess: () => { message.success('Thêm gói thầu thành công'); setBidModalOpen(false); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function handleDeleteBid(bidPackageId: string) {
    deleteBid.mutate(
      { projectId, bidPackageId },
      {
        onSuccess: () => message.success('Đã xóa gói thầu'),
        onError: () => message.error('Xóa thất bại'),
      },
    );
  }

  const bidColumns: ColumnsType<NdtBidPackageDto> = [
    { title: 'Tên gói thầu', dataIndex: 'name', key: 'name', ellipsis: true },
    {
      title: 'Giá dự toán (tr.đ)',
      dataIndex: 'estimatedPrice',
      key: 'estimated',
      width: 160,
      render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Giá trúng thầu (tr.đ)',
      dataIndex: 'winningPrice',
      key: 'winning',
      width: 160,
      render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Số QĐ phê duyệt',
      dataIndex: 'resultDecisionNumber',
      key: 'decision',
      width: 150,
      render: (v: string | null) => v ?? '—',
    },
    {
      title: '',
      key: 'actions',
      width: 60,
      render: (_, record) =>
        !isReadonly && (
          <Popconfirm
            title="Xóa gói thầu?"
            onConfirm={() => handleDeleteBid(record.id)}
            okText="Xóa"
            cancelText="Hủy"
          >
            <Button size="small" danger icon={<DeleteOutlined />} loading={deleteBid.isPending} />
          </Popconfirm>
        ),
    },
  ];

  return (
    <div>
      <Card
        size="small"
        title="Gói thầu"
        extra={
          !isReadonly && (
            <Button
              size="small"
              icon={<PlusOutlined />}
              onClick={() => setBidModalOpen(true)}
            >
              Thêm gói thầu
            </Button>
          )
        }
      >
        <Table<NdtBidPackageDto>
          rowKey="id"
          columns={bidColumns}
          dataSource={bidPackages}
          size="small"
          pagination={{ pageSize: 10, showTotal: (t) => `${t} gói thầu` }}
          locale={{ emptyText: 'Chưa có gói thầu' }}
        />
      </Card>

      <BidPackageFormModal
        open={bidModalOpen}
        onCancel={() => setBidModalOpen(false)}
        onSubmit={handleAddBid}
        saving={addBid.isPending}
      />
    </div>
  );
}
