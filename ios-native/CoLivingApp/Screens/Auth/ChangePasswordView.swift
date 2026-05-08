import SwiftUI

/// First-login password change. Backend contract: `POST /Users/change-password`
/// with `{ oldPassword, newPassword }` — min 8 chars new, must differ from old.
/// Confirm-field is client-side only (typo-prevention).
///
/// External state (`error`, `isLoading`) is driven by the parent (typically
/// `AuthFlow` reading from `AuthStore`); local `@State` holds the three
/// password fields since the parent never needs them after submit.
struct ChangePasswordView: View {
    var error: String?
    var isLoading: Bool
    var onSubmit: (_ oldPassword: String, _ newPassword: String) -> Void

    @State private var oldPassword: String = ""
    @State private var newPassword: String = ""
    @State private var confirmPassword: String = ""

    @FocusState private var focusedField: Field?
    private enum Field { case old, new, confirm }

    private var newPasswordTooShort: Bool {
        !newPassword.isEmpty && newPassword.count < 8
    }

    private var passwordsMismatch: Bool {
        !confirmPassword.isEmpty && newPassword != confirmPassword
    }

    private var canSubmit: Bool {
        !oldPassword.isEmpty
            && newPassword.count >= 8
            && confirmPassword == newPassword
            && oldPassword != newPassword
    }

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
            Text("Создайте\nпароль")
                .appText(.hero)
                .foregroundStyle(AppColor.ink)
                .multilineTextAlignment(.leading)
                .fixedSize(horizontal: false, vertical: true)

            Text("Замените временный пароль на постоянный, чтобы продолжить.")
                .appText(.body)
                .foregroundStyle(AppColor.inkSecondary)
                .fixedSize(horizontal: false, vertical: true)
                .padding(.top, Spacing.s4)
        }
    }

    private var form: some View {
        VStack(spacing: Spacing.s12) {
            AppTextField(
                icon: "lock",
                placeholder: "Временный пароль",
                text: $oldPassword,
                kind: .password,
                hasError: error != nil
            )
            .focused($focusedField, equals: .old)
            .onSubmit { focusedField = .new }

            VStack(alignment: .leading, spacing: Spacing.s6) {
                AppTextField(
                    icon: "lock",
                    placeholder: "Новый пароль",
                    text: $newPassword,
                    kind: .password,
                    hasError: newPasswordTooShort
                )
                .focused($focusedField, equals: .new)
                .onSubmit { focusedField = .confirm }

                Text(newPasswordTooShort ? "Минимум 8 символов" : "Не менее 8 символов")
                    .appText(.footnote)
                    .foregroundStyle(
                        newPasswordTooShort
                            ? AppColor.danger
                            : (newPassword.count >= 8 ? AppColor.conifer : AppColor.inkTertiary)
                    )
                    .padding(.horizontal, Spacing.s4)
                    .animation(.easeInOut(duration: 0.18), value: newPasswordTooShort)
                    .animation(.easeInOut(duration: 0.18), value: newPassword.count)
            }

            VStack(alignment: .leading, spacing: Spacing.s6) {
                AppTextField(
                    icon: "lock",
                    placeholder: "Повторите новый пароль",
                    text: $confirmPassword,
                    kind: .password,
                    hasError: passwordsMismatch
                )
                .focused($focusedField, equals: .confirm)
                .onSubmit { attemptSave() }

                if passwordsMismatch {
                    Text("Пароли не совпадают")
                        .appText(.footnote)
                        .foregroundStyle(AppColor.danger)
                        .padding(.horizontal, Spacing.s4)
                        .transition(.opacity)
                }
            }
        }
    }

    private var submit: some View {
        Button {
            attemptSave()
        } label: {
            ZStack {
                Text("Сохранить и продолжить").opacity(isLoading ? 0 : 1)
                if isLoading {
                    ProgressView()
                        .tint(.white)
                        .controlSize(.small)
                }
            }
        }
        .buttonStyle(PrimaryButtonStyle())
        .disabled(isLoading || !canSubmit)
    }

    private var footer: some View {
        Text("Не помните временный пароль? Свяжитесь с администратором дома.")
            .appText(.footnote)
            .foregroundStyle(AppColor.inkTertiary)
            .multilineTextAlignment(.leading)
            .fixedSize(horizontal: false, vertical: true)
    }

    private func attemptSave() {
        focusedField = nil
        guard canSubmit else { return }
        onSubmit(oldPassword, newPassword)
    }
}

#Preview("ChangePassword") {
    ChangePasswordView(error: nil, isLoading: false, onSubmit: { _, _ in })
}

#Preview("ChangePassword — ошибка") {
    ChangePasswordView(
        error: "Неверный текущий пароль.",
        isLoading: false,
        onSubmit: { _, _ in }
    )
}
