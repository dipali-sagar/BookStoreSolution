import React, { useState, useEffect } from 'react';
import AuthorForm from './AuthorForm';
import './Authors.css';

const Authors = () => {
  const [authors, setAuthors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editingAuthor, setEditingAuthor] = useState(null);
  const [formLoading, setFormLoading] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState(null);
  const [notification, setNotification] = useState(null);

  useEffect(() => {
    fetchAuthors();
  }, []);

  const fetchAuthors = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/Authors', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      if (!response.ok) {
        throw new Error(`Failed to fetch authors: ${response.status} ${response.statusText}`);
      }

      const data = await response.json();
      const authorsData = Array.isArray(data) ? data : (data.items || data.authors || []);
      
      setAuthors(authorsData);
      setError(null);
    } catch (err) {
      setError(`Failed to load authors: ${err.message}. Please try again later.`);
      console.error('Error fetching authors:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRetry = () => {
    fetchAuthors();
  };

  const getAuthHeaders = () => ({
    'Content-Type': 'application/json',
    'tenant-id': '226DC44F-5A83-4D39-901E-87F7A790197B'
  });

  const handleAddAuthor = () => {
    setEditingAuthor(null);
    setShowForm(true);
  };

  const handleEditAuthor = (author) => {
    setEditingAuthor(author);
    setShowForm(true);
  };

  const handleDeleteAuthor = (author) => {
    setDeleteConfirm(author);
  };

  const confirmDelete = async () => {
    if (!deleteConfirm) return;

    try {
      const authorId = deleteConfirm.id;
      const response = await fetch(`/api/Authors/${authorId}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
      });

      if (!response.ok) {
        throw new Error(`Failed to delete author: ${response.status} ${response.statusText}`);
      }

      setAuthors(prev => prev.filter(author => author.id !== authorId));
      setDeleteConfirm(null);
      showNotification('Author deleted successfully!', 'success');
    } catch (err) {
      showNotification(`Failed to delete author: ${err.message}`, 'error');
      console.error('Error deleting author:', err);
    }
  };

  const handleFormSubmit = async (formData) => {
    try {
      setFormLoading(true);

      if (editingAuthor) {
        // Update existing author
        const authorId = editingAuthor.id;
        const response = await fetch(`/api/Authors/${authorId}`, {
          method: 'PUT',
          headers: getAuthHeaders(),
          body: JSON.stringify({
            ...formData,
            id: authorId
          })
        });

        if (!response.ok) {
          throw new Error(`Failed to update author: ${response.status} ${response.statusText}`);
        }

        const updatedAuthor = await response.json();
        setAuthors(prev => prev.map(author => 
          author.id === authorId ? { ...author, ...updatedAuthor } : author
        ));
        showNotification('Author updated successfully!', 'success');
      } else {
        // Create new author
        const response = await fetch('/api/Authors', {
          method: 'POST',
          headers: getAuthHeaders(),
          body: JSON.stringify(formData)
        });

        if (!response.ok) {
          throw new Error(`Failed to create author: ${response.status} ${response.statusText}`);
        }

        const newAuthor = await response.json();
        setAuthors(prev => [newAuthor, ...prev]);
        showNotification('Author added successfully!', 'success');
      }

      setShowForm(false);
      setEditingAuthor(null);
    } catch (err) {
      showNotification(`Failed to save author: ${err.message}`, 'error');
      console.error('Error saving author:', err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingAuthor(null);
  };

  const showNotification = (message, type = 'success') => {
    setNotification({ message, type });
    setTimeout(() => setNotification(null), 3000);
  };

  if (loading) {
    return (
      <div className="authors-page">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading authors...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="authors-page">
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
    <div className="authors-page">
      {notification && (
        <div className={`notification ${notification.type}`}>
          {notification.message}
        </div>
      )}

      <div className="authors-container">
        <header className="authors-header">
          <div className="header-content">
            <h1>👥 Authors Collection</h1>
            <p>Manage your book authors</p>
          </div>
          <button 
            className="add-author-button"
            onClick={handleAddAuthor}
          >
            + Add Author
          </button>
        </header>

        <div className="authors-grid">
          {authors.length === 0 ? (
            <div className="no-authors">
              <div className="no-authors-icon">📚</div>
              <h3>No authors found</h3>
              <p>Get started by adding your first author</p>
            </div>
          ) : (
            authors.map((author) => (
              <div key={author.id} className="author-card">
                <div className="author-avatar">
                  {author.firstName && author.lastName ? (
                    <div className="avatar-initials">
                      {author.firstName[0]}{author.lastName[0]}
                    </div>
                  ) : (
                    <div className="avatar-placeholder">👤</div>
                  )}
                </div>
                <div className="author-info">
                  <h3 className="author-name">
                    {author.firstName} {author.lastName}
                  </h3>
                  {author.email && (
                    <p className="author-email">
                      📧 {author.email}
                    </p>
                  )}
                  {author.expertise && (
                    <p className="author-expertise">
                      🎯 {author.expertise}
                    </p>
                  )}
                  {author.biography && (
                    <p className="author-biography">
                      {author.biography.slice(0, 150)}...
                    </p>
                  )}
                  {author.websiteUrl && (
                    <a 
                      href={author.websiteUrl} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="author-website"
                    >
                      🔗 Website
                    </a>
                  )}
                  <div className="author-actions">
                    <button
                      className="action-button edit-button"
                      onClick={() => handleEditAuthor(author)}
                    >
                      ✏️ Edit
                    </button>
                    <button
                      className="action-button delete-button"
                      onClick={() => handleDeleteAuthor(author)}
                    >
                      🗑️ Delete
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {showForm && (
        <AuthorForm
          author={editingAuthor}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
          isLoading={formLoading}
        />
      )}

      {deleteConfirm && (
        <div className="delete-confirm-overlay">
          <div className="delete-confirm-container">
            <div className="delete-confirm-header">
              <h2>Confirm Delete</h2>
              <button 
                className="close-button" 
                onClick={() => setDeleteConfirm(null)}
              >
                ×
              </button>
            </div>
            <div className="delete-confirm-content">
              <p>Are you sure you want to delete <strong>{deleteConfirm.firstName} {deleteConfirm.lastName}</strong>?</p>
              <p className="warning-text">This action cannot be undone.</p>
              <div className="delete-confirm-actions">
                <button 
                  className="cancel-button"
                  onClick={() => setDeleteConfirm(null)}
                >
                  Cancel
                </button>
                <button 
                  className="confirm-delete-button"
                  onClick={confirmDelete}
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Authors;