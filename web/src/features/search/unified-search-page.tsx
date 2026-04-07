import { useState, useCallback } from 'react';
import { Row, Col, Typography, List, Tag, Space, Alert, Spin, Empty, Pagination } from 'antd';
import { FileTextOutlined, FolderOutlined, FormOutlined, UserOutlined, BellOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { SearchBar } from '@/shared/components/search-bar';
import { FacetSidebar } from '@/shared/components/facet-sidebar';
import { StatusBadge } from '@/shared/components/status-badge';
import { useUnifiedSearch, type SearchEntityType, type SearchResultItem } from './search-api';

const { Title, Text, Paragraph } = Typography;

// Icon map per entity type
const ENTITY_ICONS: Record<SearchEntityType, React.ReactNode> = {
  case: <FileTextOutlined />,
  file: <FolderOutlined />,
  form: <FormOutlined />,
  user: <UserOutlined />,
  notification: <BellOutlined />,
};

// Entity type color for tag
const ENTITY_COLOR: Record<SearchEntityType, string> = {
  case: 'blue',
  file: 'cyan',
  form: 'purple',
  user: 'geekblue',
  notification: 'orange',
};

// Route map — navigate to entity detail on click
const ENTITY_ROUTES: Partial<Record<SearchEntityType, (id: string) => string>> = {
  case: (id) => `/cases/${id}`,
  file: () => '/files',
  form: (id) => `/forms/${id}`,
};

// ResultCard — single search result list item with entity-aware icon and status
function ResultCard({ item }: { item: SearchResultItem }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const entityType = item.entityType as SearchEntityType;
  const routeFn = ENTITY_ROUTES[entityType];

  return (
    <List.Item
      style={{ cursor: routeFn ? 'pointer' : 'default', padding: '12px 0' }}
      onClick={() => routeFn && navigate({ to: routeFn(item.id) })}
    >
      <List.Item.Meta
        avatar={
          <div style={{ fontSize: 20, color: '#1677ff', marginTop: 2 }}>
            {ENTITY_ICONS[entityType] ?? <FileTextOutlined />}
          </div>
        }
        title={
          <Space size={6}>
            <Text strong>{item.title}</Text>
            <Tag color={ENTITY_COLOR[entityType] ?? 'default'}>{item.entityType}</Tag>
            {item.status && <StatusBadge status={item.status} />}
          </Space>
        }
        description={
          <Space direction="vertical" size={0}>
            {item.summary && <Paragraph type="secondary" style={{ margin: 0, fontSize: 13 }} ellipsis={{ rows: 2 }}>{item.summary}</Paragraph>}
            {item.updatedAt && (
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('search.updatedAt')}: {dayjs(item.updatedAt).format('DD/MM/YYYY HH:mm')}
              </Text>
            )}
          </Space>
        }
      />
    </List.Item>
  );
}

// UnifiedSearchPage — search bar + facet sidebar + entity-type-aware result cards
export function UnifiedSearchPage() {
  const { t } = useTranslation();
  const [query, setQuery] = useState('');
  const [selectedFacets, setSelectedFacets] = useState<Record<string, Set<string>>>({});
  const [page, setPage] = useState(1);

  // Stable callback to avoid SearchBar re-render loop
  const handleSearch = useCallback((val: string) => {
    setQuery(val);
    setPage(1);
  }, []);

  // Serialize facets sets → string arrays for API
  const facetsParam = Object.fromEntries(
    Object.entries(selectedFacets).map(([k, v]) => [k, Array.from(v)]),
  );

  const { data, isFetching, isError } = useUnifiedSearch({
    q: query,
    facets: facetsParam,
    pageNumber: page,
    pageSize: 20,
  });

  function handleFacetChange(facetKey: string, value: string, checked: boolean) {
    setSelectedFacets((prev) => {
      const next = { ...prev };
      const set = new Set(next[facetKey] ?? []);
      if (checked) set.add(value);
      else set.delete(value);
      next[facetKey] = set;
      return next;
    });
    setPage(1);
  }

  const results = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const facets = data?.facets ?? [];
  const showResults = query.trim().length >= 2;

  return (
    <div style={{ padding: '0 4px' }}>
      <Title level={4} style={{ marginBottom: 16 }}>{t('search.title')}</Title>

      {/* Search input */}
      <SearchBar
        onSearch={handleSearch}
        placeholder={t('search.placeholder')}
        loading={isFetching}
        width={480}
      />

      {/* Hint when query too short */}
      {!showResults && (
        <Paragraph type="secondary" style={{ marginTop: 16 }}>
          {t('search.hint')}
        </Paragraph>
      )}

      {showResults && (
        <Row gutter={16} style={{ marginTop: 16 }}>
          {/* Facet sidebar — only shown when facets are returned */}
          {facets.length > 0 && (
            <Col xs={24} sm={6}>
              <FacetSidebar
                facets={facets}
                selected={selectedFacets}
                onChange={handleFacetChange}
              />
            </Col>
          )}

          {/* Results panel */}
          <Col xs={24} sm={facets.length > 0 ? 18 : 24}>
            {isError && (
              <Alert type="error" message={t('search.error')} showIcon style={{ marginBottom: 12 }} />
            )}

            {isFetching && <Spin style={{ display: 'block', margin: '24px auto' }} />}

            {!isFetching && results.length === 0 && (
              <Empty description={t('search.noResults')} />
            )}

            {!isFetching && results.length > 0 && (
              <>
                <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
                  {t('search.resultCount', { count: total })}
                </Text>
                <List
                  dataSource={results}
                  renderItem={(item) => <ResultCard key={item.id} item={item} />}
                  split
                />
                <Pagination
                  current={page}
                  total={total}
                  pageSize={20}
                  onChange={setPage}
                  showSizeChanger={false}
                  style={{ marginTop: 16, textAlign: 'right' }}
                />
              </>
            )}
          </Col>
        </Row>
      )}
    </div>
  );
}
