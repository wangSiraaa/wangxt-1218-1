#!/usr/bin/env bash
set -euo pipefail
BASE=http://localhost:19518
TMP=$(mktemp -d)
EVIDENCE_FILE="$TMP/evidence-001.txt"
echo "司法电子证据原文-2026-AJ-0001-不可篡改" > "$EVIDENCE_FILE"
REAL_HASH=$(shasum -a 256 "$EVIDENCE_FILE" | awk '{print $1}')
echo "[setup] 证据文件: $EVIDENCE_FILE"
echo "[setup] 真实 SHA-256: $REAL_HASH"

login() { curl -s -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d "$1" | python3 -c 'import sys,json;print(json.load(sys.stdin)["token"])'; }

echo "[1] 办案人员登录"
POLICE_TOKEN=$(login '{"username":"police","password":"police123"}')
echo "  token ok"

echo "[2] 创建案件"
CASE=$(curl -s -X POST "$BASE/api/cases" -H "Authorization: Bearer $POLICE_TOKEN" -H "Content-Type: application/json" -d '{"caseNumber":"AJ-2026-0001","title":"王某盗窃案电子证据"}')
CASE_ID=$(echo "$CASE" | python3 -c 'import sys,json;print(json.load(sys.stdin)["id"])')
echo "  案件 ID=$CASE_ID"

echo "[3] 上传证据(正确哈希) -> 期望 201"
UP1=$(curl -s -o /tmp/up1.json -w "%{http_code}" -X POST "$BASE/api/evidence/upload" \
  -H "Authorization: Bearer $POLICE_TOKEN" \
  -F "caseId=$CASE_ID" -F "hash=$REAL_HASH" -F "file=@$EVIDENCE_FILE")
echo "  HTTP $UP1, body: $(cat /tmp/up1.json | head -c 200)"
EVID_ID=$(python3 -c 'import sys,json;print(json.load(open("/tmp/up1.json"))["id"])')

echo "[4] 上传证据(错误哈希) -> 期望 422 拒入库"
UP2=$(curl -s -o /tmp/up2.json -w "%{http_code}" -X POST "$BASE/api/evidence/upload" \
  -H "Authorization: Bearer $POLICE_TOKEN" \
  -F "caseId=$CASE_ID" -F "hash=deadbeefwronghash" -F "file=@$EVIDENCE_FILE")
echo "  HTTP $UP2, body: $(cat /tmp/up2.json | head -c 200)"

echo "[5] 完整性校验(重算哈希比对) -> 期望 true"
VERIFY=$(curl -s "$BASE/api/evidence/$EVID_ID/verify" -H "Authorization: Bearer $POLICE_TOKEN")
echo "  $VERIFY"

echo "[6] 检察官登录并采纳 -> 期望 200"
PROC_TOKEN=$(login '{"username":"prosecutor","password":"pro123"}')
ADOPT=$(curl -s -o /tmp/adopt.json -w "%{http_code}" -X POST "$BASE/api/evidence/$EVID_ID/adopt" \
  -H "Authorization: Bearer $PROC_TOKEN" -H "Content-Type: application/json" \
  -d '{"opinion":"证据来源合法、哈希一致，予以采纳","adopted":true}')
echo "  HTTP $ADOPT, body: $(cat /tmp/adopt.json | head -c 200)"

echo "[7] 再次采纳(已冻结) -> 期望 409"
ADOPT2=$(curl -s -o /tmp/adopt2.json -w "%{http_code}" -X POST "$BASE/api/evidence/$EVID_ID/adopt" \
  -H "Authorization: Bearer $PROC_TOKEN" -H "Content-Type: application/json" \
  -d '{"opinion":"再次意见","adopted":false}')
echo "  HTTP $ADOPT2, body: $(cat /tmp/adopt2.json | head -c 200)"

echo "[8] 书记员登录并调阅(登记用途) -> 期望 201"
CLERK_TOKEN=$(login '{"username":"clerk","password":"clerk123"}')
RETR=$(curl -s -o /tmp/retr.json -w "%{http_code}" -X POST "$BASE/api/retrieval" \
  -H "Authorization: Bearer $CLERK_TOKEN" -H "Content-Type: application/json" \
  -d "{\"evidenceId\":$EVID_ID,\"purpose\":\"2026-06-20 庭审质证展示\"}")
echo "  HTTP $RETR, body: $(cat /tmp/retr.json | head -c 200)"
LOG_ID=$(python3 -c 'import sys,json;print(json.load(open("/tmp/retr.json"))["id"])')

echo "[9] 下载庭审副本 -> 期望 200 并返回内容"
DL=$(curl -s -o "$TMP/copy.txt" -w "%{http_code}" "$BASE/api/retrieval/$LOG_ID/download" -H "Authorization: Bearer $CLERK_TOKEN")
echo "  HTTP $DL, 内容: $(cat "$TMP/copy.txt")"

echo "[10] 调阅日志查询(检察官) -> 期望含案件/人员/用途"
LOGS=$(curl -s "$BASE/api/logs?caseId=$CASE_ID" -H "Authorization: Bearer $PROC_TOKEN")
echo "  $LOGS" | head -c 400

echo ""
echo "[11] 书记员查询调阅日志(权限验证) -> 期望 200 不被 403 拦截"
LOGS_CLERK=$(curl -s -o /tmp/logs_clerk.json -w "%{http_code}" "$BASE/api/logs?caseId=$CASE_ID" -H "Authorization: Bearer $CLERK_TOKEN")
echo "  HTTP $LOGS_CLERK"
CLERK_LOG_COUNT=$(python3 -c 'import sys,json;print(len(json.load(open("/tmp/logs_clerk.json"))))')
echo "  书记员可见调阅日志条数: $CLERK_LOG_COUNT"
CLERK_LOG_CONTENT=$(python3 -c 'import sys,json;logs=json.load(open("/tmp/logs_clerk.json"));print(f"caseNumber={logs[0][\"caseNumber\"]}, userName={logs[0][\"userName\"]}, purpose={logs[0][\"purpose\"]}")' 2>/dev/null)
echo "  日志内容: $CLERK_LOG_CONTENT"

echo ""
echo "[done] 端到端业务流程测试完成"
