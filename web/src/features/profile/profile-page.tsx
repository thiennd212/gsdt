import { useState } from 'react';
import { Button, Descriptions, Typography, Form, Input, message, Card } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import { useAuth } from '@/features/auth';
import { useTranslation } from 'react-i18next';
import { useChangePassword } from './profile-api';

// ProfilePage: displays OIDC user info + self-service change password form
export function ProfilePage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const profile = user?.profile;
  const roles = Array.isArray(profile?.role) ? (profile.role as string[]).join(', ') : (String(profile?.role ?? '—'));

  const [showForm, setShowForm] = useState(false);
  const [form] = Form.useForm();
  const changePassword = useChangePassword();

  async function handleSubmit(values: { currentPassword: string; newPassword: string }) {
    try {
      await changePassword.mutateAsync(values);
      message.success(t('profile.passwordChanged'));
      form.resetFields();
      setShowForm(false);
    } catch {
      message.error(t('profile.passwordError'));
    }
  }

  return (
    <div style={{ maxWidth: 600, margin: '24px auto' }}>
      <Typography.Title level={4}>{t('nav.profile')}</Typography.Title>
      <Descriptions bordered column={1} size="middle">
        <Descriptions.Item label={t('page.admin.users.col.fullName')}>{profile?.name ?? '—'}</Descriptions.Item>
        <Descriptions.Item label={t('page.admin.users.col.email')}>{profile?.email ?? '—'}</Descriptions.Item>
        <Descriptions.Item label={t('page.admin.users.col.roles')}>{roles}</Descriptions.Item>
      </Descriptions>

      {!showForm ? (
        <Button
          style={{ marginTop: 16 }}
          icon={<LockOutlined />}
          onClick={() => setShowForm(true)}
        >
          {t('profile.changePassword')}
        </Button>
      ) : (
        <Card title={t('profile.changePassword')} style={{ marginTop: 16 }} size="small">
          <Form form={form} layout="vertical" onFinish={handleSubmit}>
            <Form.Item
              name="currentPassword"
              label={t('profile.currentPassword')}
              rules={[{ required: true, message: t('profile.currentPasswordRequired') }]}
            >
              <Input.Password />
            </Form.Item>
            <Form.Item
              name="newPassword"
              label={t('profile.newPassword')}
              rules={[
                { required: true, message: t('profile.newPasswordRequired') },
                { min: 8, message: t('profile.newPasswordMin') },
              ]}
            >
              <Input.Password />
            </Form.Item>
            <Form.Item
              name="confirmPassword"
              label={t('profile.confirmPassword')}
              dependencies={['newPassword']}
              rules={[
                { required: true, message: t('profile.confirmPasswordRequired') },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('newPassword') === value) return Promise.resolve();
                    return Promise.reject(new Error(t('profile.passwordMismatch')));
                  },
                }),
              ]}
            >
              <Input.Password />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" loading={changePassword.isPending}>
                {t('profile.save')}
              </Button>
              <Button style={{ marginLeft: 8 }} onClick={() => { setShowForm(false); form.resetFields(); }}>
                {t('common.cancel')}
              </Button>
            </Form.Item>
          </Form>
        </Card>
      )}
    </div>
  );
}
