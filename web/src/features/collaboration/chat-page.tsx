import { useState, useRef, useEffect, useCallback } from 'react';
import { Layout, List, Typography, Input, Button, Space, Badge, Empty, Spin, message } from 'antd';
import { SendOutlined, MessageOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useAuth } from '@/features/auth/use-auth';
import {
  useConversations,
  useChatMessages,
  useSendMessage,
  type ConversationDto,
  type ChatMessageDto,
} from './chat-api';
import { useChatSignalR } from './use-chat-signalr';

const { Sider, Content } = Layout;
const { Text, Title, Paragraph } = Typography;
const { TextArea } = Input;

// ─── Conversation sidebar ─────────────────────────────────────────────────────

interface ConversationListProps {
  selected: string | null;
  onSelect: (id: string) => void;
}

function ConversationList({ selected, onSelect }: ConversationListProps) {
  const { t } = useTranslation();
  const { data: conversations, isLoading } = useConversations();

  if (isLoading) return <Spin style={{ display: 'block', margin: '24px auto' }} />;
  if (!conversations?.length) return <Empty description={t('chat.noConversations')} style={{ marginTop: 40 }} />;

  return (
    <List<ConversationDto>
      dataSource={conversations}
      renderItem={(conv) => (
        <List.Item
          key={conv.id}
          onClick={() => onSelect(conv.id)}
          style={{
            cursor: 'pointer',
            padding: '10px 16px',
            background: selected === conv.id ? '#e6f4ff' : 'transparent',
            borderLeft: selected === conv.id ? '3px solid #1677ff' : '3px solid transparent',
          }}
        >
          <List.Item.Meta
            avatar={
              <Badge count={conv.unreadCount} size="small">
                <MessageOutlined style={{ fontSize: 20, color: '#1677ff' }} />
              </Badge>
            }
            title={
              <Text strong ellipsis style={{ maxWidth: 160 }}>
                {conv.title ?? conv.participantNames.join(', ')}
              </Text>
            }
            description={
              <Text type="secondary" ellipsis style={{ fontSize: 12, maxWidth: 160 }}>
                {conv.lastMessage ?? t('chat.noMessages')}
              </Text>
            }
          />
          {conv.lastMessageAt && (
            <Text type="secondary" style={{ fontSize: 11, flexShrink: 0 }}>
              {dayjs(conv.lastMessageAt).format('HH:mm')}
            </Text>
          )}
        </List.Item>
      )}
    />
  );
}

// ─── Message panel ────────────────────────────────────────────────────────────

interface MessagePanelProps {
  conversationId: string;
}

function MessagePanel({ conversationId }: MessagePanelProps) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const currentUserId = (user?.profile as Record<string, string>)?.sub ?? '';

  const [input, setInput] = useState('');
  const [localMessages, setLocalMessages] = useState<ChatMessageDto[]>([]);
  const bottomRef = useRef<HTMLDivElement>(null);

  const { data, isLoading } = useChatMessages(conversationId, { pageSize: 50 });
  const sendMutation = useSendMessage(conversationId);

  // Merge API messages with real-time additions; deduplicate by id
  const apiMessages = data?.items ?? [];
  const allMessages = [
    ...apiMessages,
    ...localMessages.filter((lm) => !apiMessages.find((am) => am.id === lm.id)),
  ].sort((a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime());

  // Receive real-time messages from SignalR
  const handleRealTimeMessage = useCallback((msg: ChatMessageDto) => {
    setLocalMessages((prev) => {
      if (prev.find((m) => m.id === msg.id)) return prev;
      return [...prev, msg];
    });
  }, []);

  useChatSignalR({ conversationId, onMessage: handleRealTimeMessage });

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [allMessages.length]);

  // Clear local messages when switching conversations
  useEffect(() => {
    setLocalMessages([]);
  }, [conversationId]);

  async function handleSend() {
    const text = input.trim();
    if (!text) return;
    setInput('');
    try {
      await sendMutation.mutateAsync({ content: text });
    } catch {
      message.error(t('chat.sendError'));
      setInput(text);
    }
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
      e.preventDefault();
      handleSend();
    }
  }

  if (isLoading) return <Spin style={{ display: 'block', margin: '40px auto' }} />;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      {/* Messages */}
      <div style={{ flex: 1, overflowY: 'auto', padding: '16px 12px' }}>
        {allMessages.length === 0 && (
          <Empty description={t('chat.noMessages')} style={{ marginTop: 60 }} />
        )}
        {allMessages.map((msg) => {
          const isMine = msg.senderId === currentUserId;
          return (
            <div
              key={msg.id}
              style={{
                display: 'flex',
                flexDirection: isMine ? 'row-reverse' : 'row',
                gap: 8,
                marginBottom: 10,
              }}
            >
              <div style={{ maxWidth: '68%' }}>
                {!isMine && (
                  <Text type="secondary" style={{ fontSize: 12, display: 'block', marginBottom: 2 }}>
                    {msg.senderName}
                  </Text>
                )}
                <div
                  style={{
                    padding: '8px 12px',
                    borderRadius: isMine ? '12px 4px 12px 12px' : '4px 12px 12px 12px',
                    background: isMine ? '#1677ff' : '#f0f0f0',
                    color: isMine ? '#fff' : 'inherit',
                    fontSize: 14,
                    whiteSpace: 'pre-wrap',
                    wordBreak: 'break-word',
                  }}
                >
                  {msg.content}
                </div>
                <Text
                  type="secondary"
                  style={{
                    fontSize: 11,
                    display: 'block',
                    textAlign: isMine ? 'right' : 'left',
                    marginTop: 2,
                  }}
                >
                  {dayjs(msg.sentAt).format('HH:mm')}
                </Text>
              </div>
            </div>
          );
        })}
        <div ref={bottomRef} />
      </div>

      {/* Input */}
      <div style={{ borderTop: '1px solid #f0f0f0', padding: '8px 12px' }}>
        <Space.Compact style={{ width: '100%' }}>
          <TextArea
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={t('chat.inputPlaceholder')}
            autoSize={{ minRows: 1, maxRows: 4 }}
            style={{ resize: 'none' }}
          />
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSend}
            disabled={!input.trim()}
            loading={sendMutation.isPending}
            style={{ height: 'auto' }}
          />
        </Space.Compact>
        <Text type="secondary" style={{ fontSize: 11, marginTop: 4, display: 'block' }}>
          {t('chat.sendHint')}
        </Text>
      </div>
    </div>
  );
}

// ─── Main chat page ───────────────────────────────────────────────────────────

// ChatPage — conversation list sidebar + real-time message panel (SignalR)
export function ChatPage() {
  const { t } = useTranslation();
  const [selectedConvId, setSelectedConvId] = useState<string | null>(null);

  return (
    <div style={{ padding: '0 4px' }}>
      <Title level={4} style={{ marginBottom: 12 }}>{t('chat.title')}</Title>
      <Layout
        style={{
          height: 'calc(100vh - 160px)',
          border: '1px solid #f0f0f0',
          borderRadius: 8,
          overflow: 'hidden',
          background: '#fff',
        }}
      >
        {/* Conversation list sidebar */}
        <Sider width={260} style={{ background: '#fff', borderRight: '1px solid #f0f0f0', overflowY: 'auto' }}>
          <div style={{ padding: '12px 16px', borderBottom: '1px solid #f0f0f0' }}>
            <Text strong>{t('chat.conversationsLabel')}</Text>
          </div>
          <ConversationList selected={selectedConvId} onSelect={setSelectedConvId} />
        </Sider>

        {/* Message panel */}
        <Content style={{ background: '#fff', overflow: 'hidden' }}>
          {selectedConvId ? (
            <MessagePanel conversationId={selectedConvId} />
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%' }}>
              <Paragraph type="secondary">{t('chat.selectConversation')}</Paragraph>
            </div>
          )}
        </Content>
      </Layout>
    </div>
  );
}
