import { Select, Space } from 'antd';
import { useProvinces, useDistricts } from '@/features/master-data/master-data-api';

interface CascadingLocationSelectProps {
  provinceId: string | null;
  districtId: string | null;
  onProvinceChange: (id: string | null) => void;
  onDistrictChange: (id: string | null) => void;
  disabled?: boolean;
}

// CascadingLocationSelect — Province → District cascading dropdowns from MasterData.
// Clears district when province changes.
export function CascadingLocationSelect({
  provinceId,
  districtId,
  onProvinceChange,
  onDistrictChange,
  disabled,
}: CascadingLocationSelectProps) {
  const { data: provinces = [], isLoading: loadingProvinces } = useProvinces();
  const { data: districts = [], isLoading: loadingDistricts } = useDistricts(provinceId);

  return (
    <Space direction="vertical" style={{ width: '100%' }} size={8}>
      <Select
        placeholder="Chọn Tỉnh/TP"
        value={provinceId}
        onChange={(val) => {
          onProvinceChange(val ?? null);
          onDistrictChange(null);
        }}
        allowClear
        showSearch
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={provinces.map((p) => ({ value: p.id, label: p.name }))}
        loading={loadingProvinces}
        disabled={disabled}
        style={{ width: '100%' }}
      />
      <Select
        placeholder="Chọn Quận/Huyện"
        value={districtId}
        onChange={(val) => onDistrictChange(val ?? null)}
        allowClear
        showSearch
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={districts.map((d) => ({ value: d.id, label: d.name }))}
        loading={loadingDistricts}
        disabled={disabled || !provinceId}
        style={{ width: '100%' }}
      />
    </Space>
  );
}
