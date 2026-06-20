import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/Login.vue'),
    meta: { guest: true }
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { auth: true },
    children: [
      { path: '', redirect: '/cases' },
      {
        path: 'cases',
        name: 'cases',
        component: () => import('@/views/Cases.vue'),
        meta: { title: '案卷工作台', roles: ['Admin', 'Police', 'Prosecutor', 'Clerk'] }
      },
      {
        path: 'evidence/upload',
        name: 'evidence-upload',
        component: () => import('@/views/EvidenceUpload.vue'),
        meta: { title: '证据上传', roles: ['Admin', 'Police'] }
      },
      {
        path: 'evidence/review',
        name: 'evidence-review',
        component: () => import('@/views/EvidenceReview.vue'),
        meta: { title: '证据审查', roles: ['Admin', 'Prosecutor'] }
      },
      {
        path: 'retrieval',
        name: 'retrieval',
        component: () => import('@/views/Retrieval.vue'),
        meta: { title: '调阅管理', roles: ['Admin', 'Clerk'] }
      },
      {
        path: 'logs',
        name: 'logs',
        component: () => import('@/views/Logs.vue'),
        meta: { title: '调阅日志', roles: ['Admin', 'Prosecutor'] }
      }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: '/' }
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.auth && !auth.token) return '/login'
  if (to.meta.guest && auth.token) return '/cases'
  const roles = to.meta.roles as string[] | undefined
  if (roles && (!auth.user || !roles.includes(auth.user.role))) return '/cases'
})

export default router
