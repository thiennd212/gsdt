// AdminTenantSelector — cross-tenant override control for SystemAdmin users
// Stores the selected tenant ID in sessionStorage as 'admin-tenant-override'.
// The api-client interceptor picks up this value and sends it as X-Tenant-Id header.

import { useState, useEffect } from 'react';
import { Button, Select, Space, Tooltip, Tag, message } from 'antd';
import { SwapOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { usePermissions } from '@/features/auth/use-permissions';
import { apiClient } from '@/core/api';

const STORAGE_KEY = 'admin-tenant-override';

interface TenantSummary {
  tenantId: string;
  tenantName: string | null;
  userCount: number;
}

/** Returns true when the current user has the SystemAdmin role */
function useIsSystemAdmin(): boolean {
  const { hasRole } = usePermissions();
  return hasRole('SystemAdmin');
}

/**
 * AdminTenantSelector — only renders for SystemAdmin role users.
 * Shows a Select dropdown to pick a tenant from existing tenants for cross-tenant API calls.
 * Displays an active indicator badge when an override is active.
 */
export function AdminTenantSelector() {
  const isSystemAdmin = useIsSystemAdmin();
  const [selectorVisible, setSelectorVisible] = useState(false);
  const [tenants, setTenants] = useState<TenantSummary[]>([]);
  const [loading, setLoading] = useState(false);
  const [activeOverride, setActiveOverride] = useState<string | null>(() =>
    sessionStorage.getItem(STORAGE_KEY)
  );

  // Sync local state and auto-fetch tenants when override is active (for tag/tooltip display)
  useEffect(() => {
    const stored = sessionStorage.getItem(STORAGE_KEY);
    setActiveOverride(stored);
    if (stored) fetchTenants();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (!isSystemAdmin) return null;

  // Fetch tenant list from BE
  async function fetchTenants() {
    if (tenants.length > 0) return; // already loaded
    setLoading(true);
    try {
      const res = await apiClient.get<TenantSummary[]>('/admin/users/tenants');
      setTenants(res.data ?? []);
    } catch {
      message.error('Không thể tải danh sách tenant');
    } finally {
      setLoading(false);
    }
  }

  function handleSelect(tenantId: string) {
    sessionStorage.setItem(STORAGE_KEY, tenantId);
    setActiveOverride(tenantId);
    setSelectorVisible(false);
  }

  function handleClear() {
    sessionStorage.removeItem(STORAGE_KEY);
    setActiveOverride(null);
    setSelectorVisible(false);
  }

  // Truncate GUID for compact display: first 8 chars
  function shortId(id: string) {
    return id.length > 8 ? `${id.substring(0, 8)}…` : id;
  }

  return (
    <Space size={4} align="center">
      {activeOverride && (
        <Tooltip title={`Đang sử dụng tenant: ${tenants.find((t) => t.tenantId === activeOverride)?.tenantName ?? activeOverride}`}>
          <Tag
            color="orange"
            closable
            onClose={handleClear}
            icon={<SwapOutlined />}
            style={{ fontSize: 11, lineHeight: '18px', cursor: 'default' }}
          >
            {tenants.find((t) => t.tenantId === activeOverride)?.tenantName
              ?? `Tenant: ${shortId(activeOverride)}`}
          </Tag>
        </Tooltip>
      )}

      {selectorVisible ? (
        <Space size={4}>
          <Select
            size="small"
            placeholder="Chọn tenant"
            loading={loading}
            value={activeOverride ?? undefined}
            onChange={handleSelect}
            onDropdownVisibleChange={(open) => { if (open) fetchTenants(); }}
            style={{ width: 280, fontSize: 12 }}
            showSearch
            optionFilterProp="label"
            options={tenants.map((t) => ({
              value: t.tenantId,
              label: t.tenantName
                ? `${t.tenantName} (${t.userCount} người dùng)`
                : `${shortId(t.tenantId)} (${t.userCount} người dùng)`,
            }))}
            notFoundContent={loading ? 'Đang tải...' : 'Không có tenant'}
          />
          <Button
            size="small"
            icon={<CloseCircleOutlined />}
            onClick={() => setSelectorVisible(false)}
          />
        </Space>
      ) : (
        <Tooltip title="Chuyển đổi tenant (chỉ SystemAdmin)">
          <Button
            size="small"
            type="text"
            icon={<SwapOutlined />}
            onClick={() => { setSelectorVisible(true); fetchTenants(); }}
            style={{ color: 'var(--gov-text-secondary)', fontSize: 12 }}
          >
            {activeOverride ? 'Đổi tenant' : 'Chuyển tenant'}
          </Button>
        </Tooltip>
      )}
    </Space>
  );
}
