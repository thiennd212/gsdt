import { useState, useEffect } from 'react';
import { Tag, Button, Space, Typography, Spin, Descriptions } from 'antd';
import { ReloadOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';

const { Text } = Typography;

interface ServiceHealth {
  name: string;
  url: string;
  status: 'healthy' | 'unhealthy' | 'loading';
  responseTime?: number;
}

const SERVICES: Omit<ServiceHealth, 'status'>[] = [
  { name: 'API (nginx)', url: '/api/v1/feature-flags/EnablePdfWatermark' },
  { name: 'AuthServer', url: `${import.meta.env.VITE_OIDC_AUTHORITY ?? 'http://localhost:5000'}/health` },
  { name: 'OpenAPI Spec', url: '/openapi/v1.json' },
];

async function checkHealth(url: string): Promise<{ ok: boolean; ms: number }> {
  const start = performance.now();
  try {
    const res = await fetch(url, { signal: AbortSignal.timeout(5000) });
    return { ok: res.ok, ms: Math.round(performance.now() - start) };
  } catch {
    return { ok: false, ms: Math.round(performance.now() - start) };
  }
}

// Admin health check page — shows status of all backend services
export function HealthCheckPage() {
  const [services, setServices] = useState<ServiceHealth[]>(
    SERVICES.map((s) => ({ ...s, status: 'loading' as const }))
  );
  const [checking, setChecking] = useState(false);

  const runChecks = async () => {
    setChecking(true);
    setServices(SERVICES.map((s) => ({ ...s, status: 'loading' })));

    const results = await Promise.all(
      SERVICES.map(async (svc) => {
        const { ok, ms } = await checkHealth(svc.url);
        return { ...svc, status: ok ? 'healthy' : 'unhealthy', responseTime: ms } as ServiceHealth;
      })
    );
    setServices(results);
    setChecking(false);
  };

  // Run checks on mount
  useEffect(() => { runChecks(); }, []);

  const allHealthy = services.every((s) => s.status === 'healthy');
  const anyLoading = services.some((s) => s.status === 'loading');

  const overallTag = anyLoading
    ? <Spin size="small" />
    : allHealthy
      ? <Tag color="green" icon={<CheckCircleOutlined />}>Tất cả hoạt động</Tag>
      : <Tag color="red" icon={<CloseCircleOutlined />}>Có lỗi</Tag>;

  return (
    <div>
      <AdminPageHeader
        title="Trạng thái hệ thống"
        actions={
          <Button icon={<ReloadOutlined />} onClick={runChecks} loading={checking}>
            Kiểm tra lại
          </Button>
        }
      />
      <AdminContentCard>
        <Descriptions column={1} bordered title={overallTag}>
          {services.map((svc) => (
            <Descriptions.Item key={svc.name} label={svc.name}>
              {svc.status === 'loading' ? (
                <Spin size="small" />
              ) : (
                <Space>
                  <Tag color={svc.status === 'healthy' ? 'green' : 'red'}>
                    {svc.status === 'healthy' ? 'Healthy' : 'Unhealthy'}
                  </Tag>
                  {svc.responseTime !== undefined && (
                    <Text type="secondary">{svc.responseTime}ms</Text>
                  )}
                </Space>
              )}
            </Descriptions.Item>
          ))}
        </Descriptions>
      </AdminContentCard>
    </div>
  );
}
