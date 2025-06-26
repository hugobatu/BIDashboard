// src/data/mockData.js

// Danh sách các chiều dữ liệu (Dimensions)
export const SERVICES = [
  { id: 'srv01', name: 'Thanh toán Online' },
  { id: 'srv02', name: 'Hệ thống CRM' },
  { id: 'srv03', name: 'Quản lý Kho' },
  { id: 'srv04', name: 'Báo cáo Nội bộ' },
];

export const PRIORITIES = [
  { id: 'p1', name: 'Critical' },
  { id: 'p2', name: 'High' },
  { id: 'p3', name: 'Medium' },
  { id: 'p4', name: 'Low' },
];

export const SHIFTS = [
  { id: 's1', name: 'Ca Sáng' },
  { id: 's2', name: 'Ca Chiều' },
  { id: 's3', name: 'Ca Đêm' },
];

// THÊM MỚI: Dimension cho Phòng ban
export const ASSIGNMENT_GROUPS = [
    { id: 'grp01', name: 'Đội Hạ tầng (Infrastructure)' },
    { id: 'grp02', name: 'Đội Hỗ trợ Ứng dụng' },
    { id: 'grp03', name: 'Đội Quản trị CSDL' },
    { id: 'grp04', name: 'Đội An ninh mạng' },
];

// Hàm tạo dữ liệu incident ngẫu nhiên
function createIncidents(count, startDate, endDate) {
  const incidents = [];
  const start = new Date(startDate).getTime();
  const end = new Date(endDate).getTime();

  for (let i = 0; i < count; i++) {
    const randomTime = start + Math.random() * (end - start);
    const date = new Date(randomTime);
    incidents.push({
      id: `inc_${i}`,
      date: date.toISOString().split('T')[0], // format YYYY-MM-DD
      serviceId: SERVICES[Math.floor(Math.random() * SERVICES.length)].id,
      priorityId: PRIORITIES[Math.floor(Math.random() * PRIORITIES.length)].id,
      shiftId: SHIFTS[Math.floor(Math.random() * SHIFTS.length)].id,
      // THÊM MỚI: Gán ngẫu nhiên một phòng ban
      assignmentGroupId: ASSIGNMENT_GROUPS[Math.floor(Math.random() * ASSIGNMENT_GROUPS.length)].id,
    });
  }
  return incidents;
}

// Tạo dữ liệu cho 2 tháng
export const rawIncidents = [
  ...createIncidents(800, '2023-10-01', '2023-10-31'),
  ...createIncidents(950, '2023-11-01', '2023-11-30'),
];