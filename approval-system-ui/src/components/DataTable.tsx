import { useState, useEffect } from 'react';
import { Card, Table, Tag, Space, Input, Button, DatePicker, Typography, Modal, Form, message, Row, Col, InputNumber } from 'antd';
import { SnippetsOutlined, SearchOutlined, ReloadOutlined, PlusOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { getApplicationFormView, addApplicationForm, editApplicationForm, submitApplicationForm, reviewApplicationForm, sendApplicationEmail, deleteApplicationForm } from '../api/datatable';

const { Text } = Typography;

interface DataTableProps {
  data: any[];
  loading: boolean;
  total: number;
  onSearch: (params: any) => void;
  currentUser?: any; // ✨ 新增這行，接收父元件的 user 資訊
}

const statusMap: Record<number, { text: string; color: string }> = {
  1: { text: '尚未陳核', color: 'default' },
  2: { text: '待簽核', color: 'orange' },
  3: { text: '已核准', color: 'green' },
  4: { text: '已退回', color: 'red' },
};

const itemColumns = (remove: (index: number) => void) => [
  {
    title: '項目名稱',
    dataIndex: 'itemName',
    render: (_: any, __: any, index: number) => (
      <Form.Item name={[index, 'itemName']} 
        rules={[
          { required: true, message: '項目名稱 為必填' },
          { max: 100, message: '項目名稱 長度不可超過100' }
        ]} 
        style={{ marginBottom: 0 }}>
        <Input placeholder="項目名稱" />
      </Form.Item>
    ),
  },
  {
    title: '單位',
    dataIndex: 'unit',
    width: 100,
    render: (_: any, __: any, index: number) => (
      <Form.Item name={[index, 'unit']} style={{ marginBottom: 0 }} 
          rules={[
          { required: true, message: '單位 為必填' },
          { max: 5, message: '單位 長度不可超過5' }
        ]}>
        <Input placeholder="個/支" /> 
      </Form.Item>
    ),
  },
  {
    title: '數量',
    dataIndex: 'quantity',
    width: 120,
    render: (_: any, __: any, index: number) => (
      <Form.Item 
        name={[index, 'quantity']} 
        style={{ marginBottom: 0 }}
        rules={[
          { required: true, message: '數量 為必填' },
          { type: 'number', min: 1, message: '必須大於 0' } // ✨ 這裡限制數字必須 >= 1
        ]}>
        <InputNumber 
          min={1}
          precision={0}
          placeholder="數量" 
          style={{ width: '100%' }}
        />
      </Form.Item>
    ),
  },
  {
    title: '單價',
    dataIndex: 'price',
    width: 120,
    render: (_: any, __: any, index: number) => (
      <Form.Item 
        name={[index, 'price']} 
        style={{ marginBottom: 0 }}
        rules={[
          { required: true, message: '單價 為必填' },
          //{ type: 'number', min: 1, message: '必須大於 0' }
        ]}>
        <InputNumber 
          //min={1} 
          precision={0}
          placeholder="金額" 
          style={{ width: '100%' }} 
          //formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} //加千分位與錢號
        />
      </Form.Item>
    ),
  },
  {
    title: '操作',
    width: 60,
    render: (_: any, __: any, index: number) => (
      <Button type="link" danger onClick={() => remove(index)}>刪除</Button>
    ),
  },
];

const DataTable: React.FC<DataTableProps> = ({ data, loading, total, onSearch, currentUser }) => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'add' | 'edit' | 'view'>('add');
  const [modalLoading, setModalLoading] = useState(false); // 彈窗內部的 Loading
  const [form] = Form.useForm();
  const [currentRowVersion, setCurrentRowVersion] = useState<string>(''); // 儲存樂觀鎖版本
  
  // 1. 篩選狀態 (對應 C# DTO)
  const [filter, setFilter] = useState({
    Reason: '',
    DeptId: '',
    ApplicationDate: null as any,
    PageNumber: 1,
    PageSize: 5,
  });

  // 開啟彈窗的處理函式
  const showModal = async (mode: 'add' | 'edit' | 'view', record?: any) => {
    setModalMode(mode);
    setIsModalOpen(true);
    setModalLoading(true); // 開始讀取
    form.resetFields();
    setCurrentRowVersion(''); // 清空舊的版本號

    try {
      if (mode === 'add') {
        form.setFieldsValue({
          applicationDate: dayjs(),
          userName: currentUser?.userName,
          deptId: currentUser?.dept,
          items: [], // 初始明細為空
        });
      } else if (record && record.applicationNo) {
        // --- 檢視或編輯模式：呼叫 View API ---
        const res = await getApplicationFormView(Number(record.applicationNo));

        if (res.isSuccess) {
          // 1. 儲存 RowVersion (編輯提交時需要)
          // 注意：這裡假設 View API 會回傳 RowVersion，如果沒有，則從列表的 record 拿
          setCurrentRowVersion(record.rowVersion || ''); 

          // 2. 填入主表單資料
          form.setFieldsValue({
            applicationNo: res.applicationNo,
            applicationDate: res.applicationDate ? dayjs(res.applicationDate) : null,
            userName: res.userName,
            deptId: res.deptId,
            reason: res.reason,
            items: res.items || [], // 將後端 List<Item> 直接塞入 Form.List
          });
        } else {
          message.error(res.message || '無法取得詳細資料');
          setIsModalOpen(false);
        }
      }
    } catch (error) {
      console.error('API Error:', error);
      message.error('連線伺服器失敗');
      setIsModalOpen(false);
    } finally {
      setModalLoading(false); // 結束讀取
    }
  };

  const handleModalOk = async () => {
    if (modalMode === 'view') {
      setIsModalOpen(false);
      return;
    }

    try {
      // 1. 驗證表單欄位（包含 Form.List 裡面的 rules）
      const values = await form.validateFields();

      // 2. 格式化資料以符合 C# DTO
      const payload = {
        ...values,
        applicationDate: values.applicationDate.format('YYYY-MM-DDTHH:mm:ss'),
        // 如果是編輯，要帶上 RowVersion 與主鍵
        ...(modalMode === 'edit' && { 
          applicationNo: form.getFieldValue('applicationNo'),
          rowVersion: currentRowVersion 
        }),
      };

      let res;
      if (modalMode === 'add') {
        res = await addApplicationForm(payload);
      } else {
        res = await editApplicationForm(payload);
      }
      
      if (res.isSuccess) {
        message.success(`${modalMode === 'add' ? '新增' : '修改'}成功`);
        setIsModalOpen(false);
        handleSearch(); // 重新整理列表
      } else {
        message.error(res.message || '操作失敗');
      }
    } catch (error: any) {
      // 處理 HTTP 400/500 等 API 報錯
      if (error.response && error.response.data) {
        const backendError = error.response.data;
        message.error(backendError.message || '系統報錯');
      } else {
        message.error('連線伺服器失敗');
      }
    }
  };

  useEffect(() => {
    if (currentUser) {
      handleReset(); // 登入後重置篩選
    }
  }, [currentUser]);

  // 2. 搜尋與排序邏輯
  const handleSearch = (newParams?: any) => {
    // 取得最新的 filter 狀態並覆蓋新參數
    const updatedFilter = { ...filter, ...newParams };
    
    // 更新本地 state，這會讓 Table UI (分頁標籤) 跟著變
    setFilter(updatedFilter);
    
    // 發送給父元件 API
    onSearch({
      ...updatedFilter,
      ApplicationDate: updatedFilter.ApplicationDate ? dayjs(updatedFilter.ApplicationDate).format('YYYY-MM-DD') : null,
      SortOrder: updatedFilter.SortOrder === 'ascend' ? 'asc' : updatedFilter.SortOrder === 'descend' ? 'desc' : null
    });
  };

  // 3. 重設按鈕
  const handleReset = () => {
    const resetValue = {
      Reason: '',
      DeptId: '',
      ApplicationDate: null,
      PageNumber: 1,
      PageSize: 5,
    };
    setFilter(resetValue);
    onSearch(resetValue);
  };

  const handleDelete = (no: number) => {
    Modal.confirm({
      title: '確定要刪除嗎？',
      content: `單號：${no}，刪除後將無法還原。`,
      okText: '確定刪除',
      okType: 'danger',
      cancelText: '取消',
      onOk: async () => {
        try {
          const res = await deleteApplicationForm(no); // 呼叫 API
          if (res.isSuccess) {
            message.success('刪除成功');
            handleSearch(); // 重新整理列表
          } else {
            message.error(res.message || '刪除失敗');
          }
        } catch (error: any) {
          // 處理 400/500 等異常
          const errorMsg = error.response?.data?.message || '伺服器異常';
          message.error(errorMsg);
        }
      },
    });
  };

  // 陳核
  const handleSubmit = (no: number) => {
    Modal.confirm({
      title: '確認陳核',
      content: `是否確認將單號 ${no} 送出陳核？`,
      okText: '確定',
      cancelText: '取消',
      onOk: async () => {
        try {
          const res = await submitApplicationForm(no);
          if (res.isSuccess) {
            message.success('已送出陳核');
            handleSearch(); // 重新整理列表
          } else {
            message.error(res.message);
          }
        } catch (error: any) {
          message.error(error.response?.data?.message || '連線失敗');
        }
      },
    });
  };

  // 審核 (核准/退回)
  const handleReview = (no: number, isApproved: boolean) => {
    Modal.confirm({
      title: isApproved ? '核准申請' : '退回申請',
      content: `確定要${isApproved ? '核准' : '退回'}此張單據嗎？`,
      okText: '確定',
      cancelText: '取消',
      onOk: async () => {
        const res = await reviewApplicationForm({ applicationNo: no, isApproved });
        if (res.isSuccess) {
          message.success(isApproved ? '已核准' : '已退回');
          handleSearch();
        } else {
          message.error(res.message);
        }
      },
    });
  };

  const handleSendEmail = (no: number) => {
  let emailValue = ""; // 用來存輸入的值

    // ✨ 定義 Email 正則表達式
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    Modal.confirm({
      title: '寄送申請單 Email',
      content: (
        <div style={{ marginTop: 16 }}>
          <p>請輸入收件者電子郵件：</p>
          <Input 
            placeholder="example@mail.com" 
            onChange={(e) => { emailValue = e.target.value; }} 
          />
        </div>
      ),
      okText: '確定',
      cancelText: '取消',
      onOk: async () => {
        if (!emailValue) {
          message.warning('請輸入 Email 地址');
          return Promise.reject(); // 擋住 Modal 不讓它關閉
        }

        // 2. ✨ 檢查格式是否正確
        if (!emailRegex.test(emailValue)) {
          message.error('Email 格式不正確，請重新輸入');
          return Promise.reject(); // 擋住，不讓 Modal 關閉
        }
        
        try {
          const res = await sendApplicationEmail({ 
            applicationNo: no, 
            toEmail: emailValue 
          });
          if (res.isSuccess) {
            message.success('Email 寄送成功');
          } else {
            message.error(res.message);
            return Promise.reject();
          }
        } catch (error: any) {
          message.error(error.response?.data?.message || '寄送失敗');
          return Promise.reject();
        }
      },
    });
  };

  const columns: ColumnsType<any> = [
    { 
      title: '申請單號', 
      dataIndex: 'applicationNo',
      key: 'applicationNo',
    },
    { 
      title: '申請事由', 
      dataIndex: 'reason',
      key: 'reason',
    },
    { 
      title: '部門', 
      dataIndex: 'deptId', 
      key: 'deptId',
    },
    { 
      title: '日期', 
      dataIndex: 'applicationDate', 
      key: 'applicationDate',
    },
    {
      title: '狀態',
      dataIndex: 'status',
      key: 'status',
      render: (status: number, record: any) => {
        // 1. 取得對應的文字與顏色
        const config = statusMap[status] || { text: '未知', color: 'default' };
        
        // 2. 邏輯判斷：如果是「待簽核(2)」，則加上簽核人姓名
        let displayText = config.text;
        if (status === 2 && record.signerName) {
          displayText = `${config.text}(${record.signerName})`;
        }

        return (
          <Tag color={config.color}>
            {displayText}
          </Tag>
        );
      },
    },
    {
      title: '功能',
      key: 'action',
      render: (_, record: any) => {
        const userLevel = currentUser?.level;
        const status = record.status;

        return (
          <Space size="small">
            {/* 所有人都有「檢視」 */}
            <a onClick={() => showModal('view', record)} style={{ color: '#000000' }}>檢視</a>

            {/* Level 1: 一般職員 */}
            {userLevel === 1 && (
              <>
                {(status === 1 || status === 4) && (
                  <>
                    <a style={{ color: '#000000' }} onClick={() => showModal('edit', record)}>編輯</a>
                    <a style={{ color: '#52c41a' }} onClick={() => handleSubmit(record.applicationNo)}>陳核</a>
                    <a style={{ color: '#ff4d4f' }} onClick={() => handleDelete(record.applicationNo)}>刪除</a>
                  </>
                )}
              </>
            )}

            {/* Level 2: 經理 */}
            {userLevel === 2 && (
              <>
                <a style={{ color: '#52c41a' }} onClick={() => handleReview(record.applicationNo, true)}>擬准</a>
                <a style={{ color: '#ff4d4f' }} onClick={() => handleReview(record.applicationNo, false)}>退回</a>
              </>
            )}

            {/* Level 3: 董事長 */}
            {userLevel === 3 && (
              <>
                <a style={{ color: '#52c41a' }} onClick={() => handleReview(record.applicationNo, true)}>核准</a>
                <a style={{ color: '#ff4d4f' }} onClick={() => handleReview(record.applicationNo, false)}>退回</a>
              </>
            )}

            {/* 所有人都有「寄送MAIL」 */}
            <a style={{ color: '#000000' }} onClick={() => handleSendEmail(record.applicationNo)}>寄送MAIL</a>
          </Space>
        );
      },
    },
  ];

  return (
    <Card title={<span><SnippetsOutlined /> 資料列表 </span>} variant="borderless"
      extra={
      currentUser?.level === 1 && (
        <Button 
          type="primary" 
          icon={<PlusOutlined />} 
          style={{ backgroundColor: '#52c41a', borderColor: '#52c41a' }}
          onClick={() => showModal('add')}
        >
          新增申請單
        </Button>
      )
    }
    >
      {/* 查詢條件區 */}
      <Space size="middle" style={{ marginBottom: '20px', flexWrap: 'wrap' }} align="center">
        <Space>
          <Text>申請事由:</Text>
          <Input 
            placeholder="請輸入事由" 
            style={{ width: 160 }} 
            value={filter.Reason}
            onChange={(e) => setFilter({ ...filter, Reason: e.target.value })}
          />
        </Space>

        <Space>
          <Text>申請部門:</Text>
          <Input 
            placeholder="請輸入部門" 
            style={{ width: 120 }} 
            value={filter.DeptId}
            onChange={(e) => setFilter({ ...filter, DeptId: e.target.value })}
          />
        </Space>

        <Space>
          <Text>申請日期:</Text>
          <DatePicker 
            placeholder="請選擇日期" 
            style={{ width: 150 }} 
            value={filter.ApplicationDate}
            onChange={(date) => setFilter({ ...filter, ApplicationDate: date })}
          />
        </Space>

        <Button type="primary" icon={<SearchOutlined />} onClick={() => handleSearch({ PageNumber: 1 })}>
          搜尋
        </Button>
        <Button icon={<ReloadOutlined />} onClick={handleReset}>
          重設篩選
        </Button>
      </Space>

      {/* 表格區 */}
      <Table 
        dataSource={data} 
        columns={columns} 
        loading={loading}
        rowKey="applicationNo"
        onChange={(pagination, sorter: any) => {
          // 當點擊分頁或欄位排序箭頭時會觸發
          handleSearch({
            PageNumber: pagination.current,
            PageSize: pagination.pageSize,
            SortField: sorter.field,
            SortOrder: sorter.order
          });
        }}
        pagination={{ 
          current: filter.PageNumber,
          pageSize: filter.PageSize,
          total: total,
          showSizeChanger: true,
          pageSizeOptions: ['5', '10', '20', '50'],
          showTotal: (total) => `共 ${total} 筆資料`,
        }} 
      />

      {/* 彈窗設計 */}
      <Modal
        title={modalMode === 'add' ? '新增申請單' : modalMode === 'edit' ? '編輯申請單' : '檢視申請單'}
        open={isModalOpen}
        forceRender
        onOk={handleModalOk}
        onCancel={() => setIsModalOpen(false)}
        width={800}
        loading={modalLoading}
      >
        <Form form={form} layout="vertical" disabled={modalMode === 'view'}>
          {/* 第一列：基本資訊 */}
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="applicationNo" label="申請單號">
                <Input disabled placeholder="系統自動編號" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="userName" label="申請人">
                <Input disabled />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="deptId" label="部門編號">
                <Input disabled />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="applicationDate" label="申請日期">
                <DatePicker disabled style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="reason" label="申請事由" rules={[{ required: true, message: "申請事由 為必填" }]}>
            <Input.TextArea rows={2} />
          </Form.Item>

          {/* 第二區塊：明細資料 */}
          <div style={{ marginTop: '20px' }}>
            <Typography.Title level={5}>申請明細項目</Typography.Title>
            <Form.List name="items">
              {(fields, { add, remove }) => (
                <>
                  <Table
                    dataSource={fields} // Table 的資料源改成 Form.List 提供的 fields
                    columns={itemColumns(remove)} // 把刪除函式傳進去
                    pagination={false}
                    size="small"
                    bordered
                    rowKey="key"
                    footer={() => (
                      modalMode !== 'view' && (
                        <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>
                          新增一列項目
                        </Button>
                      )
                    )}
                  />
                </>
              )}
            </Form.List>
          </div>
        </Form>
      </Modal>
    </Card>
  );
};

export default DataTable;