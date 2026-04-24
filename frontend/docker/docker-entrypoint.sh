#!/bin/sh
set -eu

envsubst '
${QPHISING_API_BASE_URL}
${QPHISING_AUTHORITY}
${QPHISING_REALM}
${QPHISING_CLIENT_ID}
${QPHISING_AUTH_SCOPE}
${QPHISING_AUTH_REDIRECT_URI}
${QPHISING_POST_LOGOUT_REDIRECT_URI}
' < /usr/share/nginx/html/runtime-config.template.js > /usr/share/nginx/html/runtime-config.js
