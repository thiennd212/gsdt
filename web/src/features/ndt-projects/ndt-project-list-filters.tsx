import { Input, Select, Flex } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { useSeedCatalog } from '@/features/domestic-projects/domestic-project-api';
import { useGovernmentAgencies } from '@/features/admin-catalogs/catalog-api';
import { useProvinces } from '@/features/master-data/master-data-api';

interface NdtProjectListFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  filterValues?: Record<string, string | undefined>;
  onFilterChange?: (key: string, value: string | undefined) => void;
  actions?: React.ReactNode;
}

// NdtProjectListFilters — 5 filters: search, CQCQ, investorName, status, province.
export function NdtProjectListFilters({
  search,
  onSearchChange,
  filterValues = {},
  onFilterChange,
  actions,
}: NdtProjectListFiltersProps) {
  const { data: statuses = [] } = useSeedCatalog('ndt-project-statuses');
  const { data: governmentAgencies = [] } = useGovernmentAgencies(false);
  const { data: provinces = [] } = useProvinces();

  return (
    <Flex wrap gap={8} align="center" style={{ padding: '12px 24px' }}>
      <Input
        data-testid="ndt-input-search"
        prefix={<SearchOutlined style={{ color: 'var(--gov-text-muted)' }} />}
        placeholder="Tìm kiếm theo tên hoặc mã dự án..."
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        allowClear
        style={{ width: 280 }}
      />
      <Select
        data-testid="ndt-select-competent-authority"
        placeholder="Cơ quan có thẩm quyền"
        allowClear
        showSearch
        value={filterValues['competentAuthorityId']}
        onChange={(v) => onFilterChange?.('competentAuthorityId', v)}
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={governmentAgencies
          .filter((a) => a.isActive)
          .map((a) => ({ value: a.id, label: a.name }))}
        style={{ width: 200 }}
      />
      <Input
        data-testid="ndt-input-investor-name"
        placeholder="Tên nhà đầu tư"
        value={filterValues['investorName'] ?? ''}
        onChange={(e) => onFilterChange?.('investorName', e.target.value || undefined)}
        allowClear
        style={{ width: 200 }}
      />
      <Select
        data-testid="ndt-select-status"
        placeholder="Tình trạng"
        allowClear
        value={filterValues['statusId']}
        onChange={(v) => onFilterChange?.('statusId', v)}
        options={statuses.map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 160 }}
      />
      <Select
        data-testid="ndt-select-province"
        placeholder="Tỉnh/TP"
        allowClear
        showSearch
        value={filterValues['locationProvinceId']}
        onChange={(v) => onFilterChange?.('locationProvinceId', v)}
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={provinces.map((p) => ({ value: p.id, label: p.name }))}
        style={{ width: 180 }}
      />
      {actions && <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>{actions}</div>}
    </Flex>
  );
}
