import { useState, useMemo } from 'react';
import { Table, Button, Tag, Space, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import { CATALOG_CONFIG, VALID_CATALOG_TYPES } from './catalog-config';
import {
  useCatalogItems,
  useCreateCatalogItem,
  useUpdateCatalogItem,
  useDeleteCatalogItem,
} from './catalog-api';
import { CatalogFormModal } from './catalog-form-modal';
import type { CatalogItemDto, CreateCatalogRequest, UpdateCatalogRequest } from './catalog-types';

interface GenericCatalogListPageProps {
  catalogType: string;
}

// Reusable list page for any of the 10 generic catalogs.
// Driven by catalogType prop — resolved from route param.
export function GenericCatalogListPage({ catalogType }: GenericCatalogListPageProps) {
  const meta = CATALOG_CONFIG[catalogType];
  const isValid = VALID_CATALOG_TYPES.includes(catalogType);

  const [search, setSearch] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<CatalogItemDto | null>(null);

  const { data: items = [], isLoading } = useCatalogItems(catalogType);
  const createMutation = useCreateCatalogItem(catalogType);
  const updateMutation = useUpdateCatalogItem(catalogType);
  const deleteMutation = useDeleteCatalogItem(catalogType);

  // Client-side search (diacritics-insensitive via lowercase compare)
  const filtered = useMemo(() => {
    if (!search.trim()) return items;
    const term = search.toLowerCase().trim();
    return items.filter(
      (item) =>
        item.code.toLowerCase().includes(term) ||
        item.name.toLowerCase().includes(term),
    );
  }, [items, search]);

  function openCreate() {
    setEditingItem(null);
    setModalOpen(true);
  }

  function openEdit(item: CatalogItemDto) {
    setEditingItem(item);
    setModalOpen(true);
  }

  function closeModal() {
    setModalOpen(false);
    setEditingItem(null);
  }

  function handleSubmit(values: CreateCatalogRequest | (UpdateCatalogRequest & { id: string })) {
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

  if (!isValid || !meta) {
    return (
      <AdminPageHeader
        title="Danh mục không tồn tại"
        description="Loại danh mục yêu cầu không hợp lệ."
      />
    );
  }

  const columns: ColumnsType<CatalogItemDto> = [
    {
      title: 'Mã',
      dataIndex: 'code',
      key: 'code',
      width: 120,
      sorter: (a, b) => a.code.localeCompare(b.code),
    },
    {
      title: 'Tên danh mục',
      dataIndex: 'name',
      key: 'name',
      sorter: (a, b) => a.name.localeCompare(b.name),
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
        title={meta.label}
        description={meta.description}
        icon={meta.icon}
        stats={{ total: items.length, filtered: search ? filtered.length : undefined }}
      />
      <AdminContentCard noPadding>
        <AdminTableToolbar
          searchPlaceholder="Tìm kiếm theo mã hoặc tên..."
          searchValue={search}
          onSearchChange={setSearch}
          actions={
            <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
              Thêm mới
            </Button>
          }
        />
        <Table<CatalogItemDto>
          rowKey="id"
          columns={columns}
          dataSource={filtered}
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (t) => `Tổng ${t} bản ghi` }}
        />
      </AdminContentCard>
      <CatalogFormModal
        open={modalOpen}
        editingItem={editingItem}
        saving={saving}
        onSubmit={handleSubmit}
        onCancel={closeModal}
      />
    </div>
  );
}
