import Foundation

// MARK: - Enums (string-based to match JsonStringEnumConverter on the backend)

/// Mirrors `CoLivingApp.Domain.Enums.MaintenanceCategory`. Eight known
/// values + `Other`. `caseIterable` drives the picker; `label` is the RU
/// display string. New backend values fall through to the raw string.
nonisolated enum MaintenanceCategory: String, Codable, CaseIterable, Identifiable {
    case plumbing = "Plumbing"
    case electric = "Electric"
    case furniture = "Furniture"
    case appliance = "Appliance"
    case hvac = "Hvac"
    case windowsAndDoors = "WindowsAndDoors"
    case cleaning = "Cleaning"
    case internet = "Internet"
    case other = "Other"

    var id: String { rawValue }

    var label: String {
        switch self {
        case .plumbing:        return "Сантехника"
        case .electric:        return "Электрика"
        case .furniture:       return "Мебель"
        case .appliance:       return "Бытовая техника"
        case .hvac:            return "Кондиционер / отопление"
        case .windowsAndDoors: return "Окна и двери"
        case .cleaning:        return "Уборка"
        case .internet:        return "Интернет"
        case .other:           return "Другое"
        }
    }

    var icon: String {
        switch self {
        case .plumbing:        return "drop"
        case .electric:        return "bolt"
        case .furniture:       return "chair"
        case .appliance:       return "stove"
        case .hvac:            return "thermometer.medium"
        case .windowsAndDoors: return "door.left.hand.closed"
        case .cleaning:        return "sparkles"
        case .internet:        return "wifi"
        case .other:           return "ellipsis"
        }
    }
}

/// Mirrors `MaintenancePriority`. Default `Normal` matches the backend.
nonisolated enum MaintenancePriority: String, Codable, CaseIterable, Identifiable {
    case low = "Low"
    case normal = "Normal"
    case high = "High"
    case urgent = "Urgent"

    var id: String { rawValue }

    var label: String {
        switch self {
        case .low:    return "Низкий"
        case .normal: return "Обычный"
        case .high:   return "Высокий"
        case .urgent: return "Срочно"
        }
    }
}

/// Mirrors `MaintenanceStatus`. Lifecycle-driven; `cancellable` and
/// `rateable` capture the resident-side rules: cancel before InProgress,
/// rate only when Completed.
nonisolated enum MaintenanceStatus: String, Codable {
    case reported = "Reported"
    case acknowledged = "Acknowledged"
    case assigned = "Assigned"
    case inProgress = "InProgress"
    case completed = "Completed"
    case cancelled = "Cancelled"
    case rejected = "Rejected"

    var label: String {
        switch self {
        case .reported:     return "Создана"
        case .acknowledged: return "Принята"
        case .assigned:     return "Назначен мастер"
        case .inProgress:   return "В работе"
        case .completed:    return "Закрыта"
        case .cancelled:    return "Отменена"
        case .rejected:     return "Отклонена"
        }
    }

    /// Resident may cancel any request that hasn't reached InProgress yet.
    /// Backend enforces the same rule — this just hides the button when it
    /// wouldn't succeed.
    var cancellable: Bool {
        switch self {
        case .reported, .acknowledged, .assigned: return true
        default: return false
        }
    }

    var isTerminal: Bool {
        switch self {
        case .completed, .cancelled, .rejected: return true
        default: return false
        }
    }
}

// MARK: - Read DTO

/// Mirrors `MaintenanceRequestDto` (resident view). Photo URLs and ratings
/// are surfaced; the contractor's StaffAssignment id is intentionally not.
nonisolated struct MaintenanceRequestDto: Decodable, Identifiable {
    let id: UUID
    let title: String
    let description: String
    let category: MaintenanceCategory
    let priority: MaintenancePriority
    let status: MaintenanceStatus
    let photoUrl: String?
    let completionPhotoUrl: String?
    let completionNotes: String?
    let assignedStaffName: String?
    let createdAt: Date
    let assignedAt: Date?
    let startedAt: Date?
    let completedAt: Date?
    let residentRating: Int?
}

// MARK: - Write payloads

/// Mirrors `CreateMaintenanceRequestCommand`. `reportedByUserId` is filled
/// in by the controller from the JWT — we send an empty string and the
/// server overwrites it before the handler runs.
nonisolated struct CreateMaintenancePayload: Encodable {
    let reportedByUserId: String
    let category: MaintenanceCategory
    let title: String
    let description: String
    let priority: MaintenancePriority
    let roomId: UUID?
    let apartmentId: UUID?
    let buildingId: UUID?
    let photoUrl: String?

    init(
        category: MaintenanceCategory,
        title: String,
        description: String,
        priority: MaintenancePriority,
        roomId: UUID?,
        apartmentId: UUID?,
        buildingId: UUID?,
        photoUrl: String? = nil
    ) {
        self.reportedByUserId = ""
        self.category = category
        self.title = title
        self.description = description
        self.priority = priority
        self.roomId = roomId
        self.apartmentId = apartmentId
        self.buildingId = buildingId
        self.photoUrl = photoUrl
    }
}

/// Mirrors `MaintenanceController.CancelBody`.
nonisolated struct CancelMaintenancePayload: Encodable {
    let reason: String?
}

/// Mirrors `MaintenanceController.RateBody`.
nonisolated struct RateMaintenancePayload: Encodable {
    let rating: Int
    let feedback: String?
}

/// Returned by every workflow-changing endpoint (cancel, rate). We don't
/// use the new status — the next refresh will pick it up — but decoding
/// the envelope keeps the generic `send` happy.
nonisolated struct MaintenanceWorkflowResponse: Decodable {
    let maintenanceId: UUID
    let status: String
}

/// Returned by POST /api/Maintenance.
nonisolated struct CreateMaintenanceResponse: Decodable {
    let maintenanceRequestId: UUID
}
