import { useState } from 'react';
import { Table, Button, Space, Popconfirm, Tag, Select, Input, Flex, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, EyeOutlined, EditOutlined, DeleteOutlined, SearchOutlined, ClearOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { PageBreadcrumb } from '@/shared/components/page-breadcrumb';
import { EmptyState } from '@/shared/components/empty-state';
import { useOdaProjects, useDeleteOdaProject, useSeedCatalog, useDynamicCatalog } from './oda-project-api';
import type { OdaProjectListItem, OdaProjectListParams } from './oda-project-types';

// OdaProjectListPage — 5 filters (no Nhom DA / DA thanh phan), different columns vs TN
export function OdaProjectListPage() {
  const navigate = useNavigate();
  const [params, setParams] = useState<OdaProjectListParams>({ page: 1, pageSize: 50 });
  const [search, setSearch] = useState('');

  const { data, isLoading } = useOdaProjects({ ...params, search: search || undefined });
  const deleteMutation = useDeleteOdaProject();

  const { data: odaTypes = [] } = useSeedCatalog('oda-project-types');
  const { data: statuses = [] } = useSeedCatalog('oda-project-statuses');
  const { data: managingAuthorities = [] } = useDynamicCatalog('managing-authorities');
  const { data: projectOwners = [] } = useDynamicCatalog('project-owners');

  function handleDelete(id: string, name: string) {
    deleteMutation.mutate(id, {
      onSuccess: () => message.success(`Đã xóa dự án: ${name}`),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<OdaProjectListItem> = [
    { title: 'Mã DA', dataIndex: 'projectCode', key: 'code', width: 120, sorter: (a, b) => a.projectCode.localeCompare(b.projectCode) },
    { title: 'Tên dự án', dataIndex: 'projectName', key: 'name', ellipsis: true, sorter: (a, b) => a.projectName.localeCompare(b.projectName) },
    { title: 'Loại DA', dataIndex: 'odaProjectTypeName', key: 'type', width: 130, render: (v) => v ?? '—' },
    { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'created', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Tình trạng', dataIndex: 'statusName', key: 'status', width: 140, render: (v) => v ? <Tag>{v}</Tag> : '—' },
    {
      title: 'Thao tác', key: 'actions', width: 130,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EyeOutlined />} onClick={() => navigate({ to: `/oda-projects/${record.id}` })} />
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate({ to: `/oda-projects/${record.id}/edit` })} />
          <Popconfirm
            title="Xác nhận xóa"
            description={`Bạn xác nhận xóa dự án: ${record.projectName}? Hành động không thể hoàn tác.`}
            onConfirm={() => handleDelete(record.id, record.projectName)}
            okText="Xóa" cancelText="Hủy" okButtonProps={{ danger: true }}
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div data-testid="oda-page">
      <PageBreadcrumb items={[{ label: 'Dự án ODA' }]} />
      <AdminPageHeader
        title="Dự án ODA"
        description="Quản lý dự án sử dụng vốn ODA và vốn vay ưu đãi"
        stats={{ total: data?.totalCount }}
        actions={<Button type="primary" icon={<PlusOutlined />} onClick={() => navigate({ to: '/oda-projects/new' })}>Thêm mới</Button>}
      />
      <AdminContentCard noPadding>
        <Flex wrap gap={8} align="center" style={{ padding: '12px 24px' }}>
          <Input prefix={<SearchOutlined />} placeholder="Tìm kiếm theo tên hoặc mã..." value={search} onChange={(e) => setSearch(e.target.value)} allowClear style={{ width: 280 }} />
          <Select placeholder="CQ quản lý" allowClear showSearch filterOption={(i, o) => String(o?.label ?? '').toLowerCase().includes(i.toLowerCase())} options={managingAuthorities.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))} style={{ width: 180 }} />
          <Select placeholder="Chủ đầu tư" allowClear showSearch filterOption={(i, o) => String(o?.label ?? '').toLowerCase().includes(i.toLowerCase())} options={projectOwners.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))} style={{ width: 180 }} />
          <Select placeholder="Tình trạng" allowClear options={statuses.map((i) => ({ value: i.id, label: i.name }))} style={{ width: 160 }} />
          <Select placeholder="Loại DA" allowClear options={odaTypes.map((i) => ({ value: i.id, label: i.name }))} style={{ width: 140 }} />
          <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>
            <Button icon={<SearchOutlined />} type="primary" onClick={() => setParams((p) => ({ ...p, page: 1 }))}>Tìm kiếm</Button>
            <Button icon={<ClearOutlined />} onClick={() => { setSearch(''); setParams({ page: 1, pageSize: 50 }); }}>Xóa bộ lọc</Button>
          </div>
        </Flex>
        <Table<OdaProjectListItem>
          rowKey="id" columns={columns} dataSource={data?.items} loading={isLoading} size="small"
          locale={{ emptyText: <EmptyState message="Chưa có dự án ODA" description="Nhấn Thêm mới để tạo dự án ODA" /> }}
          pagination={{
            current: params.page, pageSize: params.pageSize, total: data?.totalCount, showSizeChanger: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} / ${total} bản ghi`,
            onChange: (page, pageSize) => setParams((p) => ({ ...p, page, pageSize })),
          }}
        />
      </AdminContentCard>
    </div>
  );
}
