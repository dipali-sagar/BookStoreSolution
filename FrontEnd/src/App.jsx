import React, { useState, useEffect } from 'react';
import Login from './Login';
import Books from './Books';
import Authors from './Authors';
import './App.css';

const App = () => {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState('dashboard');

  // Check for existing session on mount
  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  const handleLogin = (email) => {
    const userData = { email, loginTime: new Date().toISOString() };
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
    setCurrentPage('dashboard');
  };

  const handleLogout = () => {
    setUser(null);
    localStorage.removeItem('user');
    setCurrentPage('dashboard');
  };

  const navigateTo = (page) => {
    setCurrentPage(page);
  };

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
      </div>
    );
  }

  return (
    <div className="app">
      {user ? (
        <div className="dashboard">
          <header className="dashboard-header">
            <h1>Welcome!</h1>
            <nav className="nav-menu">
              <button
                className={`nav-button ${currentPage === 'dashboard' ? 'active' : ''}`}
                onClick={() => navigateTo('dashboard')}
              >
                Dashboard
              </button>
              <button
                className={`nav-button ${currentPage === 'books' ? 'active' : ''}`}
                onClick={() => navigateTo('books')}
              >
                Books
              </button>
              <button
                className={`nav-button ${currentPage === 'authors' ? 'active' : ''}`}
                onClick={() => navigateTo('authors')}
              >
                Authors
              </button>
            </nav>
            <div className="user-info">
              <span className="user-email">{user.email}</span>
              <button onClick={handleLogout} className="logout-button">
                Logout
              </button>
            </div>
          </header>
          <main className="dashboard-content">
            {currentPage === 'dashboard' ? (
              <div className="welcome-card">
                <h2>👋 Successfully Logged In</h2>
                <p>You are now authenticated and can access the dashboard.</p>
                <div className="login-time">
                  <small>Logged in at: {new Date(user.loginTime).toLocaleString()}</small>
                </div>
              </div>
            ) : currentPage === 'books' ? (
              <Books />
            ) : (
              <Authors />
            )}
          </main>
        </div>
      ) : (
        <Login onLogin={handleLogin} />
      )}
    </div>
  );
};

export default App;