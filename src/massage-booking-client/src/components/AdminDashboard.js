import React, { useState, useEffect } from 'react';
import apiClient from '../api/apiClient'; // Import apiClient
// import { useAuth } from '../context/AuthContext'; // Might need for role checks

function AdminDashboard() {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    // const { user } = useAuth(); // Example if needed

    useEffect(() => {
        const fetchStats = async () => {
            setLoading(true);
            setError(null);
            try {
                // Call the actual API client method
                const fetchedStats = await apiClient.getAdminDashboardStats();
                setStats(fetchedStats);
                
            } catch (err) {
                console.error("Error fetching admin stats:", err);
                setError(err.response?.data?.message || err.message || 'Failed to load dashboard stats.');
            } finally {
                setLoading(false);
            }
        };

        fetchStats();
    }, []); // Run once on component mount

    if (loading) {
        return <div className="loading">Loading Admin Dashboard...</div>;
    }

    if (error) {
        return <div className="error">Error: {error}</div>;
    }

    return (
        <div className="card admin-dashboard">
            <h2>Admin Dashboard</h2>
            {stats ? (
                <div className="stats-grid"> 
                    <div className="stat-item">
                        <strong>Total Clients:</strong> {stats.totalClients}
                    </div>
                    <div className="stat-item">
                        <strong>Active Clients:</strong> {stats.activeClients}
                    </div>
                     <div className="stat-item">
                        <strong>Total Therapists:</strong> {stats.totalTherapists}
                    </div>
                    <div className="stat-item">
                        <strong>Active Therapists:</strong> {stats.activeTherapists}
                    </div>
                     <div className="stat-item">
                        <strong>Upcoming Appointments:</strong> {stats.upcomingAppointments}
                    </div>
                     <div className="stat-item">
                        <strong>Completed Today:</strong> {stats.completedAppointmentsToday}
                    </div>
                    <div className="stat-item">
                        <strong>Revenue Today:</strong> ${stats.totalRevenueToday?.toFixed(2)}
                    </div>
                </div>
            ) : (
                <p>No stats available.</p>
            )}
            {/* Add links or components for other admin functions here */}
        </div>
    );
}

export default AdminDashboard; 