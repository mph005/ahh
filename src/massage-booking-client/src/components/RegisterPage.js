import React, { useState, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { AuthContext } from '../context/AuthContext'; // Maybe needed if we auto-login

const RegisterPage = () => {
    const [email, setEmail] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [phone, setPhone] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    // const { login } = useContext(AuthContext); // Uncomment if auto-login is desired

    const handleSubmit = async (event) => {
        event.preventDefault();
        setError(null);

        if (password !== confirmPassword) {
            setError("Passwords do not match.");
            return;
        }

        setLoading(true);
        try {
            // Use axios directly for better debugging
            const response = await axios.post('/api/auth/register', {
                email,
                firstName,
                lastName,
                phone,
                dateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : null,
                password,
                confirmPassword
            }, {
                withCredentials: true
            });

            console.log('Registration successful:', response);
            
            // For now, redirect to login page after successful registration
            alert('Registration successful! Please log in.');
            navigate('/login'); 

        } catch (err) {
            console.error("Registration failed:", err);
            
            // Log the full error for debugging
            if (err.response) {
                console.log('Error Status:', err.response.status);
                console.log('Error Headers:', err.response.headers);
                console.log('Error Data:', err.response.data);
            } else if (err.request) {
                console.log('Error Request:', err.request);
            } else {
                console.log('Error Message:', err.message);
            }
            
            let errorMessage = "Registration failed. Please try again.";
            
            if (err.response && err.response.data) {
                if (err.response.data.message) {
                    errorMessage = err.response.data.message;
                } else if (err.response.data.Message) {
                    errorMessage = err.response.data.Message;
                } else if (typeof err.response.data === 'string') {
                    errorMessage = err.response.data;
                }
                
                // Check for validation errors
                if (err.response.data.errors) {
                    errorMessage = Object.values(err.response.data.errors).flat().join('\n');
                } else if (err.response.data.Errors) {
                    errorMessage = Array.isArray(err.response.data.Errors) 
                        ? err.response.data.Errors.join('\n') 
                        : err.response.data.Errors;
                }
            }
            
            setError(errorMessage);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="register-container">
            <h2>Register New Account</h2>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="email">Email:</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        disabled={loading}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="firstName">First Name:</label>
                    <input
                        type="text"
                        id="firstName"
                        value={firstName}
                        onChange={(e) => setFirstName(e.target.value)}
                        required
                        disabled={loading}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="lastName">Last Name:</label>
                    <input
                        type="text"
                        id="lastName"
                        value={lastName}
                        onChange={(e) => setLastName(e.target.value)}
                        required
                        disabled={loading}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="phone">Phone Number:</label>
                    <input
                        type="tel"
                        id="phone"
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        disabled={loading}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="dateOfBirth">Date of Birth:</label>
                    <input
                        type="date"
                        id="dateOfBirth"
                        value={dateOfBirth}
                        onChange={(e) => setDateOfBirth(e.target.value)}
                        disabled={loading}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="password">Password:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        minLength="8" // Match backend requirement
                        disabled={loading}
                    />
                    <small style={{ display: 'block', marginTop: '5px', color: '#666' }}>
                        Password must be at least 8 characters long and include uppercase letters and numbers.
                    </small>
                </div>
                <div className="form-group">
                    <label htmlFor="confirmPassword">Confirm Password:</label>
                    <input
                        type="password"
                        id="confirmPassword"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                        disabled={loading}
                    />
                </div>
                {error && (
                    <div className="error-message" style={{ color: 'red' }}>
                        <p>Registration failed with the following errors:</p>
                        <pre>{error}</pre>
                    </div>
                )}
                <button type="submit" disabled={loading}>
                    {loading ? 'Registering...' : 'Register'}
                </button>
            </form>
        </div>
    );
};

export default RegisterPage; 