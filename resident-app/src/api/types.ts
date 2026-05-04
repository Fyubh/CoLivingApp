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

export interface MyApartmentContextDto {
  apartmentId: string;
  apartmentName: string;
  unitNumber?: string | null;
  buildingId?: string | null;
  buildingName?: string | null;
  rooms: RoomOptionDto[];
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
