namespace LocalKnowledgeIntelligence.Application;

public sealed class UnauthorizedApplicationException(string message) : Exception(message);
