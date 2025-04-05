import React, { createContext, useState, useContext, useEffect } from 'react';
import apiClient from '../api/apiClient'; // Assuming apiClient handles API calls

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    // Initialize state: User is null until checked
    const [user, setUser] = useState(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isLoading, setIsLoading] = useState(true); // Start loading until initial check is done
    const [authError, setAuthError] = useState(null);

    // Check authentication status on initial load
    useEffect(() => {
        const checkAuthStatus = async () => {
            setIsLoading(true);
            setAuthError(null);
            try {
                // Use the new getUserInfo endpoint
                const userInfo = await apiClient.getUserInfo();
                // If getUserInfo succeeds, the user is authenticated via cookie
                setUser(userInfo); // Store user details (id, email, roles etc.)
                setIsAuthenticated(true);
                console.log('User authenticated from session:', userInfo);
            } catch (error) {
                // If getUserInfo fails (e.g., 401), the user is not authenticated
                setUser(null);
                setIsAuthenticated(false);
                if (error.response && error.response.status !== 401) {
                    // Only set error if it's not a standard 401 (which just means not logged in)
                    console.error('Error checking auth status:', error);
                    setAuthError('Failed to check authentication status.');
                } else {
                    console.log('No active user session found.');
                }
            }
            setIsLoading(false);
        };

        checkAuthStatus();
    }, []); // Empty dependency array ensures this runs only once on mount

    const login = async (email, password) => {
        setAuthError(null);
        setIsLoading(true);
        try {
            const response = await apiClient.login(email, password);

            if (response && response.success) {
                // Login successful, API returns user info
                const loggedInUser = {
                    userId: response.userId,
                    email: response.email,
                    roles: response.roles
                };
                setUser(loggedInUser); // Update state with user info from login response
                setIsAuthenticated(true);
                setIsLoading(false);
                console.log('Login successful, user set:', loggedInUser);
                return true; // Indicate success
            } else {
                // This path might not be reached if API throws error for failure
                const errorMessage = response?.message || 'Login failed. Please check credentials.';
                setAuthError(errorMessage);
                setUser(null);
                setIsAuthenticated(false);
                setIsLoading(false);
                console.warn('Login API call did not indicate success:', response);
                return false; // Indicate failure
            }
        } catch (error) {
            console.error("Login error:", error);
            const errorMessage = error.response?.data?.message || error.message || 'An unexpected error occurred during login.';
            setAuthError(errorMessage);
            setUser(null);
            setIsAuthenticated(false);
            setIsLoading(false);
            return false; // Indicate failure
        }
    };

    const logout = async () => {
        setAuthError(null);
        try {
            await apiClient.logout(); // Call the backend logout endpoint
        } catch (error) {
            console.error("Logout API call failed:", error);
            // Still clear frontend state even if API call fails
        }
        setUser(null);
        setIsAuthenticated(false);
        console.log('User logged out.');
        // No need to remove localStorage items anymore
    };

    // No need for getToken anymore

    const value = {
        user,
        isAuthenticated,
        isLoading, // Expose loading state for UI
        authError,
        login,
        logout,
    };

    // Render children only after initial auth check is complete
    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

// Custom hook to use the auth context
export const useAuth = () => {
    return useContext(AuthContext);
}; 