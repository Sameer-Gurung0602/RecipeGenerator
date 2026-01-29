#  Recipe Generator API

A comprehensive RESTful API for recipe management and intelligent ingredient matching built with ASP.NET Core 8.0 and Razor Pages.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

##  Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Database Setup](#database-setup)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [Database Schema](#database-schema)
- [Security](#security)
- [Contributing](#contributing)
- [License](#license)

##  Overview

Recipe Generator API is a modern web application that helps users discover recipes based on available ingredients. The API features intelligent recipe matching with percentage-based scoring, dietary restriction filtering, and a user favorites system.

**Live API Documentation:** `placeholder.com`

## Features

- ** Smart Recipe Matching**: Find recipes based on available ingredients with match percentage scoring
- ** Dietary Restrictions**: Filter recipes by dietary preferences (Vegan, Vegetarian, Gluten-Free)
- ** Favorites System**: Save and manage favorite recipes per user
- ** Sorting & Filtering**: Sort recipes by name, cook time, difficulty, or date
- ** Recipe Images**: Visual recipe cards with Unsplash integration
- ** Interactive API Docs**: Beautiful, self-hosted API documentation at root URL
- ** SQL Injection Protection**: Whitelist validation for all database operations
- ** Cross-Database Support**: Compatible with SQL Server and PostgreSQL

##  Tech Stack

**Backend:**
- ASP.NET Core 8.0 (Razor Pages)
- Entity Framework Core 8.0
- SQL Server / LocalDB
- C# 12.0

**Frontend (API Docs):**
- Razor Pages
- Bootstrap 5
- Custom CSS with gradient designs

**Testing:**
- xUnit
- FluentAssertions

- Azure App Service (deployment ready)

##  Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
