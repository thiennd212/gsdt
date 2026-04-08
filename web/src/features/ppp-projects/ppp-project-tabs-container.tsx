import { useState } from 'react';
import { Tabs, Badge } from 'antd';
import { CheckCircleFilled, ExclamationCircleFilled } from '@ant-design/icons';
import { PppTab1GeneralInfo } from './tabs/ppp-tab1-general-info';
import { PppTab2ContractDetails } from './tabs/ppp-tab2-contract-details';
import { PppTab3Implementation } from './tabs/ppp-tab3-implementation';
import { PppTab4Disbursement } from './tabs/ppp-tab4-disbursement';
import { PppTab6OperationRevenue } from './tabs/ppp-tab6-operation-revenue';
import { Tab4Inspection } from '@/features/shared/tabs/tab4-inspection';
import { Tab6Documents } from '@/features/shared/tabs/tab6-documents';
import { usePppProject } from './ppp-project-api';

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

// PppProjectTabsContainer — 7-tab form container.
// Tab 1 creates project (returns ID). Tabs 2-7 save independently after ID is available.
// Tab 5 → shared Tab4Inspection; Tab 7 → shared Tab6Documents.
export function PppProjectTabsContainer({ projectId, mode }: TabsContainerProps) {
  const [tabStatus, setTabStatus] = useState<TabStatus>({});
  const hasProject = Boolean(projectId);

  function markTab(key: string, status: 'saved' | 'unsaved') {
    setTabStatus((prev) => ({ ...prev, [key]: status }));
  }

  const items = [
    {
      key: 'tab1',
      label: <TabLabel label="1. Thông tin chung" status={tabStatus['tab1'] ?? 'idle'} />,
      children: (
        <PppTab1GeneralInfo
          projectId={projectId}
          mode={mode}
          onSaved={() => markTab('tab1', 'saved')}
          onDirty={() => markTab('tab1', 'unsaved')}
        />
      ),
    },
    {
      key: 'tab2',
      label: <TabLabel label="2. HĐ & NĐT" status={tabStatus['tab2'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <PppTab2ContractDetails
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab2', 'saved')}
        />
      ) : null,
    },
    {
      key: 'tab3',
      label: <TabLabel label="3. Tình hình TH" status={tabStatus['tab3'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <PppTab3Implementation
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab3', 'saved')}
        />
      ) : null,
    },
    {
      key: 'tab4',
      label: <TabLabel label="4. Giải ngân" status={tabStatus['tab4'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <PppTab4Disbursement
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab4', 'saved')}
        />
      ) : null,
    },
    {
      key: 'tab5',
      label: <TabLabel label="5. Thanh tra/KT" status={tabStatus['tab5'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab4Inspection
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab5', 'saved')}
          dataHook={usePppProject}
        />
      ) : null,
    },
    {
      key: 'tab6',
      label: <TabLabel label="6. Vận hành & DT" status={tabStatus['tab6'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <PppTab6OperationRevenue
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab6', 'saved')}
        />
      ) : null,
    },
    {
      key: 'tab7',
      label: <TabLabel label="7. Tài liệu" status={tabStatus['tab7'] ?? 'idle'} />,
      disabled: !hasProject,
      children: hasProject ? (
        <Tab6Documents
          projectId={projectId!}
          mode={mode}
          onSaved={() => markTab('tab7', 'saved')}
          dataHook={usePppProject}
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
