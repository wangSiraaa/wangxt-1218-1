import client from './client'
import type {
  AdoptionDto,
  CaseDto,
  EvidenceDetailDto,
  EvidenceDto,
  LoginResponse,
  RetrievalLogDto,
  VerifyResult
} from '@/types'

export const authApi = {
  login: (data: { username: string; password: string }) =>
    client.post<LoginResponse>('/auth/login', data).then((r) => r.data)
}

export const caseApi = {
  list: () => client.get<CaseDto[]>('/cases').then((r) => r.data),
  create: (data: { caseNumber: string; title: string }) =>
    client.post<CaseDto>('/cases', data).then((r) => r.data)
}

export const evidenceApi = {
  list: (caseId: number) =>
    client.get<EvidenceDto[]>('/evidence', { params: { caseId } }).then((r) => r.data),
  getById: (id: number) => client.get<EvidenceDetailDto>(`/evidence/${id}`).then((r) => r.data),
  upload: (caseId: number, hash: string, file: File) => {
    const form = new FormData()
    form.append('caseId', String(caseId))
    form.append('hash', hash)
    form.append('file', file)
    return client
      .post<EvidenceDto>('/evidence/upload', form, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      .then((r) => r.data)
  },
  adopt: (id: number, data: { opinion: string; adopted: boolean }) =>
    client.post<AdoptionDto>(`/evidence/${id}/adopt`, data).then((r) => r.data),
  verify: (id: number) => client.get<VerifyResult>(`/evidence/${id}/verify`).then((r) => r.data)
}

export const retrievalApi = {
  create: (data: { evidenceId: number; purpose: string }) =>
    client.post<RetrievalLogDto>('/retrieval', data).then((r) => r.data),
  download: (id: number) =>
    client.get<Blob>(`/retrieval/${id}/download`, { responseType: 'blob' }).then((r) => r.data)
}

export const logsApi = {
  list: (params: { caseId?: number; userId?: number; purpose?: string }) =>
    client.get<RetrievalLogDto[]>('/logs', { params }).then((r) => r.data)
}
