import React, { useEffect, useState, useRef } from 'react';
import { Row, Col, Card, Tag, Descriptions, Button } from 'antd';
import { HistoryOutlined, ThunderboltOutlined, DeleteOutlined } from '@ant-design/icons';
import styles from './Dashboard.module.css';
import { setupSignalRConnection } from '../api/signal';

const LogConsole: React.FC = () => {
  const [logs, setLogs] = useState<string[]>([]);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // 啟動連線，並定義收到訊息後的動作
    const connection = setupSignalRConnection((newLog) => {
      setLogs((prevLogs) => {
        const updatedLogs = [...prevLogs, newLog];
        // 為了效能，只保留最新的 100 筆，避免網頁越來越卡
        return updatedLogs.slice(-100);
      });
    });

    // 組件卸載時斷開連線，避免記憶體洩漏
    return () => {
      connection.stop();
    };
  }, []);

  // 當 logs 更新時，自動捲動到最底部
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [logs]);

  // 清空日誌功能
  const clearLogs = () => {
    setLogs([]);
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
      <Row>
        <Col span={24}>
          <Card 
            title={<span><HistoryOutlined /> LOG 即時顯示</span>} 
            variant="borderless"
            extra={ <Button type="text" danger icon={<DeleteOutlined />} onClick={clearLogs}> 清空日誌 </Button> }
            style={{ marginTop: 16 }}
          >
            <div className={styles.logTerminal} ref={scrollRef} style={{ height: '300px', overflowY: 'auto' }}>
              {logs.length === 0 ? (
                <div style={{ color: '#666' }}>{'>'} 等待系統日誌中...</div>
              ) : (
                logs.map((log, index) => (
                  <div key={index} style={{ marginBottom: '4px' }}>
                    <span style={{ color: log.includes('[Error]') ? '#ff4d4f' : 'inherit' }}>
                      {log}
                    </span>
                  </div>
                ))
              )}
            </div>
          </Card>
        </Col>
      </Row>

      <Row>
        <Col span={24}>
          <Card title={<span><ThunderboltOutlined /> Hangfire 排程</span>} variant="borderless">
            <Descriptions column={3} size="middle" bordered>
              <Descriptions.Item label="當前隊列">Default</Descriptions.Item>
              <Descriptions.Item label="執行中任務">EmailNotifyJob</Descriptions.Item>
              <Descriptions.Item label="伺服器狀態">
                <Tag color="processing">運作中</Tag>
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default LogConsole;