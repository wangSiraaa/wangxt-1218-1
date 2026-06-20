<script setup lang="ts">
import { theme } from 'ant-design-vue'
import zhCN from 'ant-design-vue/es/locale/zh_CN'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'

dayjs.locale('zh-cn')

const config = {
  algorithm: theme.defaultAlgorithm,
  token: {
    colorPrimary: '#1B2A4A',
    colorInfo: '#1B2A4A',
    colorSuccess: '#3E7D5B',
    colorWarning: '#C8A24B',
    colorError: '#B23A3A',
    borderRadius: 4,
    fontSize: 14
  }
}
</script>

<template>
  <a-config-provider :theme="config" :locale="zhCN">
    <a-app style="height: 100%">
      <router-view />
    </a-app>
  </a-config-provider>
</template>
