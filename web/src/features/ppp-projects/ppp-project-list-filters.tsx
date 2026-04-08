import { Input, Select, Flex, Radio } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { useSeedCatalog, useDynamicCatalog } from '@/features/domestic-projects/domestic-project-api';
import { PppContractType, PPP_CONTRACT_TYPE_LABELS } from './ppp-project-types';

interface PppProjectListFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  filterValues?: Record<string, string | undefined>;
  onFilterChange?: (key: string, value: string | undefined) => void;
  actions?: React.ReactNode;
}

const CONTRACT_TYPE_OPTIONS = Object.entries(PPP_CONTRACT_TYPE_LABELS).map(([value, label]) => ({
  value: Number(value) as PppContractType,
  label,
}));

// Filter bar for PPP project list — search + catalog dropdowns
export function PppProjectListFilters({
  search,
  onSearchChange,
  filterValues = {},
  onFilterChange,
  actions,
}: PppProjectListFiltersProps) {
  const { data: projectGroups = [] } = useSeedCatalog('project-groups');
  const { data: statuses = [] } = useSeedCatalog('ppp-project-statuses');
  const { data: industrySectors = [] } = useSeedCatalog('industry-sectors');
  const { data: managingAuthorities = [] } = useDynamicCatalog('managing-authorities');
  const { data: projectOwners = [] } = useDynamicCatalog('project-owners');

  return (
    <Flex wrap gap={8} align="center" style={{ padding: '12px 24px' }}>
      <Input
        data-testid="ppp-input-search"
        prefix={<SearchOutlined style={{ color: 'var(--gov-text-muted)' }} />}
        placeholder="Tìm kiếm theo tên hoặc mã dự án..."
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        allowClear
        style={{ width: 280 }}
      />
      <Select
        data-testid="ppp-select-managing-authority"
        placeholder="CQ quản lý"
        allowClear
        showSearch
        value={filterValues['managingAuthorityId']}
        onChange={(v) => onFilterChange?.('managingAuthorityId', v)}
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={managingAuthorities
          .filter((i) => i.isActive)
          .map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 180 }}
      />
      <Select
        data-testid="ppp-select-project-owner"
        placeholder="Chủ đầu tư"
        allowClear
        showSearch
        value={filterValues['projectOwnerId']}
        onChange={(v) => onFilterChange?.('projectOwnerId', v)}
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={projectOwners
          .filter((i) => i.isActive)
          .map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 180 }}
      />
      <Select
        data-testid="ppp-select-industry"
        placeholder="Lĩnh vực"
        allowClear
        value={filterValues['industrySectorId']}
        onChange={(v) => onFilterChange?.('industrySectorId', v)}
        options={industrySectors.map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 140 }}
      />
      <Select
        data-testid="ppp-select-contract-type"
        placeholder="Loại HĐ"
        allowClear
        value={filterValues['contractType']}
        onChange={(v) => onFilterChange?.('contractType', v)}
        options={CONTRACT_TYPE_OPTIONS}
        style={{ width: 160 }}
      />
      <Select
        data-testid="ppp-select-project-group"
        placeholder="Nhóm DA"
        allowClear
        value={filterValues['projectGroupId']}
        onChange={(v) => onFilterChange?.('projectGroupId', v)}
        options={projectGroups.map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 120 }}
      />
      <Select
        data-testid="ppp-select-status"
        placeholder="Tình trạng"
        allowClear
        value={filterValues['statusId']}
        onChange={(v) => onFilterChange?.('statusId', v)}
        options={statuses.map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 160 }}
      />
      <Radio.Group
        data-testid="ppp-radio-sub-project"
        defaultValue="all"
        size="small"
        optionType="button"
        buttonStyle="solid"
        onChange={(e) => {
          const val = e.target.value;
          onFilterChange?.('isSubProject', val === 'all' ? undefined : val === 'yes' ? 'true' : 'false');
        }}
      >
        <Radio.Button value="all">Tất cả</Radio.Button>
        <Radio.Button value="yes">DA TP</Radio.Button>
        <Radio.Button value="no">Không</Radio.Button>
      </Radio.Group>
      {actions && <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>{actions}</div>}
    </Flex>
  );
}
