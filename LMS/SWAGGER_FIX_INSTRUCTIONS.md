# Swagger Internal Server Error - Troubleshooting Guide

## Current Issue
Swagger UI loads but `/swagger/v1/swagger.json` returns a 500 Internal Server Error.

## Possible Causes

1. **Circular References** in DTOs
2. **Complex Generic Types** that Swagger can't serialize
3. **Missing or Invalid Controller Attributes**
4. **Problematic DTO Properties**

## What I've Fixed

1. ✅ Added custom schema ID generation to handle generic types
2. ✅ Added proper using statements
3. ✅ Configured inline enum definitions
4. ✅ Added security definition for JWT

## Next Steps to Debug

### Option 1: Check API Logs
Look at the console output where the API is running to see the actual error message.

### Option 2: Test Individual Endpoints
The API itself is working (we tested `/api/Courses/all`). The issue is only with Swagger documentation generation.

### Option 3: Temporarily Disable Problematic Controllers
If needed, we can temporarily comment out controllers to identify which one is causing the issue.

## Workaround

Since the API is working correctly, you can:
1. Use Postman or another API client
2. Test endpoints directly via HTTP requests
3. Check the API_TESTING_CHECKLIST.md for endpoint documentation

## To Fix Swagger Completely

We need to identify the specific controller or DTO causing the 500 error. Check the API console logs when accessing `/swagger/v1/swagger.json` to see the actual exception.

