export type UserResponse = {
  id: string;
  username: string;
  role: string;
};

export type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  user: UserResponse;
};

export type DocumentCreated = {
  id: string;
  title: string;
  status: string;
  chunkingStatus: string;
  embeddingStatus: string;
};

export type DashboardResponse = {
  databaseReady: boolean;
  openAiKeyConfigured: boolean;
  totalDocuments: number;
  totalChunks: number;
  embeddedChunks: number;
  failedEmbeddings: number;
  latestEvaluationScore: number | null;
  developmentWarning: string;
};

export type PagedResponse<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

export type DocumentSummary = {
  id: string;
  title: string;
  summary: string | null;
  tags: string[];
  status: string;
  chunkingStatus: string;
  embeddingStatus: string;
  createdAt: string;
  updatedAt: string;
};

export type DocumentDetail = DocumentSummary & {
  content: string;
  metadata: Record<string, unknown>;
  createdByUserId: string;
};

export type ChunkResponse = {
  id: string;
  documentId: string;
  chunkIndex: number;
  content: string;
  tokenEstimate: number;
  embeddingModel: string | null;
  embeddingDimensions: number | null;
  embeddingGeneratedAt: string | null;
};

export type SearchResult = {
  documentId: string;
  chunkId: string;
  title: string;
  snippet: string;
  vectorScore: number;
  keywordScore: number;
  recencyScore: number;
  finalScore: number;
  distance: number | null;
  matchedFields: string[];
};

export type SearchResponse = {
  query: string;
  items: SearchResult[];
};

export type Citation = {
  documentId: string;
  chunkId: string;
  title: string;
  snippet: string;
  score: number;
};

export type AgentChatResponse = {
  sessionId: string;
  messageId: string;
  answer: string;
  citations: Citation[];
  agentRunId: string;
};

export type ChatSession = {
  id: string;
  title: string;
  createdAt: string;
  updatedAt: string;
};

export type ChatMessage = {
  id: string;
  sessionId: string;
  role: string;
  content: string;
  citations: Citation[];
  createdAt: string;
};

export type ChatSessionDetail = {
  session: ChatSession;
  messages: ChatMessage[];
};

export type AgentStep = {
  id: string;
  stepIndex: number;
  toolType: string;
  input: string;
  output: string;
  durationMs: number;
  createdAt: string;
};

export type AgentRun = {
  id: string;
  sessionId: string;
  status: string;
  model: string;
  searchMode: string;
  steps: AgentStep[];
  createdAt: string;
  completedAt: string | null;
};

export type EvaluationQuestion = {
  id: string;
  question: string;
  expectedAnswerKeywords: string[];
  expectedDocumentTitles: string[];
  difficulty: string;
  createdAt: string;
};

export type EvaluationRetrievedChunk = {
  documentId: string;
  chunkId: string;
  title: string;
  snippet: string;
  finalScore: number;
  vectorScore: number;
  keywordScore: number;
};

export type EvaluationQuestionResult = {
  questionId: string;
  question: string;
  difficulty: string;
  expectedDocumentTitles: string[];
  matchedExpectedDocumentTitles: string[];
  missingExpectedDocumentTitles: string[];
  expectedAnswerKeywords: string[];
  matchedExpectedKeywords: string[];
  missingExpectedKeywords: string[];
  retrievedChunks: EvaluationRetrievedChunk[];
  score: number;
  passed: boolean;
};

export type EvaluationRun = {
  id: string;
  searchMode: string;
  questionCount: number;
  score: number;
  results: EvaluationQuestionResult[];
  createdAt: string;
  completedAt: string;
};

export type SettingsResponse = {
  embeddingModel: string;
  chatModel: string;
  embeddingDimensions: number;
  targetTokenCount: number;
  overlapTokenCount: number;
  defaultSearchLimit: number;
  maxSearchLimit: number;
  vectorWeight: number;
  keywordWeight: number;
  recencyWeight: number;
};
