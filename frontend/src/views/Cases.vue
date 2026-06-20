<script setup lang="ts">
import { onMounted, ref, computed, reactive, h } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { caseApi, evidenceApi } from '@/api/modules'
import { useAuthStore } from '@/stores/auth'
import type { CaseDto, EvidenceDto } from '@/types'
import dayjs from 'dayjs'
import {
  PlusOutlined,
  ReloadOutlined,
  FileSearchOutlined,
  CheckCircleOutlined
} from '@ant-design/icons-vue'

const auth = useAuthStore()
const router = useRouter()

const loading = ref(false)
const cases = ref<CaseDto[]>([])
const keyword = ref('')

const filtered = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  if (!kw) return cases.value
  return cases.value.filter(
    (c) =>
      c.caseNumber.toLowerCase().includes(kw) || c.title.toLowerCase().includes(kw)
  )
})

async function load() {
  loading.value = true
  try {
    cases.value = await caseApi.list()
  } finally {
    loading.value = false
  }
}

const createVisible = ref(false)
const createForm = reactive({ caseNumber: '', title: '' })
const creating = ref(false)

async function onCreate() {
  if (!createForm.caseNumber || !createForm.title) {
    message.warning('请填写案件编号与名称')
    return
  }
  creating.value = true
  try {
    await caseApi.create({ ...createForm })
    message.success('案件创建成功')
    createVisible.value = false
    createForm.caseNumber = ''
    createForm.title = ''
    await load()
  } finally {
    creating.value = false
  }
}

const evidenceVisible = ref(false)
const evidenceLoading = ref(false)
const currentCase = ref<CaseDto | null>(null)
const evidences = ref<EvidenceDto[]>([])

async function openEvidence(c: CaseDto) {
  currentCase.value = c
  evidenceVisible.value = true
  evidenceLoading.value = true
  try {
    evidences.value = await evidenceApi.list(c.id)
  } finally {
    evidenceLoading.value = false
  }
}

const stageColor: Record<string, string> = {
  Police: 'blue',
  Prosecutor: 'orange',
  Court: 'purple'
}
const stageLabel: Record<string, string> = {
  Police: '公安移交',
  Prosecutor: '检察院审查',
  Court: '法院审理'
}

function fmt(t: string) {
  return dayjs(t).format('YYYY-MM-DD HH:mm')
}
function shortHash(h: string) {
  if (!h) return '-'
  return h.length > 16 ? `${h.slice(0, 8)}…${h.slice(-8)}` : h
}

const canCreate = computed(() => auth.hasRole('Admin', 'Police'))
const canReview = computed(() => auth.hasRole('Admin', 'Prosecutor'))

onMounted(load)
</script>

<template>
  <div class="je-page">
    <div class="je-page-header">
      <div>
        <h2 class="je-page-title">案卷工作台</h2>
        <div class="je-subtitle">管理案件与电子证据，跟踪公安→检察院→法院全链路状态</div>
      </div>
      <a-space>
        <a-input-search
          v-model:value="keyword"
          placeholder="搜索案件编号/名称"
          style="width: 240px"
          allow-clear
        />
        <a-button :icon="h(ReloadOutlined)" @click="load" :loading="loading">刷新</a-button>
        <a-button v-if="canCreate" type="primary" :icon="h(PlusOutlined)" @click="createVisible = true">
          新建案件
        </a-button>
      </a-space>
    </div>

    <div class="je-card">
      <a-table
        :columns="[
          { title: '案件编号', dataIndex: 'caseNumber', width: 160 },
          { title: '案件名称', dataIndex: 'title' },
          { title: '阶段', dataIndex: 'stage', width: 120 },
          { title: '证据数量', dataIndex: 'evidenceCount', width: 100, align: 'center' },
          { title: '创建人', dataIndex: 'creatorName', width: 120 },
          { title: '创建时间', dataIndex: 'createdAt', width: 160 },
          { title: '操作', key: 'op', width: 140, fixed: 'right' }
        ]"
        :data-source="filtered"
        row-key="id"
        :loading="loading"
        :pagination="{ pageSize: 10, showSizeChanger: true }"
        size="middle"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'stage'">
            <a-tag :color="stageColor[record.stage] || 'default'">
              {{ stageLabel[record.stage] || record.stage }}
            </a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'createdAt'">{{ fmt(record.createdAt) }}</template>
          <template v-else-if="column.key === 'op'">
            <a-button type="link" size="small" :icon="h(FileSearchOutlined)" @click="openEvidence(record)">
              查看证据
            </a-button>
            <a-button v-if="canReview" type="link" size="small" @click="router.push('/evidence/review')">
              审查
            </a-button>
          </template>
        </template>
      </a-table>
    </div>

    <a-modal
      v-model:open="createVisible"
      title="新建案件"
      :confirm-loading="creating"
      ok-text="创建"
      cancel-text="取消"
      @ok="onCreate"
    >
      <a-form layout="vertical" style="margin-top: 12px">
        <a-form-item label="案件编号" required>
          <a-input v-model:value="createForm.caseNumber" placeholder="如 AJ-2026-0001" />
        </a-form-item>
        <a-form-item label="案件名称" required>
          <a-input v-model:value="createForm.title" placeholder="请输入案件名称" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-drawer
      v-model:open="evidenceVisible"
      :title="currentCase ? `${currentCase.caseNumber} - 证据清单` : '证据清单'"
      width="640"
    >
      <a-spin :spinning="evidenceLoading">
        <a-empty v-if="!evidences.length && !evidenceLoading" description="该案件暂无证据" />
        <a-list v-else :data-source="evidences" item-layout="horizontal">
          <template #renderItem="{ item }">
            <a-list-item>
              <a-list-item-meta :description="`SHA-256：${shortHash(item.sha256)}`">
                <template #title>
                  <span style="font-weight: 600">{{ item.name }}</span>
                </template>
                <template #avatar>
                  <a-avatar style="background-color: #1b2a4a">证</a-avatar>
                </template>
              </a-list-item-meta>
              <template #actions>
                <a-tag v-if="item.isAdopted" color="green">
                  <CheckCircleOutlined /> 已采纳
                </a-tag>
                <a-tag v-else color="default">待审查</a-tag>
                <a-tag :color="item.hashVerified ? 'success' : 'error'">
                  {{ item.hashVerified ? '哈希一致' : '哈希异常' }}
                </a-tag>
              </template>
            </a-list-item>
          </template>
        </a-list>
      </a-spin>
    </a-drawer>
  </div>
</template>
