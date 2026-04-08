import { useState } from 'react';
import { Tabs, Badge } from 'antd';
import { CheckCircleFilled, ExclamationCircleFilled } from '@ant-design/icons';
import { Tab1GeneralInfo } from './tabs/tab1-general-info';
import { Tab2Implementation } from './tabs/tab2-implementation';
import { Tab3Disbursement } from './tabs/tab3-disbursement';
import { Tab4Inspection } from '@/features/shared/tabs/tab4-inspection';
import { Tab5Operation } from '@/features/shared/tabs/tab5-operation';
import { Tab6Documents } from '@/features/shared/tabs/tab6-documents';

interface TabsContainerProps {
  projectId: string | null; // null = create mode (only Tab 1 active)
  mode: 'create' | 'edit' | 'detail';
}

// Tab save status tracking — each tab has independent save state
type TabStatus = Record<string, 'saved' | 'unsaved' | 'idle'>;

// Tab label with saved/unsaved badge indicator (F12 multi-tab save strategy)
function TabLabel({ label, status }: { label: string; status: 'saved' | 'unsaved' | 'idle' }) {
  if (status === 'saved') {
    return (
      <span>
        {label} <CheckCircleFilled style={{ color: '#52c41a', fontSize: 12, marginLeft: 4 }} />
      </span>
    );
  }
  if (status === 'unsaved') {
    return (
      <Badge dot color="orange" offset={[4, 0]}>
        {label}
      </Badge>
    );
  }
  return <span>{label}</span>;
}

// DomesticProjectTabsContainer — 6-tab form container with multi-tab save strategy.
// Tab 1 creates project (returns ID). Tabs 2-6 save independently after ID is available.
// Each tab has isolated form state — error on one tab does not lose data on others.
export function DomesticProjectTabsContainer({ projectId, mode }: TabsContainerProps) {
  const [tabStatus, setTabStatus] = useState<TabStatus>({});
  const isReadonly = mode === 'detail';
  const hasProject = Boolean(projectId);

  function markTab(key: string, status: 'saved' | 'unsaved') {
    setTabStatus((prev) => ({ ...prev, [key]: status }));
  }

  const items = [
    {
      key: 'tab1',
      label: <TabLabel label="1. Thông tin chung" status={tabStatus['tab1'] ?? 'idle'} />,
      children: (
        <Tab1GeneralInfo
          projectId={projectId}
          mode={mode}
          onSaved={() => markTab('tab1', 'saved')}
          onDirty={() => markTab('tab1', 'unsaved')}
        />
      ),
    },
    {
      key: 'tab2',
      label: <TabLabel label="2. Tình hình TH" status={tabStatus['tab2'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab2Implementation projectId={projectId!} mode={mode} onSaved={() => markTab('tab2', 'saved')} />
      ) : null,
    },
    {
      key: 'tab3',
      label: <TabLabel label="3. Giải ngân" status={tabStatus['tab3'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab3Disbursement projectId={projectId!} mode={mode} onSaved={() => markTab('tab3', 'saved')} />
      ) : null,
    },
    {
      key: 'tab4',
      label: <TabLabel label="4. Thanh tra/KT" status={tabStatus['tab4'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab4Inspection projectId={projectId!} mode={mode} onSaved={() => markTab('tab4', 'saved')} />
      ) : null,
    },
    {
      key: 'tab5',
      label: <TabLabel label="5. Vận hành" status={tabStatus['tab5'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab5Operation projectId={projectId!} mode={mode} onSaved={() => markTab('tab5', 'saved')} />
      ) : null,
    },
    {
      key: 'tab6',
      label: <TabLabel label="6. Tài liệu" status={tabStatus['tab6'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab6Documents projectId={projectId!} mode={mode} onSaved={() => markTab('tab6', 'saved')} />
      ) : null,
    },
  ];

  return (
    <Tabs
      defaultActiveKey="tab1"
      items={items}
      type="card"
      style={{ marginTop: 16 }}
      destroyInactiveTabPane={false}
    />
  );
}
