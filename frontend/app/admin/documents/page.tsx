"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { api } from "@/lib/api";
import type { DocumentSummary, PagedResponse } from "@/types/api";

export default function DocumentsPage() {
  const [data, setData] = useState<PagedResponse<DocumentSummary> | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState("all");
  const [tag, setTag] = useState("");

  async function load() {
    api<PagedResponse<DocumentSummary>>("/api/documents?pageNumber=1&pageSize=50")
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err.message : "Documents failed to load"));
  }

  useEffect(() => {
    void load();
  }, []);

  async function reindex(id: string) {
    setMessage("Re-indexing document...");
    try {
      const result = await api<{ chunksCreated: number; embeddingsGenerated: number }>(`/api/documents/${id}/reindex`, { method: "POST" });
      setMessage(`Re-indexed document with ${result.chunksCreated} chunks and ${result.embeddingsGenerated} embeddings.`);
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Re-index failed");
    }
  }

  async function remove(id: string) {
    setMessage("Deleting document...");
    try {
      await api(`/api/documents/${id}`, { method: "DELETE" });
      setMessage("Document deleted.");
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Delete failed");
    }
  }

  const tags = useMemo(() => {
    return Array.from(new Set((data?.items ?? []).flatMap((document) => document.tags))).sort();
  }, [data]);

  const documents = useMemo(() => {
    return (data?.items ?? []).filter((document) => {
      const matchesText = query.trim().length === 0
        || document.title.toLowerCase().includes(query.toLowerCase())
        || document.summary?.toLowerCase().includes(query.toLowerCase())
        || document.tags.some((item) => item.toLowerCase().includes(query.toLowerCase()));
      const matchesStatus = status === "all" || document.status === status;
      const matchesTag = tag === "" || document.tags.includes(tag);
      return matchesText && matchesStatus && matchesTag;
    });
  }, [data, query, status, tag]);

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">Documents</h1>
          <p className="page-subtitle">Manage local technical knowledge and indexing state.</p>
        </div>
        <Link className="btn" href="/admin/documents/new">
          New document
        </Link>
      </header>
      {error ? <div className="error">{error}</div> : null}
      {message ? <div className="message">{message}</div> : null}
      <section className="panel filter-bar">
        <div className="field">
          <label htmlFor="query">Search documents</label>
          <input id="query" value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Title, summary, or tag" />
        </div>
        <div className="field">
          <label htmlFor="status">Status</label>
          <select id="status" value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="all">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Created">Created</option>
            <option value="Indexed">Indexed</option>
            <option value="Archived">Archived</option>
          </select>
        </div>
        <div className="field">
          <label htmlFor="tag">Tag</label>
          <select id="tag" value={tag} onChange={(event) => setTag(event.target.value)}>
            <option value="">All tags</option>
            {tags.map((item) => <option key={item} value={item}>{item}</option>)}
          </select>
        </div>
      </section>
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Tags</th>
              <th>Status</th>
              <th>Indexing</th>
              <th>Updated</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {documents.map((document) => (
              <tr key={document.id}>
                <td>
                  <Link href={`/admin/documents/${document.id}`}>{document.title}</Link>
                  {document.summary ? <p className="muted">{document.summary}</p> : null}
                </td>
                <td>{document.tags.map((tag) => <span className="pill" key={tag}>{tag}</span>)}</td>
                <td>{document.status}</td>
                <td>{document.chunkingStatus} / {document.embeddingStatus}</td>
                <td>{new Date(document.updatedAt).toLocaleString()}</td>
                <td>
                  <div className="button-row">
                    <button className="btn secondary compact" type="button" onClick={() => reindex(document.id)}>Re-index</button>
                    <button className="btn danger compact" type="button" onClick={() => remove(document.id)}>Delete</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {documents.length === 0 ? <div className="empty-state">No documents match the current filters.</div> : null}
      </div>
    </>
  );
}
