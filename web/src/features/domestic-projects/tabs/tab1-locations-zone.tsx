import { Table, Button, Select, Input, Popconfirm } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useProvinces, useWardsByProvince } from '@/features/master-data/master-data-api';

// Client-side location row — no API calls, parent manages persistence
export interface LocalLocationRow {
  key: string;
  provinceId: string | null;
  /** Xã/Phường code (new 2-tier model, QĐ 19/2025) */
  wardCode: string | null;
  address: string;
}

interface LocationsZoneProps {
  /** Local rows managed by parent via state */
  rows: LocalLocationRow[];
  /** Callback when rows change */
  onChange: (rows: LocalLocationRow[]) => void;
  disabled?: boolean;
}

let nextKey = 1;

// Zone 4: Địa điểm thực hiện đầu tư — fully client-side editable table.
// Rows stored in parent state, included in create/update payload.
// No API calls — parent handles persistence when saving the project.
export function Tab1LocationsZone({ rows, onChange, disabled }: LocationsZoneProps) {
  const { data: provinces = [] } = useProvinces();

  function handleAdd() {
    onChange([...rows, { key: `loc-${nextKey++}`, provinceId: null, wardCode: null, address: '' }]);
  }

  function handleUpdate(key: string, patch: Partial<LocalLocationRow>) {
    onChange(rows.map((r) => (r.key === key ? { ...r, ...patch } : r)));
  }

  function handleDelete(key: string) {
    onChange(rows.filter((r) => r.key !== key));
  }

  function getProvinceName(id: string) {
    return provinces.find((p) => p.id === id)?.name ?? id;
  }

  const columns: ColumnsType<LocalLocationRow> = [
    {
      title: 'STT', key: 'stt', width: 50, align: 'center' as const,
      render: (_, __, i) => i + 1,
    },
    {
      title: <span>Tỉnh / Thành phố <span style={{ color: '#ff4d4f' }}>*</span></span>,
      key: 'province', width: 220,
      render: (_, row) => disabled ? getProvinceName(row.provinceId ?? '') : (
        <Select
          placeholder="-- Chọn --" size="small" value={row.provinceId}
          onChange={(v) => handleUpdate(row.key, { provinceId: v, wardCode: null })}
          allowClear showSearch style={{ width: '100%' }}
          filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
          options={provinces.map((p) => ({ value: p.id, label: p.name }))}
        />
      ),
    },
    {
      title: 'Xã / Phường', key: 'ward', width: 220,
      render: (_, row) => disabled ? (row.wardCode ?? '—') : (
        <WardSelect
          provinceCode={row.provinceId}
          value={row.wardCode}
          onChange={(v) => handleUpdate(row.key, { wardCode: v })}
        />
      ),
    },
    {
      title: 'Chi tiết địa điểm / Ghi chú', key: 'address',
      render: (_, row) => disabled ? (row.address || '—') : (
        <Input
          size="small" placeholder="Số nhà, tên công trình..."
          value={row.address}
          onChange={(e) => handleUpdate(row.key, { address: e.target.value })}
        />
      ),
    },
    {
      title: '', key: 'actions', width: 60, align: 'center' as const,
      render: (_, row) => disabled ? null : (
        <Popconfirm title="Xóa địa điểm?" onConfirm={() => handleDelete(row.key)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <div>
      {!disabled && (
        <div style={{ textAlign: 'right', marginBottom: 8 }}>
          <Button type="primary" icon={<PlusOutlined />} size="small" onClick={handleAdd}>
            Thêm địa điểm
          </Button>
        </div>
      )}
      <Table<LocalLocationRow>
        rowKey="key"
        columns={columns}
        dataSource={rows}
        size="small"
        pagination={false}
        locale={{ emptyText: 'Chưa có địa điểm — nhấn "Thêm địa điểm" để thêm' }}
      />
      <div style={{ color: '#94a3b8', fontSize: 12, marginTop: 6, fontStyle: 'italic' }}>
        * Bạn có thể thêm nhiều địa điểm nếu dự án trải dài trên nhiều địa bàn khác nhau.
      </div>
    </div>
  );
}

// Sub-component: Ward/commune select that loads wards by province (new 2-tier model)
function WardSelect({ provinceCode, value, onChange }: {
  provinceCode: string | null;
  value: string | null;
  onChange: (v: string | null) => void;
}) {
  const { data: wards = [] } = useWardsByProvince(provinceCode);
  return (
    <Select
      placeholder="-- Chọn --" size="small" value={value}
      onChange={onChange} allowClear showSearch disabled={!provinceCode}
      style={{ width: '100%' }}
      filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
      options={wards.map((w) => ({ value: w.code, label: w.nameVi }))}
    />
  );
}
