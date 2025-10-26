# 🌐 SafeReport.Web

**SafeReport.Web** is a Blazor WebAssembly application for managing and reporting incidents within the Safe Report system.  
It provides a **modern multilingual interface (Arabic / English)** with dynamic features such as 

**filtering,
**notifications,
**printing
**pagination  — all built for scalability and performance.

🔐 Authentication & Authorization

SafeReport implements user authentication and role-based authorization:
Authentication: Users must log in to access the application.
Authorization: Certain actions (like deleting reports) are restricted to specific roles (e.g., Admin).
JWT Tokens: Secures API endpoints and ensures that only authenticated users can perform actions.
Blazor Integration: Frontend pages are protected using [Authorize] and role-based <AuthorizeView> components.

## 🧩 Requirements

Before running the project, ensure the following are installed:

| Tool | Version | Purpose |Scripts | DataSeeding
|------|----------|----------|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | **8.0+** | Build & run the Blazor WebAssembly app |
| [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) | **v17.8+** | Development environment |
| **Workload** | *ASP.NET and Web Development* | Required for Blazor projects |
| [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) | 2019+ | Database engine |


| [Git] https://github.com/saraBadawy77/Scripts.git | — | Clone and manage the repository |

---

## ⚙️ Installation & Run

### 1️⃣ Clone the repository

```bash
git clone https://github.com/saraBadawy77/SafeReport.git
cd SafeReport.Web
