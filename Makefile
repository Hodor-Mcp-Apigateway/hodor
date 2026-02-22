.PHONY: build clean restore run migrate hodor hodor-install hodor-deploy hodor-health hodor-scale docker-build docker-run docker-stop docker-compose docker-compose-down deploy deploy-docker deploy-helm deploy-kind kind-setup kind-deploy kind-delete help

.DEFAULT_GOAL := help

DOCKER_IMAGE ?= hodor-mcp-gateway
DOCKER_TAG ?= latest

build: ## Build the solution
	dotnet build Hodor.slnx

test: ## Run unit tests
	dotnet test Hodor.slnx -c Release --verbosity normal

benchmark: ## Run latency/throughput benchmark (requires Hodor running)
	python3 benchmarks/run_benchmark.py 2>/dev/null || python benchmarks/run_benchmark.py

test-coverage: ## Run tests with coverage
	dotnet test Hodor.slnx -c Release --verbosity normal \
		--collect:"XPlat Code Coverage" \
		--results-directory ./TestResults/ \
		-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

clean: ## Clean the solution
	dotnet clean Hodor.slnx
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true

restore: ## Restore NuGet packages
	dotnet restore Hodor.slnx

run: ## Run the MCP Gateway
	dotnet run --project src/Hodor.Host/Hodor.Host.csproj

hodor: ## Build Hodor CLI
	dotnet build src/Hodor.Cli/Hodor.Cli.csproj

hodor-install: ## Install Hodor (Docker Compose) via CLI
	dotnet run --project src/Hodor.Cli/Hodor.Cli.csproj -- install

hodor-deploy: ## Deploy via CLI (make hodor-deploy TARGET=docker|helm|kind)
	dotnet run --project src/Hodor.Cli/Hodor.Cli.csproj -- deploy --target $(or $(TARGET),docker)

hodor-health: ## Check Hodor health via CLI
	dotnet run --project src/Hodor.Cli/Hodor.Cli.csproj -- health

hodor-scale: ## Scale Hodor replicas (make hodor-scale REPLICAS=3)
	dotnet run --project src/Hodor.Cli/Hodor.Cli.csproj -- scale --replicas $(or $(REPLICAS),2)

migrate: ## Apply database migrations
	dotnet ef database update --project src/Hodor.Persistence --startup-project src/Hodor.Host

docker-build: ## Build Docker image
	docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) .

docker-run: ## Run Docker container
	docker run -d --name $(DOCKER_IMAGE) \
		-p 8080:8080 \
		-e ASPNETCORE_ENVIRONMENT=Development \
		$(DOCKER_IMAGE):$(DOCKER_TAG)

docker-stop: ## Stop and remove Docker container
	docker stop $(DOCKER_IMAGE) || true
	docker rm $(DOCKER_IMAGE) || true

docker-logs: ## View Docker container logs
	docker logs -f $(DOCKER_IMAGE)

docker-compose: ## Start Hodor MCP Gateway (requires postgres)
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml up -d

docker-compose-build: ## Build and start Hodor with PostgreSQL + pgvector
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml up -d --build

docker-compose-minimal: ## Minimal: PostgreSQL + Hodor only (fast startup)
	cd deployment/docker && docker compose -f docker-compose-minimal.yaml up -d --build

docker-compose-up: ## Unified: PostgreSQL + Hodor (supports --scale hodor=N)
	cd deployment/docker && docker compose -f docker-compose.yaml up -d --build

docker-compose-scale: ## Scale Hodor replicas (e.g. make docker-compose-scale REPLICAS=3)
	cd deployment/docker && docker compose -f docker-compose.yaml up -d --scale hodor=$(or $(REPLICAS),2)

docker-compose-down: ## Stop all containers
	cd deployment/docker && docker compose -f docker-compose.yaml down

deploy: ## Deploy (docker by default)
	chmod +x deployment/scripts/deploy.sh && ./deployment/scripts/deploy.sh docker

deploy-docker: ## Deploy via Docker Compose
	chmod +x deployment/scripts/deploy.sh && ./deployment/scripts/deploy.sh docker

deploy-helm: ## Deploy via Helm (Kubernetes)
	chmod +x deployment/scripts/deploy.sh && ./deployment/scripts/deploy.sh helm

deploy-kind: ## Deploy via Kind + Helm
	chmod +x deployment/scripts/deploy.sh && ./deployment/scripts/deploy.sh kind

docker-compose-down-legacy: ## Stop legacy compose
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml down

kind-setup: ## Create Kind cluster and deploy Hodor
	cd deployment/kind && chmod +x setup.sh && ./setup.sh all

kind-deploy: ## Deploy/update Hodor on existing Kind cluster
	cd deployment/kind && ./setup.sh deploy

kind-delete: ## Delete Kind cluster
	kind delete cluster --name hodor-cluster

help: ## Show this help message
	@echo "Hodor MCP Gateway - Makefile Commands"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-25s\033[0m %s\n", $$1, $$2}'
