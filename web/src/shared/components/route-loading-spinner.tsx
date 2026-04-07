import { Spin } from 'antd';

// RouteLoadingSpinner: fallback shown while lazy route chunks are loading
export function RouteLoadingSpinner() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', minHeight: 200 }}>
      <Spin tip="Loading..." size="large" />
    </div>
  );
}
