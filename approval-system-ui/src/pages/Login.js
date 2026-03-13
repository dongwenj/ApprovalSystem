import React from 'react';
import { Button, Card, Input, Form, message } from 'antd';
import axios from 'axios';

const BASE_URL = import.meta.env.REACT_APP_API_URL || 'http://localhost/api'; //本地開發PORT須改為5266對應.Net8的launchsetting.json

const Login = () => {
  const onFinish = async (values) => {
    try {
      const response = await axios.post(`${BASE_URL}/Login`, {
        id: parseInt(values.id),
        password: values.password
      });
      message.success('連線成功！Token: ' + response.data.token);
    } catch (error) {
      message.error('連 line 失敗，請檢查 API 或 CORS');
    }
  };

  return (
    <div style={{ padding: '50px', display: 'flex', justifyContent: 'center' }}>
      <Card title="SignProcess 登入" style={{ width: 300 }}>
        <Form onFinish={onFinish} layout="vertical">
          <Form.Item label="ID" name="id"><Input /></Form.Item>
          <Form.Item label="密碼" name="password"><Input.Password /></Form.Item>
          <Button type="primary" htmlType="submit" block>登入測試</Button>
        </Form>
      </Card>
    </div>
  );
};

export default Login;