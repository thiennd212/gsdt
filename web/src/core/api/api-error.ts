import i18next from 'i18next';

// Normalized API error shape used throughout the app
export interface ApiError {
  status: number;
  code: string;        // e.g. "GOV_RPT_001"
  message: string;     // Vietnamese-first if available
  detail?: string;
  validationErrors?: Record<string, string[]>;
}

// Internal shape from backend RFC 9457 problem details
interface ProblemDetail {
  title?: string;
  detail?: string;
  detail_vi?: string;
  status?: number;
  errors?: Record<string, string[]>;
  code?: string;
}

// Backend ApiResponse envelope — matches shared/types/api.ts but re-declared
// here to avoid coupling api-client to shared layer
interface BackendApiError {
  code: string;
  message: string;
  field?: string;
}

interface BackendEnvelope<T = unknown> {
  data: T;
  success: boolean;
  message: string | null;
  errors: BackendApiError[] | null;
}

/** Map an HTTP status + raw response body to a normalized ApiError */
export function normalizeError(status: number, body: unknown): ApiError {
  // RFC 9457 problem detail (4xx/5xx from ASP.NET)
  if (body && typeof body === 'object') {
    const problem = body as ProblemDetail;
    const envelope = body as BackendEnvelope;

    // Prefer Vietnamese detail when available
    const message =
      problem.detail_vi ??
      problem.detail ??
      envelope.message ??
      problem.title ??
      defaultMessage(status);

    const code = problem.code ?? envelope.errors?.[0]?.code ?? String(status);

    const validationErrors: Record<string, string[]> | undefined =
      problem.errors && Object.keys(problem.errors).length > 0
        ? problem.errors
        : undefined;

    return { status, code, message, detail: problem.detail, validationErrors };
  }

  return { status, code: String(status), message: defaultMessage(status) };
}

// Vietnamese fallback strings (used when i18next is not yet initialized — e.g. tests, early boot)
const VI_FALLBACKS: Record<number, string> = {
  400: 'Yêu cầu không hợp lệ.',
  401: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
  403: 'Bạn không có quyền thực hiện thao tác này.',
  404: 'Không tìm thấy tài nguyên.',
  409: 'Dữ liệu đã bị thay đổi bởi người dùng khác. Vui lòng tải lại trang.',
  422: 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.',
  429: 'Quá nhiều yêu cầu. Vui lòng thử lại sau.',
  500: 'Lỗi hệ thống. Vui lòng liên hệ quản trị viên.',
};

function defaultMessage(status: number): string {
  const fallback = VI_FALLBACKS[status] ?? 'Đã xảy ra lỗi không xác định.';
  // Guard: i18next.t returns undefined when not initialized (e.g. unit tests, SSR).
  // Fall back to hardcoded Vietnamese strings in that case.
  if (!i18next.isInitialized) return fallback;
  switch (status) {
    case 400: return i18next.t('error.http.badRequest',      { defaultValue: fallback });
    case 401: return i18next.t('error.http.unauthorized',    { defaultValue: fallback });
    case 403: return i18next.t('error.http.forbidden',       { defaultValue: fallback });
    case 404: return i18next.t('error.http.notFound',        { defaultValue: fallback });
    case 409: return i18next.t('error.http.conflict',        { defaultValue: fallback });
    case 422: return i18next.t('error.http.unprocessable',   { defaultValue: fallback });
    case 429: return i18next.t('error.http.tooManyRequests', { defaultValue: fallback });
    case 500: return i18next.t('error.http.serverError',     { defaultValue: fallback });
    default:  return i18next.t('error.http.unknown',         { defaultValue: fallback });
  }
}
