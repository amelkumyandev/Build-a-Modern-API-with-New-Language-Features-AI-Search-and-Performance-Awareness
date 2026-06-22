"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { DashboardResponse } from "@/types/api";

export default function DashboardPage() {
  const [data, setData] = useState<DashboardResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [busy, setBusy] = useState<string | null>(null);

  async function load() {
    try {
      setData(await api<DashboardResponse>("/api/admin/dashboard"));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Dashboard failed to load");
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function seed() {
    setBusy("seed");
    setMessage("Seeding...");
    try {
      const result = await api<{ documentsCreated: number; evaluationQuestionsCreated: number }>("/api/admin/seed", { method: "POST" });
      setMessage(`Seed complete. Created ${result.documentsCreated} new documents and ${result.evaluationQuestionsCreated} new questions; existing generated seed documents were refreshed when needed.`);
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Seed failed");
    } finally {
      setBusy(null);
    }
  }

  async function reindexAll() {
    setBusy("reindex");
    setMessage("Re-indexing all active documents...");
    try {
      const result = await api<Array<{ chunksCreated: number; embeddingsGenerated: number }>>("/api/admin/reindex", { method: "POST" });
      const chunks = result.reduce((sum, item) => sum + item.chunksCreated, 0);
      const embeddings = result.reduce((sum, item) => sum + item.embeddingsGenerated, 0);
      setMessage(`Re-indexed ${result.length} documents, created ${chunks} chunks, and generated ${embeddings} embeddings.`);
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Re-index failed");
    } finally {
      setBusy(null);
    }
  }

  async function runEvaluation() {
    setBusy("evaluation");
    setMessage("Running hybrid retrieval evaluation...");
    try {
      const result = await api<{ questionCount: number; score: number }>("/api/evaluation/run", {
        method: "POST",
        body: JSON.stringify({ searchMode: "Hybrid", topK: 8 })
      });
      setMessage(`Evaluation completed across ${result.questionCount} questions with score ${result.score}.`);
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Evaluation failed");
    } finally {
      setBusy(null);
    }
  }

  const pendingChunks = data ? Math.max(0, data.totalChunks - data.embeddedChunks) : 0;

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <p className="page-subtitle">Runtime status for the local RAG platform.</p>
        </div>
        <div className="button-row">
          <button className="btn secondary" type="button" disabled={busy !== null} onClick={seed}>
            {busy === "seed" ? "Seeding..." : "Generate seed data"}
          </button>
          <button className="btn secondary" type="button" disabled={busy !== null} onClick={reindexAll}>
            {busy === "reindex" ? "Re-indexing..." : "Re-index all"}
          </button>
          <button className="btn" type="button" disabled={busy !== null} onClick={runEvaluation}>
            {busy === "evaluation" ? "Running..." : "Run evaluation"}
          </button>
        </div>
      </header>
      {error ? <div className="error">{error}</div> : null}
      {message ? <div className="message">{message}</div> : null}
      <section className="grid grid-4">
        <Metric label="Database" value={data?.databaseReady ? "Ready" : "Offline"} tone={data?.databaseReady ? "ok" : "warning"} />
        <Metric label="OpenAI key" value={data?.openAiKeyConfigured ? "Configured" : "Missing"} tone={data?.openAiKeyConfigured ? "ok" : "warning"} />
        <Metric label="Documents" value={data?.totalDocuments ?? "..."} />
        <Metric label="Embedded chunks" value={`${data?.embeddedChunks ?? 0}/${data?.totalChunks ?? 0}`} />
        <Metric label="Waiting embeddings" value={pendingChunks} tone={pendingChunks > 0 ? "warning" : "ok"} />
        <Metric label="Failed embeddings" value={data?.failedEmbeddings ?? 0} tone={(data?.failedEmbeddings ?? 0) > 0 ? "warning" : "ok"} />
        <Metric label="Latest evaluation" value={data?.latestEvaluationScore ?? "Not run"} />
        <Metric label="Agent runs" value="Inspect in Agent" />
      </section>
      <section className="panel" style={{ marginTop: 16 }}>
        <h2>Quick actions</h2>
        <div className="button-row">
          <Link className="btn secondary" href="/admin/search">Open search</Link>
          <Link className="btn secondary" href="/admin/agent">Open agent chat</Link>
          <Link className="btn secondary" href="/admin/documents/new">Create document</Link>
          <Link className="btn secondary" href="/admin/evaluation">Open evaluation</Link>
        </div>
      </section>
      {data ? <section className="panel warning-panel" style={{ marginTop: 16 }}>{data.developmentWarning}</section> : null}
    </>
  );
}

function Metric({ label, value, tone }: { label: string; value: string | number; tone?: "ok" | "warning" }) {
  return (
    <div className="metric">
      <div className="metric-label">{label}</div>
      <div className={`metric-value ${tone ?? ""}`}>{value}</div>
    </div>
  );
}
