import { useState } from 'react';
import { Table, Card, Button, Space, Popconfirm, Input, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useDomesticProject, useAddDocument, useDeleteDocument } from '@/features/domestic-projects/domestic-project-api';
import type { ProjectDocumentDto } from '@/features/domestic-projects/domestic-project-types';

interface Tab6Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// Tab 6: Tài liệu — shared component for TN + ODA.
// Document list with search + add/delete actions.
export function Tab6Documents({ projectId, mode }: Tab6Props) {
  const { data: project } = useDomesticProject(projectId);
  const documents = project?.documents ?? [];
  const isReadonly = mode === 'detail';
  const [search, setSearch] = useState('');

  const deleteDoc = useDeleteDocument();

  const filtered = search
    ? documents.filter((d) => d.title.toLowerCase().includes(search.toLowerCase()))
    : documents;

  function handleDelete(documentId: string) {
    deleteDoc.mutate({ projectId, documentId }, {
      onSuccess: () => message.success('Đã xóa tài liệu'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<ProjectDocumentDto> = [
    { title: 'Tiêu đề', dataIndex: 'title', key: 'title', ellipsis: true },
    { title: 'Ngày tải lên', dataIndex: 'uploadedAt', key: 'date', width: 130, render: (v: string) => dayjs(v).format('DD/MM/YYYY HH:mm') },
    { title: 'Ghi chú', dataIndex: 'notes', key: 'notes', width: 200, ellipsis: true, render: (v) => v ?? '—' },
    {
      title: 'Thao tác', key: 'actions', width: 70,
      render: (_, record) => !isReadonly && (
        <Popconfirm title="Xóa tài liệu?" onConfirm={() => handleDelete(record.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <Card
      size="small"
      title="Tài liệu dự án"
      extra={
        <Space>
          <Input
            prefix={<SearchOutlined />}
            placeholder="Tìm kiếm..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            allowClear
            size="small"
            style={{ width: 200 }}
          />
        </Space>
      }
    >
      <Table<ProjectDocumentDto>
        rowKey="id"
        columns={columns}
        dataSource={filtered}
        size="small"
        pagination={{ pageSize: 10, showTotal: (t) => `${t} tài liệu` }}
      />
    </Card>
  );
}
