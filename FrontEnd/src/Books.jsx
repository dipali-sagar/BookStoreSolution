import React, { useState, useEffect } from 'react';
import BookForm from './BookForm';
import './Books.css';

const Books = () => {
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editingBook, setEditingBook] = useState(null);
  const [formLoading, setFormLoading] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState(null);
  const [notification, setNotification] = useState(null);

  useEffect(() => {
    fetchBooks();
  }, []);

  const fetchBooks = async () => {
    try {
      setLoading(true);
      // Calling local .NET Web API via Vite proxy with tenant-id in headers
      const response = await fetch('/api/Books', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'tenant-id': '226DC44F-5A83-4D39-901E-87F7A790197B'
        }
      });
      
      if (!response.ok) {
        throw new Error(`Failed to fetch books: ${response.status} ${response.statusText}`);
      }

      const data = await response.json();
      
      // Handle different response formats from .NET API
      const booksData = Array.isArray(data) ? data : (data.items || data.books || []);
      
      setBooks(booksData);
      setError(null);
    } catch (err) {
      setError(`Failed to load books: ${err.message}. Please try again later.`);
      console.error('Error fetching books:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRetry = () => {
    fetchBooks();
  };

  const getAuthHeaders = () => ({
    'Content-Type': 'application/json',
    'tenant-id': '226DC44F-5A83-4D39-901E-87F7A790197B'
  });

  const handleAddBook = () => {
    setEditingBook(null);
    setShowForm(true);
  };

  const handleEditBook = (book) => {
    setEditingBook(book);
    setShowForm(true);
  };

  const handleDeleteBook = (book) => {
    setDeleteConfirm(book);
  };

  const confirmDelete = async () => {
    if (!deleteConfirm) return;

    try {
      const bookId = deleteConfirm.id || deleteConfirm.bookId;
      const response = await fetch(`/api/Books/${bookId}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
      });

      if (!response.ok) {
        throw new Error(`Failed to delete book: ${response.status} ${response.statusText}`);
      }

      setBooks(prev => prev.filter(book => 
        (book.id || book.bookId) !== bookId
      ));
      setDeleteConfirm(null);
      showNotification('Book deleted successfully!', 'success');
    } catch (err) {
      showNotification(`Failed to delete book: ${err.message}`, 'error');
      console.error('Error deleting book:', err);
    }
  };

  const handleFormSubmit = async (formData) => {
    try {
      setFormLoading(true);

      if (editingBook) {
        // Update existing book
        const bookId = editingBook.id || editingBook.bookId;
        const response = await fetch(`/api/Books/${bookId}`, {
          method: 'PUT',
          headers: getAuthHeaders(),
          body: JSON.stringify({
            ...formData,
            id: bookId
          })
        });

        if (!response.ok) {
          throw new Error(`Failed to update book: ${response.status} ${response.statusText}`);
        }

        const updatedBook = await response.json();
        setBooks(prev => prev.map(book => 
          (book.id || book.bookId) === bookId ? { ...book, ...updatedBook } : book
        ));
        showNotification('Book updated successfully!', 'success');
      } else {
        // Create new book
        const response = await fetch('/api/Books', {
          method: 'POST',
          headers: getAuthHeaders(),
          body: JSON.stringify(formData)
        });

        if (!response.ok) {
          throw new Error(`Failed to create book: ${response.status} ${response.statusText}`);
        }

        const newBook = await response.json();
        setBooks(prev => [newBook, ...prev]);
        showNotification('Book added successfully!', 'success');
      }

      setShowForm(false);
      setEditingBook(null);
    } catch (err) {
      showNotification(`Failed to save book: ${err.message}`, 'error');
      console.error('Error saving book:', err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingBook(null);
  };

  const showNotification = (message, type = 'success') => {
    setNotification({ message, type });
    setTimeout(() => setNotification(null), 3000);
  };

  if (loading) {
    return (
      <div className="books-page">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading books...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="books-page">
        <div className="error-container">
          <div className="error-icon">⚠️</div>
          <h2>Oops! Something went wrong</h2>
          <p>{error}</p>
          <button onClick={handleRetry} className="retry-button">
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="books-page">
      {notification && (
        <div className={`notification ${notification.type}`}>
          {notification.message}
        </div>
      )}

      <div className="books-container">
        <header className="books-header">
          <div className="header-content">
            <h1>📚 Books Collection</h1>
            <p>Books from your local library API</p>
          </div>
          <button 
            className="add-book-button"
            onClick={handleAddBook}
          >
            + Add Book
          </button>
        </header>

        <div className="books-grid">
          {books.map((book) => {
            // Handle different API response formats
            const bookData = book.volumeInfo || book;
            const bookId = book.id || book.bookId || Math.random().toString(36).substr(2, 9);
            
            return (
              <div key={bookId} className="book-card">
                <div className="book-cover">
                  {bookData.imageLinks?.thumbnail || bookData.coverUrl || bookData.imageUrl ? (
                    <img
                      src={bookData.imageLinks?.thumbnail || bookData.coverUrl || bookData.imageUrl}
                      alt={bookData.title || 'Book cover'}
                      loading="lazy"
                    />
                  ) : (
                    <div className="no-cover">📖</div>
                  )}
                </div>
                <div className="book-info">
                  <h3 className="book-title">
                    {bookData.title || 'Untitled Book'}
                  </h3>
                  <p className="book-authors">
                    By: {Array.isArray(bookData.authors) 
                      ? bookData.authors.join(', ') 
                      : (bookData.author || bookData.authors || 'Unknown Author')}
                  </p>
                  <p className="book-description">
                    {bookData.description
                      ? bookData.description.slice(0, 150) + '...'
                      : (bookData.summary || 'No description available')}
                  </p>
                  {(bookData.publishedDate || bookData.publishDate || bookData.year) && (
                    <p className="book-date">
                      Published: {bookData.publishedDate || bookData.publishDate || bookData.year}
                    </p>
                  )}
                  <div className="book-actions">
                    <button
                      className="action-button edit-button"
                      onClick={() => handleEditBook(book)}
                      title="Edit book"
                    >
                      ✏️ Edit
                    </button>
                    <button
                      className="action-button delete-button"
                      onClick={() => handleDeleteBook(book)}
                      title="Delete book"
                    >
                      🗑️ Delete
                    </button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {books.length === 0 && !loading && !error && (
          <div className="empty-state">
            <div className="empty-icon">📚</div>
            <h2>No books found</h2>
            <p>Start by adding your first book to the collection!</p>
            <button 
              className="add-book-button"
              onClick={handleAddBook}
            >
              + Add Your First Book
            </button>
          </div>
        )}
      </div>

      {showForm && (
        <BookForm
          book={editingBook}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
          isLoading={formLoading}
        />
      )}

      {deleteConfirm && (
        <div className="delete-confirm-overlay">
          <div className="delete-confirm-dialog">
            <div className="delete-confirm-header">
              <h2>⚠️ Confirm Delete</h2>
              <button 
                className="close-button" 
                onClick={() => setDeleteConfirm(null)}
              >
                ×
              </button>
            </div>
            <div className="delete-confirm-content">
              <p>Are you sure you want to delete <strong>"{deleteConfirm.title || deleteConfirm.volumeInfo?.title || 'this book'}"</strong>?</p>
              <p className="warning-text">This action cannot be undone.</p>
            </div>
            <div className="delete-confirm-actions">
              <button 
                className="cancel-button"
                onClick={() => setDeleteConfirm(null)}
              >
                Cancel
              </button>
              <button 
                className="delete-button"
                onClick={confirmDelete}
              >
                Delete Book
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Books;