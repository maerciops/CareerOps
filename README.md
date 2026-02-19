# 🚀 CareerOps

> **Enterprise Career CRM & AI-Driven Analytics Platform**
> *Architected for Scalability, Multi-tenancy, and Data-Driven Job Searching.*

![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white)
![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-black?style=for-the-badge)
![Gemini 2.5 Flash](https://img.shields.io/badge/AI-Gemini%202.5%20Flash-4285F4?style=for-the-badge&logo=google&logoColor=white)

## 📋 Project Overview

**CareerOps** is a modern, cloud-native Career CRM designed to give candidates telemetry and strategic control over their job search. It moves beyond traditional "apply and pray" methods by leveraging **Generative AI** to analyze resumes against job descriptions, providing actionable gap analysis and A/B testing insights.

This project serves as a comprehensive engineering portfolio demonstrating a **Senior-level architecture** suitable for the European Enterprise market. It strictly adheres to **Clean Architecture**, **Domain-Driven Design (DDD)** principles, and **SaaS Multi-tenancy** strategies.

### 🎯 Key Goals
- **Architecture First:** Demonstrate a robust Modular Monolith ready for microservices and data pipelines extraction.
- **FinOps Oriented:** Architecture optimized for low cloud costs using efficient AI models (**Gemini 2.5 Flash**) and local text processing.
- **Privacy by Design:** GDPR-compliant architecture with strict data isolation per tenant and audit trails.

---

## 🏗️ Architecture

The solution follows **Clean Architecture** principles to ensure separation of concerns, testability, and adherence to the Dependency Inversion Principle.

```mermaid
graph TD
    %% Atores e Camadas Externas
    User((User)) --> API["Presentation Layer (Web API / Blazor)"]

    %% Fluxo de Dependências (Quem conhece quem)
    subgraph Core ["Core Business Logic"]
        direction TB
        Application["Application Layer<br/>(Interfaces, Use Cases, DTOs)"] --> Domain["Domain Layer<br/>(Entities, Logic)"]
    end

    %% A API conhece a Infra para Injeção de Dependência (Composition Root)
    API --> Core
    API --> Infrastructure["Infrastructure Layer<br/>(Implementation)"]

    %% A Infra conhece o Core para implementar as Interfaces
    Infrastructure --> Application
    Infrastructure --> Domain

    %% Recursos Externos (Acessados pela Infra)
    Infrastructure -.-> DB[("Azure SQL")]
    Infrastructure -.-> Blob[("Azure Blob Storage")]
    Infrastructure -.-> AI["Google Gemini API"]

    %% Estilização
    classDef core fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef infra fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;
    classDef ext fill:#fff3e0,stroke:#e65100,stroke-width:2px,stroke-dasharray: 5 5;
    
    class Domain,Application core;
    class Infrastructure,API infra;
    class DB,Blob,AI ext;
