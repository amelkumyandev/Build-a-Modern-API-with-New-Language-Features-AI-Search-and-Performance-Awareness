"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import type { DocumentCreated } from "@/types/api";

export default function NewDocumentPage() {
  const router = useRouter();
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [tags, setTags] = useState("dotnet,rag");
  const [metadata, setMetadata] = useState("{\"source\":\"manual\"}");
  const [error, setError] = useState<string | null>(null);
  const [indexAfterCreate, setIndexAfterCreate] = useState(false);
  const [busy, setBusy] = useState(false);
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadTitle, setUploadTitle] = useState("");
  const [uploadTags, setUploadTags] = useState("upload,rag");
  const [uploadMetadata, setUploadMetadata] = useState("{\"source\":\"upload\"}");
  const [uploadIndexAfterCreate, setUploadIndexAfterCreate] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploadBusy, setUploadBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const submitter = (event.nativeEvent as SubmitEvent).submitter as HTMLButtonElement | null;
      const shouldIndex = submitter?.dataset.index === "true" || indexAfterCreate;
      const parsedMetadata = JSON.parse(metadata) as Record<string, unknown>;
      const response = await api<DocumentCreated>(`/api/documents${shouldIndex ? "?index=true" : ""}`, {
        method: "POST",
        body: JSON.stringify({
          title,
          content,
          tags: tags.split(",").map((tag) => tag.trim()).filter(Boolean),
          metadata: parsedMetadata
        })
      });
      router.push(`/admin/documents/${response.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Create failed");
    } finally {
      setBusy(false);
    }
  }

  async function upload(event: FormEvent) {
    event.preventDefault();
    setUploadError(null);

    if (!uploadFile) {
      setUploadError("Choose a file to upload.");
      return;
    }

    setUploadBusy(true);
    try {
      if (uploadMetadata.trim().length > 0) {
        JSON.parse(uploadMetadata);
      }

      const form = new FormData();
      form.set("file", uploadFile);
      if (uploadTitle.trim().length > 0) {
        form.set("title", uploadTitle.trim());
      }
      form.set("tags", uploadTags);
      form.set("metadata", uploadMetadata);
      form.set("index", String(uploadIndexAfterCreate));

      const response = await api<DocumentCreated>(`/api/documents/upload${uploadIndexAfterCreate ? "?index=true" : ""}`, {
        method: "POST",
        body: form
      });
      router.push(`/admin/documents/${response.id}`);
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setUploadBusy(false);
    }
  }

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">New document</h1>
          <p className="page-subtitle">Create local source material for chunking, embedding, and retrieval.</p>
        </div>
      </header>
      <section className="grid grid-2">
        <form className="form panel" onSubmit={submit}>
          <h2>Manual entry</h2>
          <div className="field">
            <label htmlFor="title">Title</label>
            <input id="title" value={title} onChange={(event) => setTitle(event.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="content">Content</label>
            <textarea id="content" value={content} onChange={(event) => setContent(event.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="tags">Tags</label>
            <input id="tags" value={tags} onChange={(event) => setTags(event.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="metadata">Metadata JSON</label>
            <textarea id="metadata" value={metadata} onChange={(event) => setMetadata(event.target.value)} />
          </div>
          <label className="checkbox-row">
            <input type="checkbox" checked={indexAfterCreate} onChange={(event) => setIndexAfterCreate(event.target.checked)} />
            Save and embed immediately
          </label>
          {error ? <div className="error">{error}</div> : null}
          <div className="button-row">
            <button className="btn secondary" type="submit" disabled={busy} data-index="false">
              {busy && !indexAfterCreate ? "Saving..." : "Save"}
            </button>
            <button className="btn" type="submit" disabled={busy} data-index="true">
              {busy && indexAfterCreate ? "Saving and embedding..." : "Save and embed"}
            </button>
          </div>
        </form>

        <form className="form panel" onSubmit={upload}>
          <h2>Upload file</h2>
          <div className="field">
            <label htmlFor="upload-file">File</label>
            <input
              id="upload-file"
              type="file"
              accept=".txt,.md,.markdown,.json,.csv,.log,text/plain,text/markdown,application/json,text/csv"
              onChange={(event) => setUploadFile(event.target.files?.[0] ?? null)}
            />
          </div>
          <div className="field">
            <label htmlFor="upload-title">Title override</label>
            <input id="upload-title" value={uploadTitle} onChange={(event) => setUploadTitle(event.target.value)} placeholder="Defaults to the file name" />
          </div>
          <div className="field">
            <label htmlFor="upload-tags">Tags</label>
            <input id="upload-tags" value={uploadTags} onChange={(event) => setUploadTags(event.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="upload-metadata">Metadata JSON</label>
            <textarea id="upload-metadata" value={uploadMetadata} onChange={(event) => setUploadMetadata(event.target.value)} />
          </div>
          <label className="checkbox-row">
            <input type="checkbox" checked={uploadIndexAfterCreate} onChange={(event) => setUploadIndexAfterCreate(event.target.checked)} />
            Save and embed immediately
          </label>
          {uploadError ? <div className="error">{uploadError}</div> : null}
          <div className="button-row">
            <button className="btn" type="submit" disabled={uploadBusy}>
              {uploadBusy ? "Uploading..." : "Upload"}
            </button>
          </div>
        </form>
      </section>
    </>
  );
}
