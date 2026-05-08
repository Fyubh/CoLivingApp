import SwiftUI

/// Shared body for Phase 2 tab placeholders. SF Symbol icon, "В разработке"
/// caption, short hint text on Linen background. Pulled out so the five tab
/// stubs stay one-screen each and the empty-state language is consistent.
struct TabPlaceholderBody: View {
    let icon: String
    let hint: String

    var body: some View {
        ZStack {
            AppBackground()

            VStack(spacing: Spacing.s18) {
                Image(systemName: icon)
                    .font(.system(size: 44, weight: .light))
                    .foregroundStyle(AppColor.inkTertiary)

                VStack(spacing: Spacing.s8) {
                    Text("В разработке")
                        .appText(.bodyMed)
                        .foregroundStyle(AppColor.inkSecondary)

                    Text(hint)
                        .appText(.body)
                        .foregroundStyle(AppColor.inkTertiary)
                        .multilineTextAlignment(.center)
                        .padding(.horizontal, Spacing.s24)
                }
            }
            .frame(maxWidth: .infinity, maxHeight: .infinity)
        }
    }
}

#Preview {
    NavigationStack {
        TabPlaceholderBody(icon: "house", hint: "Контекст здания, уведомления и быстрые действия.")
            .navigationTitle("Главная")
    }
}
