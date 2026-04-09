import { useState } from 'react';
import { Table, Button, Space, Popconfirm, Tag, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, EyeOutlined, EditOutlined, DeleteOutlined, SearchOutlined, ClearOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { PageBreadcrumb } from '@/shared/components/page-breadcrumb';
import { EmptyState } from '@/shared/components/empty-state';
import { PermissionGate } from '@/shared/components/permission-gate';
import { PppProjectListFilters } from './ppp-project-list-filters';
import { usePppProjects, useDeletePppProject } from './ppp-project-api';
import { PPP_CONTRACT_TYPE_LABELS } from './ppp-project-types';
import type { PppProjectListItem, PppProjectListParams } from './ppp-project-types';

// PppProjectListPage — danh sách dự án PPP with filters, search, pagination
export function PppProjectListPage() {
  const navigate = useNavigate();
  const [params, setParams] = useState<PppProjectListParams>({ page: 1, pageSize: 50 });
  const [search, setSearch] = useState('');
  const [filterValues, setFilterValues] = useState<Record<string, string | undefined>>({});

  const { data, isLoading } = usePppProjects({ ...params, ...filterValues, search: search || undefined });
  const deleteMutation = useDeletePppProject();

  function handleSearch() {
    setParams((p) => ({ ...p, page: 1 }));
  }

  function handleClearFilters() {
    setSearch('');
    setFilterValues({});
    setParams({ page: 1, pageSize: 50 });
  }

  function handleFilterChange(key: string, value: string | undefined) {
    setFilterValues((prev) => ({ ...prev, [key]: value }));
  }

  function handleDelete(id: string, name: string) {
    deleteMutation.mutate(id, {
      onSuccess: () => message.success(`Đã xóa dự án: ${name}`),
      onError: () => message.error('Xóa thất bại, vui lòng thử lại'),
    });
  }

  const columns: ColumnsType<PppProjectListItem> = [
    {
      title: 'STT',
      key: 'stt',
      width: 55,
      render: (_, __, idx) => {
        const page = params.page ?? 1;
        const pageSize = params.pageSize ?? 50;
        return (page - 1) * pageSize + idx + 1;
      },
    },
    {
      title: 'Tên dự án',
      dataIndex: 'projectName',
      key: 'projectName',
      ellipsis: true,
      sorter: (a, b) => a.projectName.localeCompare(b.projectName),
    },
    {
      title: 'Loại HĐ',
      dataIndex: 'contractType',
      key: 'contractType',
      width: 130,
      render: (v: number) => PPP_CONTRACT_TYPE_LABELS[v as keyof typeof PPP_CONTRACT_TYPE_LABELS] ?? v,
    },
    {
      title: 'Số QĐĐT',
      dataIndex: 'latestDecisionNumber',
      key: 'latestDecisionNumber',
      width: 130,
      render: (v: string | null) => v ?? '—',
    },
    {
      title: 'Ngày ký',
      dataIndex: 'latestDecisionDate',
      key: 'latestDecisionDate',
      width: 110,
      render: (v: string | null) => (v ? dayjs(v).format('DD/MM/YYYY') : '—'),
    },
    {
      title: 'CĐT / CQCQ',
      dataIndex: 'competentAuthorityName',
      key: 'competentAuthorityName',
      width: 180,
      ellipsis: true,
      render: (v: string | null) => v ?? '—',
    },
    {
      title: 'Tình trạng',
      dataIndex: 'statusName',
      key: 'statusName',
      width: 140,
      render: (v: string | null) => (v ? <Tag>{v}</Tag> : '—'),
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 130,
      render: (_, record) => (
        <Space size="small">
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => navigate({ to: `/ppp-projects/${record.id}` })}
          />
          <PermissionGate permission="INV.PPP.WRITE">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => navigate({ to: `/ppp-projects/${record.id}/edit` })}
            />
          </PermissionGate>
          <PermissionGate permission="INV.PPP.DELETE">
            <Popconfirm
              title="Xác nhận xóa"
              description={`Bạn chắc chắn muốn xóa dự án: ${record.projectName}? Hành động này không thể hoàn tác.`}
              onConfirm={() => handleDelete(record.id, record.projectName)}
              okText="Xóa"
              cancelText="Hủy"
              okButtonProps={{ danger: true }}
            >
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </PermissionGate>
        </Space>
      ),
    },
  ];

  return (
    <div data-testid="ppp-page">
      <PageBreadcrumb items={[{ label: 'Dự án PPP' }]} />
      <AdminPageHeader
        title="Dự án đầu tư theo hình thức PPP"
        description="Quản lý danh sách dự án đầu tư theo phương thức đối tác công tư"
        stats={{ total: data?.totalCount }}
        actions={
          <PermissionGate permission="INV.PPP.WRITE">
            <Button
              data-testid="ppp-btn-create"
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate({ to: '/ppp-projects/new' })}
            >
              Thêm mới
            </Button>
          </PermissionGate>
        }
      />
      <AdminContentCard noPadding>
        <PppProjectListFilters
          search={search}
          onSearchChange={setSearch}
          filterValues={filterValues}
          onFilterChange={handleFilterChange}
          actions={
            <Space>
              <Button data-testid="ppp-btn-search" icon={<SearchOutlined />} type="primary" onClick={handleSearch}>
                Tìm kiếm
              </Button>
              <Button data-testid="ppp-btn-clear" icon={<ClearOutlined />} onClick={handleClearFilters}>
                Xóa bộ lọc
              </Button>
            </Space>
          }
        />
        <Table<PppProjectListItem>
          data-testid="ppp-table-projects"
          rowKey="id"
          columns={columns}
          dataSource={data?.items}
          loading={isLoading}
          size="small"
          locale={{ emptyText: <EmptyState message="Chưa có dự án" description="Nhấn Thêm mới để tạo dự án PPP" /> }}
          pagination={{
            current: params.page,
            pageSize: params.pageSize,
            total: data?.totalCount,
            showSizeChanger: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} / ${total} bản ghi`,
            onChange: (page, pageSize) => setParams((p) => ({ ...p, page, pageSize })),
          }}
        />
      </AdminContentCard>
    </div>
  );
}
