# Agent Design

The agent does not train OpenAI on the local database. It retrieves local chunks, builds a grounded prompt, asks OpenAI for an answer, and returns citations.

Rules:

- Retrieve local chunks before answering.
- Use hybrid search by default.
- Return citations from actual retrieved chunks only.
- Say when local context is insufficient.
- Store agent runs and steps for inspection.
- Do not modify documents from the agent endpoint.
