import SwiftUI

/// Maintenance status chip. Three colour buckets — neutral (open / in-flight),
/// success (completed), danger (cancelled / rejected) — keep the list
/// visually scannable without learning seven shades.
struct StatusPill: View {
    let status: MaintenanceStatus

    var body: some View {
        Pill(status.label, variant: variant)
    }

    private var variant: Pill.Variant {
        switch status {
        case .reported, .acknowledged, .assigned, .inProgress:
            return .neutral
        case .completed:
            return .success
        case .cancelled, .rejected:
            return .danger
        }
    }
}

/// Priority chip — only rendered for non-Normal priorities. Normal is the
/// implicit default, no need to mark every card.
struct PriorityPill: View {
    let priority: MaintenancePriority

    var body: some View {
        if priority == .normal {
            EmptyView()
        } else {
            Pill(priority.label, variant: variant, icon: priority == .urgent ? "exclamationmark.triangle.fill" : nil)
        }
    }

    private var variant: Pill.Variant {
        switch priority {
        case .low:    return .neutral
        case .normal: return .neutral
        case .high:   return .warning
        case .urgent: return .danger
        }
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(alignment: .leading, spacing: 12) {
            StatusPill(status: .reported)
            StatusPill(status: .acknowledged)
            StatusPill(status: .assigned)
            StatusPill(status: .inProgress)
            StatusPill(status: .completed)
            StatusPill(status: .cancelled)
            StatusPill(status: .rejected)
            Divider()
            PriorityPill(priority: .high)
            PriorityPill(priority: .urgent)
        }
        .padding()
    }
}
