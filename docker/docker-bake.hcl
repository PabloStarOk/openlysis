group "default" {
  targets = [ "database", "auth", "analysis-orchestrator", "api" ]
}

target "database" {
    context = "../"
    dockerfile = "docker/db/Dockerfile"
    tags = [ "openlysis-db:latest" ]
    no-cache = true
    description = "Database of Openlysis."
}

target "auth" {
    context = "../"
    dockerfile = "src/Openlysis.Authentication.API/Dockerfile"
    tags = [ "openlysis-auth:latest" ]
    no-cache = true
    description = "Authentication API for managing user identities and access."
}

target "analysis-orchestrator" {
    context = "../"
    dockerfile = "src/Infrastructure/Services/Openlysis.AnalysisOrchestrator/Dockerfile"
    tags = [ "openlysis-analysis-orchestrator:latest" ]
    no-cache = true
    description = "The service to start and poll analyses using different services."
}

target "api" {
    context = "../"
    dockerfile = "src/Openlysis.API/Dockerfile"
    tags = [ "openlysis-api:latest" ]
    no-cache = true
    description = "API to receive requests from clients to perform analysis."
}