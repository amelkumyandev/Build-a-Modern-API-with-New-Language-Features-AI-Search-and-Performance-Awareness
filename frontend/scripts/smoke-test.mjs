const frontendBaseUrl = process.env.FRONTEND_BASE_URL ?? "http://localhost:3000";
const apiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5000";

const routes = [
  "/login",
  "/admin/dashboard",
  "/admin/documents",
  "/admin/documents/new",
  "/admin/search",
  "/admin/agent",
  "/admin/evaluation",
  "/admin/settings"
];

async function assertOk(url, label) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`${label} returned ${response.status}`);
  }

  return response;
}

for (const route of routes) {
  await assertOk(`${frontendBaseUrl}${route}`, `Frontend route ${route}`);
}

async function postJson(url, body, token) {
  const response = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify(body)
  });

  if (!response.ok) {
    throw new Error(`${url} returned ${response.status}: ${await response.text()}`);
  }

  return response.json();
}

const auth = await postJson(`${apiBaseUrl}/api/auth/login`, { username: "admin", password: "admin" });
const token = auth.accessToken;
const dashboard = await fetch(`${apiBaseUrl}/api/admin/dashboard`, {
  headers: { Authorization: `Bearer ${token}` }
});

if (!dashboard.ok) {
  throw new Error(`Dashboard API returned ${dashboard.status}: ${await dashboard.text()}`);
}

const dashboardBody = await dashboard.json();
if (typeof dashboardBody.totalDocuments !== "number" || typeof dashboardBody.totalChunks !== "number") {
  throw new Error("Dashboard API did not return document and chunk counters.");
}

const unique = Date.now();
const created = await postJson(`${apiBaseUrl}/api/documents`, {
  title: `Smoke created document ${unique}`,
  content: "Smoke test content verifies authenticated document creation and get-by-id behavior for the admin UI detail route. ".repeat(3),
  tags: ["smoke", "manual"],
  metadata: { source: "smoke" }
}, token);

const detail = await fetch(`${apiBaseUrl}/api/documents/${created.id}`, {
  headers: { Authorization: `Bearer ${token}` }
});

if (!detail.ok) {
  throw new Error(`Document detail API returned ${detail.status}: ${await detail.text()}`);
}

await assertOk(`${frontendBaseUrl}/admin/documents/${created.id}`, `Frontend route /admin/documents/${created.id}`);

const uploadForm = new FormData();
uploadForm.set(
  "file",
  new Blob([
    "Smoke upload content verifies multipart document upload and conversion into searchable local knowledge text. ".repeat(3)
  ], { type: "text/plain" }),
  `smoke-upload-${unique}.md`);
uploadForm.set("tags", "smoke,upload");
uploadForm.set("metadata", "{\"source\":\"smoke\"}");

const uploaded = await fetch(`${apiBaseUrl}/api/documents/upload`, {
  method: "POST",
  headers: { Authorization: `Bearer ${token}` },
  body: uploadForm
});

if (!uploaded.ok) {
  throw new Error(`Document upload API returned ${uploaded.status}: ${await uploaded.text()}`);
}

console.log(`Smoke tests passed for ${routes.length + 1} frontend routes, authenticated document detail, and upload API.`);
