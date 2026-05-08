import SwiftUI

/// Root view that picks Login / ChangePassword / MainPlaceholder by reading
/// `AuthStore.state`. State transitions cross-fade so the auth → app jump
/// doesn't snap. The store is owned here so it survives the whole session.
struct AuthFlow: View {
    @State private var auth = AuthStore()

    var body: some View {
        Group {
            switch auth.state {
            case .signedOut:
                LoginView(
                    error: auth.error,
                    isLoading: auth.isLoading,
                    onSubmit: { email, password in
                        Task { await auth.signIn(email: email, password: password) }
                    }
                )

            case .requiresPasswordChange:
                ChangePasswordView(
                    error: auth.error,
                    isLoading: auth.isLoading,
                    onSubmit: { old, new in
                        Task { await auth.changePassword(oldPassword: old, newPassword: new) }
                    }
                )

            case .signedIn:
                RootTabView(onSignOut: { auth.signOut() })
            }
        }
        .animation(.easeInOut(duration: 0.32), value: auth.state)
        .transition(.opacity)
    }
}

#Preview {
    AuthFlow()
}
