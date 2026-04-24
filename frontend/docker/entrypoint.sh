#!/bin/sh
set -eu

runtime_config_path="/usr/share/nginx/html/runtime-config.js"

api_base_url="${QPHISING_API_BASE_URL:-http://localhost:8080}"
authority="${QPHISING_AUTHORITY:-http://localhost:6060}"
realm="${QPHISING_REALM:-QPhising}"
client_id="${QPHISING_CLIENT_ID:-qphising}"
auth_scope="${QPHISING_AUTH_SCOPE:-openid profile email}"
auth_redirect_uri="${QPHISING_AUTH_REDIRECT_URI:-http://localhost:4200/auth/callback}"
post_logout_redirect_uri="${QPHISING_POST_LOGOUT_REDIRECT_URI:-http://localhost:4200/auth/unauthorized}"

cat > "$runtime_config_path" <<RUNTIME_CONFIG
window.__QPHISING_API_BASE_URL__ = '${api_base_url}';
window.__QPHISING_AUTHORITY__ = '${authority}';
window.__QPHISING_REALM__ = '${realm}';
window.__QPHISING_CLIENT_ID__ = '${client_id}';
window.__QPHISING_AUTH_SCOPE__ = '${auth_scope}';
window.__QPHISING_AUTH_REDIRECT_URI__ = '${auth_redirect_uri}';
window.__QPHISING_POST_LOGOUT_REDIRECT_URI__ = '${post_logout_redirect_uri}';
RUNTIME_CONFIG
