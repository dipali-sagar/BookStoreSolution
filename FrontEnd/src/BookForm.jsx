import React, { useState, useEffect } from 'react';
import './BookForm.css';

const BookForm = ({ book, onSubmit, onCancel, isLoading }) => {
  const [formData, setFormData] = useState({
    title: '',
    author: '',
    description: '',
    publishedDate: '',
    coverUrl: '',
    isbn: '',
    genre: ''
  });
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (book) {
      setFormData({
        title: book.title || book.volumeInfo?.title || '',
        author: book.author || book.volumeInfo?.authors?.[0] || '',
        description: book.description || book.volumeInfo?.description || '',
        publishedDate: book.publishedDate || book.publishDate || book.volumeInfo?.publishedDate || '',
        coverUrl: book.coverUrl || book.imageUrl || book.volumeInfo?.imageLinks?.thumbnail || '',
        isbn: book.isbn || '',
        genre: book.genre || ''
      });
    }
  }, [book]);

  const validateForm = () => {
    const newErrors = {};
    let isValid = true;

    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
      isValid = false;
    }

    if (!formData.author.trim()) {
      newErrors.author = 'Author is required';
      isValid = false;
    }

    if (formData.description && formData.description.length > 500) {
      newErrors.description = 'Description must be less than 500 characters';
      isValid = false;
    }

    if (formData.isbn && !/^[0-9X-]{10,17}$/.test(formData.isbn)) {
      newErrors.isbn = 'Invalid ISBN format';
      isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    onSubmit(formData);
  };

  return (
    <div className="book-form-overlay">
      <div className="book-form-container">
        <div className="book-form-header">
          <h2>{book ? 'Edit Book' : 'Add New Book'}</h2>
          <button 
            className="close-button" 
            onClick={onCancel}
            disabled={isLoading}
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="book-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="title">Title *</label>
              <input
                type="text"
                id="title"
                name="title"
                value={formData.title}
                onChange={handleChange}
                className={errors.title ? 'error' : ''}
                placeholder="Enter book title"
                disabled={isLoading}
                required
              />
              {errors.title && <span className="error-message">{errors.title}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="author">Author *</label>
              <input
                type="text"
                id="author"
                name="author"
                value={formData.author}
                onChange={handleChange}
                className={errors.author ? 'error' : ''}
                placeholder="Enter author name"
                disabled={isLoading}
                required
              />
              {errors.author && <span className="error-message">{errors.author}</span>}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="isbn">ISBN</label>
              <input
                type="text"
                id="isbn"
                name="isbn"
                value={formData.isbn}
                onChange={handleChange}
                className={errors.isbn ? 'error' : ''}
                placeholder="Enter ISBN (optional)"
                disabled={isLoading}
              />
              {errors.isbn && <span className="error-message">{errors.isbn}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="publishedDate">Published Date</label>
              <input
                type="date"
                id="publishedDate"
                name="publishedDate"
                value={formData.publishedDate}
                onChange={handleChange}
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="genre">Genre</label>
            <select
              id="genre"
              name="genre"
              value={formData.genre}
              onChange={handleChange}
              disabled={isLoading}
            >
              <option value="">Select genre (optional)</option>
              <option value="Fiction">Fiction</option>
              <option value="Non-Fiction">Non-Fiction</option>
              <option value="Science Fiction">Science Fiction</option>
              <option value="Fantasy">Fantasy</option>
              <option value="Mystery">Mystery</option>
              <option value="Thriller">Thriller</option>
              <option value="Romance">Romance</option>
              <option value="Biography">Biography</option>
              <option value="Self-Help">Self-Help</option>
              <option value="Technical">Technical</option>
              <option value="Programming">Programming</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="coverUrl">Cover Image URL</label>
            <input
              type="url"
              id="coverUrl"
              name="coverUrl"
              value={formData.coverUrl}
              onChange={handleChange}
              placeholder="Enter image URL (optional)"
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              className={errors.description ? 'error' : ''}
              placeholder="Enter book description (optional)"
              rows="4"
              disabled={isLoading}
              maxLength="500"
            />
            {errors.description && <span className="error-message">{errors.description}</span>}
            <span className="character-count">
              {formData.description.length}/500 characters
            </span>
          </div>

          <div className="form-actions">
            <button
              type="button"
              className="cancel-button"
              onClick={onCancel}
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="submit-button"
              disabled={isLoading}
            >
              {isLoading ? 'Saving...' : (book ? 'Update Book' : 'Add Book')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default BookForm;