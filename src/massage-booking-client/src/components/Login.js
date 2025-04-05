import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
// import { useNavigate } from 'react-router-dom'; // Uncomment if using react-router for redirection

function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const { login, authError } = useAuth();
    // const navigate = useNavigate(); // Uncomment for redirection

    const handleSubmit = async (event) => {
        event.preventDefault();
        setLoading(true);
        const success = await login(email, password);
        setLoading(false);
        if (success) {
            console.log('Login successful!');
            // Redirect after successful login (e.g., to dashboard)
            // navigate('/dashboard'); // Uncomment for redirection
        } else {
            // Error message is handled by authError state from context
            console.log('Login failed.');
        }
    };

    return (
        <div className="card login-form">
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="email">Email:</label>
                    <input 
                        type="email" 
                        id="email" 
                        value={email} 
                        onChange={(e) => setEmail(e.target.value)} 
                        required 
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
                    />
                </div>
                {authError && <p className="error-message">{authError}</p>}
                <button type="submit" disabled={loading}>
                    {loading ? 'Logging in...' : 'Login'}
                </button>
            </form>
        </div>
    );
}

export default Login; 