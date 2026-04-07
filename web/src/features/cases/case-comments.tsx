import { useState, useMemo } from 'react';
import { List, Input, Button, Space, Typography, Avatar, message, Select, Tag } from 'antd';
import { UserOutlined, SendOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useAddComment } from './case-api';
import { useUsers } from '@/features/users/user-api';
import type { CaseComment } from './case-types';

const { TextArea } = Input;
const { Text } = Typography;

interface CaseCommentsProps {
  caseId: string;
  comments: CaseComment[];
}

// Regex to match @mentions in comment text (email-like patterns)
const MENTION_REGEX = /@([\w.+-]+@[\w.-]+)/g;

/** Render comment content with @mentions highlighted as tags */
function renderContentWithMentions(content: string) {
  const parts = content.split(MENTION_REGEX);
  if (parts.length === 1) return <Text style={{ whiteSpace: 'pre-wrap' }}>{content}</Text>;

  return (
    <Text style={{ whiteSpace: 'pre-wrap' }}>
      {parts.map((part, i) =>
        i % 2 === 1
          ? <Tag key={i} color="blue" style={{ margin: '0 2px' }}>@{part}</Tag>
          : part
      )}
    </Text>
  );
}

// CaseComments — comment list with @mention support + add-comment form
export function CaseComments({ caseId, comments }: CaseCommentsProps) {
  const { t } = useTranslation();
  const [content, setContent] = useState('');
  const [mentionedIds, setMentionedIds] = useState<string[]>([]);
  const [showMentionPicker, setShowMentionPicker] = useState(false);
  const addComment = useAddComment();

  // Fetch users for mention picker (light query, first page only)
  const { data: usersData } = useUsers({ pageNumber: 1, pageSize: 50 });
  const userOptions = useMemo(() =>
    (usersData?.items ?? []).map((u) => ({
      label: `${u.fullName} (${u.email})`,
      value: u.id,
      email: u.email,
      name: u.fullName,
    })),
    [usersData],
  );

  function handleAddMention(userId: string) {
    const user = userOptions.find((u) => u.value === userId);
    if (!user) return;

    // Append @email to content and track user ID
    setContent((prev) => `${prev}@${user.email} `);
    setMentionedIds((prev) => prev.includes(userId) ? prev : [...prev, userId]);
    setShowMentionPicker(false);
  }

  async function handleSubmit() {
    const trimmed = content.trim();
    if (!trimmed) return;
    try {
      await addComment.mutateAsync({
        id: caseId,
        body: {
          content: trimmed,
          mentionedUserIds: mentionedIds.length > 0 ? mentionedIds : undefined,
        },
      });
      setContent('');
      setMentionedIds([]);
      message.success(t('page.cases.comments.addSuccess'));
    } catch {
      message.error(t('page.cases.comments.addError'));
    }
  }

  return (
    <div>
      <List
        dataSource={comments}
        locale={{ emptyText: t('page.cases.comments.empty') }}
        renderItem={(item) => (
          <List.Item style={{ alignItems: 'flex-start', padding: '8px 0' }}>
            <List.Item.Meta
              avatar={<Avatar size="small" icon={<UserOutlined />} />}
              title={
                <Space size={8}>
                  <Text strong style={{ fontSize: 13 }}>
                    {item.authorName ?? item.authorId}
                  </Text>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {dayjs(item.createdAt).format('DD/MM/YYYY HH:mm')}
                  </Text>
                </Space>
              }
              description={renderContentWithMentions(item.content)}
            />
          </List.Item>
        )}
      />

      <div style={{ marginTop: 12 }}>
        <TextArea
          rows={3}
          placeholder={t('page.cases.comments.placeholder')}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          style={{ marginBottom: 8 }}
        />
        <Space wrap>
          <Button
            size="small"
            onClick={() => setShowMentionPicker(!showMentionPicker)}
          >
            {t('page.cases.comments.mentionBtn')}
          </Button>
          {showMentionPicker && (
            <Select
              showSearch
              size="small"
              placeholder={t('page.cases.comments.mentionSearch')}
              style={{ width: 250 }}
              options={userOptions}
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              onSelect={handleAddMention}
              autoFocus
            />
          )}
          {mentionedIds.length > 0 && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              {t('page.cases.comments.mentionCount', { count: mentionedIds.length })}
            </Text>
          )}
        </Space>
        <div style={{ marginTop: 8 }}>
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSubmit}
            loading={addComment.isPending}
            disabled={!content.trim()}
          >
            {t('page.cases.comments.submitBtn')}
          </Button>
        </div>
      </div>
    </div>
  );
}
