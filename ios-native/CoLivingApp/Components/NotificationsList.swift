import SwiftUI

/// Stack of notification rows wrapped in one glass panel — one container
/// keeps the section readable on Linen instead of the iOS-default per-row
/// card stutter. Tapping an unread row hands its id back via `onTap`;
/// HomeStore handles the optimistic flip and POST.
struct NotificationsList: View {
    let notifications: [ResidentNotificationDto]
    let onTap: (UUID) -> Void

    var body: some View {
        if notifications.isEmpty {
            EmptyState()
        } else {
            GlassCard(padding: 0) {
                VStack(spacing: 0) {
                    ForEach(Array(notifications.enumerated()), id: \.element.id) { idx, item in
                        if idx > 0 {
                            Rectangle()
                                .fill(AppColor.hairline)
                                .frame(height: 0.5)
                        }
                        Button {
                            if !item.isRead { onTap(item.id) }
                        } label: {
                            NotificationRow(item: item)
                        }
                        .buttonStyle(.plain)
                    }
                }
            }
        }
    }
}

private struct NotificationRow: View {
    let item: ResidentNotificationDto

    var body: some View {
        HStack(alignment: .top, spacing: Spacing.s12) {
            UnreadDot(item: item)
                .padding(.top, 7)

            VStack(alignment: .leading, spacing: 4) {
                HStack(alignment: .firstTextBaseline, spacing: Spacing.s8) {
                    Text(item.title)
                        .font(item.isRead ? .subheadline : .subheadline.weight(.semibold))
                        .foregroundStyle(item.isRead ? AppColor.inkSecondary : AppColor.ink)
                        .lineLimit(2)
                        .frame(maxWidth: .infinity, alignment: .leading)

                    if item.isImportant {
                        Pill("Важно", variant: .danger)
                    }
                }

                Text(item.body)
                    .appText(.body)
                    .foregroundStyle(AppColor.inkSecondary)
                    .lineLimit(3)

                Text(Self.relative(item.createdAt))
                    .appText(.footnote)
                    .foregroundStyle(AppColor.inkTertiary)
                    .padding(.top, 2)
            }
        }
        .padding(.horizontal, Spacing.s16)
        .padding(.vertical, Spacing.s12)
        .frame(maxWidth: .infinity, alignment: .leading)
        .contentShape(Rectangle())
    }

    /// Russian relative time. Locale is forced to `ru_RU` until proper
    /// localization lands (Phase 8) — App Review demo will be on an English
    /// device, but the app's primary copy is Russian.
    private static func relative(_ date: Date) -> String {
        let f = RelativeDateTimeFormatter()
        f.unitsStyle = .short
        f.locale = Locale(identifier: "ru_RU")
        return f.localizedString(for: date, relativeTo: Date())
    }
}

private struct UnreadDot: View {
    let item: ResidentNotificationDto

    var body: some View {
        let color: Color = item.isRead
            ? .clear
            : (item.isImportant ? AppColor.danger : AppColor.conifer)
        Circle()
            .fill(color)
            .frame(width: 8, height: 8)
    }
}

private struct EmptyState: View {
    var body: some View {
        GlassCard {
            VStack(spacing: Spacing.s8) {
                Image(systemName: "bell.slash")
                    .font(.system(size: 28, weight: .light))
                    .foregroundStyle(AppColor.inkTertiary)
                Text("Уведомлений пока нет")
                    .appText(.body)
                    .foregroundStyle(AppColor.inkSecondary)
            }
            .frame(maxWidth: .infinity)
            .padding(.vertical, Spacing.s16)
        }
    }
}

#Preview {
    ZStack {
        AppBackground()
        ScrollView {
            VStack(spacing: 16) {
                NotificationsList(notifications: [], onTap: { _ in })
                NotificationsList(notifications: [
                    .init(id: UUID(), buildingId: UUID(),
                          title: "Плановое отключение воды",
                          body: "Завтра с 09:00 до 14:00 в стояке № 2 будет отключена холодная вода.",
                          isImportant: true, isRead: false,
                          createdAt: Date().addingTimeInterval(-3600), readAt: nil),
                    .init(id: UUID(), buildingId: UUID(),
                          title: "Доставка посылки",
                          body: "Курьер оставил вашу посылку на ресепшн.",
                          isImportant: false, isRead: false,
                          createdAt: Date().addingTimeInterval(-7200), readAt: nil),
                    .init(id: UUID(), buildingId: UUID(),
                          title: "Новости здания",
                          body: "В коммуналке появилась настольная игра. Берите играйте.",
                          isImportant: false, isRead: true,
                          createdAt: Date().addingTimeInterval(-86400),
                          readAt: Date().addingTimeInterval(-3600))
                ], onTap: { _ in })
            }
            .padding()
        }
    }
}
