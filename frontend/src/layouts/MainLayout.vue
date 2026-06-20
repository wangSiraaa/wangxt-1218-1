<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  FolderOpenOutlined,
  CloudUploadOutlined,
  AuditOutlined,
  FileSearchOutlined,
  ProfileOutlined,
  LogoutOutlined,
  SafetyCertificateOutlined
} from '@ant-design/icons-vue'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const collapsed = ref(false)

interface MenuItem {
  key: string
  label: string
  icon: any
  roles: string[]
}

const allMenus: MenuItem[] = [
  { key: '/cases', label: '案卷工作台', icon: FolderOpenOutlined, roles: ['Admin', 'Police', 'Prosecutor', 'Clerk'] },
  { key: '/evidence/upload', label: '证据上传', icon: CloudUploadOutlined, roles: ['Admin', 'Police'] },
  { key: '/evidence/review', label: '证据审查', icon: AuditOutlined, roles: ['Admin', 'Prosecutor'] },
  { key: '/retrieval', label: '调阅管理', icon: FileSearchOutlined, roles: ['Admin', 'Clerk'] },
  { key: '/logs', label: '调阅日志', icon: ProfileOutlined, roles: ['Admin', 'Prosecutor'] }
]

const menus = computed(() => allMenus.filter((m) => auth.hasRole(...m.roles)))
const selectedKeys = computed(() => [route.path])

function go(path: string) {
  router.push(path)
}

function onMenuClick(info: { key: string }) {
  go(info.key)
}

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <a-layout class="je-layout">
    <a-layout-sider v-model:collapsed="collapsed" :trigger="null" collapsible class="je-sider">
      <div class="je-logo">
        <SafetyCertificateOutlined class="je-logo-icon" />
        <span v-if="!collapsed" class="je-logo-text">司法电子证据</span>
      </div>
      <a-menu
        :selected-keys="selectedKeys"
        mode="inline"
        theme="dark"
        @click="onMenuClick"
      >
        <a-menu-item v-for="m in menus" :key="m.key">
          <component :is="m.icon" />
          <span>{{ m.label }}</span>
        </a-menu-item>
      </a-menu>
    </a-layout-sider>

    <a-layout>
      <a-layout-header class="je-header">
        <div class="je-header-left">
          <span class="je-header-title">{{ String(route.meta.title || '案卷工作台') }}</span>
        </div>
        <a-dropdown>
          <a class="je-user" @click.prevent>
            <a-avatar style="background-color: #c8a24b">{{ auth.user?.fullName?.charAt(0) }}</a-avatar>
            <span class="je-user-name">{{ auth.user?.fullName }}</span>
            <span class="je-user-role">{{ auth.roleLabel }}</span>
          </a>
          <template #overlay>
            <a-menu>
              <a-menu-item key="logout" @click="logout">
                <LogoutOutlined />
                <span>退出登录</span>
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </a-layout-header>

      <a-layout-content class="je-content">
        <router-view />
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<style scoped>
.je-layout {
  height: 100vh;
}

.je-sider {
  background: #1b2a4a;
}

.je-logo {
  height: 56px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 18px;
  color: #fff;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.je-logo-icon {
  font-size: 22px;
  color: #c8a24b;
}

.je-logo-text {
  font-size: 15px;
  font-weight: 600;
  letter-spacing: 1px;
  white-space: nowrap;
}

.je-header {
  background: #fff;
  border-bottom: 1px solid var(--je-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  height: 56px;
}

.je-header-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--je-primary);
}

.je-user {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.je-user-name {
  font-size: 14px;
  color: #1f2937;
  font-weight: 500;
}

.je-user-role {
  font-size: 12px;
  color: #8a94a6;
  background: #eef1f7;
  padding: 2px 8px;
  border-radius: 10px;
}

.je-content {
  overflow: auto;
  background: var(--je-bg);
}
</style>
