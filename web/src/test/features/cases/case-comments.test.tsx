import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockUseAddComment = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));
const mockUseUsers = vi.fn(() => ({ data: { items: [] } }));

vi.mock('@/features/cases/case-api', () => ({
  useAddComment: () => mockUseAddComment(),
}));

vi.mock('@/features/users/user-api', () => ({
  useUsers: () => mockUseUsers(),
}));

import { CaseComments } from '@/features/cases/case-comments';
import type { CaseComment } from '@/features/cases/case-types';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

const MOCK_COMMENTS: CaseComment[] = [
  {
    id: 'c1',
    caseId: 'case-1',
    authorId: 'user-1',
    authorName: 'Nguyễn Văn A',
    content: 'This is a comment',
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: 'c2',
    caseId: 'case-1',
    authorId: 'user-2',
    authorName: 'Trần Thị B',
    content: 'Another comment',
    createdAt: '2024-01-16T09:30:00Z',
  },
];

describe('CaseComments — empty state', () => {
  it('renders without crashing with no comments', () => {
    const { container } = render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={[]} />,
      { wrapper: makeWrapper() },
    );
    expect(container.firstChild).toBeTruthy();
  });

  // t mock returns key as-is — empty text renders as i18n key
  it('shows empty text key when no comments', () => {
    render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={[]} />,
      { wrapper: makeWrapper() },
    );
    expect(screen.getByText('page.cases.comments.empty')).toBeTruthy();
  });
});

describe('CaseComments — with data', () => {
  it('renders all comment author names', () => {
    render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={MOCK_COMMENTS} />,
      { wrapper: makeWrapper() },
    );
    expect(screen.getByText('Nguyễn Văn A')).toBeTruthy();
    expect(screen.getByText('Trần Thị B')).toBeTruthy();
  });

  it('renders comment content text', () => {
    render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={MOCK_COMMENTS} />,
      { wrapper: makeWrapper() },
    );
    expect(screen.getByText('This is a comment')).toBeTruthy();
  });
});

describe('CaseComments — input form', () => {
  // t mock returns key — placeholder and button render as i18n keys
  it('renders comment textarea placeholder key', () => {
    render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={[]} />,
      { wrapper: makeWrapper() },
    );
    expect(
      screen.getByPlaceholderText('page.cases.comments.placeholder'),
    ).toBeTruthy();
  });

  it('renders submit button key', () => {
    render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={[]} />,
      { wrapper: makeWrapper() },
    );
    expect(screen.getByText('page.cases.comments.submitBtn')).toBeTruthy();
  });

  it('renders mention button key', () => {
    render(
      <CaseComments caseId="case-1" tenantId="tenant-1" comments={[]} />,
      { wrapper: makeWrapper() },
    );
    expect(screen.getByText('page.cases.comments.mentionBtn')).toBeTruthy();
  });
});
