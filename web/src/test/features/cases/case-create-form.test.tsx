import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
}));

const mockUseCreateCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));

vi.mock('@/features/cases/case-api', () => ({
  useCreateCase: () => mockUseCreateCase(),
}));

import { CaseCreateForm } from '@/features/cases/case-create-form';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

// t mock returns key as-is — all labels/buttons render as i18n keys
describe('CaseCreateForm — modal render', () => {
  it('does not render modal content when open=false', () => {
    render(<CaseCreateForm open={false} onClose={vi.fn()} />, { wrapper: makeWrapper() });
    expect(screen.queryByText('page.cases.create.modalTitle')).toBeNull();
  });

  it('renders modal title key when open=true', () => {
    render(<CaseCreateForm open={true} onClose={vi.fn()} />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.cases.create.modalTitle')).toBeTruthy();
  });

  it('renders form field label keys when open=true', () => {
    render(<CaseCreateForm open={true} onClose={vi.fn()} />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.cases.create.titleLabel')).toBeTruthy();
    expect(screen.getByText('page.cases.create.descriptionLabel')).toBeTruthy();
    expect(screen.getByText('page.cases.create.typeLabel')).toBeTruthy();
    expect(screen.getByText('page.cases.create.priorityLabel')).toBeTruthy();
  });

  it('renders submit and cancel button keys', () => {
    render(<CaseCreateForm open={true} onClose={vi.fn()} />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.cases.create.okBtn')).toBeTruthy();
    expect(screen.getByText('common.cancel')).toBeTruthy();
  });
});
