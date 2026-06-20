<script setup lang="ts">
import { onMounted, ref, reactive, h } from 'vue'
import { message } from 'ant-design-vue'
import { caseApi, evidenceApi } from '@/api/modules'
import type { CaseDto, EvidenceDto, EvidenceDetailDto, VerifyResult } from '@/types'
import dayjs from 'dayjs'
import {
  ReloadOutlined,
  AuditOutlined,
  CheckCircleOutlined,
  SafetyCertificateOutlined,
  EyeOutlined
} from '@ant-design/icons-vue'

const loading = ref(false)
const cases = ref<CaseDto[]>([])
const selectedCaseId = ref<number | undefined>(undefined)
const evidences = ref<EvidenceDto[]>([])

const detailVisible = ref(false)
const detailLoading = ref(false)
const detail = ref<EvidenceDetailDto | null>(null)

const adoptVisible = ref(false)
const adoptTarget = ref<EvidenceDto | null>(null)
const adoptForm = reactive({ opinion: '', adopted: true })
const adopting = ref(false)

const verifyMap = ref<Record<number, VerifyResult>>({})

async function loadCases() {
  loading.value = true
  try {
    cases.value = await caseApi.list()
    if (cases.value.length && !selectedCaseId.value) {
      selectedCaseId.value = cases.value[0].id
      await loadEvidence()
    }
  } finally {
    loading.value = false
  }
}

async function loadEvidence() {
  if (!selectedCaseId.value) return
  loading.value = true
  try {
    evidences.value = await evidenceApi.list(selectedCaseId.value)
  } finally {
    loading.value = false
  }
}

async function openDetail(e: EvidenceDto) {
  detail.value = null
  detailVisible.value = true
  detailLoading.value = true
  try {
    detail.value = await evidenceApi.getById(e.id)
  } finally {
    detailLoading.value = false
  }
}

function openAdopt(e: EvidenceDto) {
  adoptTarget.value = e
  adoptForm.opinion = ''
  adoptForm.adopted = true
  adoptVisible.value = true
}

async function onAdopt() {
  if (!adoptTarget.value) return
  if (!adoptForm.opinion.trim()) {
    message.warning('请填写审查意见')
    return
  }
  adopting.value = true
  try {
    await evidenceApi.adopt(adoptTarget.value.id, { ...adoptForm })
    message.success(adoptForm.adopted ? '已采纳，原文件已冻结' : '已记录不予采纳意见')
    adoptVisible.value = false
    await loadEvidence()
  } catch {
    /* 含 409 已采纳冻结 */
  } finally {
    adopting.value = false
  }
}

async function doVerify(e: EvidenceDto) {
  try {
    const r = await evidenceApi.verify(e.id)
    verifyMap.value = { ...verifyMap.value, [e.id]: r }
    message[r.integrityOk ? 'success' : 'error'](
      r.integrityOk ? '完整性校验通过，文件未被篡改' : '完整性校验失败，文件可能被篡改'
    )
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

const columns = [
  { title: '证据名称', dataIndex: 'name' },
  { title: 'SHA-256', key: 'sha', width: 200 },
  { title: '状态', key: 'status', width: 110 },
  { title: '哈希', key: 'hash', width: 110 },
  { title: '上传人', dataIndex: 'uploaderName', width: 110 },
  { title: '上传时间', dataIndex: 'uploadedAt', width: 150 },
  { title: '操作', key: 'op', width: 220, fixed: 'right' as const }
]

onMounted(loadCases)
</script>

<template>
  <div class="je-page">
    <div class="je-page-header">
      <div>
        <h2 class="je-page-title">证据审查</h2>
        <div class="je-subtitle">检察官审查证据完整性并标记采用意见，采纳后原文件冻结不可覆盖</div>
      </div>
      <a-space>
        <a-select
          v-model:value="selectedCaseId"
          placeholder="选择案件"
          style="width: 280px"
          show-search
          option-filter-prop="label"
          @change="loadEvidence"
        >
          <a-select-option v-for="c in cases" :key="c.id" :value="c.id" :label="c.caseNumber">
            {{ c.caseNumber }} - {{ c.title }}
          </a-select-option>
        </a-select>
        <a-button :icon="h(ReloadOutlined)" @click="loadEvidence" :loading="loading">刷新</a-button>
      </a-space>
    </div>

    <div class="je-card">
      <a-table
        :columns="columns"
        :data-source="evidences"
        row-key="id"
        :loading="loading"
        :pagination="{ pageSize: 10 }"
        size="middle"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'sha'">
            <span class="je-hash">{{ shortHash(record.sha256) }}</span>
          </template>
          <template v-else-if="column.key === 'status'">
            <a-tag v-if="record.isAdopted" color="green"><CheckCircleOutlined /> 已采纳</a-tag>
            <a-tag v-else color="default">待审查</a-tag>
          </template>
          <template v-else-if="column.key === 'hash'">
            <a-tag :color="record.hashVerified ? 'success' : 'error'">
              {{ record.hashVerified ? '一致' : '异常' }}
            </a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'uploadedAt'">{{ fmt(record.uploadedAt) }}</template>
          <template v-else-if="column.key === 'op'">
            <a-button type="link" size="small" :icon="h(EyeOutlined)" @click="openDetail(record)">详情</a-button>
            <a-button
              type="link"
              size="small"
              :icon="h(AuditOutlined)"
              :disabled="record.isAdopted"
              @click="openAdopt(record)"
            >
              {{ record.isAdopted ? '已采纳' : '采纳' }}
            </a-button>
            <a-button type="link" size="small" :icon="h(SafetyCertificateOutlined)" @click="doVerify(record)">
              校验
            </a-button>
          </template>
        </template>
      </a-table>
    </div>

    <a-drawer v-model:open="detailVisible" title="证据详情与链路" width="720">
      <a-spin :spinning="detailLoading">
        <a-descriptions v-if="detail" :column="1" bordered size="small">
          <a-descriptions-item label="证据名称">{{ detail.name }}</a-descriptions-item>
          <a-descriptions-item label="SHA-256">
            <span class="je-hash">{{ detail.sha256 }}</span>
          </a-descriptions-item>
          <a-descriptions-item label="提交哈希">
            <span class="je-hash">{{ detail.uploadedHash }}</span>
          </a-descriptions-item>
          <a-descriptions-item label="哈希校验">
            <a-tag :color="detail.hashVerified ? 'success' : 'error'">
              {{ detail.hashVerified ? '一致' : '异常' }}
            </a-tag>
          </a-descriptions-item>
          <a-descriptions-item label="采纳状态">
            <a-tag v-if="detail.isAdopted" color="green">已采纳（冻结）</a-tag>
            <a-tag v-else>待审查</a-tag>
          </a-descriptions-item>
          <a-descriptions-item label="上传人">{{ detail.uploaderName }}</a-descriptions-item>
          <a-descriptions-item label="上传时间">{{ fmt(detail.uploadedAt) }}</a-descriptions-item>
        </a-descriptions>

        <h3 class="je-section" v-if="detail">采用意见</h3>
        <a-empty v-if="detail && !detail.adoptions.length" description="暂无审查意见" />
        <a-timeline v-if="detail && detail.adoptions.length">
          <a-timeline-item v-for="a in detail.adoptions" :key="a.id">
            <p class="je-tl-title">
              <a-tag :color="a.adopted ? 'green' : 'red'">{{ a.adopted ? '采纳' : '不予采纳' }}</a-tag>
              {{ a.reviewerName }} · {{ fmt(a.createdAt) }}
            </p>
            <p class="je-tl-opinion">{{ a.opinion }}</p>
          </a-timeline-item>
        </a-timeline>

        <h3 class="je-section" v-if="detail">调阅记录</h3>
        <a-empty v-if="detail && !detail.retrievals.length" description="暂无调阅记录" />
        <a-list v-if="detail && detail.retrievals.length" :data-source="detail.retrievals" size="small">
          <template #renderItem="{ item }">
            <a-list-item>
              <a-list-item-meta>
                <template #title>{{ item.userName }} · {{ item.caseNumber }}</template>
                <template #description>用途：{{ item.purpose }} · {{ fmt(item.retrievedAt) }}</template>
              </a-list-item-meta>
            </a-list-item>
          </template>
        </a-list>
      </a-spin>
    </a-drawer>

    <a-modal
      v-model:open="adoptVisible"
      :title="adoptTarget ? `标记采用意见 - ${adoptTarget.name}` : '标记采用意见'"
      :confirm-loading="adopting"
      ok-text="提交意见"
      cancel-text="取消"
      @ok="onAdopt"
    >
      <a-form layout="vertical" style="margin-top: 12px">
        <a-form-item label="采用意见" required>
          <a-radio-group v-model:value="adoptForm.adopted">
            <a-radio :value="true">采纳</a-radio>
            <a-radio :value="false">不予采纳</a-radio>
          </a-radio-group>
        </a-form-item>
        <a-form-item label="审查意见说明" required>
          <a-textarea
            v-model:value="adoptForm.opinion"
            :rows="4"
            placeholder="说明证据来源合法性、哈希一致性、关联性等审查结论"
          />
        </a-form-item>
        <a-alert
          v-if="adoptForm.adopted"
          type="warning"
          show-icon
          message="采纳后原文件将被冻结，不可覆盖或更改意见。"
        />
      </a-form>
    </a-modal>
  </div>
</template>

<style scoped>
.je-section {
  font-size: 14px;
  color: var(--je-primary);
  margin: 18px 0 10px;
  border-left: 3px solid var(--je-gold);
  padding-left: 8px;
}
.je-tl-title {
  margin: 0;
  font-weight: 600;
}
.je-tl-opinion {
  margin: 4px 0 0;
  color: #52617a;
}
</style>
