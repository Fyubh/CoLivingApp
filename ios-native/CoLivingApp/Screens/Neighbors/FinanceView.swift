import SwiftUI

/// Финансы pane inside `NeighborsView`. Two stacked sections — per-roommate
/// **Баланс** at the top, then the apartment-scoped **Расходы** feed.
/// Tapping a row I owe opens a settle sheet; the toolbar `+` opens the
/// expense form sheet. Empty states cover three flavours: no roommates,
/// roommates but zero balance, and no expenses yet.
struct FinanceView: View {
    let apartment: MyApartmentContextDto
    let auth: AuthStore

    @State private var store: FinanceStore
    @State private var showingForm: Bool = false
    @State private var settleTarget: UserBalanceDto?

    init(apartment: MyApartmentContextDto, auth: AuthStore) {
        self.apartment = apartment
        self.auth = auth
        _store = State(initialValue: FinanceStore(apartment: apartment, auth: auth))
    }

    var body: some View {
        content
            .toolbar {
                ToolbarItem(placement: .primaryAction) {
                    Button { showingForm = true } label: {
                        Image(systemName: "plus")
                            .font(.body.weight(.semibold))
                    }
                    .tint(AppColor.conifer)
                }
            }
            .task { await store.bootstrap() }
            .sheet(isPresented: $showingForm) {
                ExpenseFormSheet(
                    store: store,
                    onDismiss: { showingForm = false }
                )
            }
            .sheet(item: $settleTarget) { target in
                SettleDebtSheet(
                    target: target,
                    store: store,
                    onDismiss: { settleTarget = nil }
                )
            }
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView().tint(AppColor.conifer)
                .frame(maxWidth: .infinity, maxHeight: .infinity)
        } else if !store.hasLoaded, let error = store.errorMessage {
            FinanceErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else {
            ScrollView {
                VStack(alignment: .leading, spacing: Spacing.s24) {
                    BalanceSection(
                        balances: store.balances,
                        onSettle: { settleTarget = $0 }
                    )
                    ExpensesSection(
                        expenses: store.expenses,
                        currentUserId: apartment.meUserId
                    )
                }
                .padding(.horizontal, Spacing.s20)
                .padding(.top, Spacing.s12)
                .padding(.bottom, Spacing.s32)
            }
            .refreshable { await store.refresh() }
        }
    }
}

// MARK: - Balance section

private struct BalanceSection: View {
    let balances: [UserBalanceDto]
    let onSettle: (UserBalanceDto) -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            SectionHeader("Баланс")

            if balances.isEmpty {
                GlassCard {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        Text("Соседей пока нет")
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                        Text("Здесь появятся взаиморасчёты, когда менеджер добавит соседей в вашу квартиру.")
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                    }
                }
            } else if balances.allSatisfy({ $0.balance == 0 }) {
                GlassCard {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        Text("Всё сведено")
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                        Text("Никто никому не должен.")
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                    }
                }
            } else {
                GlassCard(padding: 0) {
                    VStack(spacing: 0) {
                        ForEach(Array(balances.enumerated()), id: \.element.id) { idx, b in
                            if idx > 0 {
                                Rectangle()
                                    .fill(AppColor.hairline)
                                    .frame(height: 0.5)
                            }
                            BalanceRow(balance: b, onSettle: onSettle)
                        }
                    }
                }
            }
        }
    }
}

private struct BalanceRow: View {
    let balance: UserBalanceDto
    let onSettle: (UserBalanceDto) -> Void

    private var iOweThem: Bool { balance.balance < 0 }
    private var theyOweMe: Bool { balance.balance > 0 }
    private var amountLabel: String { formatMoney(abs(balance.balance)) }

    var body: some View {
        HStack(spacing: Spacing.s12) {
            Avatar(balance.userName, size: .sm)

            VStack(alignment: .leading, spacing: 2) {
                Text(balance.userName)
                    .appText(.bodyMed)
                    .foregroundStyle(AppColor.ink)
                Text(subtitle)
                    .appText(.footnote)
                    .foregroundStyle(AppColor.inkSecondary)
            }

            Spacer(minLength: Spacing.s8)

            VStack(alignment: .trailing, spacing: 4) {
                Text(amountLabel)
                    .appText(.bodyMed)
                    .foregroundStyle(amountColor)
                if iOweThem {
                    Button("Вернуть") { onSettle(balance) }
                        .font(.footnote.weight(.semibold))
                        .foregroundStyle(AppColor.conifer)
                        .buttonStyle(.plain)
                }
            }
        }
        .padding(.horizontal, Spacing.s16)
        .padding(.vertical, Spacing.s12)
    }

    private var subtitle: String {
        if theyOweMe { return "должен(на) вам" }
        if iOweThem  { return "вы должны" }
        return "вровень"
    }

    private var amountColor: Color {
        if theyOweMe { return AppColor.success }
        if iOweThem  { return AppColor.danger }
        return AppColor.inkSecondary
    }
}

// MARK: - Expenses section

private struct ExpensesSection: View {
    let expenses: [ExpenseDto]
    let currentUserId: String

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            SectionHeader("Расходы")

            if expenses.isEmpty {
                GlassCard {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        Text("Расходов пока нет")
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                        Text("Добавьте первый общий расход — оплата делится поровну между соседями.")
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                    }
                }
            } else {
                GlassCard(padding: 0) {
                    VStack(spacing: 0) {
                        ForEach(Array(expenses.enumerated()), id: \.element.id) { idx, exp in
                            if idx > 0 {
                                Rectangle()
                                    .fill(AppColor.hairline)
                                    .frame(height: 0.5)
                            }
                            ExpenseRow(expense: exp, isMine: exp.payerId == currentUserId)
                        }
                    }
                }
            }
        }
    }
}

private struct ExpenseRow: View {
    let expense: ExpenseDto
    let isMine: Bool

    var body: some View {
        HStack(spacing: Spacing.s12) {
            Image(systemName: expense.category?.icon ?? "ellipsis")
                .font(.system(size: 16, weight: .regular))
                .foregroundStyle(AppColor.conifer)
                .frame(width: 32, height: 32)
                .background(Circle().fill(AppColor.conifer.opacity(0.08)))

            VStack(alignment: .leading, spacing: 2) {
                Text(expense.description)
                    .appText(.bodyMed)
                    .foregroundStyle(AppColor.ink)
                    .lineLimit(1)
                Text(subtitle)
                    .appText(.footnote)
                    .foregroundStyle(AppColor.inkSecondary)
                    .lineLimit(1)
            }

            Spacer(minLength: Spacing.s8)

            Text(formatMoney(expense.amount))
                .appText(.bodyMed)
                .foregroundStyle(AppColor.ink)
        }
        .padding(.horizontal, Spacing.s16)
        .padding(.vertical, Spacing.s12)
    }

    private var subtitle: String {
        let payer = isMine ? "вы" : expense.payerName
        let cat = expense.category?.label
        let date = formatRelativeDate(expense.date)
        if let cat { return "\(payer) · \(cat) · \(date)" }
        return "\(payer) · \(date)"
    }
}

// MARK: - Helpers

private func formatMoney(_ value: Decimal) -> String {
    let nf = NumberFormatter()
    nf.numberStyle = .currency
    nf.currencyCode = "EUR"
    nf.locale = Locale(identifier: "ru_RU")
    nf.maximumFractionDigits = (value.isWhole ? 0 : 2)
    return nf.string(from: value as NSDecimalNumber) ?? "\(value) €"
}

private func formatRelativeDate(_ date: Date) -> String {
    let f = RelativeDateTimeFormatter()
    f.locale = Locale(identifier: "ru_RU")
    f.unitsStyle = .short
    return f.localizedString(for: date, relativeTo: Date())
}

private extension Decimal {
    var isWhole: Bool {
        var copy = self
        var rounded = Decimal()
        NSDecimalRound(&rounded, &copy, 0, .plain)
        return rounded == self
    }
}

// MARK: - Error

private struct FinanceErrorState: View {
    let message: String
    let onRetry: () -> Void

    var body: some View {
        VStack(spacing: Spacing.s16) {
            Image(systemName: "exclamationmark.triangle")
                .font(.system(size: 32, weight: .light))
                .foregroundStyle(AppColor.danger.opacity(0.7))
            Text(message)
                .appText(.body)
                .foregroundStyle(AppColor.inkSecondary)
                .multilineTextAlignment(.center)
            Button("Повторить", action: onRetry)
                .buttonStyle(SecondaryButtonStyle(size: .md, fullWidth: false))
        }
        .padding(Spacing.s24)
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }
}
