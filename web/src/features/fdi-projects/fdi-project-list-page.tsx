import { useState } from 'react';
import { Table, Button, Space, Popconfirm, Tag, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, EyeOutlined, EditOutlined, DeleteOutlined, SearchOutlined, ClearOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { PageBreadcrumb } from '@/shared/components/page-breadcrumb';
import { EmptyState } from '@/shared/components/empty-state';
import { PermissionGate } from '@/shared/components/permission-gate';
import { FdiProjectListFilters } from './fdi-project-list-filters';
import { useFdiProjects, useDeleteFdiProject } from './fdi-project-api';
import type { FdiProjectListItem, FdiProjectListParams } from './fdi-project-types';

// FdiProjectListPage — danh sách dự án FDI with filters, search, pagination.
export function FdiProjectListPage() {
  const navigate = useNavigate();
  const [params, setParams] = useState<FdiProjectListParams>({ page: 1, pageSize: 50 });
  const [search, setSearch] = useState('');
  const [filterValues, setFilterValues] = useState<Record<string, string | undefined>>({});

  const { data, isLoading } = useFdiProjects({ ...params, ...filterValues, search: search || undefined });
  const deleteMutation = useDeleteFdiProject();

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

  const columns: ColumnsType<FdiProjectListItem> = [
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
      title: 'Mã DA',
      dataIndex: 'projectCode',
      key: 'projectCode',
      width: 120,
    },
    {
      title: 'Tên dự án',
      dataIndex: 'projectName',
      key: 'projectName',
      ellipsis: true,
      sorter: (a, b) => a.projectName.localeCompare(b.projectName),
    },
    {
      title: 'Nhà đầu tư',
      dataIndex: 'investorName',
      key: 'investorName',
      ellipsis: true,
      width: 200,
      render: (v: string | null) => v ?? '—',
    },
    {
      title: 'TMĐT (tr.đ)',
      dataIndex: 'prelimTotalInvestment',
      key: 'prelimTotalInvestment',
      width: 140,
      render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'CQCQ',
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
            onClick={() => navigate({ to: `/fdi-projects/${record.id}` })}
          />
          <PermissionGate permission="INV.FDI.WRITE">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => navigate({ to: `/fdi-projects/${record.id}/edit` })}
            />
          </PermissionGate>
          <PermissionGate permission="INV.FDI.DELETE">
            <Popconfirm
              title="Xác nhận xóa"
              description={`Bạn chắc chắn muốn xóa dự án: ${record.projectName}?`}
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
    <div data-testid="fdi-page">
      <PageBreadcrumb items={[{ label: 'Dự án FDI' }]} />
      <AdminPageHeader
        title="Dự án đầu tư nhà đầu tư nước ngoài (FDI)"
        description="Quản lý danh sách dự án đầu tư của nhà đầu tư nước ngoài"
        stats={{ total: data?.totalCount }}
        actions={
          <PermissionGate permission="INV.FDI.WRITE">
            <Button
              data-testid="fdi-btn-create"
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate({ to: '/fdi-projects/new' })}
            >
              Thêm mới
            </Button>
          </PermissionGate>
        }
      />
      <AdminContentCard noPadding>
        <FdiProjectListFilters
          search={search}
          onSearchChange={setSearch}
          filterValues={filterValues}
          onFilterChange={handleFilterChange}
          actions={
            <Space>
              <Button data-testid="fdi-btn-search" icon={<SearchOutlined />} type="primary" onClick={handleSearch}>
                Tìm kiếm
              </Button>
              <Button data-testid="fdi-btn-clear" icon={<ClearOutlined />} onClick={handleClearFilters}>
                Xóa bộ lọc
              </Button>
            </Space>
          }
        />
        <Table<FdiProjectListItem>
          data-testid="fdi-table-projects"
          rowKey="id"
          columns={columns}
          dataSource={data?.items}
          loading={isLoading}
          size="small"
          locale={{ emptyText: <EmptyState message="Chưa có dự án" description="Nhấn Thêm mới để tạo dự án FDI" /> }}
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
