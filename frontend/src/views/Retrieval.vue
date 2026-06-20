<script setup lang="ts">
import { onMounted, ref, computed, reactive, h } from 'vue'
import { message } from 'ant-design-vue'
import { caseApi, evidenceApi, retrievalApi, logsApi } from '@/api/modules'
import type { CaseDto, EvidenceDto, RetrievalLogDto } from '@/types'
import dayjs from 'dayjs'
import { ReloadOutlined, DownloadOutlined, FileSearchOutlined } from '@ant-design/icons-vue'

const loading = ref(false)
const cases = ref<CaseDto[]>([])
const selectedCaseId = ref<number | undefined>()
const evidences = ref<EvidenceDto[]>([])
const selectedEvidenceId = ref<number | undefined>()

const logs = ref<RetrievalLogDto[]>([])

const form = reactive({ purpose: '' })
const submitting = ref(false)

async function loadCases() {
  loading.value = true
  try {
    cases.value = await caseApi.list()
    if (cases.value.length) {
      selectedCaseId.value = cases.value[0].id
      await loadEvidence()
    }
  } finally {
    loading.value = false
  }
}

async function loadEvidence() {
  if (!selectedCaseId.value) return
  selectedEvidenceId.value = undefined
  evidences.value = await evidenceApi.list(selectedCaseId.value)
}

async function loadLogs() {
  try {
    logs.value = await logsApi.list({})
  } catch {
    /* ignore */
  }
}

async function onSubmit() {
  if (!selectedEvidenceId.value) {
    message.warning('请选择要调阅的证据')
    return
  }
  if (!form.purpose.trim()) {
    message.warning('请填写调阅用途，调阅必须留痕')
    return
  }
  submitting.value = true
  try {
    const log = await retrievalApi.create({
      evidenceId: selectedEvidenceId.value,
      purpose: form.purpose.trim()
    })
    message.success('调阅申请已登记，可下载庭审展示副本')
    form.purpose = ''
    await loadLogs()
    await download(log.id, log.evidenceName)
  } catch {
    /* 拦截器提示 */
  } finally {
    submitting.value = false
  }
}

async function download(id: number, name: string) {
  try {
    const blob = await retrievalApi.download(id)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `副本_${name}`
    document.body.appendChild(a)
    a.click()
    a.remove()
    URL.revokeObjectURL(url)
    message.success('副本已下载')
  } catch {
    /* 拦截器提示 */
  }
}

function fmt(t: string) {
  return dayjs(t).format('YYYY-MM-DD HH:mm')
}
function shortHash(h: string) {
  if (!h) return '-'
  return h.length > 16 ? `${h.slice(0, 8)}…${h.slice(-8)}` : h
}

const selectedEvidence = computed(
  () => evidences.value.find((e) => e.id === selectedEvidenceId.value) || null
)

const logColumns = [
  { title: '证据名称', dataIndex: 'evidenceName' },
  { title: '案件编号', dataIndex: 'caseNumber', width: 150 },
  { title: '调阅人', dataIndex: 'userName', width: 120 },
  { title: '调阅用途', dataIndex: 'purpose' },
  { title: '调阅时间', dataIndex: 'retrievedAt', width: 150 },
  { title: '操作', key: 'op', width: 110, fixed: 'right' as const }
]

onMounted(async () => {
  await loadCases()
  await loadLogs()
})
</script>

<template>
  <div class="je-page">
    <div class="je-page-header">
      <div>
        <h2 class="je-page-title">调阅管理</h2>
        <div class="je-subtitle">书记员调取庭审展示副本，每次调阅记录案件、人员与用途</div>
      </div>
      <a-button :icon="h(ReloadOutlined)" @click="loadCases" :loading="loading">刷新</a-button>
    </div>

    <div class="je-grid">
      <div class="je-card je-left">
        <div class="je-block-title">
          <FileSearchOutlined /> 选择证据
        </div>
        <a-select
          v-model:value="selectedCaseId"
          style="width: 100%; margin-bottom: 12px"
          placeholder="选择案件"
          show-search
          option-filter-prop="label"
          @change="loadEvidence"
        >
          <a-select-option v-for="c in cases" :key="c.id" :value="c.id" :label="c.caseNumber">
            {{ c.caseNumber }} - {{ c.title }}
          </a-select-option>
        </a-select>
        <a-table
          :columns="[
            { title: '证据名称', dataIndex: 'name' },
            { title: 'SHA-256', key: 'sha', width: 160 },
            { title: '状态', key: 'st', width: 90 }
          ]"
          :data-source="evidences"
          row-key="id"
          :loading="loading"
          :pagination="false"
          size="small"
          :row-selection="{
            type: 'radio',
            selectedRowKeys: selectedEvidenceId ? [selectedEvidenceId] : [],
            onChange: (keys: any[]) => (selectedEvidenceId = keys[0] as number)
          }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'sha'">
              <span class="je-hash">{{ shortHash(record.sha256) }}</span>
            </template>
            <template v-else-if="column.key === 'st'">
              <a-tag v-if="record.isAdopted" color="green">已采纳</a-tag>
              <a-tag v-else>待审查</a-tag>
            </template>
          </template>
        </a-table>
      </div>

      <div class="je-card je-right">
        <div class="je-block-title">
          <DownloadOutlined /> 调阅登记
        </div>
        <a-descriptions v-if="selectedEvidence" :column="1" size="small" style="margin-bottom: 16px">
          <a-descriptions-item label="证据">{{ selectedEvidence.name }}</a-descriptions-item>
          <a-descriptions-item label="SHA-256">
            <span class="je-hash">{{ selectedEvidence.sha256 }}</span>
          </a-descriptions-item>
        </a-descriptions>
        <a-empty v-else description="请先选择证据" style="margin: 24px 0" />
        <a-form layout="vertical">
          <a-form-item label="调阅用途（必填，留痕）" required>
            <a-textarea
              v-model:value="form.purpose"
              :rows="4"
              placeholder="如：2026-06-20 第X次庭审质证展示"
              :disabled="!selectedEvidence"
            />
          </a-form-item>
          <a-button
            type="primary"
            size="large"
            block
            :loading="submitting"
            :disabled="!selectedEvidence"
            @click="onSubmit"
          >
            申请调阅并下载副本
          </a-button>
        </a-form>
        <a-alert
          type="info"
          show-icon
          message="系统将生成庭审展示副本并记录调阅日志"
          description="调阅日志包含案件编号、调阅人、用途与时间，检察官与管理员可查阅。"
          style="margin-top: 16px"
        />
      </div>
    </div>

    <div class="je-card" style="margin-top: 16px">
      <div class="je-block-title"><FileSearchOutlined /> 调阅记录</div>
      <a-table
        :columns="logColumns"
        :data-source="logs"
        row-key="id"
        :pagination="{ pageSize: 8 }"
        size="middle"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'retrievedAt'">{{ fmt(record.retrievedAt) }}</template>
          <template v-else-if="column.key === 'op'">
            <a-button type="link" size="small" :icon="h(DownloadOutlined)" @click="download(record.id, record.evidenceName)">
              下载
            </a-button>
          </template>
        </template>
      </a-table>
    </div>
  </div>
</template>

<style scoped>
.je-grid {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 16px;
  align-items: start;
}
.je-left,
.je-right {
  padding: 20px;
}
.je-block-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--je-primary);
  margin-bottom: 14px;
  display: flex;
  align-items: center;
  gap: 6px;
}
@media (max-width: 960px) {
  .je-grid {
    grid-template-columns: 1fr;
  }
}
</style>
