# 🚀 How to Access Swagger UI

## ✅ API is Starting...

The backend API is now running. Here's how to access Swagger:

## 🌐 Swagger UI URLs

### Option 1: HTTP (Recommended)
```
http://localhost:5206/swagger
```

### Option 2: HTTPS
```
https://localhost:7033/swagger
```

## 📝 Quick Start Guide

1. **Open your browser** (Chrome, Edge, Firefox, etc.)

2. **Navigate to**: `http://localhost:5206/swagger`

3. **You should see**:
   - List of all API endpoints
   - Interactive API documentation
   - Try it out buttons for each endpoint

## 🔐 Testing Authentication

1. **First, login to get a token**:
   - Find `POST /api/User/login` endpoint
   - Click on it to expand
   - Click "Try it out" button
   - Enter this in the request body:
     ```json
     {
       "EmailOrUserName": "admin@lms.com",
       "Password": "3lemny_"
     }
     ```
   - Click "Execute"
   - Copy the `accessToken` from the response

2. **Authorize in Swagger**:
   - Click the green "Authorize" button at the top right
   - In the "Value" field, enter: `Bearer {your-token-here}`
     (Replace `{your-token-here}` with the actual token)
   - Click "Authorize"
   - Click "Close"

3. **Now you can test protected endpoints** that require authentication!

## 📋 Available Endpoints

You'll see all these controllers in Swagger:
- **UserController** - Login, Register, User Management
- **CoursesController** - Course CRUD operations
- **EnrollmentController** - Student/Instructor enrollment
- **AssignmentController** - Assignments and grading
- **QuizzesController** - Quiz management
- **QuestionsController** - Question management
- **NotificationsController** - Notifications
- **DashboardController** - Admin/Instructor/Student dashboards
- **CertificatesController** - Certificate generation
- **CourseReviewsController** - Course reviews
- And more...

## 🎯 Test Public Endpoints First

These don't require authentication:
- `GET /api/Courses/all` - View all courses
- `GET /api/Courses/{id}` - View course details
- `GET /api/Quizzes/all` - View all quizzes

## ⚡ If Swagger Doesn't Load

1. **Wait a few seconds** - The API might still be starting
2. **Check the console window** - Look for any error messages
3. **Try both URLs**:
   - `http://localhost:5206/swagger`
   - `https://localhost:7033/swagger`
4. **Check if API is running**:
   - Open: `http://localhost:5206/api/Courses/all`
   - Should return JSON data

## 🔍 Troubleshooting

### "This site can't be reached"
- Make sure the API is running
- Check if port 5206 is not blocked by firewall

### "Connection refused"
- The API might still be starting
- Wait 10-15 seconds and try again

### "Swagger UI not found"
- Make sure you're in Development mode
- Check `Program.cs` - Swagger should be enabled for Development

---

**Happy Testing! 🎉**
