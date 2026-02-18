# 🚀 ApplyWise

> **Enterprise AI-Powered Career Assistant & ATS**
> *Architected for Scalability, Multi-tenancy, and Cost-Efficiency.*

![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white)
![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-black?style=for-the-badge)

## 📋 Project Overview

**ApplyWise** is a modern, cloud-native Application Tracking System (ATS) tailored for candidates. It leverages **Generative AI** to analyze resumes against job descriptions, providing actionable feedback to increase hiring chances.

Built as a portfolio project demonstrating a **Senior-level migration from Delphi to the .NET ecosystem**, this project strictly adheres to **Clean Architecture**, **Domain-Driven Design (DDD)** principles, and **SaaS Multi-tenancy** strategies suitable for the European Enterprise market.

### 🎯 Key Goals
- **Architecture First:** Demonstrate a robust Modular Monolith ready for microservices extraction.
- **FinOps Oriented:** Architecture optimized for low cloud costs using Serverless and efficient AI models (Gemini 1.5 Flash).
- **Privacy by Design:** GDPR-compliant architecture with strict data isolation per tenant.

---

## 🏗️ Architecture

The solution follows the **Clean Architecture** principles to ensure separation of concerns, testability, and independence from frameworks.

```mermaid
graph TD
    User((User)) --> API[Presentation Layer (Web API / Blazor)]
    API --> Application[Application Layer (Use Cases, DTOs)]
    Application --> Domain[Domain Layer (Entities, Logic)]
    Application --> Infrastructure[Infrastructure Layer]
    Infrastructure --> DB[(SQL Server)]
    Infrastructure --> Blob[Azure Blob Storage]
    Infrastructure --> AI[Google Gemini via Semantic Kernel]
    
    classDef core fill:#f9f,stroke:#333,stroke-width:2px;
    class Domain,Application core;
