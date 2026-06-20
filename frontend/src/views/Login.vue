<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { useAuthStore } from '@/stores/auth'
import { SafetyCertificateOutlined } from '@ant-design/icons-vue'

const auth = useAuthStore()
const router = useRouter()
const loading = ref(false)

const form = reactive({ username: 'admin', password: 'admin123' })

const demoAccounts = [
  { role: '系统管理员', username: 'admin', password: 'admin123' },
  { role: '办案人员', username: 'police', password: 'police123' },
  { role: '检察官', username: 'prosecutor', password: 'pro123' },
  { role: '书记员', username: 'clerk', password: 'clerk123' }
]

async function onSubmit() {
  loading.value = true
  try {
    await auth.login(form.username, form.password)
    message.success(`欢迎，${auth.user?.fullName}`)
    router.push('/cases')
  } catch {
    /* 错误已由拦截器提示 */
  } finally {
    loading.value = false
  }
}

function fill(u: string, p: string) {
  form.username = u
  form.password = p
}
</script>

<template>
  <div class="je-login">
    <div class="je-login-bg" />
    <div class="je-login-panel">
      <div class="je-login-brand">
        <SafetyCertificateOutlined class="je-login-logo" />
        <h1>司法电子证据管理系统</h1>
        <p>公安移交 · 检察院审查 · 法院调阅 · 全链路不可破坏</p>
      </div>

      <a-form layout="vertical" @finish="onSubmit">
        <a-form-item label="账号" name="username" :rules="[{ required: true, message: '请输入账号' }]">
          <a-input v-model:value="form.username" size="large" placeholder="请输入账号" />
        </a-form-item>
        <a-form-item label="密码" name="password" :rules="[{ required: true, message: '请输入密码' }]">
          <a-input-password v-model:value="form.password" size="large" placeholder="请输入密码" @press-enter="onSubmit" />
        </a-form-item>
        <a-button type="primary" size="large" block :loading="loading" @click="onSubmit">
          登 录
        </a-button>
      </a-form>

      <div class="je-demo">
        <div class="je-demo-title">演示账号（点击填充）</div>
        <div class="je-demo-list">
          <div v-for="d in demoAccounts" :key="d.username" class="je-demo-item" @click="fill(d.username, d.password)">
            <span class="je-demo-role">{{ d.role }}</span>
            <span class="je-demo-acc">{{ d.username }} / {{ d.password }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.je-login {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.je-login-bg {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 20% 20%, #243b63 0%, #1b2a4a 45%, #0f1830 100%);
}

.je-login-bg::after {
  content: '';
  position: absolute;
  inset: 0;
  background-image: linear-gradient(135deg, rgba(200, 162, 75, 0.08) 0%, transparent 40%);
}

.je-login-panel {
  position: relative;
  z-index: 1;
  width: 380px;
  background: #fff;
  border-radius: 12px;
  padding: 36px 32px 24px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.35);
}

.je-login-brand {
  text-align: center;
  margin-bottom: 24px;
}

.je-login-logo {
  font-size: 40px;
  color: var(--je-gold);
}

.je-login-brand h1 {
  font-size: 20px;
  color: var(--je-primary);
  margin: 12px 0 6px;
  letter-spacing: 1px;
}

.je-login-brand p {
  font-size: 12px;
  color: #8a94a6;
  margin: 0;
}

.je-demo {
  margin-top: 20px;
  border-top: 1px dashed var(--je-border);
  padding-top: 14px;
}

.je-demo-title {
  font-size: 12px;
  color: #8a94a6;
  margin-bottom: 8px;
}

.je-demo-list {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
}

.je-demo-item {
  border: 1px solid var(--je-border);
  border-radius: 6px;
  padding: 6px 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.je-demo-item:hover {
  border-color: var(--je-gold);
  background: #fbf6ea;
}

.je-demo-role {
  display: block;
  font-size: 12px;
  color: var(--je-primary);
  font-weight: 600;
}

.je-demo-acc {
  font-size: 11px;
  color: #8a94a6;
}
</style>
