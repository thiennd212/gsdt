import { useState } from 'react';
import { Table, Button, Space, Select, Input, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useProvinces, useDistricts } from '@/features/master-data/master-data-api';
import { usePppProject } from '../ppp-project-api';
import { apiClient } from '@/core/api';
import { useQueryClient } from '@tanstack/react-query';
import { pppProjectKeys } from '../ppp-project-api';
import type { PppLocationDto } from '../ppp-project-types';

interface LocationsZoneProps {
  projectId: string;
  disabled?: boolean;
}

// PppTab1LocationsZone — inline add/delete with cascading Province → District selects.
// Mirrors domestic Tab1LocationsZone but uses PPP endpoints.
export function PppTab1LocationsZone({ projectId, disabled }: LocationsZoneProps) {
  const qc = useQueryClient();
  const { data: project } = usePppProject(projectId);
  const locations = project?.locations ?? [];

  const [provinceId, setProvinceId] = useState<string | null>(null);
  const [districtId, setDistrictId] = useState<string | null>(null);
  const [address, setAddress] = useState('');
  const [adding, setAdding] = useState(false);
  const [deleting, setDeleting] = useState<string | null>(null);

  const { data: provinces = [] } = useProvinces();
  const { data: districts = [] } = useDistricts(provinceId);

  function getProvinceName(id: string) {
    return provinces.find((p) => p.id === id)?.name ?? id;
  }

  async function handleAdd() {
    if (!provinceId) { message.warning('Vui lòng chọn Tỉnh/TP'); return; }
    setAdding(true);
    try {
      await apiClient.post(`/ppp-projects/${projectId}/locations`, {
        provinceId,
        districtId: districtId ?? undefined,
        address: address || undefined,
      });
      await qc.invalidateQueries({ queryKey: pppProjectKeys.detail(projectId) });
      message.success('Thêm địa điểm thành công');
      setProvinceId(null);
      setDistrictId(null);
      setAddress('');
    } catch {
      message.error('Thêm thất bại');
    } finally {
      setAdding(false);
    }
  }

  async function handleDelete(locationId: string) {
    setDeleting(locationId);
    try {
      await apiClient.delete(`/ppp-projects/${projectId}/locations/${locationId}`);
      await qc.invalidateQueries({ queryKey: pppProjectKeys.detail(projectId) });
      message.success('Đã xóa');
    } catch {
      message.error('Xóa thất bại');
    } finally {
      setDeleting(null);
    }
  }

  const columns: ColumnsType<PppLocationDto> = [
    { title: 'Tỉnh/TP', key: 'province', render: (_, r) => getProvinceName(r.provinceId), width: 200 },
    { title: 'Quận/Huyện', dataIndex: 'districtId', key: 'district', width: 200, render: (v) => v ?? '—' },
    { title: 'Chi tiết', dataIndex: 'address', key: 'address', render: (v) => v ?? '—' },
    {
      title: '',
      key: 'actions',
      width: 60,
      render: (_, r) =>
        !disabled && (
          <Popconfirm title="Xóa địa điểm?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
            <Button size="small" danger icon={<DeleteOutlined />} loading={deleting === r.id} />
          </Popconfirm>
        ),
    },
  ];

  return (
    <div>
      {!disabled && (
        <Space style={{ marginBottom: 12 }} wrap>
          <Select
            placeholder="Tỉnh/TP"
            value={provinceId}
            onChange={(v) => { setProvinceId(v); setDistrictId(null); }}
            allowClear
            showSearch
            filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
            options={provinces.map((p) => ({ value: p.id, label: p.name }))}
            style={{ width: 200 }}
          />
          <Select
            placeholder="Quận/Huyện"
            value={districtId}
            onChange={setDistrictId}
            allowClear
            showSearch
            disabled={!provinceId}
            filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
            options={districts.map((d) => ({ value: d.id, label: d.name }))}
            style={{ width: 200 }}
          />
          <Input
            placeholder="Chi tiết địa điểm"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            style={{ width: 240 }}
          />
          <Button icon={<PlusOutlined />} onClick={handleAdd} loading={adding}>
            Thêm
          </Button>
        </Space>
      )}
      <Table<PppLocationDto>
        rowKey="id"
        columns={columns}
        dataSource={locations}
        size="small"
        pagination={{ pageSize: 5, showTotal: (t) => `${t} địa điểm` }}
      />
    </div>
  );
}
