import SwiftUI

/// External state (`error`, `isLoading`) is driven by the parent (typically
/// `AuthFlow` reading from `AuthStore`); the form keeps email/password in
/// local `@State` since the parent never needs them after submit.
struct LoginView: View {
    var error: String?
    var isLoading: Bool
    var onSubmit: (_ email: String, _ password: String) -> Void

    @State private var email: String = ""
    @State private var password: String = ""

    @FocusState private var focusedField: Field?
    private enum Field { case email, password }

    var body: some View {
        ZStack {
            AppBackground()

            ScrollView {
                VStack(alignment: .leading, spacing: 0) {
                    brandMark
                        .padding(.top, Spacing.s30)

                    hero
                        .padding(.top, Spacing.s30)

                    form
                        .padding(.top, Spacing.s30)

                    if let error {
                        Text(error)
                            .appText(.footMed)
                            .foregroundStyle(AppColor.danger)
                            .padding(.top, Spacing.s12)
                            .padding(.horizontal, Spacing.s4)
                            .transition(.opacity.combined(with: .move(edge: .top)))
                    }

                    submit
                        .padding(.top, Spacing.s24)

                    footer
                        .padding(.top, Spacing.s30)
                        .padding(.bottom, Spacing.s30)
                }
                .padding(.horizontal, Spacing.s24)
                .frame(maxWidth: .infinity, alignment: .leading)
            }
            .scrollDismissesKeyboard(.interactively)
        }
        .preferredColorScheme(.light)
        .animation(.spring(response: 0.32, dampingFraction: 0.82), value: error)
    }

    // Tiny vertical Conifer rule + caps wordmark — reads as a building plate,
    // not a logo. Keeps the surface clean while the hero carries the moment.
    private var brandMark: some View {
        HStack(spacing: Spacing.s12) {
            Rectangle()
                .fill(AppColor.conifer)
                .frame(width: 3, height: 22)
            Text("CO·LIVING")
                .font(.caption2.weight(.semibold))
                .tracking(1.6)
                .foregroundStyle(AppColor.inkSecondary)
        }
    }

    private var hero: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            Text("Добро\nпожаловать")
                .appText(.hero)
                .foregroundStyle(AppColor.ink)
                .multilineTextAlignment(.leading)
                .fixedSize(horizontal: false, vertical: true)

            Text("Войдите данными, которые выдал администратор дома.")
                .appText(.body)
                .foregroundStyle(AppColor.inkSecondary)
                .fixedSize(horizontal: false, vertical: true)
                .padding(.top, Spacing.s4)
        }
    }

    private var form: some View {
        VStack(spacing: Spacing.s12) {
            AppTextField(
                icon: "envelope",
                placeholder: "E-mail",
                text: $email,
                kind: .email,
                hasError: error != nil
            )
            .focused($focusedField, equals: .email)
            .onSubmit { focusedField = .password }

            AppTextField(
                icon: "lock",
                placeholder: "Пароль",
                text: $password,
                kind: .password,
                hasError: error != nil
            )
            .focused($focusedField, equals: .password)
            .onSubmit { attemptSignIn() }
        }
    }

    private var submit: some View {
        Button {
            attemptSignIn()
        } label: {
            ZStack {
                Text("Войти").opacity(isLoading ? 0 : 1)
                if isLoading {
                    ProgressView()
                        .tint(.white)
                        .controlSize(.small)
                }
            }
        }
        .buttonStyle(PrimaryButtonStyle())
        .disabled(isLoading || email.isEmpty || password.isEmpty)
    }

    private var footer: some View {
        Text("Не получается войти? Свяжитесь с администратором дома.")
            .appText(.footnote)
            .foregroundStyle(AppColor.inkTertiary)
            .multilineTextAlignment(.leading)
            .fixedSize(horizontal: false, vertical: true)
    }

    private func attemptSignIn() {
        focusedField = nil
        guard !email.isEmpty, !password.isEmpty else { return }
        onSubmit(email, password)
    }
}

#Preview("Login") {
    LoginView(error: nil, isLoading: false, onSubmit: { _, _ in })
}

#Preview("Login — ошибка") {
    LoginView(
        error: "Неверный e-mail или пароль.",
        isLoading: false,
        onSubmit: { _, _ in }
    )
}

#Preview("Login — загрузка") {
    LoginView(error: nil, isLoading: true, onSubmit: { _, _ in })
}
