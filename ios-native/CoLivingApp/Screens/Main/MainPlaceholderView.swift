import SwiftUI

/// Phase 1 stub for the signed-in state. Phase 2 replaces it with the
/// 5-tab `RootTabView`. Kept dead simple — proof that auth completed,
/// plus a sign-out so we can re-test the flow without redeploying.
struct MainPlaceholderView: View {
    var onSignOut: () -> Void

    var body: some View {
        ZStack {
            AppBackground()

            VStack(spacing: Spacing.s24) {
                Image(systemName: "checkmark.seal.fill")
                    .font(.system(size: 56, weight: .light))
                    .foregroundStyle(AppColor.conifer)

                VStack(spacing: Spacing.s8) {
                    Text("Вы вошли")
                        .appText(.display)
                        .foregroundStyle(AppColor.ink)
                    Text("Главный экран — следующий шаг.")
                        .appText(.body)
                        .foregroundStyle(AppColor.inkSecondary)
                        .multilineTextAlignment(.center)
                }

                Button("Выйти", role: .destructive) {
                    onSignOut()
                }
                .buttonStyle(SecondaryButtonStyle(size: .md, fullWidth: false))
                .padding(.top, Spacing.s16)
            }
            .padding(.horizontal, Spacing.s24)
        }
        .preferredColorScheme(.light)
    }
}

#Preview {
    MainPlaceholderView(onSignOut: {})
}
