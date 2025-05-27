group "default" {
  targets = [ "database", "analyzer-worker", "api" ]
}

target "database" {
    context = "../"
    dockerfile = "docker/db/Dockerfile"
    tags = [ "openlysis-db:latest" ]
    no-cache = true
    description = "Database of Openlysis."
}

target "analyzer-worker" {
    context = "../"
    dockerfile = "src/Infrastructure/Workers/Openlysis.MultiAnalyzer/Dockerfile"
    tags = [ "openlysis-analyzer-worker:latest" ]
    no-cache = true
    description = "Worker service to execute and update analyses using different services."
}

target "api" {
    context = "../"
    dockerfile = "src/Openlysis.API/Dockerfile"
    tags = [ "openlysis-api:latest" ]
    no-cache = true
    description = "API to receive requests from clients to perform analysis."
}