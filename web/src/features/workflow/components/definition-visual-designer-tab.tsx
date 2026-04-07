// Visual Designer tab — wires ReactFlowCanvas, DesignerToolbar, PropertiesSidebar
// Manages local graph state with undo support; saves via useSaveDefinitionGraph

import { useState, useCallback, useRef } from 'react';
import { message } from 'antd';
import { useAuth } from '@/features/auth';
import { useTranslation } from 'react-i18next';
import type {
  WorkflowDefinitionDto,
  WorkflowStateDto,
  WorkflowTransitionDto,
  GraphStateInput,
  GraphTransitionInput,
} from '../workflow-types';
import { useSaveDefinitionGraph } from '../workflow-api';
import { ReactFlowCanvas } from './react-flow-canvas';
import { DesignerToolbar } from './designer-toolbar';
import { PropertiesSidebar } from './properties-sidebar';
import type { SelectedElement } from './properties-sidebar';
import { AddStateModal } from './add-state-modal';

interface Props {
  definition: WorkflowDefinitionDto;
}

// Convert StateDto → GraphStateInput (drop server-assigned id)
function stateToInput(s: WorkflowStateDto): GraphStateInput {
  return {
    name: s.name, displayNameVi: s.displayNameVi, displayNameEn: s.displayNameEn,
    isInitial: s.isInitial, isFinal: s.isFinal, color: s.color, sortOrder: s.sortOrder,
  };
}

// Convert TransitionDto → GraphTransitionInput (resolve stateId → stateName)
function transitionToInput(tr: WorkflowTransitionDto, stateMap: Map<string, string>): GraphTransitionInput {
  return {
    fromStateName: stateMap.get(tr.fromStateId) ?? tr.fromStateId,
    toStateName: stateMap.get(tr.toStateId) ?? tr.toStateId,
    actionName: tr.actionName, actionLabelVi: tr.actionLabelVi, actionLabelEn: tr.actionLabelEn,
    requiredRoleCode: tr.requiredRoleCode, conditionsJson: tr.conditionsJson, sortOrder: tr.sortOrder,
  };
}

// Auto-layout: initial states first, finals last, rest sorted by sortOrder
function applyAutoLayout(states: WorkflowStateDto[]): WorkflowStateDto[] {
  return [...states].sort((a, b) => {
    if (a.isInitial && !b.isInitial) return -1;
    if (!a.isInitial && b.isInitial) return 1;
    if (a.isFinal && !b.isFinal) return 1;
    if (!a.isFinal && b.isFinal) return -1;
    return a.sortOrder - b.sortOrder;
  });
}

export function DefinitionVisualDesignerTab({ definition }: Props) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const saveMutation = useSaveDefinitionGraph(definition.id);

  // Local graph state — starts from definition data
  const [states, setStates] = useState<WorkflowStateDto[]>(definition.states);
  const [transitions, setTransitions] = useState<WorkflowTransitionDto[]>(definition.transitions);

  // Snapshot for undo — reset to last saved/initial state
  const savedSnapshot = useRef({ states: definition.states, transitions: definition.transitions });
  const [hasChanges, setHasChanges] = useState(false);

  const [selectedElement, setSelectedElement] = useState<SelectedElement>(null);
  const [addStateOpen, setAddStateOpen] = useState(false);

  // ─── Canvas change handler ────────────────────────────────────────────────────
  const handleGraphChange = useCallback(() => setHasChanges(true), []);

  // ─── Node / Edge selection ────────────────────────────────────────────────────
  const handleNodeSelect = useCallback(
    (stateId: string | null) => {
      if (!stateId) { setSelectedElement(null); return; }
      const found = states.find((s) => s.id === stateId);
      if (found) setSelectedElement({ type: 'node', data: found });
    },
    [states],
  );

  const handleEdgeSelect = useCallback(
    (transitionId: string | null) => {
      if (!transitionId) { setSelectedElement(null); return; }
      const found = transitions.find((tr) => tr.id === transitionId);
      if (found) setSelectedElement({ type: 'edge', data: found });
    },
    [transitions],
  );

  // ─── Properties sidebar update ────────────────────────────────────────────────
  const handleElementUpdate = useCallback((updated: SelectedElement) => {
    if (!updated) return;
    if (updated.type === 'node') {
      setStates((prev) => prev.map((s) => (s.id === updated.data.id ? updated.data : s)));
    } else {
      setTransitions((prev) => prev.map((tr) => (tr.id === updated.data.id ? updated.data : tr)));
    }
    setHasChanges(true);
    setSelectedElement(updated);
  }, []);

  // ─── Auto Layout ──────────────────────────────────────────────────────────────
  const handleAutoLayout = useCallback(() => {
    setStates((prev) => applyAutoLayout(prev));
    setHasChanges(true);
  }, []);

  // ─── Undo ─────────────────────────────────────────────────────────────────────
  const handleUndo = useCallback(() => {
    setStates(savedSnapshot.current.states);
    setTransitions(savedSnapshot.current.transitions);
    setHasChanges(false);
    setSelectedElement(null);
  }, []);

  // ─── Save ─────────────────────────────────────────────────────────────────────
  const handleSave = useCallback(async () => {
    const stateMap = new Map(states.map((s) => [s.id, s.name]));
    try {
      await saveMutation.mutateAsync({
        tenantId: definition.tenantId,
        modifiedBy: user?.profile?.sub ?? '',
        states: states.map(stateToInput),
        transitions: transitions.map((tr) => transitionToInput(tr, stateMap)),
      });
      savedSnapshot.current = { states, transitions };
      setHasChanges(false);
      message.success(t('workflow.designer.saved', 'Đã lưu sơ đồ'));
    } catch {
      message.error(t('common.error', 'Có lỗi xảy ra'));
    }
  }, [saveMutation, states, transitions, definition.tenantId, t]);

  // ─── Add State ────────────────────────────────────────────────────────────────
  const handleStateAdded = useCallback((newState: WorkflowStateDto) => {
    setStates((prev) => [...prev, newState]);
    setHasChanges(true);
    setAddStateOpen(false);
  }, []);

  return (
    <div style={{ position: 'relative' }}>
      <DesignerToolbar
        onAddState={() => setAddStateOpen(true)}
        onAutoLayout={handleAutoLayout}
        onSave={handleSave}
        onUndo={handleUndo}
        hasChanges={hasChanges}
        saving={saveMutation.isPending}
      />

      <ReactFlowCanvas
        states={states}
        transitions={transitions}
        onGraphChange={handleGraphChange}
        onNodeSelect={handleNodeSelect}
        onEdgeSelect={handleEdgeSelect}
      />

      <PropertiesSidebar
        selectedElement={selectedElement}
        onUpdate={handleElementUpdate}
        onClose={() => setSelectedElement(null)}
      />

      <AddStateModal
        open={addStateOpen}
        nextSortOrder={states.length}
        onAdd={handleStateAdded}
        onCancel={() => setAddStateOpen(false)}
      />
    </div>
  );
}
