import { useState, useMemo } from 'react';
import { Table, Button, Tag, Space, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import { KHLCNT_META } from './catalog-config';
import {
  useContractorSelectionPlans,
  useCreateContractorSelectionPlan,
  useUpdateContractorSelectionPlan,
  useDeleteContractorSelectionPlan,
} from './catalog-api';
import { KhlcntFormModal } from './khlcnt-form-modal';
import type {
  ContractorSelectionPlanDto,
  CreateContractorSelectionPlanRequest,
  UpdateContractorSelectionPlanRequest,
} from './catalog-types';

// Dedicated page for KHLCNT (Ke hoach lua chon nha thau) — 5-field datagrid
export function KhlcntCatalogPage() {
  const [search, setSearch] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<ContractorSelectionPlanDto | null>(null);

  const { data: items = [], isLoading } = useContractorSelectionPlans();
  const createMutation = useCreateContractorSelectionPlan();
  const updateMutation = useUpdateContractorSelectionPlan();
  const deleteMutation = useDeleteContractorSelectionPlan();

  const filtered = useMemo(() => {
    if (!search.trim()) return items;
    const term = search.toLowerCase().trim();
    return items.filter(
      (item) =>
        item.nameVi.toLowerCase().includes(term) ||
        item.nameEn.toLowerCase().includes(term) ||
        item.signedBy.toLowerCase().includes(term),
    );
  }, [items, search]);

  function openCreate() {
    setEditingItem(null);
    setModalOpen(true);
  }

  function openEdit(item: ContractorSelectionPlanDto) {
    setEditingItem(item);
    setModalOpen(true);
  }

  function closeModal() {
    setModalOpen(false);
    setEditingItem(null);
  }

  function handleSubmit(
    values:
      | CreateContractorSelectionPlanRequest
      | (UpdateContractorSelectionPlanRequest & { id: string }),
  ) {
    const isEdit = 'id' in values;
    const onSuccess = () => {
      message.success(isEdit ? 'Cập nhật thành công' : 'Thêm mới thành công');
      closeModal();
    };
    const onError = () => message.error('Thao tác thất bại, vui lòng thử lại');

    if (isEdit) {
      updateMutation.mutate(values, { onSuccess, onError });
    } else {
      createMutation.mutate(values, { onSuccess, onError });
    }
  }

  function handleDelete(id: string) {
    deleteMutation.mutate(id, {
      onSuccess: () => message.success('Xóa thành công'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<ContractorSelectionPlanDto> = [
    {
      title: 'STT',
      dataIndex: 'orderNumber',
      key: 'orderNumber',
      width: 70,
      sorter: (a, b) => a.orderNumber - b.orderNumber,
    },
    {
      title: 'Tên kế hoạch (VN)',
      dataIndex: 'nameVi',
      key: 'nameVi',
      ellipsis: true,
      sorter: (a, b) => a.nameVi.localeCompare(b.nameVi),
    },
    {
      title: 'Tên kế hoạch (EN)',
      dataIndex: 'nameEn',
      key: 'nameEn',
      ellipsis: true,
    },
    {
      title: 'Ngày ký',
      dataIndex: 'signedDate',
      key: 'signedDate',
      width: 120,
      render: (date: string) => (date ? dayjs(date).format('DD/MM/YYYY') : '—'),
      sorter: (a, b) => dayjs(a.signedDate).unix() - dayjs(b.signedDate).unix(),
    },
    {
      title: 'Người ký',
      dataIndex: 'signedBy',
      key: 'signedBy',
      width: 160,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 120,
      render: (active: boolean) => (
        <Tag color={active ? 'green' : 'default'}>
          {active ? 'Hoạt động' : 'Ngừng'}
        </Tag>
      ),
      filters: [
        { text: 'Hoạt động', value: true },
        { text: 'Ngừng', value: false },
      ],
      onFilter: (value, record) => record.isActive === value,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 100,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(record)} />
          <Popconfirm
            title="Xác nhận xóa"
            description="Bạn có chắc chắn muốn xóa bản ghi này?"
            onConfirm={() => handleDelete(record.id)}
            okText="Xóa"
            cancelText="Hủy"
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const saving = createMutation.isPending || updateMutation.isPending;

  return (
    <div>
      <AdminPageHeader
        title={KHLCNT_META.label}
        description={KHLCNT_META.description}
        icon={KHLCNT_META.icon}
        stats={{ total: items.length, filtered: search ? filtered.length : undefined }}
      />
      <AdminContentCard noPadding>
        <AdminTableToolbar
          searchPlaceholder="Tìm kiếm theo tên kế hoạch hoặc người ký..."
          searchValue={search}
          onSearchChange={setSearch}
          actions={
            <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
              Thêm mới
            </Button>
          }
        />
        <Table<ContractorSelectionPlanDto>
          rowKey="id"
          columns={columns}
          dataSource={filtered}
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (t) => `Tổng ${t} bản ghi` }}
        />
      </AdminContentCard>
      <KhlcntFormModal
        open={modalOpen}
        editingItem={editingItem}
        saving={saving}
        onSubmit={handleSubmit}
        onCancel={closeModal}
      />
    </div>
  );
}
