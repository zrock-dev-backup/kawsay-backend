.PHONY: help clean build run develop all docker-build docker-run docker-push deps check

# Variables
IMAGE_NAME := kawsay-backend
TAG := $(shell git rev-parse --short HEAD)
REGISTRY := your-registry.com
ENVIRONMENT := development

# Default target
all: build

# === NIX TARGETS ===
build:
	@echo "Building with Nix..."
	nix build .#$(ENVIRONMENT)

build-prod:
	@echo "Building production with Nix..."
	nix build .#production

run:
	@echo "Running locally..."
	./result/bin/Api

develop:
	@echo "Entering development shell..."
	KAWSAY_CONNECTION_STRING="Host=localhost;Port=5432;Database=test;Username=user;Password=password" nix develop

deps:
	@echo "Generating NuGet dependencies..."
	nix run .#generateDeps

docker-build:
	nix build '.#dockerImage'
	docker load < result

docker-run:
	docker run kawsay-backend:latest

migrate:
	KAWSAY_CONNECTION_STRING="Host=localhost;Port=5432;Database=test;Username=user;Password=password" nix run .#migrate

# Utility targets
check:
	@echo "Checking build output..."
	@echo "Result structure:"
	find result/ -type f | head -10
	@echo "Executable info:"
	file result/bin/Api
	@echo "Size:"
	du -h result/bin/Api

# Health check
health:
	@echo "Health checking..."
	curl -f http://localhost:8080/health || echo "Health check failed"
