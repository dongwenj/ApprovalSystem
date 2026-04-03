import { useState, useEffect } from 'react'; // 加入 useEffect
import { Row, Col, Card, Statistic, message } from 'antd';
import { CheckCircleOutlined, ClockCircleOutlined, SyncOutlined } from '@ant-design/icons';
import { loginApi } from '../api/auth.ts'; 
import { queryApplicationForms } from '../api/datatable.ts'; 

import AccountCard from './AccountCard';
import DataTable from './DataTable';
import LogConsole from './Signal';
import styles from './Dashboard.module.css';

const AdminDashboard = () => {
  const [loading, setLoading] = useState(false);
  const [tableData, setTableData] = useState<any[]>([]); 
  const [total, setTotal] = useState<number>(0); // 修正 total 紅線
  const [userProfile, setUserProfile] = useState<any>(null);

  // 2. 定義處理搜尋與分頁的邏輯 (修正 handleSearch 紅線)
  // 1. 核心 API 呼叫邏輯
  const handleSearch = async (params: any) => {
    setLoading(true);
    try {
      // 轉換參數符合 ApplicationFormQueryReq 介面
      const queryParams = {
        reason: params.Reason || undefined,
        deptId: params.DeptId || undefined,
        applicationDate: params.ApplicationDate || undefined,
        pageNumber: params.PageNumber || 1,
        pageSize: params.PageSize || 5,
      };

      // 呼叫真正的 API
      const result = await queryApplicationForms(queryParams);
      if (result && result.isSuccess) {
        setTableData(result.dataList); // 將後端 DataItem[] 存入表格
        setTotal(result.totalCount); 
      } else {
        message.error(result.message || "查詢失敗");
      }

    } catch (error: any) {
      console.error("API Error:", error);
      message.error("連線伺服器失敗");
    } finally {
      setLoading(false);
    }
  };

  // 2. 初始載入
  useEffect(() => {
  const initialize = async () => {
    // 步驟 A: 先執行自動登入 (帳號: '1', 密碼: 'test123')
    await handleLogin('1'); 
    
    // 步驟 B: 登入完成後，執行初始資料查詢
    handleSearch({ PageNumber: 1, PageSize: 5 });
  };

  initialize();
}, []);

  const handleLogin = async (id: string) => {
    setLoading(true);
    try {
      const response = await loginApi({ id: id, password: 'pws' });

      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        setUserProfile(response.data); 
        message.success(`歡迎 ${response.data.userName}，登入成功`);
      }
    } catch (err) {
      message.error("登入失敗");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.container}>
      {/* 1. 統計區塊 */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card variant="borderless" hoverable className={styles.statisticCard}>
            <Statistic title="待簽核項目" value={12} prefix={<ClockCircleOutlined />} styles={{ content: {color: '#faad14' } }} />
          </Card>
        </Col>
        <Col span={6}>
          <Card variant="borderless" hoverable>
            <Statistic title="今日已核准" value={45} prefix={<CheckCircleOutlined />} styles={{ content: {color: '#52c41a'} }} />
          </Card>
        </Col>
        <Col span={6}>
          <Card variant="borderless" hoverable>
            <Statistic title="排程執行中" value={3} prefix={<SyncOutlined spin />} />
          </Card>
        </Col>
        <Col span={6}>
          <Card variant="borderless" hoverable>
            <Statistic title="異常警示" value={0} styles={{ content: {color: '#ff4d4f'} }} />
          </Card>
        </Col>
      </Row>

      {/* 2. 帳號資訊區 */}
      <AccountCard onLogin={handleLogin} currentUser={userProfile} />
      
      {/* 3. 資料表格 */}
      <DataTable 
        data={tableData} 
        loading={loading} 
        total={total} 
        onSearch={handleSearch} 
        currentUser={userProfile} 
      />

      {/* 4. 日誌顯示 */}
      <LogConsole />
    </div>
  );
};

export default AdminDashboard;