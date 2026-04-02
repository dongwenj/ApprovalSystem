import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Descriptions, Space, Button, Tag } from 'antd'; // 引入 Tag
import { UserOutlined } from '@ant-design/icons';

interface AccountCardProps {
  onLogin: (roleId: string) => void;
  currentUser?: {
    userName: string;
    level: number;
    dept: string;
  };
}

const AccountCard: React.FC<AccountCardProps> = ({ onLogin, currentUser }) => {
  const [activeRole, setActiveRole] = useState<string | null>(null);

  // 當 currentUser 改變時（例如初始化登入成功），同步更新 activeRole
  useEffect(() => {
    if (currentUser?.level) {
      // 將 level 轉成字串，以符合 activeRole 的類型與按鈕判斷條件
      setActiveRole(currentUser.level.toString());
    }
  }, [currentUser]); // 核心：監聽 currentUser

  // 1. 定義職級對應表 (包含名稱與顏色)
  const levelConfig: Record<number, { name: string; color: string }> = {
    1: { name: '一般職員', color: 'blue' },
    2: { name: '經理', color: 'orange' },
    3: { name: '董事長', color: 'red' },
  };

  const handleButtonClick = (roleId: string) => {
    setActiveRole(roleId);
    onLogin(roleId);
  };

  return (
    <Card variant="borderless" style={{ marginBottom: '24px', boxShadow: '0 1px 2px rgba(0,0,0,0.03)' }}>
      <Row align="middle" gutter={24}>
        <Col span={12}>
          <Descriptions title="當前登入資訊" column={2} size="small">
            <Descriptions.Item label="使用者名稱">
              <strong>{currentUser?.userName || "未登入"}</strong>
            </Descriptions.Item>
            <Descriptions.Item label="所屬部門">
              {currentUser?.dept || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="職務角色">
              {/* 2. 根據 level 自動轉換成中文標籤 */}
              {currentUser?.level ? (
                <Tag color={levelConfig[currentUser.level]?.color}>
                  {levelConfig[currentUser.level]?.name}
                </Tag>
              ) : (
                "-"
              )}
            </Descriptions.Item>
          </Descriptions>
        </Col>
        
        <Col span={12} style={{ textAlign: 'right' }}>
          <Space>
            <Button 
              type={activeRole === '3' ? 'primary' : 'default'} 
              danger={activeRole === '3'}
              icon={<UserOutlined />} 
              onClick={() => handleButtonClick('3')}
            >
              董事長登入
            </Button>
            
            <Button 
              type={activeRole === '2' ? 'primary' : 'default'} 
              onClick={() => handleButtonClick('2')}
            >
              經理登入
            </Button>
            
            <Button 
              type={activeRole === '1' ? 'primary' : 'default'} 
              onClick={() => handleButtonClick('1')}
            >
              一般職員登入
            </Button>
          </Space>
        </Col>
      </Row>
    </Card>
  );
};

export default AccountCard;