import SwiftUI

/// A tappable row inside a Profile section: SF Symbol + title (+ optional
/// subtitle) + chevron. Used for support / privacy / delete-account entries.
/// Pass `tint: AppColor.danger` for destructive variants — only the icon is
/// tinted, the title stays in ink so the row reads as a navigation row first
/// and a destructive action second.
struct ProfileLinkRow: View {
    let icon: String
    let title: String
    var subtitle: String? = nil
    var tint: Color = AppColor.conifer
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            HStack(spacing: Spacing.s12) {
                Image(systemName: icon)
                    .font(.system(size: 18, weight: .regular))
                    .foregroundStyle(tint)
                    .frame(width: 24, alignment: .center)

                VStack(alignment: .leading, spacing: 2) {
                    Text(title)
                        .appText(.bodyMed)
                        .foregroundStyle(AppColor.ink)
                    if let subtitle {
                        Text(subtitle)
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                            .lineLimit(1)
                            .truncationMode(.middle)
                    }
                }

                Spacer(minLength: Spacing.s8)

                Image(systemName: "chevron.right")
                    .font(.footnote.weight(.semibold))
                    .foregroundStyle(AppColor.inkTertiary)
            }
            .padding(.vertical, Spacing.s12)
            .contentShape(Rectangle())
        }
        .buttonStyle(.plain)
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(spacing: 0) {
            GlassCard(padding: Spacing.s8) {
                VStack(spacing: 0) {
                    ProfileLinkRow(
                        icon: "envelope",
                        title: "Связаться с поддержкой",
                        subtitle: "support@coliving-os.example",
                        action: {}
                    )
                    Divider().padding(.leading, 36)
                    ProfileLinkRow(
                        icon: "lock.shield",
                        title: "Политика конфиденциальности",
                        action: {}
                    )
                    Divider().padding(.leading, 36)
                    ProfileLinkRow(
                        icon: "trash",
                        title: "Удалить аккаунт",
                        tint: AppColor.danger,
                        action: {}
                    )
                }
            }
        }
        .padding()
    }
}
