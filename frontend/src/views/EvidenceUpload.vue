<script setup lang="ts">
import { onMounted, ref, computed, h, watch } from 'vue'
import { message } from 'ant-design-vue'
import { caseApi, evidenceApi } from '@/api/modules'
import type { CaseDto, EvidenceDto } from '@/types'
import dayjs from 'dayjs'
import {
  InboxOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SafetyCertificateOutlined,
  ReloadOutlined
} from '@ant-design/icons-vue'

const loading = ref(false)
const cases = ref<CaseDto[]>([])
const selectedCaseId = ref<number | undefined>()

const file = ref<File | null>(null)
const computedHash = ref('')
const hashing = ref(false)
const uploadedHash = ref('')

const resultVisible = ref(false)
const result = ref<EvidenceDto | null>(null)
const uploading = ref(false)

async function loadCases() {
  loading.value = true
  try {
    cases.value = await caseApi.list()
    if (cases.value.length && !selectedCaseId.value) selectedCaseId.value = cases.value[0].id
  } finally {
    loading.value = false
  }
}

async function computeSha256(f: File) {
  hashing.value = true
  computedHash.value = ''
  try {
    const buf = await f.arrayBuffer()
    const digest = await crypto.subtle.digest('SHA-256', buf)
    const bytes = Array.from(new Uint8Array(digest))
    computedHash.value = bytes.map((b) => b.toString(16).padStart(2, '0')).join('')
    if (!uploadedHash.value) uploadedHash.value = computedHash.value
  } catch {
    message.error('文件哈希计算失败')
  } finally {
    hashing.value = false
  }
}

function beforeUpload(f: File) {
  file.value = f
  uploadedHash.value = ''
  computeSha256(f)
  return false
}

watch(uploadedHash, (v) => {
  if (v) uploadedHash.value = v.toLowerCase().trim()
})

const hashMatch = computed(() => {
  if (!computedHash.value || !uploadedHash.value) return null
  return computedHash.value === uploadedHash.value.toLowerCase().trim()
})

const canUpload = computed(
  () => !!selectedCaseId.value && !!file.value && !!uploadedHash.value && hashMatch.value === true
)

async function onUpload() {
  if (!selectedCaseId.value || !file.value || !uploadedHash.value) {
    message.warning('请选择案件、文件并填写哈希值')
    return
  }
  if (hashMatch.value === false) {
    message.error('哈希值不一致，上传后将被系统拒入库')
  }
  uploading.value = true
  try {
    result.value = await evidenceApi.upload(selectedCaseId.value, uploadedHash.value, file.value)
    resultVisible.value = true
    message.success('证据已入库，哈希校验通过')
    file.value = null
    computedHash.value = ''
    uploadedHash.value = ''
  } catch {
    /* 拦截器已提示，含 422 哈希校验失败 */
  } finally {
    uploading.value = false
  }
}

function fmt(t: string) {
  return dayjs(t).format('YYYY-MM-DD HH:mm:ss')
}

onMounted(loadCases)
</script>

<template>
  <div class="je-page">
    <div class="je-page-header">
      <div>
        <h2 class="je-page-title">证据上传</h2>
        <div class="je-subtitle">办案人员上传电子证据包与哈希值，哈希校验失败将被拒入库</div>
      </div>
      <a-button :icon="h(ReloadOutlined)" @click="loadCases" :loading="loading">刷新案件</a-button>
    </div>

    <div class="je-grid">
      <div class="je-card je-form-card">
        <a-form layout="vertical">
          <a-form-item label="归属案件" required>
            <a-select
              v-model:value="selectedCaseId"
              placeholder="请选择案件"
              :loading="loading"
              show-search
              option-filter-prop="label"
            >
              <a-select-option
                v-for="c in cases"
                :key="c.id"
                :value="c.id"
                :label="c.caseNumber"
              >
                {{ c.caseNumber }} - {{ c.title }}
              </a-select-option>
            </a-select>
          </a-form-item>

          <a-form-item label="证据文件" required>
            <a-upload-dragger
              :before-upload="beforeUpload"
              :show-upload-list="true"
              :max-count="1"
              accept="*"
            >
              <p class="ant-upload-drag-icon">
                <InboxOutlined />
              </p>
              <p class="ant-upload-text">点击或拖拽文件到此区域上传</p>
              <p class="ant-upload-hint">支持任意类型电子证据包，单个文件最大 512MB</p>
            </a-upload-dragger>
          </a-form-item>

          <a-form-item label="文件实际 SHA-256（系统计算）">
            <a-input
              :value="computedHash"
              placeholder="选择文件后自动计算"
              readonly
              :loading="hashing"
            >
              <template #prefix><SafetyCertificateOutlined /></template>
            </a-input>
          </a-form-item>

          <a-form-item label="证据哈希值（需与实际一致方可入库）" required>
            <a-input
              v-model:value="uploadedHash"
              placeholder="粘贴办案机关提供的哈希值"
              allow-clear
            />
            <div v-if="hashMatch === true" class="je-hash-tip je-stat-ok">
              <CheckCircleOutlined /> 哈希一致，可入库
            </div>
            <div v-else-if="hashMatch === false" class="je-hash-tip" style="color: #b23a3a">
              <CloseCircleOutlined /> 哈希不一致，系统将拒入库
            </div>
          </a-form-item>

          <a-button
            type="primary"
            size="large"
            block
            :loading="uploading"
            :disabled="!canUpload"
            @click="onUpload"
          >
            提交入库
          </a-button>
        </a-form>
      </div>

      <div class="je-card je-rule-card">
        <h3 class="je-rule-title"><SafetyCertificateOutlined /> 司法证据入库规则</h3>
        <ul class="je-rule-list">
          <li>上传时系统计算文件实际 SHA-256，并与提交哈希比对。</li>
          <li><b>哈希校验失败 → 证据不入库</b>，文件不落盘。</li>
          <li>已采纳的证据原文件被冻结，不可覆盖或替换。</li>
          <li>调阅下载将自动记录案件、人员与用途，留痕可查。</li>
        </ul>
        <a-alert
          v-if="hashMatch === false"
          type="error"
          show-icon
          message="哈希校验未通过"
          description="当前哈希与文件实际值不一致，提交后服务端将返回 422 并拒绝入库。"
          style="margin-top: 12px"
        />
      </div>
    </div>

    <a-modal v-model:open="resultVisible" title="入库结果" :footer="null" width="560">
      <a-result
        v-if="result"
        status="success"
        title="证据已成功入库"
        :sub-title="`文件 ${result.name} 已存入本地对象目录并记录 SHA-256`"
      >
        <template #extra>
          <a-descriptions :column="1" bordered size="small" style="text-align: left">
            <a-descriptions-item label="证据 ID">{{ result.id }}</a-descriptions-item>
            <a-descriptions-item label="SHA-256">
              <span class="je-hash">{{ result.sha256 }}</span>
            </a-descriptions-item>
            <a-descriptions-item label="哈希校验">
              <a-tag color="success"><CheckCircleOutlined /> 一致</a-tag>
            </a-descriptions-item>
            <a-descriptions-item label="上传时间">{{ fmt(result.uploadedAt) }}</a-descriptions-item>
          </a-descriptions>
          <a-button type="primary" @click="resultVisible = false" style="margin-top: 16px">
            完成
          </a-button>
        </template>
      </a-result>
    </a-modal>
  </div>
</template>

<style scoped>
.je-grid {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: 16px;
  align-items: start;
}

.je-form-card {
  padding: 24px;
}

.je-rule-card {
  padding: 20px;
}

.je-rule-title {
  font-size: 15px;
  color: var(--je-primary);
  margin: 0 0 12px;
}

.je-rule-list {
  padding-left: 18px;
  margin: 0;
  color: #52617a;
  font-size: 13px;
  line-height: 2;
}

.je-hash-tip {
  margin-top: 6px;
  font-size: 12px;
}

@media (max-width: 960px) {
  .je-grid {
    grid-template-columns: 1fr;
  }
}
</style>
