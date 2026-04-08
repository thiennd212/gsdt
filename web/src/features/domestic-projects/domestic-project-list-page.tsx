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
import { DomesticProjectListFilters } from './domestic-project-list-filters';
import { useDomesticProjects, useDeleteDomesticProject } from './domestic-project-api';
import type { DomesticProjectListItem, DomesticProjectListParams } from './domestic-project-types';

// DomesticProjectListPage — danh sách dự án trong nước with filters, search, pagination (50/page)
export function DomesticProjectListPage() {
  const navigate = useNavigate();
  const [params, setParams] = useState<DomesticProjectListParams>({ page: 1, pageSize: 50 });
  const [search, setSearch] = useState('');

  const { data, isLoading } = useDomesticProjects({ ...params, search: search || undefined });
  const deleteMutation = useDeleteDomesticProject();

  function handleSearch() {
    setParams((p) => ({ ...p, page: 1 }));
  }

  function handleClearFilters() {
    setSearch('');
    setParams({ page: 1, pageSize: 50 });
  }

  function handleDelete(id: string, name: string) {
    deleteMutation.mutate(id, {
      onSuccess: () => message.success(`Đã xóa dự án: ${name}`),
      onError: () => message.error('Xóa thất bại, vui lòng thử lại'),
    });
  }

  const columns: ColumnsType<DomesticProjectListItem> = [
    {
      title: 'Mã DA',
      dataIndex: 'projectCode',
      key: 'projectCode',
      width: 120,
      sorter: (a, b) => a.projectCode.localeCompare(b.projectCode),
    },
    {
      title: 'Tên dự án',
      dataIndex: 'projectName',
      key: 'projectName',
      ellipsis: true,
      sorter: (a, b) => a.projectName.localeCompare(b.projectName),
    },
    {
      title: 'Số QĐ',
      dataIndex: 'latestDecisionNumber',
      key: 'latestDecisionNumber',
      width: 130,
      render: (v: string | null) => v || '—',
    },
    {
      title: 'Ngày ký',
      dataIndex: 'latestDecisionDate',
      key: 'latestDecisionDate',
      width: 110,
      render: (v: string | null) => (v ? dayjs(v).format('DD/MM/YYYY') : '—'),
    },
    {
      title: 'Chủ đầu tư',
      dataIndex: 'projectOwnerName',
      key: 'projectOwnerName',
      width: 180,
      ellipsis: true,
      render: (v: string | null) => v || '—',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'statusName',
      key: 'statusName',
      width: 140,
      render: (v: string | null) => v ? <Tag>{v}</Tag> : '—',
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
            onClick={() => navigate({ to: `/domestic-projects/${record.id}` })}
          />
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => navigate({ to: `/domestic-projects/${record.id}/edit` })}
          />
          <Popconfirm
            title="Xác nhận xóa"
            description={`Bạn xác nhận chắc chắn xóa dự án: ${record.projectName}? Lưu ý: Hành động này không thể hoàn tác.`}
            onConfirm={() => handleDelete(record.id, record.projectName)}
            okText="Xóa"
            cancelText="Hủy"
            okButtonProps={{ danger: true }}
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <PageBreadcrumb items={[{ label: 'Dự án trong nước' }]} />
      <AdminPageHeader
        title="Dự án đầu tư trong nước"
        description="Quản lý danh sách dự án đầu tư công trong nước"
        stats={{ total: data?.totalCount }}
        actions={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => navigate({ to: '/domestic-projects/new' })}
          >
            Thêm mới
          </Button>
        }
      />
      <AdminContentCard noPadding>
        <DomesticProjectListFilters
          search={search}
          onSearchChange={setSearch}
          actions={
            <Space>
              <Button icon={<SearchOutlined />} type="primary" onClick={handleSearch}>
                Tìm kiếm
              </Button>
              <Button icon={<ClearOutlined />} onClick={handleClearFilters}>
                Xóa bộ lọc
              </Button>
            </Space>
          }
        />
        <Table<DomesticProjectListItem>
          rowKey="id"
          columns={columns}
          dataSource={data?.items}
          loading={isLoading}
          size="small"
          locale={{ emptyText: <EmptyState message="Chưa có dự án" description="Nhấn Thêm mới để tạo dự án đầu tư trong nước" /> }}
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
