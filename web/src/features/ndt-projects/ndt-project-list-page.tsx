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
import { NdtProjectListFilters } from './ndt-project-list-filters';
import { useNdtProjects, useDeleteNdtProject } from './ndt-project-api';
import type { NdtProjectListItem, NdtProjectListParams } from './ndt-project-types';

// NdtProjectListPage — danh sách dự án NĐT with filters, search, pagination.
export function NdtProjectListPage() {
  const navigate = useNavigate();
  const [params, setParams] = useState<NdtProjectListParams>({ page: 1, pageSize: 50 });
  const [search, setSearch] = useState('');
  const [filterValues, setFilterValues] = useState<Record<string, string | undefined>>({});

  const { data, isLoading } = useNdtProjects({ ...params, ...filterValues, search: search || undefined });
  const deleteMutation = useDeleteNdtProject();

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

  const columns: ColumnsType<NdtProjectListItem> = [
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
            onClick={() => navigate({ to: `/ndt-projects/${record.id}` })}
          />
          <PermissionGate permission="INV.NDT.WRITE">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => navigate({ to: `/ndt-projects/${record.id}/edit` })}
            />
          </PermissionGate>
          <PermissionGate permission="INV.NDT.DELETE">
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
    <div data-testid="ndt-page">
      <PageBreadcrumb items={[{ label: 'Dự án NĐT' }]} />
      <AdminPageHeader
        title="Dự án đầu tư nhà đầu tư trong nước (NĐT)"
        description="Quản lý danh sách dự án đầu tư của nhà đầu tư trong nước"
        stats={{ total: data?.totalCount }}
        actions={
          <PermissionGate permission="INV.NDT.WRITE">
            <Button
              data-testid="ndt-btn-create"
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate({ to: '/ndt-projects/new' })}
            >
              Thêm mới
            </Button>
          </PermissionGate>
        }
      />
      <AdminContentCard noPadding>
        <NdtProjectListFilters
          search={search}
          onSearchChange={setSearch}
          filterValues={filterValues}
          onFilterChange={handleFilterChange}
          actions={
            <Space>
              <Button data-testid="ndt-btn-search" icon={<SearchOutlined />} type="primary" onClick={handleSearch}>
                Tìm kiếm
              </Button>
              <Button data-testid="ndt-btn-clear" icon={<ClearOutlined />} onClick={handleClearFilters}>
                Xóa bộ lọc
              </Button>
            </Space>
          }
        />
        <Table<NdtProjectListItem>
          data-testid="ndt-table-projects"
          rowKey="id"
          columns={columns}
          dataSource={data?.items}
          loading={isLoading}
          size="small"
          locale={{ emptyText: <EmptyState message="Chưa có dự án" description="Nhấn Thêm mới để tạo dự án NĐT" /> }}
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
