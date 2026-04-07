import request from './request.ts';

// 請求參數
interface ApplicationFormQueryReq {
  reason?: string;
  applicationDate?: string;
  deptId?: string;
  pageNumber: number;
  pageSize: number;
}

interface ApplicationFormDataItem {
  applicationNo: string;
  applicationDate: string;
  userName: string;
  deptId: string;
  reason: string;
  rowVersion: string; // Base64 字串
  status: string;
  signerName: string;
}

// 資料結果
interface ApplicationFormQueryRes {
  isSuccess: boolean;
  status: number;
  message: string;
  dataList: ApplicationFormDataItem[]; // 後端的 List<DataItem>
  totalCount: number;               
  draftStats: number;     // 尚未陳核
  pendingStats: number;   // 待簽核
  approvalStats: number;  // 已核准
  rejectedStats: number;  // 已退回
}

// --- 新增：細項 Item 的共用介面 ---
interface FormItem {
  id?: number; // 新增時不需要 ID，編輯/檢視需要
  itemName: string;
  unit: string;
  quantity: number;
  price: number;
}

// --- 新增：View (檢視) 相關介面 ---
interface ApplicationFormViewRes {
  isSuccess: boolean;
  message: string;
  applicationNo: number;
  applicationDate: string;
  userName: string;
  deptId: string;
  reason: string;
  items: FormItem[];
}

// --- 新增：Add (新增) 相關介面 ---
interface ApplicationFormAddReq {
  applicationDate: string;
  reason: string;
  items: FormItem[];
}

// --- 新增：Edit (編輯) 相關介面 ---
interface ApplicationFormEditReq {
  applicationNo: number;
  applicationDate: string;
  reason: string;
  items: FormItem[];
  rowVersion: string; // 對應 C# byte[]，前端帶入 Base64 字串
}

// --- 通用回傳格式 (針對 Add, Edit, Delete) ---
interface BaseRes {
  isSuccess: boolean;
  status: number;
  message: string;
}

// ==========================================
// API Functions
// ==========================================

// 分頁查詢
export const queryApplicationForms = async (params: ApplicationFormQueryReq) => {
  const response = await request.get<ApplicationFormQueryRes>('/form/query', { params });
  return response.data;
};

// 取得單筆詳細資料 (View)
export const getApplicationFormView = async (applicationNo: number) => {
  const response = await request.get<ApplicationFormViewRes>('/form/view', {
    params: { applicationNo }
  });
  return response.data;
};

// 新增申請單
export const addApplicationForm = async (data: ApplicationFormAddReq) => {
  const response = await request.post<BaseRes>('/form/add', data);
  return response.data;
};

// 編輯申請單
export const editApplicationForm = async (data: ApplicationFormEditReq) => {
  const response = await request.put<BaseRes>('/form/edit', data); // 注意：若後端是 [HttpPut] 則改為 request.put
  return response.data;
};

// 刪除申請單
export const deleteApplicationForm = async (applicationNo: number) => {
  const response = await request.delete<BaseRes>('/form/delete', {
    params: { applicationNo }
  });
  return response.data;
};

// 提交陳核
export const submitApplicationForm = async (applicationNo: number) => {
  const response = await request.post<BaseRes>('/form/present', { applicationNo });
  return response.data;
};

// 審核 (核准或退回)
export const reviewApplicationForm = async (data: { applicationNo: number; isApproved: boolean }) => {
  const response = await request.post<BaseRes>('/form/review', data);
  return response.data;
};

// 寄送 Email
export const sendApplicationEmail = async (data: { applicationNo: number; toEmail: string }) => {
  const response = await request.post<BaseRes>('/form/send', data);
  return response.data;
};