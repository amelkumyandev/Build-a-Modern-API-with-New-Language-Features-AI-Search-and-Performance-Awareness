"use client";

import { FormEvent, useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { SettingsResponse } from "@/types/api";

export default function SettingsPage() {
  const [settings, setSettings] = useState<SettingsResponse | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    api<SettingsResponse>("/api/admin/settings").then(setSettings).catch((err) => setMessage(err instanceof Error ? err.message : "Settings failed to load"));
  }, []);

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!settings) return;
    const updated = await api<SettingsResponse>("/api/admin/settings", {
      method: "PUT",
      body: JSON.stringify({
        chatModel: settings.chatModel,
        defaultSearchLimit: settings.defaultSearchLimit,
        vectorWeight: settings.vectorWeight,
        keywordWeight: settings.keywordWeight,
        recencyWeight: settings.recencyWeight
      })
    });
    setSettings(updated);
    setMessage("Settings updated for this running API instance.");
  }

  if (!settings) {
    return <div>Loading...</div>;
  }

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">Settings</h1>
          <p className="page-subtitle">Runtime search and model settings for local development.</p>
        </div>
      </header>
      {message ? <div className="message">{message}</div> : null}
      <form className="form panel" onSubmit={submit}>
        <div className="grid grid-2">
          <ReadOnly label="Embedding model" value={settings.embeddingModel} />
          <ReadOnly label="Embedding dimensions" value={settings.embeddingDimensions.toString()} />
          <ReadOnly label="Chunk size" value={`${settings.targetTokenCount} tokens`} />
          <ReadOnly label="Chunk overlap" value={`${settings.overlapTokenCount} tokens`} />
          <div className="field">
            <label htmlFor="chatModel">Chat model</label>
            <input id="chatModel" value={settings.chatModel} onChange={(event) => setSettings({ ...settings, chatModel: event.target.value })} />
          </div>
          <div className="field">
            <label htmlFor="limit">Default search limit</label>
            <input id="limit" type="number" value={settings.defaultSearchLimit} onChange={(event) => setSettings({ ...settings, defaultSearchLimit: Number(event.target.value) })} />
          </div>
          <div className="field">
            <label htmlFor="vector">Vector weight</label>
            <input id="vector" type="number" step="0.01" value={settings.vectorWeight} onChange={(event) => setSettings({ ...settings, vectorWeight: Number(event.target.value) })} />
          </div>
          <div className="field">
            <label htmlFor="keyword">Keyword weight</label>
            <input id="keyword" type="number" step="0.01" value={settings.keywordWeight} onChange={(event) => setSettings({ ...settings, keywordWeight: Number(event.target.value) })} />
          </div>
          <div className="field">
            <label htmlFor="recency">Recency weight</label>
            <input id="recency" type="number" step="0.01" value={settings.recencyWeight} onChange={(event) => setSettings({ ...settings, recencyWeight: Number(event.target.value) })} />
          </div>
          <ReadOnly label="Max result limit" value={settings.maxSearchLimit.toString()} />
          <ReadOnly label="Max context chunks" value="20" />
          <ReadOnly label="Temperature" value="Provider default" />
          <ReadOnly label="Admin password change" value="Placeholder for production identity flow" />
        </div>
        <button className="btn" type="submit">Save settings</button>
      </form>
    </>
  );
}

function ReadOnly({ label, value }: { label: string; value: string }) {
  return (
    <div className="field">
      <label>{label}</label>
      <input value={value} readOnly />
    </div>
  );
}
