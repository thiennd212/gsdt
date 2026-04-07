// Custom React Flow node — renders a single WorkflowState as a rounded card
// Shows: displayNameVi, initial/final badges, configurable background color
// Source Handle at bottom, Target Handle at top

import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import type { NodeProps } from '@xyflow/react';
import { Tag, Typography } from 'antd';
import type { WorkflowStateDto } from '../workflow-types';

const { Text } = Typography;

// Darken a hex color for border usage
function darkenHex(hex: string): string {
  const n = parseInt(hex.replace('#', ''), 16);
  const r = Math.max(0, ((n >> 16) & 0xff) - 40);
  const g = Math.max(0, ((n >> 8) & 0xff) - 40);
  const b = Math.max(0, (n & 0xff) - 40);
  return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
}

// CustomStateNode — displayed in the visual workflow designer canvas
export const CustomStateNode = memo(function CustomStateNode({ data, selected }: NodeProps) {
  const state = data as unknown as WorkflowStateDto;
  const bg = state.color ?? '#808080';
  const border = selected ? `2px solid #1677ff` : `2px solid ${darkenHex(bg)}`;

  return (
    <div
      style={{
        background: bg,
        border,
        borderRadius: 10,
        padding: '8px 14px',
        minWidth: 140,
        maxWidth: 200,
        boxShadow: selected ? '0 0 0 2px rgba(22,119,255,0.3)' : '0 2px 6px rgba(0,0,0,0.15)',
        cursor: 'default',
      }}
    >
      {/* Target Handle — incoming transitions arrive at top */}
      <Handle type="target" position={Position.Top} style={{ background: '#555' }} />

      <div style={{ display: 'flex', alignItems: 'center', gap: 4, flexWrap: 'wrap' }}>
        <Text
          strong
          ellipsis
          style={{ color: '#fff', textShadow: '0 1px 2px rgba(0,0,0,0.4)', maxWidth: 150, flex: 1 }}
          title={state.displayNameVi}
        >
          {state.displayNameVi}
        </Text>
      </div>

      <Text
        type="secondary"
        style={{ fontSize: 10, color: 'rgba(255,255,255,0.75)', display: 'block' }}
      >
        {state.name}
      </Text>

      {(state.isInitial || state.isFinal) && (
        <div style={{ marginTop: 4, display: 'flex', gap: 4 }}>
          {state.isInitial && (
            <Tag color="green" style={{ fontSize: 10, lineHeight: '16px', padding: '0 4px', margin: 0 }}>
              Đầu
            </Tag>
          )}
          {state.isFinal && (
            <Tag color="red" style={{ fontSize: 10, lineHeight: '16px', padding: '0 4px', margin: 0 }}>
              Cuối
            </Tag>
          )}
        </div>
      )}

      {/* Source Handle — outgoing transitions leave from bottom */}
      <Handle type="source" position={Position.Bottom} style={{ background: '#555' }} />
    </div>
  );
});
