import { useState } from 'react';
import { Table, Button, Input, Popconfirm } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { MoneyInput } from './money-input';
import type { DesignEstimateItemDto } from '@/features/ppp-projects/ppp-project-types';

// Local editable row — id may be empty string for new (unsaved) rows
type EditableItem = Omit<DesignEstimateItemDto, 'id'> & { id: string; _key: string };

interface DesignEstimateItemsTableProps {
  items: DesignEstimateItemDto[];
  onChange: (items: Omit<DesignEstimateItemDto, 'id'>[]) => void;
  disabled?: boolean;
}

let _seq = 0;
function nextKey() { return `new-${++_seq}`; }

function toEditable(items: DesignEstimateItemDto[]): EditableItem[] {
  return items.map((i) => ({ ...i, _key: i.id || nextKey() }));
}

// DesignEstimateItemsTable — inline editable table for design estimate line items.
// Parent holds the source of truth; this component fires onChange on every mutation.
export function DesignEstimateItemsTable({ items, onChange, disabled }: DesignEstimateItemsTableProps) {
  const [rows, setRows] = useState<EditableItem[]>(() => toEditable(items));

  function emit(next: EditableItem[]) {
    setRows(next);
    onChange(next.map(({ _key, ...rest }) => rest));
  }

  function handleAdd() {
    emit([...rows, { id: '', _key: nextKey(), name: '', scale: null, cost: 0, fileId: null }]);
  }

  function handleDelete(key: string) {
    emit(rows.filter((r) => r._key !== key));
  }

  function handleChange<K extends keyof EditableItem>(key: string, field: K, value: EditableItem[K]) {
    emit(rows.map((r) => (r._key === key ? { ...r, [field]: value } : r)));
  }

  const columns: ColumnsType<EditableItem> = [
    {
      title: 'Tên hạng mục',
      dataIndex: 'name',
      key: 'name',
      render: (v: string, record) =>
        disabled ? v : (
          <Input
            size="small"
            value={v}
            onChange={(e) => handleChange(record._key, 'name', e.target.value)}
            placeholder="Tên hạng mục"
          />
        ),
    },
    {
      title: 'Quy mô',
      dataIndex: 'scale',
      key: 'scale',
      width: 160,
      render: (v: string | null, record) =>
        disabled ? (v ?? '—') : (
          <Input
            size="small"
            value={v ?? ''}
            onChange={(e) => handleChange(record._key, 'scale', e.target.value || null)}
            placeholder="Quy mô"
          />
        ),
    },
    {
      title: 'Chi phí (triệu VNĐ)',
      dataIndex: 'cost',
      key: 'cost',
      width: 180,
      render: (v: number, record) =>
        disabled ? v?.toLocaleString('vi-VN') : (
          <MoneyInput
            size="small"
            value={v}
            onChange={(val) => handleChange(record._key, 'cost', (val as number) ?? 0)}
          />
        ),
    },
    {
      title: '',
      key: 'actions',
      width: 50,
      render: (_, record) =>
        !disabled && (
          <Popconfirm
            title="Xóa hạng mục?"
            onConfirm={() => handleDelete(record._key)}
            okText="Xóa"
            cancelText="Hủy"
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        ),
    },
  ];

  return (
    <div>
      <Table<EditableItem>
        rowKey="_key"
        columns={columns}
        dataSource={rows}
        size="small"
        pagination={false}
        locale={{ emptyText: 'Chưa có hạng mục' }}
      />
      {!disabled && (
        <Button
          size="small"
          icon={<PlusOutlined />}
          onClick={handleAdd}
          style={{ marginTop: 8 }}
        >
          Thêm hạng mục
        </Button>
      )}
    </div>
  );
}
