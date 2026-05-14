import { api } from './api';

export type StaffRole = 'BuildingAdmin' | 'Reception' | 'Contractor' | 'Cleaner' | 'Security';
export type ContractorType = 'Plumbing' | 'Electric' | 'Furniture' | 'Appliance' | 'Hvac' | 'WindowsAndDoors' | 'Internet' | 'Other';
export type RoomType = 'Studio' | 'Single' | 'Double' | 'Shared';

export interface LoginResponse {
    token: string;
    mustChangePassword: boolean;
}

export interface BuildingDto {
    id: string;
    name: string;
    city: string;
    country: string;
    isActive: boolean;
}

export interface FloorDto {
    id: string;
    number: number;
    name?: string | null;
}

export interface RoomInApartmentDto {
    roomId: string;
    number: string;
    type: RoomType;
    currentOccupants: number;
    maxOccupancy: number;
}

export interface ApartmentDto {
    apartmentId: string;
    name: string;
    unitNumber?: string | null;
    floorNumber: number;
    rooms: RoomInApartmentDto[];
}

export interface AvailableRoomDto {
    roomId: string;
    roomNumber: string;
    type: RoomType;
    currentOccupants: number;
    maxOccupancy: number;
    apartmentId: string;
    apartmentName: string;
    unitNumber?: string | null;
    floorNumber: number;
}

export interface BuildingResidentDto {
    userId: string;
    name: string;
    email: string;
    apartmentId: string;
    apartmentName: string;
    unitNumber?: string | null;
    roomId?: string | null;
    roomNumber?: string | null;
    joinedAt: string;
}

export interface StaffDto {
    staffAssignmentId: string;
    userId: string;
    name: string;
    email: string;
    role: StaffRole;
    specialization?: ContractorType | null;
    isActive: boolean;
    isOnShift: boolean;
    completedTasksCount: number;
}

export interface MaintenanceRequestDto {
    id: string;
    title: string;
    description: string;
    category: string;
    priority: string;
    status: string;
    reporterName: string;
    unitNumber?: string | null;
    roomNumber?: string | null;
    photoUrl?: string | null;
    assignedStaffName?: string | null;
    createdAt: string;
    completedAt?: string | null;
    residentRating?: number | null;
}

export interface RoomTemplate {
    number: string;
    type: RoomType;
    maxOccupancy: number;
    squareMeters?: number | null;
    monthlyRent?: number | null;
}

export interface CreatedUserDto {
    userId: string;
    email: string;
    name: string;
    tempPassword: string;
}

export interface ChatMessageDto {
    id: string;
    senderId: string;
    senderName: string;
    text: string;
    sentAt: string;
    isDeleted: boolean;
}

export interface CreateBuildingRequest {
    name: string;
    addressLine: string;
    city: string;
    country: string;
    postalCode?: string | null;
    timeZone?: string | null;
    totalFloors: number;
}

export const adminService = {
    login: async (email: string, password: string): Promise<LoginResponse> => {
        const response = await api.post<LoginResponse>('/Auth/login', { email, password });
        return response.data;
    },

    changePassword: async (oldPassword: string, newPassword: string): Promise<LoginResponse> => {
        const response = await api.post<LoginResponse>('/Users/change-password', { oldPassword, newPassword });
        return response.data;
    },

    getBuildings: async (): Promise<BuildingDto[]> => {
        const response = await api.get<BuildingDto[]>('/admin/onboarding/buildings');
        return response.data;
    },

    createBuilding: async (payload: CreateBuildingRequest): Promise<{ buildingId: string }> => {
        const response = await api.post<{ buildingId: string }>('/admin/onboarding/buildings', {
            name: payload.name,
            addressLine: payload.addressLine,
            city: payload.city,
            country: payload.country,
            postalCode: payload.postalCode || null,
            timeZone: payload.timeZone || null,
            totalFloors: payload.totalFloors,
            operatorId: null,
        });
        return response.data;
    },

    getFloors: async (buildingId: string): Promise<FloorDto[]> => {
        const response = await api.get<FloorDto[]>(`/admin/onboarding/buildings/${buildingId}/floors`);
        return response.data;
    },

    getApartments: async (buildingId: string): Promise<ApartmentDto[]> => {
        const response = await api.get<ApartmentDto[]>(`/admin/onboarding/buildings/${buildingId}/apartments`);
        return response.data;
    },

    getAvailableRooms: async (buildingId: string): Promise<AvailableRoomDto[]> => {
        const response = await api.get<AvailableRoomDto[]>(`/admin/onboarding/buildings/${buildingId}/available-rooms`);
        return response.data;
    },

    getResidents: async (buildingId: string): Promise<BuildingResidentDto[]> => {
        const response = await api.get<BuildingResidentDto[]>(`/admin/onboarding/buildings/${buildingId}/residents`);
        return response.data;
    },

    getStaff: async (buildingId: string): Promise<StaffDto[]> => {
        const response = await api.get<StaffDto[]>(`/admin/onboarding/buildings/${buildingId}/staff`);
        return response.data;
    },

    getMaintenance: async (buildingId: string): Promise<MaintenanceRequestDto[]> => {
        const response = await api.get<MaintenanceRequestDto[]>(`/Maintenance/building/${buildingId}`);
        return response.data;
    },

    createApartment: async (buildingId: string, floorId: string, unitNumber: string, name: string, rooms: RoomTemplate[]) => {
        const response = await api.post('/admin/onboarding/apartments', {
            buildingId,
            floorId,
            unitNumber,
            name: name || null,
            rooms,
        });
        return response.data;
    },

    createTenant: async (buildingId: string, email: string, name: string, roomId: string): Promise<CreatedUserDto> => {
        const response = await api.post<CreatedUserDto>('/admin/onboarding/tenants', {
            buildingId,
            email,
            name,
            roomId,
        });
        return response.data;
    },

    createStaff: async (
        buildingId: string,
        email: string,
        name: string,
        role: StaffRole,
        specialization?: ContractorType | null
    ): Promise<CreatedUserDto> => {
        const response = await api.post<CreatedUserDto>('/admin/onboarding/staff', {
            buildingId,
            email,
            name,
            role,
            specialization: specialization || null,
        });
        return response.data;
    },

    acknowledgeMaintenance: async (id: string) => {
        const response = await api.post(`/Maintenance/${id}/acknowledge`);
        return response.data;
    },

    assignMaintenance: async (id: string, staffAssignmentId: string) => {
        const response = await api.post(`/Maintenance/${id}/assign`, { staffAssignmentId });
        return response.data;
    },

    sendPersonalNotification: async (
        buildingId: string,
        targetUserId: string,
        title: string,
        body: string,
        isImportant: boolean
    ) => {
        const response = await api.post('/Notifications/admin/personal', {
            buildingId,
            targetUserId,
            title,
            body,
            isImportant,
        });
        return response.data;
    },

    sendBuildingNotification: async (
        buildingId: string,
        title: string,
        body: string,
        isImportant: boolean
    ) => {
        const response = await api.post('/Notifications/admin/building', {
            buildingId,
            title,
            body,
            isImportant,
        });
        return response.data;
    },

    getBuildingChat: async (buildingId: string): Promise<ChatMessageDto[]> => {
        const response = await api.get<ChatMessageDto[]>(`/Chat/building/${buildingId}`);
        return response.data;
    },
};
