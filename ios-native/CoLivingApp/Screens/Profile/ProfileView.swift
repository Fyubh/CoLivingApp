import SwiftUI

/// Phase 2 placeholder with a working sign-out so the auth flow can be
/// re-tested without redeploying. Phase 4 fills this with apartment summary,
/// account list, support links, and account-deletion via mailto.
struct ProfileView: View {
    var onSignOut: () -> Void

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                VStack(spacing: Spacing.s24) {
                    TabPlaceholderBody(
                        icon: "person.crop.circle",
                        hint: "Профиль, контекст квартиры и настройки."
                    )

                    Button("Выйти", role: .destructive) {
                        onSignOut()
                    }
                    .buttonStyle(SecondaryButtonStyle(size: .md, fullWidth: false))
                    .padding(.bottom, Spacing.s32)
                }
            }
            .navigationTitle("Профиль")
        }
    }
}

#Preview {
    ProfileView(onSignOut: {})
}
