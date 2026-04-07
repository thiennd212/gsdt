// React Flow canvas for workflow designer — wraps ReactFlowProvider + ReactFlow
// Converts WorkflowStateDto[] + WorkflowTransitionDto[] to nodes/edges
// Supports drag-reposition, connect (draw transitions), and node/edge selection

import '@xyflow/react/dist/style.css';

import { useCallback, useMemo, useEffect } from 'react';
import {
  ReactFlow,
  ReactFlowProvider,
  Controls,
  Background,
  MiniMap,
  useNodesState,
  useEdgesState,
  addEdge,
} from '@xyflow/react';
import type { Connection, NodeMouseHandler, EdgeMouseHandler } from '@xyflow/react';
import type { WorkflowStateDto, WorkflowTransitionDto } from '../workflow-types';
import { CustomStateNode } from './custom-state-node';
import { CustomTransitionEdge } from './custom-transition-edge';

// Node/edge type keys — defined outside render to prevent re-registration
const NODE_TYPES = { workflowState: CustomStateNode } as const;
const EDGE_TYPES = { workflowTransition: CustomTransitionEdge } as const;

const STATE_NODE_WIDTH = 180;
const STATE_NODE_HEIGHT = 80;
const HORIZONTAL_GAP = 60;

// Build initial node positions in a grid layout (3 columns)
function buildNodes(states: WorkflowStateDto[]) {
  const COLS = 3;
  return states.map((s, i) => ({
    id: s.id,
    type: 'workflowState' as const,
    position: {
      x: (i % COLS) * (STATE_NODE_WIDTH + HORIZONTAL_GAP),
      y: Math.floor(i / COLS) * (STATE_NODE_HEIGHT + 60),
    },
    data: s as unknown as Record<string, unknown>,
  }));
}

// Build edges from transitions — edge id maps to transition id
function buildEdges(transitions: WorkflowTransitionDto[]) {
  return transitions.map((tr) => ({
    id: tr.id,
    source: tr.fromStateId,
    target: tr.toStateId,
    type: 'workflowTransition' as const,
    data: tr as unknown as Record<string, unknown>,
  }));
}

export interface GraphChange {
  nodes: typeof NODE_TYPES extends object ? ReturnType<typeof buildNodes> : never;
  edges: ReturnType<typeof buildEdges>;
}

interface ReactFlowCanvasProps {
  states: WorkflowStateDto[];
  transitions: WorkflowTransitionDto[];
  onGraphChange: (nodes: ReturnType<typeof buildNodes>, edges: ReturnType<typeof buildEdges>) => void;
  onNodeSelect: (stateId: string | null) => void;
  onEdgeSelect: (transitionId: string | null) => void;
}

// Inner canvas — must be inside ReactFlowProvider
function WorkflowFlowInner({
  states,
  transitions,
  onGraphChange,
  onNodeSelect,
  onEdgeSelect,
}: ReactFlowCanvasProps) {
  const initialNodes = useMemo(() => buildNodes(states), [states]);
  const initialEdges = useMemo(() => buildEdges(transitions), [transitions]);

  const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes);
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);

  // Re-sync when external states/transitions change (after save/undo)
  useEffect(() => { setNodes(buildNodes(states)); }, [states, setNodes]);
  useEffect(() => { setEdges(buildEdges(transitions)); }, [transitions, setEdges]);

  // Notify parent of graph changes for save/undo tracking
  const notifyChange = useCallback(
    (ns: typeof nodes, es: typeof edges) => onGraphChange(ns as ReturnType<typeof buildNodes>, es as ReturnType<typeof buildEdges>),
    [onGraphChange],
  );

  const handleNodesChange: typeof onNodesChange = useCallback(
    (changes) => {
      onNodesChange(changes);
      setNodes((nds) => { notifyChange(nds, edges); return nds; });
    },
    [onNodesChange, setNodes, notifyChange, edges],
  );

  const handleEdgesChange: typeof onEdgesChange = useCallback(
    (changes) => {
      onEdgesChange(changes);
      setEdges((eds) => { notifyChange(nodes, eds); return eds; });
    },
    [onEdgesChange, setEdges, notifyChange, nodes],
  );

  // Draw new transition by connecting handles
  const handleConnect = useCallback(
    (connection: Connection) => {
      setEdges((eds) => {
        const newEdges = addEdge(
          { ...connection, type: 'workflowTransition', data: {} },
          eds,
        );
        notifyChange(nodes, newEdges as ReturnType<typeof buildEdges>);
        return newEdges;
      });
    },
    [setEdges, notifyChange, nodes],
  );

  const handleNodeClick: NodeMouseHandler = useCallback(
    (_e, node) => onNodeSelect(node.id),
    [onNodeSelect],
  );

  const handleEdgeClick: EdgeMouseHandler = useCallback(
    (_e, edge) => onEdgeSelect(edge.id),
    [onEdgeSelect],
  );

  const handlePaneClick = useCallback(() => {
    onNodeSelect(null);
    onEdgeSelect(null);
  }, [onNodeSelect, onEdgeSelect]);

  const showMiniMap = states.length > 8;

  return (
    <div style={{ width: '100%', height: 520 }}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={NODE_TYPES}
        edgeTypes={EDGE_TYPES}
        onNodesChange={handleNodesChange}
        onEdgesChange={handleEdgesChange}
        onConnect={handleConnect}
        onNodeClick={handleNodeClick}
        onEdgeClick={handleEdgeClick}
        onPaneClick={handlePaneClick}
        fitView
        fitViewOptions={{ padding: 0.25 }}
        minZoom={0.3}
        deleteKeyCode="Delete"
      >
        <Controls />
        <Background gap={16} />
        {showMiniMap && <MiniMap nodeStrokeWidth={3} />}
      </ReactFlow>
    </div>
  );
}

// ReactFlowCanvas — exported canvas wrapped in required ReactFlowProvider
export function ReactFlowCanvas(props: ReactFlowCanvasProps) {
  return (
    <ReactFlowProvider>
      <WorkflowFlowInner {...props} />
    </ReactFlowProvider>
  );
}
