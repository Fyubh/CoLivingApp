export type MaintenanceCategory =
  | 'Plumbing'
  | 'Electric'
  | 'Furniture'
  | 'Appliance'
  | 'Hvac'
  | 'WindowsAndDoors'
  | 'Cleaning'
  | 'Internet'
  | 'Other';

export type MaintenancePriority = 'Low' | 'Normal' | 'High' | 'Urgent';

export type MaintenanceStatus =
  | 'Reported'
  | 'Acknowledged'
  | 'Assigned'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'Rejected';

export interface LoginResponse {
  token: string;
  mustChangePassword: boolean;
}

export interface RoomOptionDto {
  id: string;
  number: string;
  typeLabel: string;
}

export interface RoommateDto {
  userId: string;
  name: string;
  roomNumber?: string | null;
  joinedAt: string;
  isMe: boolean;
}

export interface MyApartmentContextDto {
  apartmentId: string;
  apartmentName: string;
  unitNumber?: string | null;
  buildingId?: string | null;
  buildingName?: string | null;
  rooms: RoomOptionDto[];
  meUserId: string;
  meName: string;
  roommates: RoommateDto[];
}

export interface ResidentNotificationDto {
  id: string;
  buildingId: string;
  audience: 'Personal' | 'Building';
  title: string;
  body: string;
  isImportant: boolean;
  isRead: boolean;
  createdAt: string;
  readAt?: string | null;
}

export interface MaintenanceRequestDto {
  id: string;
  title: string;
  description: string;
  category: MaintenanceCategory;
  priority: MaintenancePriority;
  status: MaintenanceStatus;
  photoUrl?: string | null;
  completionPhotoUrl?: string | null;
  completionNotes?: string | null;
  assignedStaffName?: string | null;
  createdAt: string;
  assignedAt?: string | null;
  startedAt?: string | null;
  completedAt?: string | null;
  residentRating?: number | null;
}

export interface CreateMaintenanceRequest {
  category: MaintenanceCategory;
  title: string;
  description: string;
  priority: MaintenancePriority;
  roomId?: string;
  apartmentId?: string;
  buildingId?: string;
}

// ─── Expenses ──────────────────────────────────────────────────────────

export type ExpenseCategoryName =
  | 'Groceries'
  | 'Rent'
  | 'Utilities'
  | 'Internet'
  | 'Household'
  | 'Other';

export interface UserBalanceDto {
  userId: string;
  userName: string;
  balance: number;
}

export interface ExpenseDto {
  id: string;
  description: string;
  amount: number;
  date: string;
  payerId: string;
  payerName: string;
  categoryId: number;
}

export interface RecurringExpenseDto {
  id: string;
  description: string;
  amount: number;
  pattern: number;
  interval: number;
  nextRunDate: string;
  payerName: string;
}

export interface SettlementDto {
  id: string;
  senderName: string;
  receiverName: string;
  amount: number;
  date: string;
}

export interface CreateExpenseRequest {
  apartmentId: string;
  amount: number;
  description: string;
  category: ExpenseCategoryName;
}

export interface SettleDebtRequest {
  apartmentId: string;
  receiverId: string;
  amount: number;
}

// ─── Inventory ─────────────────────────────────────────────────────────

export type ItemStatusName = 'Available' | 'RunningLow' | 'InCart' | 'Consumed';
export type ItemCategoryName = 'Food' | 'Household' | 'Hygiene' | 'Other';
export type StorageLocationName = 'Fridge' | 'Freezer' | 'Pantry' | 'Bathroom' | 'Other';
export type UnitTypeName =
  | 'Piece'
  | 'Package'
  | 'Bottle'
  | 'Kilogram'
  | 'Gram'
  | 'Liter'
  | 'Milliliter';

export interface ItemDto {
  id: string;
  name: string;
  quantity: number;
  unit: number;
  category: number;
  location: number;
  expiryDate?: string | null;
}

export interface CreateItemRequest {
  apartmentId: string;
  customName?: string | null;
  quantity: number;
  unit: ItemUnitInt;
  status: ItemStatusInt;
  category: ItemCategoryInt;
  location: StorageLocationInt;
  expiryDate?: string | null;
}

export type ItemStatusInt = 0 | 1 | 2 | 3;
export type ItemCategoryInt = 0 | 1 | 2 | 3;
export type StorageLocationInt = 0 | 1 | 2 | 3 | 4;
export type ItemUnitInt = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export interface CheckoutRequest {
  apartmentId: string;
  totalAmount: number;
  itemIds: string[];
}

// ─── Chores ────────────────────────────────────────────────────────────

export type ChoreCategoryName =
  | 'Vacuum'
  | 'Mop'
  | 'Toilet'
  | 'Kitchen'
  | 'Dishes'
  | 'Trash'
  | 'Other';

export type ChoreCategoryInt = 0 | 1 | 2 | 3 | 4 | 5 | 6;
export type ChoreStatusInt = 0 | 1 | 2;

export interface ChoreDto {
  id: string;
  title: string;
  description?: string | null;
  category: number;
  status: number;
  dueDate?: string | null;
  assignedName?: string | null;
  canComplete: boolean;
  canReview: boolean;
}

export interface CreateChoreRequest {
  apartmentId: string;
  title: string;
  description?: string | null;
  category: ChoreCategoryInt;
  assignedUserId?: string | null;
  dueDate?: string | null;
}

// ─── Chat ──────────────────────────────────────────────────────────────

export interface ChatMessageDto {
  id: string;
  senderId: string;
  senderName: string;
  text: string;
  sentAt: string;
}

export interface SendMessageRequest {
  apartmentId: string;
  text: string;
}
