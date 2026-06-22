"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import type { SearchResponse } from "@/types/api";

export default function SearchPage() {
  const [query, setQuery] = useState("hybrid search vector database");
  const [mode, setMode] = useState("hybrid");
  const [limit, setLimit] = useState(10);
  const [result, setResult] = useState<SearchResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      setResult(await api<SearchResponse>(`/api/search/${mode}?query=${encodeURIComponent(query)}&limit=${limit}`));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Search failed");
    }
  }

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">Search</h1>
          <p className="page-subtitle">Run keyword, semantic, or hybrid retrieval over local chunks.</p>
        </div>
      </header>
      <form className="form panel" onSubmit={submit}>
        <div className="grid grid-2">
          <div className="field">
            <label htmlFor="query">Query</label>
            <input id="query" value={query} onChange={(event) => setQuery(event.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="mode">Mode</label>
            <select id="mode" value={mode} onChange={(event) => setMode(event.target.value)}>
              <option value="keyword">Keyword</option>
              <option value="semantic">Semantic</option>
              <option value="hybrid">Hybrid</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="limit">Limit</label>
            <input id="limit" type="number" min={1} max={50} value={limit} onChange={(event) => setLimit(Number(event.target.value))} />
          </div>
        </div>
        <button className="btn" type="submit">Search</button>
      </form>
      {error ? <div className="error">{error}</div> : null}
      <section className="grid" style={{ marginTop: 16 }}>
        {(result?.items ?? []).map((item) => (
          <article className="panel" key={item.chunkId}>
            <strong><Link href={`/admin/documents/${item.documentId}`}>{item.title}</Link></strong>
            <p>{item.snippet}</p>
            <div className="score-grid">
              <span className="pill">final {item.finalScore}</span>
              <span className="pill">vector {item.vectorScore}</span>
              <span className="pill">keyword {item.keywordScore}</span>
              <span className="pill">recency {item.recencyScore}</span>
              <span className="pill">distance {item.distance ?? "n/a"}</span>
              <span className="pill">{item.matchedFields.join(", ")}</span>
            </div>
            <div className="button-row">
              <Link className="btn secondary compact" href={`/admin/documents/${item.documentId}`}>Open source</Link>
              <Link className="btn secondary compact" href={`/admin/agent?message=${encodeURIComponent(`Explain this result from ${item.title}: ${item.snippet}`)}`}>Ask agent</Link>
            </div>
          </article>
        ))}
      </section>
    </>
  );
}
