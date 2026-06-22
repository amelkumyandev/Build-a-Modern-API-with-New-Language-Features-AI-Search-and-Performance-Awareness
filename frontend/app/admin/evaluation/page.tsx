"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import type { EvaluationQuestion, EvaluationQuestionResult, EvaluationRun } from "@/types/api";

export default function EvaluationPage() {
  const [questions, setQuestions] = useState<EvaluationQuestion[]>([]);
  const [run, setRun] = useState<EvaluationRun | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [mode, setMode] = useState("Hybrid");
  const [topK, setTopK] = useState(8);
  const [busy, setBusy] = useState(false);

  async function load() {
    try {
      setQuestions(await api<EvaluationQuestion[]>("/api/evaluation/questions"));
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Questions failed to load");
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function generate() {
    setBusy(true);
    setMessage("Generating questions...");
    try {
      const result = await api<{ questionsCreated: number }>("/api/evaluation/generate-questions", { method: "POST" });
      setMessage(`Created ${result.questionsCreated} questions.`);
      await load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Question generation failed");
    } finally {
      setBusy(false);
    }
  }

  async function runEvaluation() {
    setBusy(true);
    setMessage("Running evaluation...");
    try {
      const result = await api<EvaluationRun>("/api/evaluation/run", {
        method: "POST",
        body: JSON.stringify({ searchMode: mode, topK })
      });
      setRun(result);
      setMessage("Evaluation completed.");
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Evaluation failed");
    } finally {
      setBusy(false);
    }
  }

  const failedResults = run?.results.filter((result) => !result.passed) ?? [];
  const passedCount = run ? run.results.filter((result) => result.passed).length : 0;

  return (
    <>
      <header className="page-header">
        <div>
          <h1 className="page-title">Evaluation</h1>
          <p className="page-subtitle">Generate and run retrieval-quality checks.</p>
        </div>
        <div className="button-row">
          <button className="btn secondary" type="button" disabled={busy} onClick={generate}>Generate questions</button>
          <button className="btn" type="button" disabled={busy} onClick={runEvaluation}>{busy ? "Working..." : "Run evaluation"}</button>
        </div>
      </header>
      {message ? <div className="message">{message}</div> : null}
      <section className="panel filter-bar">
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
      </section>
      {run ? (
        <section className="grid grid-4" style={{ marginBottom: 16 }}>
          <div className="metric"><div className="metric-label">Latest score</div><div className="metric-value">{run.score}</div></div>
          <div className="metric"><div className="metric-label">Questions</div><div className="metric-value">{run.questionCount}</div></div>
          <div className="metric"><div className="metric-label">Mode</div><div className="metric-value">{run.searchMode}</div></div>
          <div className="metric"><div className="metric-label">Failed cases</div><div className="metric-value">{failedResults.length}</div></div>
        </section>
      ) : null}
      {run ? (
        <section className="panel" style={{ marginBottom: 16 }}>
          <h2>Retrieval success</h2>
          <p className="muted">{passedCount} of {run.questionCount} questions retrieved all expected source documents.</p>
          {failedResults.length > 0 ? (
            <div className="stack">
              {failedResults.map((result) => <EvaluationFailure key={result.questionId} result={result} />)}
            </div>
          ) : <div className="message">No failed cases in this run.</div>}
        </section>
      ) : null}
      {run ? (
        <section className="panel" style={{ marginBottom: 16 }}>
          <h2>Per-question results</h2>
          <div className="table-wrap inline-table">
            <table>
              <thead>
                <tr>
                  <th>Question</th>
                  <th>Score</th>
                  <th>Matched documents</th>
                  <th>Keyword coverage</th>
                  <th>Top retrieved</th>
                </tr>
              </thead>
              <tbody>
                {run.results.map((result) => (
                  <tr key={result.questionId}>
                    <td>{result.question}</td>
                    <td>{result.score}</td>
                    <td>{result.matchedExpectedDocumentTitles.length}/{result.expectedDocumentTitles.length}</td>
                    <td>{result.matchedExpectedKeywords.length}/{result.expectedAnswerKeywords.length}</td>
                    <td>{result.retrievedChunks[0]?.title ?? "No retrieval"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      ) : null}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Question</th>
              <th>Expected documents</th>
              <th>Difficulty</th>
            </tr>
          </thead>
          <tbody>
            {questions.map((question) => (
              <tr key={question.id}>
                <td>{question.question}</td>
                <td>{question.expectedDocumentTitles.join(", ")}</td>
                <td>{question.difficulty}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <section className="panel" style={{ marginTop: 16 }}>
        <h2>Answer quality rubric</h2>
        <p className="muted">The evaluator scores whether retrieved chunks include expected source documents, and records expected keyword coverage for diagnosis. Failed cases show missing documents and the top retrieved chunks from the run.</p>
      </section>
    </>
  );
}

function EvaluationFailure({ result }: { result: EvaluationQuestionResult }) {
  return (
    <article className="message">
      <strong>{result.question}</strong>
      <p>Missing expected documents: {result.missingExpectedDocumentTitles.join(", ") || "None"}</p>
      <p>Missing expected keywords: {result.missingExpectedKeywords.join(", ") || "None"}</p>
      <div className="stack">
        {result.retrievedChunks.map((chunk) => (
          <div key={chunk.chunkId}>
            <strong><Link href={`/admin/documents/${chunk.documentId}`}>{chunk.title}</Link></strong>
            <p>{chunk.snippet}</p>
            <span className="pill">final {chunk.finalScore}</span>
            <span className="pill">vector {chunk.vectorScore}</span>
            <span className="pill">keyword {chunk.keywordScore}</span>
          </div>
        ))}
      </div>
    </article>
  );
}
