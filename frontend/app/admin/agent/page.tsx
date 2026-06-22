"use client";

import { FormEvent, useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { AgentChatResponse, AgentRun, ChatMessage, ChatSession } from "@/types/api";

type ConversationItem = {
  id: string;
  role: string;
  content: string;
  citations: AgentChatResponse["citations"];
};

export default function AgentPage() {
  const [message, setMessage] = useState("Explain the local vector database pipeline.");
  const [mode, setMode] = useState("Hybrid");
  const [topK, setTopK] = useState(8);
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [sessions, setSessions] = useState<ChatSession[]>([]);
  const [conversation, setConversation] = useState<ConversationItem[]>([]);
  const [latestRun, setLatestRun] = useState<AgentRun | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function loadSessions() {
    try {
      setSessions(await api<ChatSession[]>("/api/agent/sessions"));
    } catch {
      setSessions([]);
    }
  }

  useEffect(() => {
    void loadSessions();
    const initialMessage = new URLSearchParams(window.location.search).get("message");
    if (initialMessage) {
      setMessage(initialMessage);
    }
  }, []);

  async function openSession(id: string) {
    setError(null);
    const detail = await api<{ messages: ChatMessage[] }>(`/api/agent/sessions/${id}`);
    setSessionId(id);
    setConversation(detail.messages.map((item) => ({
      id: item.id,
      role: item.role,
      content: item.content,
      citations: item.citations
    })));
    setLatestRun(null);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setBusy(true);
    const userMessage: ConversationItem = { id: crypto.randomUUID(), role: "User", content: message, citations: [] };
    setConversation((items) => [...items, userMessage]);
    try {
      const response = await api<AgentChatResponse>("/api/agent/chat", {
        method: "POST",
        body: JSON.stringify({ sessionId, message, searchMode: mode, topK })
      });
      setSessionId(response.sessionId);
      setConversation((items) => [...items, { id: response.messageId, role: "Assistant", content: response.answer, citations: response.citations }]);
      setLatestRun(await api<AgentRun>(`/api/agent/runs/${response.agentRunId}`));
      await loadSessions();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Agent failed");
    } finally {
      setBusy(false);
    }
  }

  function newSession() {
    setSessionId(null);
    setConversation([]);
    setLatestRun(null);
  }

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">Agent</h1>
          <p className="page-subtitle">Ask grounded questions over local indexed chunks.</p>
        </div>
        <button className="btn secondary" type="button" onClick={newSession}>New session</button>
      </header>
      <section className="grid grid-2">
        <form className="form panel" onSubmit={submit}>
          <div className="field">
            <label htmlFor="message">Message</label>
            <textarea id="message" value={message} onChange={(event) => setMessage(event.target.value)} />
          </div>
          <div className="grid grid-2">
            <div className="field">
              <label htmlFor="mode">Search mode</label>
              <select id="mode" value={mode} onChange={(event) => setMode(event.target.value)}>
                <option>Keyword</option>
                <option>Semantic</option>
                <option>Hybrid</option>
              </select>
            </div>
            <div className="field">
              <label htmlFor="topK">Top-K</label>
              <input id="topK" type="number" min={1} max={20} value={topK} onChange={(event) => setTopK(Number(event.target.value))} />
            </div>
          </div>
          <button className="btn" type="submit" disabled={busy}>{busy ? "Thinking..." : "Ask agent"}</button>
        </form>
        <aside className="panel">
          <h2>Session history</h2>
          <div className="stack">
            {sessions.length === 0 ? <p className="muted">No saved sessions yet.</p> : null}
            {sessions.map((session) => (
              <button className="list-button" type="button" key={session.id} onClick={() => openSession(session.id)}>
                <strong>{session.title}</strong>
                <span>{new Date(session.updatedAt).toLocaleString()}</span>
              </button>
            ))}
          </div>
        </aside>
      </section>
      {error ? <div className="error">{error}</div> : null}
      <section className="chat-log" style={{ marginTop: 16 }}>
        {conversation.map((item) => (
          <article className={`chat-message ${item.role.toLowerCase() === "user" ? "user" : "assistant"}`} key={item.id}>
            <strong>{item.role}</strong>
            <p>{item.content}</p>
            {item.citations.length > 0 ? <strong>Citations</strong> : null}
            {item.citations.map((citation) => (
              <div className="message" key={citation.chunkId}>
                {citation.title} / score {citation.score}
                <p>{citation.snippet}</p>
              </div>
            ))}
          </article>
        ))}
      </section>
      {latestRun ? (
        <section className="panel" style={{ marginTop: 16 }}>
          <h2>Agent steps</h2>
          <p className="muted">{latestRun.model} / {latestRun.searchMode} / {latestRun.status}</p>
          <div className="table-wrap inline-table">
            <table>
              <thead>
                <tr>
                  <th>Step</th>
                  <th>Tool</th>
                  <th>Duration</th>
                  <th>Output</th>
                </tr>
              </thead>
              <tbody>
                {latestRun.steps.map((step) => (
                  <tr key={step.id}>
                    <td>{step.stepIndex}</td>
                    <td>{step.toolType}</td>
                    <td>{step.durationMs} ms</td>
                    <td>{step.output}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      ) : null}
    </>
  );
}
