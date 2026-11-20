import React, { useState, useEffect } from 'react';
import './AuthorForm.css';

const AuthorForm = ({ author, onSubmit, onCancel, isLoading }) => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    expertise: '',
    biography: '',
    websiteUrl: ''
  });
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (author) {
      setFormData({
        firstName: author.firstName || '',
        lastName: author.lastName || '',
        email: author.email || '',
        expertise: author.expertise || '',
        biography: author.biography || '',
        websiteUrl: author.websiteUrl || ''
      });
    }
  }, [author]);

  const validateForm = () => {
    const newErrors = {};
    let isValid = true;

    if (!formData.firstName.trim()) {
      newErrors.firstName = 'First name is required';
      isValid = false;
    }

    if (!formData.lastName.trim()) {
      newErrors.lastName = 'Last name is required';
      isValid = false;
    }

    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
      isValid = false;
    }

    if (formData.websiteUrl && !/^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/.test(formData.websiteUrl)) {
      newErrors.websiteUrl = 'Invalid website URL format';
      isValid = false;
    }

    if (formData.biography && formData.biography.length > 1000) {
      newErrors.biography = 'Biography must be less than 1000 characters';
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
    <div className="author-form-overlay">
      <div className="author-form-container">
        <div className="author-form-header">
          <h2>{author ? 'Edit Author' : 'Add New Author'}</h2>
          <button 
            className="close-button" 
            onClick={onCancel}
            disabled={isLoading}
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="author-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">First Name *</label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                className={errors.firstName ? 'error' : ''}
                placeholder="Enter first name"
                disabled={isLoading}
                required
              />
              {errors.firstName && <span className="error-message">{errors.firstName}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Last Name *</label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                className={errors.lastName ? 'error' : ''}
                placeholder="Enter last name"
                disabled={isLoading}
                required
              />
              {errors.lastName && <span className="error-message">{errors.lastName}</span>}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="email">Email</label>
              <input
                type="email"
                id="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                className={errors.email ? 'error' : ''}
                placeholder="Enter email address"
                disabled={isLoading}
              />
              {errors.email && <span className="error-message">{errors.email}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="expertise">Expertise</label>
              <input
                type="text"
                id="expertise"
                name="expertise"
                value={formData.expertise}
                onChange={handleChange}
                placeholder="Enter area of expertise"
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="form-group full-width">
            <label htmlFor="websiteUrl">Website URL</label>
            <input
              type="url"
              id="websiteUrl"
              name="websiteUrl"
              value={formData.websiteUrl}
              onChange={handleChange}
              className={errors.websiteUrl ? 'error' : ''}
              placeholder="https://example.com"
              disabled={isLoading}
            />
            {errors.websiteUrl && <span className="error-message">{errors.websiteUrl}</span>}
          </div>

          <div className="form-group full-width">
            <label htmlFor="biography">Biography</label>
            <textarea
              id="biography"
              name="biography"
              value={formData.biography}
              onChange={handleChange}
              className={errors.biography ? 'error' : ''}
              placeholder="Enter author biography"
              rows="4"
              disabled={isLoading}
              maxLength="1000"
            />
            {errors.biography && <span className="error-message">{errors.biography}</span>}
            <span className="character-count">
              {formData.biography.length}/1000 characters
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
              {isLoading ? 'Saving...' : (author ? 'Update Author' : 'Add Author')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default AuthorForm;