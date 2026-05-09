import SwiftUI

/// Sheet form for paying down what the current user owes a roommate.
/// Suggested default amount is the full debt (`abs(balance)`); the user can
/// edit downward for partial payments. Server records this as a
/// `Settlement(senderId=me, receiverId=target, amount)` and the next
/// balance refresh closes the loop.
///
/// Balance row only opens this sheet when `balance < 0` (we owe), so we
/// don't have to handle the inverse case here.
struct SettleDebtSheet: View {
    let target: UserBalanceDto
    let store: FinanceStore
    let onDismiss: () -> Void

    @State private var amountText: String = ""

    init(target: UserBalanceDto, store: FinanceStore, onDismiss: @escaping () -> Void) {
        self.target = target
        self.store = store
        self.onDismiss = onDismiss
        let suggested = abs(target.balance)
        _amountText = State(initialValue: Self.format(suggested))
    }

    private var parsedAmount: Decimal? {
        let trimmed = amountText
            .replacingOccurrences(of: ",", with: ".")
            .trimmingCharacters(in: .whitespaces)
        guard !trimmed.isEmpty,
              let value = Decimal(string: trimmed, locale: Locale(identifier: "en_US_POSIX")),
              value > 0
        else { return nil }
        return value
    }

    private var canSubmit: Bool {
        parsedAmount != nil && !store.isSubmitting
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                ScrollView {
                    VStack(alignment: .leading, spacing: Spacing.s20) {
                        GlassCard {
                            HStack(spacing: Spacing.s12) {
                                Avatar(target.userName, size: .md)
                                VStack(alignment: .leading, spacing: 2) {
                                    Text(target.userName)
                                        .appText(.bodyMed)
                                        .foregroundStyle(AppColor.ink)
                                    Text("Долг: \(formatMoney(abs(target.balance)))")
                                        .appText(.footnote)
                                        .foregroundStyle(AppColor.inkSecondary)
                                }
                                Spacer(minLength: 0)
                            }
                        }

                        FieldGroup(label: "Сумма перевода (€)") {
                            AppTextField(
                                icon: "eurosign",
                                placeholder: "0,00",
                                text: $amountText
                            )
                            #if canImport(UIKit)
                            .keyboardType(.decimalPad)
                            #endif
                        }

                        Text("Запись о переводе появится в общей истории. Вы платите вне приложения — здесь только фиксируете факт.")
                            .appText(.footnote)
                            .foregroundStyle(AppColor.inkSecondary)

                        if let err = store.errorMessage {
                            Text(err)
                                .appText(.body)
                                .foregroundStyle(AppColor.danger)
                        }

                        Button("Отметить перевод") { submit() }
                            .buttonStyle(PrimaryButtonStyle(size: .md))
                            .disabled(!canSubmit)
                            .padding(.top, Spacing.s8)
                    }
                    .padding(Spacing.s20)
                }
            }
            .navigationTitle("Возврат долга")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { onDismiss() }
                        .tint(AppColor.conifer)
                }
            }
        }
    }

    private func submit() {
        guard let amount = parsedAmount else { return }
        Task {
            let ok = await store.settleDebt(receiverId: target.userId, amount: amount)
            if ok { onDismiss() }
        }
    }

    private static func format(_ value: Decimal) -> String {
        let nf = NumberFormatter()
        nf.numberStyle = .decimal
        nf.locale = Locale(identifier: "en_US_POSIX")
        nf.minimumFractionDigits = 0
        nf.maximumFractionDigits = 2
        return nf.string(from: value as NSDecimalNumber) ?? "\(value)"
    }
}

// MARK: - Pieces

private struct FieldGroup<Content: View>: View {
    let label: String
    @ViewBuilder let content: Content

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            Text(label)
                .font(.caption2.weight(.semibold))
                .tracking(0.6)
                .textCase(.uppercase)
                .foregroundStyle(AppColor.inkSecondary)
            content
        }
    }
}

private func formatMoney(_ value: Decimal) -> String {
    let nf = NumberFormatter()
    nf.numberStyle = .currency
    nf.currencyCode = "EUR"
    nf.locale = Locale(identifier: "ru_RU")
    return nf.string(from: value as NSDecimalNumber) ?? "\(value) €"
}
