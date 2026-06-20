import axios, { type AxiosError } from 'axios'
import { message } from 'ant-design-vue'

const client = axios.create({ baseURL: '/api', timeout: 120000 })

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('je_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

client.interceptors.response.use(
  (res) => res,
  (err: AxiosError<{ message?: string }>) => {
    if (err.response) {
      const status = err.response.status
      const msg = err.response.data?.message || err.message
      if (status === 401) {
        localStorage.removeItem('je_token')
        localStorage.removeItem('je_user')
        message.error('登录已过期，请重新登录')
        if (!location.pathname.startsWith('/login')) location.href = '/login'
      } else {
        message.error(msg)
      }
    } else {
      message.error('网络异常，请稍后重试')
    }
    return Promise.reject(err)
  }
)

export default client
