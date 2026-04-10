import { useState } from 'react';
import { Table, Button, Select, Input, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useProvinces, useDistricts } from '@/features/master-data/master-data-api';
import { useAddLocation, useDeleteLocation, useDomesticProject } from '../domestic-project-api';
import type { ProjectLocationDto } from '../domestic-project-types';

interface LocationsZoneProps {
  projectId: string | null;
  disabled?: boolean;
}

// Inline new-row state for the SRS-style editable table row
interface NewLocationRow {
  provinceId: string | null;
  districtId: string | null;
  address: string;
}

// Zone 4: Địa điểm thực hiện đầu tư — SRS mockup style.
// Inline table with cascading Province → District selects directly in table rows.
// "Thêm địa điểm" button adds a new editable row at the bottom.
export function Tab1LocationsZone({ projectId, disabled }: LocationsZoneProps) {
  const { data: project } = useDomesticProject(projectId ?? undefined);
  const locations = project?.locations ?? [];

  const [newRows, setNewRows] = useState<NewLocationRow[]>([]);
  const [savingIdx, setSavingIdx] = useState<number | null>(null);

  const { data: provinces = [] } = useProvinces();
  const addMutation = useAddLocation();
  const deleteMutation = useDeleteLocation();

  function handleAddRow() {
    setNewRows((prev) => [...prev, { provinceId: null, districtId: null, address: '' }]);
  }

  function updateNewRow(idx: number, patch: Partial<NewLocationRow>) {
    setNewRows((prev) => prev.map((r, i) => (i === idx ? { ...r, ...patch } : r)));
  }

  function handleSaveRow(idx: number) {
    const row = newRows[idx];
    if (!row?.provinceId) { message.warning('Vui lòng chọn Tỉnh/TP'); return; }
    if (!projectId) return;
    setSavingIdx(idx);
    addMutation.mutate(
      { projectId, provinceId: row.provinceId, districtId: row.districtId ?? undefined, address: row.address || undefined },
      {
        onSuccess: () => {
          message.success('Thêm địa điểm thành công');
          setNewRows((prev) => prev.filter((_, i) => i !== idx));
          setSavingIdx(null);
        },
        onError: () => { message.error('Thêm thất bại'); setSavingIdx(null); },
      },
    );
  }

  function handleDelete(locationId: string) {
    if (!projectId) return;
    deleteMutation.mutate({ projectId, locationId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  function getProvinceName(id: string) { return provinces.find((p) => p.id === id)?.name ?? id; }

  // Combine saved locations + new editable rows for display
  type DisplayRow = { key: string; isNew: boolean; idx: number; data: ProjectLocationDto | NewLocationRow };
  const displayRows: DisplayRow[] = [
    ...locations.map((loc, i) => ({ key: loc.id, isNew: false, idx: i, data: loc })),
    ...newRows.map((nr, i) => ({ key: `new-${i}`, isNew: true, idx: i, data: nr })),
  ];

  const columns: ColumnsType<DisplayRow> = [
    {
      title: 'STT', key: 'stt', width: 50, align: 'center' as const,
      render: (_, __, i) => i + 1,
    },
    {
      title: <span>Tỉnh / Thành phố <span style={{ color: '#ff4d4f' }}>*</span></span>,
      key: 'province', width: 220,
      render: (_, row) => {
        if (row.isNew) {
          const nr = row.data as NewLocationRow;
          return (
            <Select
              placeholder="-- Chọn --" size="small" value={nr.provinceId}
              onChange={(v) => updateNewRow(row.idx, { provinceId: v, districtId: null })}
              allowClear showSearch style={{ width: '100%' }}
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={provinces.map((p) => ({ value: p.id, label: p.name }))}
            />
          );
        }
        return getProvinceName((row.data as ProjectLocationDto).provinceId);
      },
    },
    {
      title: 'Quận / Huyện / Xã', key: 'district', width: 220,
      render: (_, row) => {
        if (row.isNew) {
          const nr = row.data as NewLocationRow;
          return <DistrictSelect provinceId={nr.provinceId} value={nr.districtId} onChange={(v) => updateNewRow(row.idx, { districtId: v })} />;
        }
        return (row.data as ProjectLocationDto).districtId ?? '—';
      },
    },
    {
      title: 'Chi tiết địa điểm / Ghi chú', key: 'address',
      render: (_, row) => {
        if (row.isNew) {
          const nr = row.data as NewLocationRow;
          return (
            <Input
              size="small" placeholder="Số nhà, tên công trình..." value={nr.address}
              onChange={(e) => updateNewRow(row.idx, { address: e.target.value })}
              onPressEnter={() => handleSaveRow(row.idx)}
            />
          );
        }
        return (row.data as ProjectLocationDto).address ?? '—';
      },
    },
    {
      title: '', key: 'actions', width: 60, align: 'center' as const,
      render: (_, row) => {
        if (disabled) return null;
        if (row.isNew) {
          return (
            <Button size="small" type="link" onClick={() => handleSaveRow(row.idx)} loading={savingIdx === row.idx}>
              Lưu
            </Button>
          );
        }
        return (
          <Popconfirm title="Xóa địa điểm?" onConfirm={() => handleDelete((row.data as ProjectLocationDto).id)} okText="Xóa" cancelText="Hủy">
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        );
      },
    },
  ];

  return (
    <div>
      {/* "Thêm địa điểm" button — top-right per SRS */}
      {!disabled && projectId && (
        <div style={{ textAlign: 'right', marginBottom: 8 }}>
          <Button type="primary" icon={<PlusOutlined />} size="small" onClick={handleAddRow}>
            Thêm địa điểm
          </Button>
        </div>
      )}
      <Table<DisplayRow>
        rowKey="key"
        columns={columns}
        dataSource={displayRows}
        size="small"
        pagination={false}
        locale={{ emptyText: projectId ? 'Chưa có địa điểm' : 'Lưu dự án trước để thêm địa điểm' }}
      />
      <div style={{ color: '#94a3b8', fontSize: 12, marginTop: 6, fontStyle: 'italic' }}>
        * Bạn có thể thêm nhiều địa điểm nếu dự án trải dài trên nhiều địa bàn khác nhau.
      </div>
    </div>
  );
}

// Sub-component: District select that loads districts based on provinceId
function DistrictSelect({ provinceId, value, onChange }: { provinceId: string | null; value: string | null; onChange: (v: string | null) => void }) {
  const { data: districts = [] } = useDistricts(provinceId);
  return (
    <Select
      placeholder="-- Chọn --" size="small" value={value}
      onChange={onChange} allowClear showSearch disabled={!provinceId}
      style={{ width: '100%' }}
      filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
      options={districts.map((d) => ({ value: d.id, label: d.name }))}
    />
  );
}
