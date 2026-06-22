"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { api } from "@/lib/api";
import type { ChunkResponse, DocumentDetail, SearchResult } from "@/types/api";

export default function DocumentDetailPage() {
  const params = useParams<{ id: string }>();
  const documentId = Array.isArray(params.id) ? params.id[0] : params.id;
  const [document, setDocument] = useState<DocumentDetail | null>(null);
  const [chunks, setChunks] = useState<ChunkResponse[]>([]);
  const [similar, setSimilar] = useState<SearchResult[]>([]);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    if (!documentId) {
      return;
    }

    setError(null);
    try {
      const detail = await api<DocumentDetail>(`/api/documents/${documentId}`);
      const chunkRows = await api<ChunkResponse[]>(`/api/documents/${documentId}/chunks`);
      setDocument(detail);
      setChunks(chunkRows);
      const ownChunkIds = new Set(chunkRows.map((chunk) => chunk.id));
      const result = await api<{ items: SearchResult[] }>(`/api/search/hybrid?query=${encodeURIComponent(detail.title)}&limit=5`);
      setSimilar(result.items.filter((item) => !ownChunkIds.has(item.chunkId)));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Document failed to load");
      setSimilar([]);
    }
  }

  useEffect(() => {
    void load();
  }, [documentId]);

  async function action(path: string) {
    if (!documentId) {
      return;
    }

    setMessage("Working...");
    try {
      await api(`/api/documents/${documentId}/${path}`, { method: "POST" });
      setMessage("Done.");
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Action failed");
    }
  }

  if (!document) {
    return error ? <div className="error">{error}</div> : <div>Loading...</div>;
  }

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">{document.title}</h1>
          <p className="page-subtitle">{document.status} / {document.chunkingStatus} / {document.embeddingStatus}</p>
        </div>
        <div className="button-row">
          <Link className="btn secondary" href={`/admin/agent?message=${encodeURIComponent(`Summarize ${document.title} and cite the strongest chunks.`)}`}>Ask agent</Link>
          <button className="btn secondary" type="button" onClick={() => action("chunk")}>Chunk</button>
          <button className="btn secondary" type="button" onClick={() => action("embed")}>Embed</button>
          <button className="btn" type="button" onClick={() => action("reindex")}>Re-index</button>
        </div>
      </header>
      {message ? <div className="message">{message}</div> : null}
      {document.summary ? <section className="panel lead-panel"><strong>Summary</strong><p>{document.summary}</p></section> : null}
      <section className="grid grid-2">
        <div className="panel">
          <h2>Content</h2>
          <p>{document.content}</p>
        </div>
        <div className="panel">
          <h2>Metadata</h2>
          <pre>{JSON.stringify(document.metadata, null, 2)}</pre>
          <div>{document.tags.map((tag) => <span className="pill" key={tag}>{tag}</span>)}</div>
        </div>
      </section>
      <section className="panel" style={{ marginTop: 16 }}>
        <h2>Chunks</h2>
        <div className="grid">
          {chunks.map((chunk) => (
            <div className="message" key={chunk.id}>
              <strong>#{chunk.chunkIndex}</strong> {chunk.tokenEstimate} tokens / {chunk.embeddingModel ?? "not embedded"}
              {chunk.embeddingGeneratedAt ? <span className="pill">embedded {new Date(chunk.embeddingGeneratedAt).toLocaleString()}</span> : <span className="pill warning">pending embedding</span>}
              <p>{chunk.content}</p>
            </div>
          ))}
        </div>
      </section>
      <section className="panel" style={{ marginTop: 16 }}>
        <h2>Similar chunks</h2>
        {similar.length === 0 ? <p className="muted">Run re-indexing or search to populate related chunk results.</p> : null}
        <div className="grid">
          {similar.map((item) => (
            <article className="message" key={item.chunkId}>
              <strong><Link href={`/admin/documents/${item.documentId}`}>{item.title}</Link></strong>
              <p>{item.snippet}</p>
              <span className="pill">final {item.finalScore}</span>
              <span className="pill">vector {item.vectorScore}</span>
              <span className="pill">keyword {item.keywordScore}</span>
            </article>
          ))}
        </div>
      </section>
    </>
  );
}
