export interface ApiProblem {
  type?: string
  title: string
  status: number
  detail: string
  instance?: string
  traceId?: string
}

export class ApiError extends Error {
  readonly problem: ApiProblem

  constructor(problem: ApiProblem) {
    super(problem.detail || problem.title)
    this.name = 'ApiError'
    this.problem = problem
  }
}

export const isApiProblem = (value: unknown): value is ApiProblem => {
  if (!value || typeof value !== 'object') return false
  const problem = value as Partial<ApiProblem>
  return typeof problem.title === 'string' && typeof problem.status === 'number' && typeof problem.detail === 'string'
}

export const problemMessage = (error: unknown, fallback = 'Não foi possível concluir a operação.') => {
  if (error instanceof ApiError) return error.problem.detail || error.problem.title
  if (error instanceof Error) return error.message
  return fallback
}
