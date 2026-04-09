import { useState } from 'react';
import { Table, Button, Space, Select, Input, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useProvinces, useDistricts } from '@/features/master-data/master-data-api';
import { useAddFdiLocation, useDeleteFdiLocation, useFdiProject, fdiProjectKeys } from '../fdi-project-api';
import type { FdiLocationDto } from '../fdi-project-types';

interface LocationsZoneProps {
  projectId: string;
  disabled?: boolean;
}

// FdiTab1LocationsZone — inline add/delete with Province → District cascade + KKT/KCN field.
// Extra field `industrialZoneName` captures KKT/KCN/KCX/FTZ/TTTC zone names for FDI.
export function FdiTab1LocationsZone({ projectId, disabled }: LocationsZoneProps) {
  const { data: project } = useFdiProject(projectId);
  const locations = project?.locations ?? [];

  const [provinceId, setProvinceId] = useState<string | null>(null);
  const [districtId, setDistrictId] = useState<string | null>(null);
  const [address, setAddress] = useState('');
  const [industrialZoneName, setIndustrialZoneName] = useState('');

  const { data: provinces = [] } = useProvinces();
  const { data: districts = [] } = useDistricts(provinceId);

  const addMutation = useAddFdiLocation();
  const deleteMutation = useDeleteFdiLocation();

  function getProvinceName(id: string) {
    return provinces.find((p) => p.id === id)?.name ?? id;
  }

  function handleAdd() {
    if (!provinceId) { message.warning('Vui lòng chọn Tỉnh/TP'); return; }
    addMutation.mutate(
      {
        projectId,
        provinceId,
        districtId: districtId ?? undefined,
        address: address || undefined,
        industrialZoneName: industrialZoneName || undefined,
      },
      {
        onSuccess: () => {
          message.success('Thêm địa điểm thành công');
          setProvinceId(null);
          setDistrictId(null);
          setAddress('');
          setIndustrialZoneName('');
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

  const columns: ColumnsType<FdiLocationDto> = [
    { title: 'Tỉnh/TP', key: 'province', render: (_, r) => getProvinceName(r.provinceId), width: 180 },
    { title: 'Quận/Huyện', dataIndex: 'districtId', key: 'district', width: 160, render: (v) => v ?? '—' },
    { title: 'KKT/KCN/KCX', dataIndex: 'industrialZoneName', key: 'zone', width: 180, render: (v) => v ?? '—' },
    { title: 'Chi tiết', dataIndex: 'address', key: 'address', render: (v) => v ?? '—' },
    {
      title: '',
      key: 'actions',
      width: 60,
      render: (_, r) =>
        !disabled && (
          <Popconfirm title="Xóa địa điểm?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
            <Button size="small" danger icon={<DeleteOutlined />} loading={deleteMutation.isPending} />
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
            style={{ width: 180 }}
          />
          <Input
            placeholder="Tên KKT/KCN/KCX/FTZ"
            value={industrialZoneName}
            onChange={(e) => setIndustrialZoneName(e.target.value)}
            style={{ width: 200 }}
          />
          <Input
            placeholder="Chi tiết địa điểm"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            style={{ width: 220 }}
          />
          <Button icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>
            Thêm
          </Button>
        </Space>
      )}
      <Table<FdiLocationDto>
        rowKey="id"
        columns={columns}
        dataSource={locations}
        size="small"
        pagination={{ pageSize: 5, showTotal: (t) => `${t} địa điểm` }}
      />
    </div>
  );
}
