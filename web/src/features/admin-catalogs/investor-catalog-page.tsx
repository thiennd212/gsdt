import { useState, useMemo } from 'react';
import { Table, Button, Tag, Space, Popconfirm, message, Radio } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import { INVESTOR_META } from './catalog-config';
import {
  useInvestors,
  useCreateInvestor,
  useUpdateInvestor,
  useDeleteInvestor,
} from './catalog-api';
import { InvestorFormModal } from './investor-form-modal';
import type { InvestorDto, CreateInvestorRequest, UpdateInvestorRequest } from './catalog-types';

const TYPE_FILTER_OPTIONS = [
  { label: 'Tất cả', value: '' },
  { label: 'Doanh nghiệp', value: 'Doanh nghiệp' },
  { label: 'Cá nhân', value: 'Cá nhân' },
  { label: 'Tổ chức khác', value: 'Tổ chức khác' },
];

// Dedicated page for Investor catalog — Table view with type filter and search
export function InvestorCatalogPage() {
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<InvestorDto | null>(null);

  const { data: items = [], isLoading } = useInvestors();
  const createMutation = useCreateInvestor();
  const updateMutation = useUpdateInvestor();
  const deleteMutation = useDeleteInvestor();

  const filtered = useMemo(() => {
    const term = search.toLowerCase().trim();
    return items.filter((item) => {
      const matchType = !typeFilter || item.investorType === typeFilter;
      const matchSearch =
        !term ||
        item.nameVi.toLowerCase().includes(term) ||
        item.businessIdOrCccd.toLowerCase().includes(term) ||
        (item.nameEn ?? '').toLowerCase().includes(term);
      return matchType && matchSearch;
    });
  }, [items, search, typeFilter]);

  function openEdit(item: InvestorDto) { setEditingItem(item); setModalOpen(true); }
  function closeModal() { setModalOpen(false); setEditingItem(null); }

  function handleSubmit(values: CreateInvestorRequest | (UpdateInvestorRequest & { id: string })) {
    const isEdit = 'id' in values;
    const onSuccess = () => { message.success(isEdit ? 'Cập nhật thành công' : 'Thêm mới thành công'); closeModal(); };
    const onError = () => message.error('Thao tác thất bại, vui lòng thử lại');
    if (isEdit) updateMutation.mutate(values, { onSuccess, onError });
    else createMutation.mutate(values, { onSuccess, onError });
  }

  function handleDelete(id: string) {
    deleteMutation.mutate(id, {
      onSuccess: () => message.success('Xóa thành công'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<InvestorDto> = [
    { title: 'Loại nhà đầu tư', dataIndex: 'investorType', key: 'investorType', width: 150,
      render: (t: string) => <Tag color="blue">{t}</Tag> },
    { title: 'Mã số thuế / CCCD', dataIndex: 'businessIdOrCccd', key: 'businessIdOrCccd', width: 160 },
    { title: 'Tên nhà đầu tư (VN)', dataIndex: 'nameVi', key: 'nameVi', ellipsis: true,
      sorter: (a, b) => a.nameVi.localeCompare(b.nameVi) },
    { title: 'Tên nhà đầu tư (EN)', dataIndex: 'nameEn', key: 'nameEn', ellipsis: true,
      render: (v: string | null) => v ?? '—' },
    {
      title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 120,
      render: (active: boolean) => <Tag color={active ? 'green' : 'default'}>{active ? 'Hoạt động' : 'Ngừng'}</Tag>,
      filters: [{ text: 'Hoạt động', value: true }, { text: 'Ngừng', value: false }],
      onFilter: (value, record) => record.isActive === value,
    },
    {
      title: 'Thao tác', key: 'actions', width: 100,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(record)} />
          <Popconfirm
            title="Xác nhận xóa"
            description="Bạn có chắc chắn muốn xóa nhà đầu tư này?"
            onConfirm={() => handleDelete(record.id)}
            okText="Xóa" cancelText="Hủy"
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
        title={INVESTOR_META.label}
        description={INVESTOR_META.description}
        icon={INVESTOR_META.icon}
        stats={{ total: items.length, filtered: search || typeFilter ? filtered.length : undefined }}
      />
      <AdminContentCard noPadding>
        <AdminTableToolbar
          searchPlaceholder="Tìm kiếm theo tên hoặc mã số thuế/CCCD..."
          searchValue={search}
          onSearchChange={setSearch}
          actions={
            <Space size={8}>
              <Radio.Group
                options={TYPE_FILTER_OPTIONS}
                value={typeFilter}
                onChange={(e) => setTypeFilter(e.target.value)}
                optionType="button"
                size="small"
              />
              <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditingItem(null); setModalOpen(true); }}>
                Thêm mới
              </Button>
            </Space>
          }
        />
        <Table<InvestorDto>
          rowKey="id"
          columns={columns}
          dataSource={filtered}
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (t) => `Tổng ${t} bản ghi` }}
        />
      </AdminContentCard>
      <InvestorFormModal
        open={modalOpen}
        editingItem={editingItem}
        saving={saving}
        onSubmit={handleSubmit}
        onCancel={closeModal}
      />
    </div>
  );
}
