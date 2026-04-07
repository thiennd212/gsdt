import { useEffect } from 'react';
import { Badge, Popover, List, Typography, Button, Tag, App as AntApp } from 'antd';
import { BellOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useUnreadCount, useNotifications, useMarkAsRead } from './notification-api';
import { startSignalR, onNotification } from './notification-signalr';
import { useQueryClient } from '@tanstack/react-query';
import type { NotificationDto } from './notification-types';

const { Text } = Typography;

// NotificationBell — topbar bell icon with unread badge and recent notifications popover
export function NotificationBell() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { notification } = AntApp.useApp();
  const { data: unreadData } = useUnreadCount();
  const { data: recentData } = useNotifications(1, 10);
  const { mutate: markRead } = useMarkAsRead();

  const unreadCount = unreadData?.count ?? 0;
  const notifications = recentData?.items ?? [];

  // Start SignalR connection and refresh counts on new notification
  useEffect(() => {
    startSignalR();
    const unsub = onNotification((n) => {
      notification.info({ message: n.title, description: n.body, placement: 'topRight', duration: 5 });
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    });
    return unsub;
  }, [queryClient]);

  function handleClick(n: NotificationDto) {
    if (!n.isRead) markRead(n.id);
    // L-01: Only navigate to relative paths — prevent open redirect via server-provided deepLink
    if (n.deepLink?.startsWith('/')) navigate({ to: n.deepLink });
  }

  const popoverContent = (
    <div style={{ width: 320 }}>
      <List
        size="small"
        dataSource={notifications}
        locale={{ emptyText: t('page.notifications.empty') }}
        renderItem={(n) => (
          <List.Item
            role="button"
            tabIndex={0}
            style={{ cursor: 'pointer', opacity: n.isRead ? 0.6 : 1 }}
            onClick={() => handleClick(n)}
            onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleClick(n); } }}
          >
            <List.Item.Meta
              title={
                <span>
                  {!n.isRead && <Tag color="blue" style={{ marginRight: 6 }}>{t('page.notifications.newTag')}</Tag>}
                  <Text strong={!n.isRead}>{n.title}</Text>
                </span>
              }
              description={
                <span>
                  <Text type="secondary" style={{ fontSize: 12 }}>{n.body}</Text>
                  <br />
                  <Text type="secondary" style={{ fontSize: 11 }}>
                    {dayjs(n.createdAt).format('DD/MM/YYYY HH:mm')}
                  </Text>
                </span>
              }
            />
          </List.Item>
        )}
      />
      <div style={{ textAlign: 'center', paddingTop: 8, borderTop: '1px solid #f0f0f0' }}>
        <Button type="link" size="small" onClick={() => navigate({ to: '/notifications' })}>
          {t('page.notifications.viewAll')}
        </Button>
      </div>
    </div>
  );

  return (
    <Popover
      content={popoverContent}
      title={t('page.notifications.title')}
      trigger="click"
      placement="bottomRight"
    >
      <Badge count={unreadCount} size="small" style={{ cursor: 'pointer' }}>
        <BellOutlined style={{ fontSize: 18, color: '#fff', cursor: 'pointer' }} />
      </Badge>
    </Popover>
  );
}
