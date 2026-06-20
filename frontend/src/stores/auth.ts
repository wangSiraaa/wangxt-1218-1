import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/modules'
import type { AuthUser } from '@/types'

const ROLE_LABELS: Record<string, string> = {
  Admin: '系统管理员',
  Police: '办案人员',
  Prosecutor: '检察官',
  Clerk: '书记员'
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('je_token') || '')
  const user = ref<AuthUser | null>(
    JSON.parse(localStorage.getItem('je_user') || 'null')
  )

  const roleLabel = computed(() =>
    user.value ? ROLE_LABELS[user.value.role] || user.value.role : ''
  )
  const isAdmin = computed(() => user.value?.role === 'Admin')

  async function login(username: string, password: string) {
    const res = await authApi.login({ username, password })
    token.value = res.token
    user.value = res.user
    localStorage.setItem('je_token', res.token)
    localStorage.setItem('je_user', JSON.stringify(res.user))
  }

  function logout() {
    token.value = ''
    user.value = null
    localStorage.removeItem('je_token')
    localStorage.removeItem('je_user')
  }

  function hasRole(...roles: string[]) {
    return !!user.value && roles.includes(user.value.role)
  }

  return { token, user, roleLabel, isAdmin, login, logout, hasRole }
})
