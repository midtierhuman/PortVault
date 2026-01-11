# Using Enhanced Swagger with GitHub Copilot

## Quick Start

### 1. Export Your API Spec
Run your API and download the OpenAPI specification:

```bash
# Start your API
dotnet run --project PortVault.Api

# In another terminal, download the spec
curl http://localhost:5000/swagger/v1/swagger.json -o portvault-api.json
```

### 2. Add to Your Frontend Project
```
frontend/
??? src/
?   ??? api/
?   ?   ??? portvault-api.json    ? Place here
?   ??? ...
```

### 3. Reference in Copilot Chat

Now you can use Copilot with full API context!

---

## Copilot Prompts for Frontend Development

### Generate TypeScript Types

**Prompt:**
```
@workspace Generate TypeScript interfaces from portvault-api.json 
for the CreateTransactionRequest and TransactionResponse models.
Include all validation rules as JSDoc comments.
```

**Expected Output:**
```typescript
/**
 * Request model for creating a transaction
 */
export interface CreateTransactionRequest {
  /** Stock symbol (1-100 characters) */
  symbol: string;
  
  /** ISIN identifier (1-50 characters) */
  isin: string;
  
  /** Date of the trade */
  tradeDate: Date;
  
  /** Market segment (e.g., "EQ", "MF") */
  segment: string;
  
  /** Series (e.g., "EQ", "BE") */
  series?: string;
  
  /** "Buy" or "Sell" */
  tradeType: 'Buy' | 'Sell';
  
  /** Number of shares/units (minimum: 0.000001) */
  quantity: number;
  
  /** Price per share/unit (minimum: 0.000001) */
  price: number;
  
  /** Exact time of execution */
  orderExecutionTime?: Date;
  
  /** Trade ID (max 100 characters) */
  tradeID?: string;
  
  /** Order ID (max 100 characters) */
  orderID?: string;
}
```

---

### Create API Service Class

**Prompt:**
```
@workspace Using portvault-api.json, create a TypeScript service class 
for the Transactions API with methods for:
- Getting transactions (with pagination)
- Adding a single transaction
- Uploading transactions from file
- Deleting transactions

Include error handling and proper typing.
```

**Expected Output:**
```typescript
import axios, { AxiosInstance } from 'axios';

export class TransactionService {
  private api: AxiosInstance;
  
  constructor(baseURL: string, token: string) {
    this.api = axios.create({
      baseURL,
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
  }
  
  async getTransactions(
    portfolioName: string,
    options?: {
      page?: number;
      pageSize?: number;
      from?: Date;
      to?: Date;
      search?: string;
    }
  ): Promise<PaginatedTransactionsResponse> {
    const response = await this.api.get(
      `/api/portfolio/${portfolioName}/transactions`,
      { params: options }
    );
    return response.data.data;
  }
  
  async addTransaction(
    portfolioName: string,
    request: CreateTransactionRequest
  ): Promise<void> {
    await this.api.post(
      `/api/portfolio/${portfolioName}/transactions`,
      request
    );
  }
  
  // ... more methods
}
```

---

### Create React Hook

**Prompt:**
```
@workspace Create a React hook called useTransactions that:
1. Fetches transactions for a portfolio using the API spec
2. Supports pagination
3. Handles loading and error states
4. Allows adding new transactions
5. Uses TypeScript with proper types from portvault-api.json
```

**Expected Output:**
```typescript
import { useState, useEffect } from 'react';
import { TransactionService } from './services/transaction-service';
import type { 
  TransactionResponse, 
  CreateTransactionRequest,
  PaginatedTransactionsResponse 
} from './types/api';

interface UseTransactionsOptions {
  portfolioName: string;
  page?: number;
  pageSize?: number;
}

export const useTransactions = ({
  portfolioName,
  page = 1,
  pageSize = 20
}: UseTransactionsOptions) => {
  const [data, setData] = useState<PaginatedTransactionsResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  
  const service = new TransactionService(
    process.env.REACT_APP_API_URL!,
    localStorage.getItem('token')!
  );
  
  const fetchTransactions = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await service.getTransactions(portfolioName, {
        page,
        pageSize
      });
      setData(result);
    } catch (err) {
      setError(err as Error);
    } finally {
      setLoading(false);
    }
  };
  
  const addTransaction = async (request: CreateTransactionRequest) => {
    await service.addTransaction(portfolioName, request);
    await fetchTransactions(); // Refresh
  };
  
  useEffect(() => {
    fetchTransactions();
  }, [portfolioName, page, pageSize]);
  
  return { data, loading, error, refetch: fetchTransactions, addTransaction };
};
```

---

### Generate Angular Service

**Prompt:**
```
@workspace Generate an Angular service from portvault-api.json 
for managing transactions with proper dependency injection and observables.
```

---

### Create Validation Schema

**Prompt:**
```
@workspace Using portvault-api.json, create a Zod validation schema 
for CreateTransactionRequest with all the validation rules.
```

**Expected Output:**
```typescript
import { z } from 'zod';

export const CreateTransactionSchema = z.object({
  symbol: z.string().min(1).max(100),
  isin: z.string().min(1).max(50),
  tradeDate: z.date(),
  segment: z.string().max(50),
  series: z.string().max(50).optional(),
  tradeType: z.enum(['Buy', 'Sell']),
  quantity: z.number().min(0.000001),
  price: z.number().min(0.000001),
  orderExecutionTime: z.date().optional(),
  tradeID: z.string().max(100).optional(),
  orderID: z.string().max(100).optional()
});

export type CreateTransactionRequest = z.infer<typeof CreateTransactionSchema>;
```

---

### Generate Form Component

**Prompt:**
```
@workspace Create a React form component for adding a transaction.
Use the CreateTransactionRequest schema from portvault-api.json.
Include validation and error handling.
Use react-hook-form and zod for validation.
```

---

## Advanced Copilot Usage

### Context-Aware Development

**Prompt:**
```
@workspace I want to create a transaction management page.
Using portvault-api.json:

1. Create a component that lists transactions in a table
2. Add pagination using the API's pagination parameters
3. Include a button to add new transactions
4. Show a modal form when adding
5. Refresh the list after adding

Use Material-UI for the UI and React Query for data fetching.
```

Copilot will generate a complete solution because it understands:
- Exact endpoint structure
- Request/response types
- Pagination parameters
- Authentication requirements
- Error responses

---

### Generate API Client SDK

**Prompt:**
```
@workspace Generate a complete TypeScript SDK from portvault-api.json 
with classes for each controller (Auth, Portfolio, Transactions, etc.).

Include:
- Axios-based HTTP client
- Bearer token authentication
- Error handling with custom error classes
- Type-safe request/response models
- JSDoc documentation for all methods
```

---

### Create Mock Data

**Prompt:**
```
@workspace Generate mock data for testing based on portvault-api.json.
Create factory functions for:
- TransactionResponse (10 samples)
- CreateTransactionRequest (valid examples)
- PaginatedTransactionsResponse

Use faker.js for realistic data.
```

---

## Best Practices

### 1. **Keep API Spec Updated**
```bash
# Add to your package.json
{
  "scripts": {
    "fetch-api-spec": "curl http://localhost:5000/swagger/v1/swagger.json -o src/api/spec.json"
  }
}
```

### 2. **Version Control the Spec**
Commit `portvault-api.json` to your frontend repo so Copilot always has the latest API contract.

### 3. **Use Specific Prompts**
Instead of: "Create an API service"
Use: "Using portvault-api.json, create a TypeScript service for the Transactions endpoints"

### 4. **Reference Specific Models**
```
@workspace Generate a React component that uses the 
CreateTransactionRequest and TransactionResponse models from portvault-api.json
```

### 5. **Combine with Workspace Context**
```
@workspace Using portvault-api.json and the existing useAuth hook,
create a useTransactions hook with automatic token injection
```

---

## Example: Complete Feature Development

**Prompt:**
```
@workspace Create a complete transaction management feature:

1. **Types** (from portvault-api.json):
   - TransactionResponse
   - CreateTransactionRequest
   - PaginatedTransactionsResponse

2. **Service** (src/services/transaction.service.ts):
   - getTransactions(portfolioName, page, pageSize)
   - addTransaction(portfolioName, data)
   - deleteTransaction(portfolioName, id)

3. **Hook** (src/hooks/useTransactions.ts):
   - Fetch transactions with React Query
   - Pagination support
   - Add/delete mutations

4. **Components**:
   - TransactionList (table with pagination)
   - AddTransactionModal (form with validation)
   - TransactionRow (single row with delete button)

5. **Validation**:
   - Zod schema from API spec
   - React Hook Form integration

Use TypeScript, Material-UI, React Query, and follow the existing code style.
```

Copilot will generate a complete, type-safe feature because it has:
? API contract from swagger  
? Workspace code style  
? Existing patterns from your codebase  
? Validation rules from OpenAPI spec  

---

## VS Code Setup

### Recommended Extensions
1. **GitHub Copilot** - AI pair programmer
2. **GitHub Copilot Chat** - Chat interface for Copilot
3. **OpenAPI (Swagger) Editor** - View and edit OpenAPI specs
4. **REST Client** - Test API endpoints from VS Code

### Settings
Add to `.vscode/settings.json`:
```json
{
  "github.copilot.enable": {
    "*": true,
    "yaml": true,
    "json": true,
    "typescript": true,
    "typescriptreact": true
  },
  "github.copilot.advanced": {
    "debug.overrideEngine": "gpt-4"
  }
}
```

---

## Troubleshooting

### Copilot Not Using API Spec
- Ensure the spec file is in your workspace
- Reference it explicitly: `Using portvault-api.json`
- Open the spec file before prompting

### Types Not Matching
- Re-export the swagger.json after API changes
- Clear Copilot cache (reload VS Code)
- Verify the spec file is up to date

### Incomplete Responses
- Be more specific in prompts
- Break complex tasks into smaller steps
- Reference specific endpoints or models

---

## Real-World Example Workflow

### Day 1: API Development
```bash
# Backend team creates endpoints with Swagger docs
cd backend
dotnet run

# Export spec
curl http://localhost:5000/swagger/v1/swagger.json -o swagger.json
git add swagger.json
git commit -m "Add transaction endpoints"
```

### Day 2: Frontend Development
```bash
# Frontend team pulls latest
git pull

# Generate types
@workspace Generate TypeScript types from swagger.json

# Create service
@workspace Create API service from swagger.json

# Build UI
@workspace Create transaction list component using the API service
```

### Result
- ? Type-safe API calls
- ? No manual type definitions
- ? Automatic validation
- ? Faster development
- ? Fewer bugs

---

Your enhanced Swagger documentation is now the **single source of truth** for both backend and frontend, enabling seamless development with GitHub Copilot! ??
