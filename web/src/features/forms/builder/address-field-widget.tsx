// address-field-widget.tsx — cascading province/district/ward selects from masterdata API
import { useCallback } from 'react';
import { Select, Space } from 'antd';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api';

interface AddressValue {
  province?: string;
  district?: string;
  ward?: string;
}

interface AddressFieldWidgetProps {
  value?: AddressValue;
  onChange?: (value: AddressValue) => void;
  disabled?: boolean;
}

interface MasterDataItem {
  code: string;
  nameVi: string;
}

function useProvinces() {
  return useQuery({
    queryKey: ['masterdata', 'provinces'],
    queryFn: () => apiClient.get<MasterDataItem[]>('/masterdata/provinces').then((r) => r.data),
    staleTime: 30 * 60 * 1000,
  });
}

function useDistricts(provinceCode?: string) {
  return useQuery({
    queryKey: ['masterdata', 'districts', provinceCode],
    queryFn: () =>
      apiClient
        .get<MasterDataItem[]>(`/masterdata/provinces/${provinceCode}/districts`)
        .then((r) => r.data),
    enabled: Boolean(provinceCode),
    staleTime: 30 * 60 * 1000,
  });
}

function useWards(provinceCode?: string, districtCode?: string) {
  return useQuery({
    queryKey: ['masterdata', 'wards', provinceCode, districtCode],
    queryFn: () =>
      apiClient
        .get<MasterDataItem[]>(
          `/masterdata/provinces/${provinceCode}/districts/${districtCode}/wards`
        )
        .then((r) => r.data),
    enabled: Boolean(provinceCode && districtCode),
    staleTime: 30 * 60 * 1000,
  });
}

const filterOption = (input: string, option?: { label: string }) =>
  (option?.label ?? '').toLowerCase().includes(input.toLowerCase());

export function AddressFieldWidget({ value, onChange, disabled }: AddressFieldWidgetProps) {
  const { data: provinces, isLoading: pLoading, isError: pError } = useProvinces();
  const { data: districts, isLoading: dLoading, isError: dError } = useDistricts(value?.province);
  const { data: wards, isLoading: wLoading, isError: wError } = useWards(value?.province, value?.district);

  const handleProvince = useCallback(
    (code: string) => onChange?.({ province: code, district: undefined, ward: undefined }),
    [onChange]
  );

  const handleDistrict = useCallback(
    (code: string) => onChange?.({ province: value?.province, district: code, ward: undefined }),
    [onChange, value?.province]
  );

  const handleWard = useCallback(
    (code: string) => onChange?.({ province: value?.province, district: value?.district, ward: code }),
    [onChange, value?.province, value?.district]
  );

  return (
    <Space.Compact style={{ width: '100%' }}>
      <Select
        style={{ width: '33%' }}
        placeholder={pError ? 'Lỗi tải dữ liệu' : 'Tỉnh/Thành phố'}
        value={value?.province}
        onChange={handleProvince}
        loading={pLoading}
        disabled={disabled}
        status={pError ? 'error' : undefined}
        showSearch
        filterOption={filterOption}
        options={provinces?.map((p) => ({ value: p.code, label: p.nameVi }))}
        allowClear
      />
      <Select
        style={{ width: '33%' }}
        placeholder={dError ? 'Lỗi tải dữ liệu' : 'Quận/Huyện'}
        value={value?.district}
        onChange={handleDistrict}
        loading={dLoading}
        disabled={disabled || !value?.province}
        status={dError ? 'error' : undefined}
        showSearch
        filterOption={filterOption}
        options={districts?.map((d) => ({ value: d.code, label: d.nameVi }))}
        allowClear
      />
      <Select
        style={{ width: '34%' }}
        placeholder={wError ? 'Lỗi tải dữ liệu' : 'Phường/Xã'}
        value={value?.ward}
        onChange={handleWard}
        loading={wLoading}
        disabled={disabled || !value?.district}
        status={wError ? 'error' : undefined}
        showSearch
        filterOption={filterOption}
        options={wards?.map((w) => ({ value: w.code, label: w.nameVi }))}
        allowClear
      />
    </Space.Compact>
  );
}
