import SwiftUI

/// One row in the maintenance list. Three layers of information:
/// (1) category icon + title + status pill — the summary glance,
/// (2) one-line description preview, (3) a footer with timestamp and any
/// non-Normal priority pill. Tapping opens the detail sheet.
struct MaintenanceCard: View {
    let request: MaintenanceRequestDto
    let onTap: () -> Void

    var body: some View {
        Button(action: onTap) {
            VStack(alignment: .leading, spacing: Spacing.s12) {
                HStack(alignment: .top, spacing: Spacing.s12) {
                    Image(systemName: request.category.icon)
                        .font(.system(size: 16, weight: .medium))
                        .foregroundStyle(AppColor.conifer)
                        .frame(width: 28, height: 28)
                        .background(
                            Circle().fill(AppColor.conifer.opacity(0.10))
                        )

                    VStack(alignment: .leading, spacing: 2) {
                        Text(request.title)
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                            .lineLimit(2)
                            .multilineTextAlignment(.leading)
                        Text(request.category.label)
                            .appText(.footnote)
                            .foregroundStyle(AppColor.inkSecondary)
                    }

                    Spacer(minLength: Spacing.s8)

                    StatusPill(status: request.status)
                }

                if !request.description.isEmpty {
                    Text(request.description)
                        .appText(.body)
                        .foregroundStyle(AppColor.inkSecondary)
                        .lineLimit(2)
                        .multilineTextAlignment(.leading)
                }

                HStack(spacing: Spacing.s8) {
                    Text(Self.relative(request.createdAt))
                        .appText(.footnote)
                        .foregroundStyle(AppColor.inkTertiary)

                    if request.priority != .normal {
                        PriorityPill(priority: request.priority)
                    }

                    Spacer(minLength: 0)

                    if let rating = request.residentRating, request.status == .completed {
                        StarRating(value: rating, size: 12)
                    }
                }
            }
            .padding(Spacing.s16)
            .frame(maxWidth: .infinity, alignment: .leading)
            .contentShape(Rectangle())
        }
        .buttonStyle(.plain)
    }

    /// Short relative date in Russian, locked to `ru_RU` until proper
    /// localization lands (matches NotificationsList).
    private static func relative(_ date: Date) -> String {
        let f = RelativeDateTimeFormatter()
        f.unitsStyle = .short
        f.locale = Locale(identifier: "ru_RU")
        return f.localizedString(for: date, relativeTo: Date())
    }
}

/// Container for a list of MaintenanceCards. Single GlassCard surface like
/// NotificationsList — avoids per-row card stutter on Linen.
struct MaintenanceList: View {
    let requests: [MaintenanceRequestDto]
    let onTap: (MaintenanceRequestDto) -> Void

    var body: some View {
        if requests.isEmpty {
            EmptyMaintenanceState()
        } else {
            GlassCard(padding: 0) {
                VStack(spacing: 0) {
                    ForEach(Array(requests.enumerated()), id: \.element.id) { idx, item in
                        if idx > 0 {
                            Rectangle()
                                .fill(AppColor.hairline)
                                .frame(height: 0.5)
                        }
                        MaintenanceCard(request: item) { onTap(item) }
                    }
                }
            }
        }
    }
}

private struct EmptyMaintenanceState: View {
    var body: some View {
        GlassCard {
            VStack(spacing: Spacing.s8) {
                Image(systemName: "wrench.and.screwdriver")
                    .font(.system(size: 28, weight: .light))
                    .foregroundStyle(AppColor.inkTertiary)
                Text("Заявок пока нет")
                    .appText(.body)
                    .foregroundStyle(AppColor.inkSecondary)
            }
            .frame(maxWidth: .infinity)
            .padding(.vertical, Spacing.s16)
        }
    }
}
