import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockUseFiles = vi.fn(() => ({ data: { items: [], totalCount: 0 }, isLoading: false }));
const mockUseUploadFile = vi.fn(() => ({ mutate: vi.fn(), isPending: false }));

const mockUseDeleteFile = vi.fn(() => ({ mutate: vi.fn(), isPending: false }));

vi.mock('@/features/files/file-api', () => ({
  useFiles: () => mockUseFiles(),
  useUploadFile: () => mockUseUploadFile(),
  useDeleteFile: () => mockUseDeleteFile(),
  downloadFile: vi.fn(),
}));

import { FileListPage } from '@/features/files/file-list-page';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

describe('FileListPage — render', () => {
  it('renders without crashing', () => {
    const { container } = render(<FileListPage />, { wrapper: makeWrapper() });
    expect(container.firstChild).toBeTruthy();
  });

  it('renders page title', () => {
    render(<FileListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.files.title')).toBeTruthy();
  });

  it('renders upload button', () => {
    render(<FileListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.files.upload')).toBeTruthy();
  });

  it('renders file table', () => {
    const { container } = render(<FileListPage />, { wrapper: makeWrapper() });
    expect(container.querySelector('table')).toBeTruthy();
  });

  it('renders file column headers', () => {
    render(<FileListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.files.col.fileName')).toBeTruthy();
    expect(screen.getByText('page.files.col.sizeBytes')).toBeTruthy();
    expect(screen.getByText('page.files.col.virusScan')).toBeTruthy();
  });

  it('renders file rows when data is provided', () => {
    mockUseFiles.mockReturnValue({
      data: {
        items: [
          {
            // PascalCase fields match the BE Dapper response (raw SQL) used as dataIndex in the table
            Id: 'file-1',
            OriginalFileName: 'report.pdf',
            ContentType: 'application/pdf',
            SizeBytes: 102400,
            VirusScanStatus: 'Clean',
            UploadedBy: 'user-1',
            UploadedAt: '2024-01-10T08:00:00Z',
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    });
    render(<FileListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('report.pdf')).toBeTruthy();
  });
});
