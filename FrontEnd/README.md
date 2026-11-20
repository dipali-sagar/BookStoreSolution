# React Login Application with Book Management

A comprehensive React.js application with login/logout functionality, form validation, and full CRUD operations for book management integrated with a local .NET Web API.

## Features

- ✅ User authentication (login/logout)
- ✅ Email validation (format check)
- ✅ Password validation (minimum 6 characters)
- ✅ Session persistence (localStorage)
- ✅ Responsive design
- ✅ Modern gradient UI
- ✅ Loading states
- ✅ Error handling and feedback
- ✅ Navigation between Dashboard and Books pages
- ✅ Integration with local .NET Web API
- ✅ Dynamic book collection display
- ✅ **Full CRUD operations** for books:
  - ➕ **Create** new books with detailed form
  - ✏️ **Update** existing book information
  - 🗑️ **Delete** books with confirmation
  - 📖 **Read** and display book collection
- ✅ Form validation for book data
- ✅ Real-time notifications for operations
- ✅ Confirmation dialogs for destructive actions
- ✅ Empty state handling

## Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

The application will open automatically at `http://localhost:3000`

## Usage

### Login
1. Enter a valid email address (e.g., `user@example.com`)
2. Enter a password (minimum 6 characters)
3. Click "Login"

### Navigation
1. After login, use the navigation tabs to switch between Dashboard and Books pages
2. Click "Books" to view the book collection from your local API
3. Click "Dashboard" to return to the welcome screen

### Book Management (CRUD Operations)

#### Add a New Book
1. Navigate to the Books page
2. Click the "+ Add Book" button in the header
3. Fill in the book details:
   - Title (required)
   - Author (required)
   - ISBN (optional)
   - Published Date (optional)
   - Genre (optional)
   - Cover Image URL (optional)
   - Description (optional, max 500 characters)
4. Click "Add Book" to save

#### Edit a Book
1. Find the book you want to edit in the collection
2. Click the "✏️ Edit" button on the book card
3. Modify the desired fields
4. Click "Update Book" to save changes

#### Delete a Book
1. Find the book you want to delete in the collection
2. Click the "🗑️ Delete" button on the book card
3. Review the confirmation dialog
4. Click "Delete Book" to confirm or "Cancel" to abort

### Logout
1. Click the "Logout" button in the header
2. You'll be redirected back to the login page

## API Integration

The application integrates with a local .NET Web API with full CRUD operations:

### Endpoints

#### Get All Books
- **Endpoint**: `/api/Books`
- **Method**: GET
- **Headers**: 
  - `Content-Type: application/json`
  - `tenant-id: 226DC44F-5A83-4D39-901E-87F7A790197B`

#### Create New Book
- **Endpoint**: `/api/Books`
- **Method**: POST
- **Headers**: 
  - `Content-Type: application/json`
  - `tenant-id: 226DC44F-5A83-4D39-901E-87F7A790197B`
- **Body**: Book object with title, author, and optional fields

#### Update Book
- **Endpoint**: `/api/Books/{id}`
- **Method**: PUT
- **Headers**: 
  - `Content-Type: application/json`
  - `tenant-id: 226DC44F-5A83-4D39-901E-87F7A790197B`
- **Body**: Updated book object

#### Delete Book
- **Endpoint**: `/api/Books/{id}`
- **Method**: DELETE
- **Headers**: 
  - `Content-Type: application/json`
  - `tenant-id: 226DC44F-5A83-4D39-901E-87F7A790197B`

### Configuration
- **Proxy**: Vite development server proxies API requests to `http://localhost:5087`
- **Format**: Handles various JSON response formats from .NET APIs
- **CORS**: Proxy configuration handles cross-origin requests

**Note**: Make sure your .NET Web API is running on `http://localhost:5087` before accessing the Books page.

## Project Structure

```
├── index.html          # HTML entry point
├── package.json        # Project dependencies
├── vite.config.js      # Vite configuration with API proxy
├── README.md           # Documentation
└── src/
    ├── App.jsx         # Main app component with auth state and navigation
    ├── App.css         # App styles including navigation
    ├── Login.jsx       # Login form component
    ├── Login.css       # Login form styles
    ├── Books.jsx       # Books component with CRUD operations
    ├── Books.css       # Books page styles with CRUD UI
    ├── BookForm.jsx    # Book form component for add/edit
    ├── BookForm.css    # Book form styles
    └── main.jsx        # React entry point
```

## Technologies Used

- **React 18** - UI library with hooks
- **Vite** - Build tool and dev server with API proxy
- **LocalStorage** - Session persistence
- **Fetch API** - HTTP requests to .NET Web API
- **CSS3** - Modern styling with animations and responsive design

## Build for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

## Preview Production Build

```bash
npm run preview
```

## Notes

- This application integrates with a local .NET Web API for book data with full CRUD operations
- User session is stored in localStorage
- API requests are proxied through Vite dev server to avoid CORS issues
- All CRUD operations include proper error handling and user feedback
- Form validation ensures data quality before API calls
- Confirmation dialogs prevent accidental data loss
- For production, implement proper authentication with a backend API
- Add HTTPS and secure cookie handling for real-world applications
- Ensure your .NET API is running before accessing the Books page
- The .NET API should support the following endpoints: GET, POST, PUT, DELETE for /api/Books