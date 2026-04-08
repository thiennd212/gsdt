import { useState } from 'react';
import { Table, Button, Space, Select, Input, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useProvinces, useDistricts } from '@/features/master-data/master-data-api';
import { useAddLocation, useDeleteLocation, useDomesticProject } from '../domestic-project-api';
import type { ProjectLocationDto } from '../domestic-project-types';

interface LocationsZoneProps {
  projectId: string;
  disabled?: boolean;
}

// Zone 4: Địa điểm TH — inline add/delete with cascading Province → District selects
export function Tab1LocationsZone({ projectId, disabled }: LocationsZoneProps) {
  const { data: project } = useDomesticProject(projectId);
  const locations = project?.locations ?? [];

  const [provinceId, setProvinceId] = useState<string | null>(null);
  const [districtId, setDistrictId] = useState<string | null>(null);
  const [address, setAddress] = useState('');

  const { data: provinces = [] } = useProvinces();
  const { data: districts = [] } = useDistricts(provinceId);

  const addMutation = useAddLocation();
  const deleteMutation = useDeleteLocation();

  function handleAdd() {
    if (!provinceId) { message.warning('Vui lòng chọn Tỉnh/TP'); return; }
    addMutation.mutate(
      { projectId, provinceId, districtId: districtId ?? undefined, address: address || undefined },
      {
        onSuccess: () => {
          message.success('Thêm địa điểm thành công');
          setProvinceId(null);
          setDistrictId(null);
          setAddress('');
        },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function handleDelete(locationId: string) {
    deleteMutation.mutate({ projectId, locationId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  // Resolve names for display
  function getProvinceName(id: string) { return provinces.find((p) => p.id === id)?.name ?? id; }

  const columns: ColumnsType<ProjectLocationDto> = [
    { title: 'Tỉnh/TP', key: 'province', render: (_, r) => getProvinceName(r.provinceId), width: 200 },
    { title: 'Quận/Huyện', dataIndex: 'districtId', key: 'district', width: 200, render: (v) => v ?? '—' },
    { title: 'Chi tiết', dataIndex: 'address', key: 'address', render: (v) => v ?? '—' },
    {
      title: '', key: 'actions', width: 60,
      render: (_, r) => !disabled && (
        <Popconfirm title="Xóa địa điểm?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
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
            allowClear showSearch
            filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
            options={provinces.map((p) => ({ value: p.id, label: p.name }))}
            style={{ width: 200 }}
          />
          <Select
            placeholder="Quận/Huyện"
            value={districtId}
            onChange={setDistrictId}
            allowClear showSearch disabled={!provinceId}
            filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
            options={districts.map((d) => ({ value: d.id, label: d.name }))}
            style={{ width: 200 }}
          />
          <Input placeholder="Chi tiết địa điểm" value={address} onChange={(e) => setAddress(e.target.value)} style={{ width: 240 }} />
          <Button icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>Thêm</Button>
        </Space>
      )}
      <Table<ProjectLocationDto>
        rowKey="id"
        columns={columns}
        dataSource={locations}
        size="small"
        pagination={{ pageSize: 5, showTotal: (t) => `${t} địa điểm` }}
      />
    </div>
  );
}
