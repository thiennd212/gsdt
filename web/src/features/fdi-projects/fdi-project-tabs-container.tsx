import { useState } from 'react';
import { Tabs, Badge } from 'antd';
import { CheckCircleFilled } from '@ant-design/icons';
import { FdiTab1GeneralInfo } from './tabs/fdi-tab1-general-info';
import { FdiTab2Implementation } from './tabs/fdi-tab2-implementation';
import { Tab4Inspection } from '@/features/shared/tabs/tab4-inspection';
import { Tab5Operation } from '@/features/shared/tabs/tab5-operation';
import { Tab6Documents } from '@/features/shared/tabs/tab6-documents';
import { useFdiProject } from './fdi-project-api';

interface TabsContainerProps {
  projectId: string | null; // null = create mode (only Tab 1 active)
  mode: 'create' | 'edit' | 'detail';
}

type TabStatus = Record<string, 'saved' | 'unsaved' | 'idle'>;

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

// FdiProjectTabsContainer — 5-tab form container for FDI projects.
// NO Tab HĐ FDI (investor contract), NO TKTT (design estimates).
export function FdiProjectTabsContainer({ projectId, mode }: TabsContainerProps) {
  const [tabStatus, setTabStatus] = useState<TabStatus>({});
  const hasProject = Boolean(projectId);

  function markTab(key: string, status: 'saved' | 'unsaved') {
    setTabStatus((prev) => ({ ...prev, [key]: status }));
  }

  const items = [
    {
      key: 'tab1',
      label: <TabLabel label="1. Thông tin QĐĐT" status={tabStatus['tab1'] ?? 'idle'} />,
      children: (
        <FdiTab1GeneralInfo
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
        <FdiTab2Implementation
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab2', 'saved')}
        />
      ) : null,
    },
    {
      key: 'tab3',
      label: <TabLabel label="3. Thanh tra/KT" status={tabStatus['tab3'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab4Inspection
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab3', 'saved')}
          dataHook={useFdiProject}
        />
      ) : null,
    },
    {
      key: 'tab4',
      label: <TabLabel label="4. Khai thác/VH" status={tabStatus['tab4'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab5Operation
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab4', 'saved')}
          dataHook={useFdiProject}
        />
      ) : null,
    },
    {
      key: 'tab5',
      label: <TabLabel label="5. Tài liệu" status={tabStatus['tab5'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab6Documents
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab5', 'saved')}
          dataHook={useFdiProject}
        />
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
