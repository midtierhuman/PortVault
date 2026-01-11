# Corporate Actions - Implementation Guide

## Overview
Corporate actions (splits, bonuses, mergers, etc.) are now automatically accounted for when calculating holdings and portfolio values. The system adjusts historical transaction quantities and prices based on the ex-date of each corporate action.

## How It Works

### 1. Recording Corporate Actions
When you add a corporate action via the Admin API, it gets stored in the database with:
- **Type**: Split, Bonus, Merger, Demerger, or NameChange
- **Ex-Date**: The date the corporate action becomes effective
- **Parent Instrument**: The instrument being affected
- **Ratio**: Numerator and Denominator defining the adjustment

### 2. Automatic Adjustment During Holdings Calculation
When you call the **Recalculate Holdings** endpoint, the system:

1. Fetches all transactions for the portfolio
2. For each instrument, retrieves applicable corporate actions
3. **Adjusts each transaction** based on corporate actions that occurred AFTER the transaction date
4. Calculates holdings using the adjusted quantities and prices

### 3. Adjustment Formula

For **Splits and Bonuses**:
```
Multiplier = RatioNumerator / RatioDenominator

Adjusted Quantity = Original Quantity × Multiplier
Adjusted Price = Original Price ÷ Multiplier

Total Investment remains constant (Qty × Price stays the same)
```

## Example: Tata Steel 1:10 Split

### Scenario
- You bought **100 shares** of Tata Steel at **?500/share** on **January 1, 2022**
- Total Investment: **?50,000**
- On **July 27, 2022**, Tata Steel undergoes a **1:10 split**

### Corporate Action Payload
```json
{
  "type": "Split",
  "exDate": "2022-07-27T18:30:00.000Z",
  "parentInstrumentId": 42,
  "ratioNumerator": 10,
  "ratioDenominator": 1,
  "costPercentageAllocated": 0
}
```

### Result After Recalculation
When you recalculate holdings:

**Before Split (Original Transaction)**:
- Quantity: 100 shares
- Price: ?500/share
- Investment: ?50,000

**After Split (Adjusted for Display)**:
- Quantity: **1,000 shares** (100 × 10/1)
- Average Price: **?50/share** (?500 ÷ 10/1)
- Investment: **?50,000** (unchanged)

## API Endpoints

### Admin Endpoints (Require Admin Role)

#### Create Corporate Action
```http
POST /api/CorporateAction
Authorization: Bearer <admin-token>

{
  "type": "Split",
  "exDate": "2022-07-27T18:30:00.000Z",
  "parentInstrumentId": 42,
  "ratioNumerator": 10,
  "ratioDenominator": 1,
  "costPercentageAllocated": 0
}
```

#### Get All Corporate Actions
```http
GET /api/CorporateAction
Authorization: Bearer <admin-token>
```

#### Get Corporate Actions for an Instrument
```http
GET /api/CorporateAction/instrument/{instrumentId}
Authorization: Bearer <admin-token>
```

#### Update Corporate Action
```http
PUT /api/CorporateAction/{id}
Authorization: Bearer <admin-token>

{
  "type": "Split",
  "exDate": "2022-07-27T18:30:00.000Z",
  "parentInstrumentId": 42,
  "ratioNumerator": 10,
  "ratioDenominator": 1,
  "costPercentageAllocated": 0
}
```

#### Delete Corporate Action
```http
DELETE /api/CorporateAction/{id}
Authorization: Bearer <admin-token>
```

### User Endpoints

#### Recalculate Holdings (Applies Corporate Actions)
```http
PUT /api/portfolio/{name}/holdings/recalculate
Authorization: Bearer <user-token>
```

This endpoint will:
1. Fetch all transactions
2. Apply corporate action adjustments
3. Recalculate holdings with adjusted quantities and prices
4. Update portfolio invested value

## Corporate Action Types & Ratios

### 1. Stock Split (1:10)
**Meaning**: 1 old share becomes 10 new shares
```json
{
  "type": "Split",
  "ratioNumerator": 10,
  "ratioDenominator": 1,
  "childInstrumentId": null
}
```
- Quantity: Multiplied by 10
- Price: Divided by 10

### 2. Stock Split (1:2)
**Meaning**: 1 old share becomes 2 new shares
```json
{
  "type": "Split",
  "ratioNumerator": 2,
  "ratioDenominator": 1,
  "childInstrumentId": null
}
```

### 3. Bonus (1:1)
**Meaning**: Get 1 free share for every 1 share held
```json
{
  "type": "Bonus",
  "ratioNumerator": 1,
  "ratioDenominator": 1,
  "childInstrumentId": null
}
```
- Quantity: Multiplied by 2 (original + bonus)
- Price: Divided by 2

### 4. Bonus (2:1)
**Meaning**: Get 1 free share for every 2 shares held
```json
{
  "type": "Bonus",
  "ratioNumerator": 1,
  "ratioDenominator": 2,
  "childInstrumentId": null
}
```
- Quantity: Multiplied by 1.5 (original + 0.5 bonus)
- Price: Divided by 1.5

### 5. Merger
**Meaning**: Stock A merges into Stock B
```json
{
  "type": "Merger",
  "ratioNumerator": 1,
  "ratioDenominator": 1,
  "parentInstrumentId": 10,
  "childInstrumentId": 20
}
```
*(Future implementation - transactions need to be migrated to child instrument)*

### 6. Demerger
**Meaning**: Stock A spins off Stock B
```json
{
  "type": "Demerger",
  "ratioNumerator": 1,
  "ratioDenominator": 5,
  "parentInstrumentId": 10,
  "childInstrumentId": 30,
  "costPercentageAllocated": 20
}
```
*(Future implementation - creates new holdings in child instrument with cost allocation)*

## Important Notes

### Timing
- Corporate actions only affect transactions that occurred **before** the ex-date
- The system looks forward from each transaction and applies all subsequent corporate actions

### Original Data Preservation
- Original transaction data is **never modified** in the database
- Adjustments are applied **dynamically** during calculations
- You can always audit the original transaction history

### Workflow
1. **Admin** adds corporate action via API
2. **User** recalculates holdings (manually or automatically)
3. System applies adjustments and updates holdings table
4. Portfolio values reflect adjusted quantities and prices

### Multiple Corporate Actions
If an instrument has multiple corporate actions:
- They are applied in **chronological order** (oldest to newest)
- Each adjustment compounds on the previous one

**Example**: 
- Buy 100 shares on Jan 1, 2022 at ?1000
- Split 1:10 on Jul 27, 2022 ? 1000 shares at ?100
- Bonus 1:1 on Dec 1, 2022 ? 2000 shares at ?50

### Future Enhancements
- Automatic recalculation when corporate actions are added
- Support for mergers and demergers (instrument migration)
- Corporate action history tracking in transaction view
- Notification system for pending corporate actions

## Testing Your Implementation

1. **Create an instrument** (e.g., Tata Steel)
2. **Add transactions** before the split date
3. **Record the split** via Corporate Action API
4. **Recalculate holdings** 
5. **Verify** that quantities and prices are adjusted correctly

The holdings should show adjusted values while maintaining the same total investment amount.
