import axios from 'axios';

const isDev = import.meta.env.DEV;
const service = axios.create({
  baseURL: isDev 
    ? 'http://localhost:5266/api' 
    : (import.meta.env.VITE_API_URL || '/api'),
  timeout: 5000
}); 

// Request 攔截器：統一加上 Token
service.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

export default service;