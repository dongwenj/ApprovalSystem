import request from './request.ts';

export interface LoginValues {
  id: string | number;
  password: string;
}

export const loginApi = (data: LoginValues) => {
  return request.post('/login', {
    id: typeof data.id === 'string' ? parseInt(data.id) : data.id,
    password: data.password
  });
};