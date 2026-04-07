import { describe, it, expect } from 'vitest';
import { normalizeError } from '@/core/api/api-error';

describe('normalizeError', () => {
  it('prefers detail_vi (Vietnamese) over English detail', () => {
    const err = normalizeError(400, {
      title: 'Bad Request',
      detail: 'Invalid input',
      detail_vi: 'Dữ liệu đầu vào không hợp lệ',
    });
    expect(err.message).toBe('Dữ liệu đầu vào không hợp lệ');
    expect(err.status).toBe(400);
  });

  it('falls back to English detail when detail_vi absent', () => {
    const err = normalizeError(404, { detail: 'Resource not found' });
    expect(err.message).toBe('Resource not found');
    expect(err.status).toBe(404);
  });

  it('uses default Vietnamese message when body is empty', () => {
    const err = normalizeError(500, null);
    expect(err.message).toBe('Lỗi hệ thống. Vui lòng liên hệ quản trị viên.');
    expect(err.status).toBe(500);
  });

  it('uses default Vietnamese message for 401', () => {
    const err = normalizeError(401, {});
    expect(err.message).toBe('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
  });

  it('uses default Vietnamese message for 403', () => {
    const err = normalizeError(403, {});
    expect(err.message).toBe('Bạn không có quyền thực hiện thao tác này.');
  });

  it('extracts validation errors from RFC 9457 errors map', () => {
    const err = normalizeError(422, {
      detail_vi: 'Dữ liệu không hợp lệ',
      errors: { Name: ['Tên không được để trống'], Email: ['Email không đúng định dạng'] },
    });
    expect(err.validationErrors).toEqual({
      Name: ['Tên không được để trống'],
      Email: ['Email không đúng định dạng'],
    });
  });

  it('extracts code from backend error code field', () => {
    const err = normalizeError(400, { code: 'GOV_RPT_001', detail_vi: 'Lỗi báo cáo' });
    expect(err.code).toBe('GOV_RPT_001');
  });

  it('falls back to string status as code when no code in body', () => {
    const err = normalizeError(503, { detail: 'Service unavailable' });
    expect(err.code).toBe('503');
  });
});
