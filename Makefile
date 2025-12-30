.PHONY: build clean restore test run \
        template-install template-uninstall template-list template-help \
        template-create template-create-minimal template-create-full template-create-api template-create-realtime template-create-worker \
        docker-build docker-run docker-stop docker-compose-infra docker-compose-infra-down docker-compose-app docker-compose-all docker-compose-down \
        help

# Default target
.DEFAULT_GOAL := help

# Variables
TEMPLATE_NAME := papel-template
PROJECT_NAME ?= MyMicroservice
OUTPUT_DIR ?= /tmp/$(PROJECT_NAME)
DOCKER_IMAGE ?= papel-integration
DOCKER_TAG ?= latest

# Build targets
build: ## Build the solution
	dotnet build

clean: ## Clean the solution
	dotnet clean
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true

restore: ## Restore NuGet packages
	dotnet restore

test: ## Run all tests
	dotnet test

run: ## Run the application
	dotnet run --project src/Papel.Integration.Presentation.Starter/Papel.Integration.Presentation.Starter.csproj

# Template targets
template-install: ## Install the template locally
	dotnet new install . --force

template-uninstall: ## Uninstall the template
	dotnet new uninstall .

template-list: ## List installed templates
	dotnet new list $(TEMPLATE_NAME)

template-help: ## Show template options
	dotnet new $(TEMPLATE_NAME) --help

# Template creation examples
template-create-minimal: template-install ## Create minimal project (REST only)
	@rm -rf $(OUTPUT_DIR)
	dotnet new $(TEMPLATE_NAME) \
		-n $(PROJECT_NAME) \
		-o $(OUTPUT_DIR) \
		--EnableRest true \
		--EnableGraphQL false \
		--EnableGrpc false \
		--EnableSignalR false \
		--EnableKafka false \
		--EnableRedisCache false \
		--EnableTests true \
		--allow-scripts Yes
	@echo "Project created at $(OUTPUT_DIR)"
	@echo "Run: cd $(OUTPUT_DIR) && dotnet build"

template-create-full: template-install ## Create full project (all features enabled)
	@rm -rf $(OUTPUT_DIR)
	dotnet new $(TEMPLATE_NAME) \
		-n $(PROJECT_NAME) \
		-o $(OUTPUT_DIR) \
		--EnableRest true \
		--EnableGraphQL true \
		--EnableGrpc true \
		--EnableSignalR true \
		--EnableKafka true \
		--EnableRedisCache true \
		--EnableTests true \
		--allow-scripts Yes
	@echo "Project created at $(OUTPUT_DIR)"
	@echo "Run: cd $(OUTPUT_DIR) && dotnet build"

template-create-api: template-install ## Create REST + GraphQL + gRPC project
	@rm -rf $(OUTPUT_DIR)
	dotnet new $(TEMPLATE_NAME) \
		-n $(PROJECT_NAME) \
		-o $(OUTPUT_DIR) \
		--EnableRest true \
		--EnableGraphQL true \
		--EnableGrpc true \
		--EnableSignalR false \
		--EnableKafka false \
		--EnableRedisCache false \
		--EnableTests true \
		--allow-scripts Yes
	@echo "Project created at $(OUTPUT_DIR)"
	@echo "Run: cd $(OUTPUT_DIR) && dotnet build"

template-create-realtime: template-install ## Create REST + SignalR + Kafka project
	@rm -rf $(OUTPUT_DIR)
	dotnet new $(TEMPLATE_NAME) \
		-n $(PROJECT_NAME) \
		-o $(OUTPUT_DIR) \
		--EnableRest true \
		--EnableGraphQL false \
		--EnableGrpc false \
		--EnableSignalR true \
		--EnableKafka true \
		--EnableRedisCache true \
		--EnableTests true \
		--allow-scripts Yes
	@echo "Project created at $(OUTPUT_DIR)"
	@echo "Run: cd $(OUTPUT_DIR) && dotnet build"

template-create-worker: template-install ## Create Kafka worker project (no REST API)
	@rm -rf $(OUTPUT_DIR)
	dotnet new $(TEMPLATE_NAME) \
		-n $(PROJECT_NAME) \
		-o $(OUTPUT_DIR) \
		--EnableRest false \
		--EnableGraphQL false \
		--EnableGrpc false \
		--EnableSignalR false \
		--EnableKafka true \
		--EnableRedisCache false \
		--EnableTests false \
		--allow-scripts Yes
	@echo "Project created at $(OUTPUT_DIR)"
	@echo "Run: cd $(OUTPUT_DIR) && dotnet build"

template-create: template-install ## Create custom project (use PROJECT_NAME=X and TEMPLATE_OPTIONS)
	@rm -rf $(OUTPUT_DIR)
	dotnet new $(TEMPLATE_NAME) \
		-n $(PROJECT_NAME) \
		-o $(OUTPUT_DIR) \
		$(TEMPLATE_OPTIONS) \
		--allow-scripts Yes
	@echo "Project created at $(OUTPUT_DIR)"
	@echo "Run: cd $(OUTPUT_DIR) && dotnet build"

# Docker targets
docker-build: ## Build Docker image
	docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) .

docker-run: ## Run Docker container
	docker run -d --name $(DOCKER_IMAGE) \
		-p 8080:8080 -p 8081:8081 \
		-e ASPNETCORE_ENVIRONMENT=Development \
		$(DOCKER_IMAGE):$(DOCKER_TAG)

docker-stop: ## Stop and remove Docker container
	docker stop $(DOCKER_IMAGE) || true
	docker rm $(DOCKER_IMAGE) || true

docker-logs: ## View Docker container logs
	docker logs -f $(DOCKER_IMAGE)

# Docker Compose targets
docker-compose-infra: ## Start infrastructure only (PostgreSQL, Kafka, Redis, Jaeger)
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml up -d

docker-compose-infra-down: ## Stop infrastructure
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml down

docker-compose-infra-logs: ## View infrastructure logs
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml logs -f

docker-compose-app: ## Start application with infrastructure
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml up -d

docker-compose-app-build: ## Build and start application with infrastructure
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml up -d --build

docker-compose-down: ## Stop all containers
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml down

docker-compose-clean: ## Stop all containers and remove volumes
	cd deployment/docker && docker compose -f docker-compose-infrastructure.yaml -f docker-compose-app.yaml down -v

# Kubernetes targets
k8s-infra-install: ## Install infrastructure on Kubernetes (PostgreSQL, Kafka, Redis)
	cd deployment/k8s/infrastructure && chmod +x install.sh && ./install.sh

k8s-deploy: ## Deploy application to Kubernetes
	helm upgrade --install template-service deployment/k8s/.helm/template-service

k8s-uninstall: ## Uninstall application from Kubernetes
	helm uninstall template-service

# Help
help: ## Show this help message
	@echo "Papel Integration Template - Makefile Commands"
	@echo ""
	@echo "Usage: make [target] [VARIABLE=value]"
	@echo ""
	@echo "Variables:"
	@echo "  PROJECT_NAME      Name of the project to create (default: MyMicroservice)"
	@echo "  OUTPUT_DIR        Output directory (default: /tmp/PROJECT_NAME)"
	@echo "  TEMPLATE_OPTIONS  Additional template options"
	@echo "  DOCKER_IMAGE      Docker image name (default: papel-integration)"
	@echo "  DOCKER_TAG        Docker image tag (default: latest)"
	@echo ""
	@echo "Targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-25s\033[0m %s\n", $$1, $$2}'
	@echo ""
	@echo "Examples:"
	@echo "  make template-create-minimal PROJECT_NAME=OrderService"
	@echo "  make template-create-full PROJECT_NAME=PaymentService OUTPUT_DIR=./projects/PaymentService"
	@echo "  make template-create PROJECT_NAME=MyService TEMPLATE_OPTIONS='--EnableRest true --EnableKafka true'"
	@echo "  make docker-compose-infra      # Start PostgreSQL, Kafka, Redis, Jaeger"
	@echo "  make docker-compose-app-build  # Build and run application with infrastructure"
