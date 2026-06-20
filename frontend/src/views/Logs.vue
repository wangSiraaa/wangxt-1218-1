<script setup lang="ts">
import { onMounted, ref, reactive, h } from 'vue'
import { message } from 'ant-design-vue'
import { logsApi, caseApi } from '@/api/modules'
import type { RetrievalLogDto, CaseDto } from '@/types'
import dayjs from 'dayjs'
import { ReloadOutlined, SearchOutlined, ProfileOutlined } from '@ant-design/icons-vue'

const loading = ref(false)
const logs = ref<RetrievalLogDto[]>([])
const cases = ref<CaseDto[]>([])

const filter = reactive({
  caseId: undefined as number | undefined,
  userId: undefined as number | undefined,
  purpose: ''
})

async function loadCases() {
  try {
    cases.value = await caseApi.list()
  } catch {
    /* ignore */
  }
}

async function search() {
  loading.value = true
  try {
    logs.value = await logsApi.list({
      caseId: filter.caseId,
      userId: filter.userId,
      purpose: filter.purpose.trim() || undefined
    })
    message.success(`查询到 ${logs.value.length} 条调阅记录`)
  } finally {
    loading.value = false
  }
}

function reset() {
  filter.caseId = undefined
  filter.userId = undefined
  filter.purpose = ''
  search()
}

function fmt(t: string) {
  return dayjs(t).format('YYYY-MM-DD HH:mm:ss')
}

const columns = [
  { title: '证据名称', dataIndex: 'evidenceName', width: 220 },
  { title: '案件编号', dataIndex: 'caseNumber', width: 150 },
  { title: '调阅人', dataIndex: 'userName', width: 120 },
  { title: '调阅用途', dataIndex: 'purpose' },
  { title: '调阅时间', dataIndex: 'retrievedAt', width: 170 }
]

onMounted(async () => {
  await loadCases()
  await search()
})
</script>

<template>
  <div class="je-page">
    <div class="je-page-header">
      <div>
        <h2 class="je-page-title">调阅日志</h2>
        <div class="je-subtitle">审计证据调阅留痕，可按案件、人员、用途检索</div>
      </div>
    </div>

    <div class="je-card" style="margin-bottom: 16px">
      <a-form layout="inline">
        <a-form-item label="案件">
          <a-select
            v-model:value="filter.caseId"
            placeholder="全部案件"
            style="width: 240px"
            allow-clear
            show-search
            option-filter-prop="label"
          >
            <a-select-option v-for="c in cases" :key="c.id" :value="c.id" :label="c.caseNumber">
              {{ c.caseNumber }} - {{ c.title }}
            </a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="调阅人ID">
          <a-input-number
            v-model:value="filter.userId"
            placeholder="用户ID"
            style="width: 140px"
            :min="1"
          />
        </a-form-item>
        <a-form-item label="用途关键词">
          <a-input v-model:value="filter.purpose" placeholder="如 质证" style="width: 180px" allow-clear />
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" :icon="h(SearchOutlined)" @click="search">查询</a-button>
            <a-button :icon="h(ReloadOutlined)" @click="reset">重置</a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </div>

    <div class="je-card">
      <a-table
        :columns="columns"
        :data-source="logs"
        row-key="id"
        :loading="loading"
        :pagination="{ pageSize: 10, showSizeChanger: true }"
        size="middle"
      >
        <template #emptyText>
          <a-empty :image="h(ProfileOutlined)" description="暂无调阅记录" />
        </template>
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'retrievedAt'">{{ fmt(record.retrievedAt) }}</template>
        </template>
      </a-table>
    </div>
  </div>
</template>
