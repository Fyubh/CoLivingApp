import SwiftUI

/// Sheet form for creating a shared expense. The current user is recorded
/// as the payer server-side (controller pulls payerId from the JWT). Splits
/// across all active roommates are computed by the backend handler — the
/// form only needs description / amount / category.
struct ExpenseFormSheet: View {
    let store: FinanceStore
    let onDismiss: () -> Void

    @State private var description: String = ""
    @State private var amountText: String = ""
    @State private var category: ExpenseCategory = .groceries

    private var parsedAmount: Decimal? {
        let trimmed = amountText
            .replacingOccurrences(of: ",", with: ".")
            .trimmingCharacters(in: .whitespaces)
        guard !trimmed.isEmpty else { return nil }
        guard let value = Decimal(string: trimmed, locale: Locale(identifier: "en_US_POSIX")) else {
            return nil
        }
        return value > 0 ? value : nil
    }

    private var canSubmit: Bool {
        !description.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty &&
        parsedAmount != nil &&
        !store.isSubmitting
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                ScrollView {
                    VStack(alignment: .leading, spacing: Spacing.s20) {
                        FieldGroup(label: "Описание") {
                            AppTextField(
                                icon: nil,
                                placeholder: "Например, «Продукты в Lidl»",
                                text: $description
                            )
                        }

                        FieldGroup(label: "Сумма (€)") {
                            AppTextField(
                                icon: "eurosign",
                                placeholder: "0,00",
                                text: $amountText
                            )
                            #if canImport(UIKit)
                            .keyboardType(.decimalPad)
                            #endif
                        }

                        FieldGroup(label: "Категория") {
                            CategoryPicker(selection: $category)
                        }

                        if let err = store.errorMessage {
                            Text(err)
                                .appText(.body)
                                .foregroundStyle(AppColor.danger)
                        }

                        Button("Добавить расход") { submit() }
                            .buttonStyle(PrimaryButtonStyle(size: .md))
                            .disabled(!canSubmit)
                            .padding(.top, Spacing.s8)
                    }
                    .padding(Spacing.s20)
                }
            }
            .navigationTitle("Новый расход")
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
        let trimmedDescription = description.trimmingCharacters(in: .whitespacesAndNewlines)
        Task {
            let ok = await store.createExpense(
                amount: amount,
                description: trimmedDescription,
                category: category
            )
            if ok { onDismiss() }
        }
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

private struct CategoryPicker: View {
    @Binding var selection: ExpenseCategory

    var body: some View {
        Menu {
            ForEach(ExpenseCategory.allCases) { c in
                Button {
                    selection = c
                } label: {
                    Label(c.label, systemImage: c.icon)
                }
            }
        } label: {
            HStack(spacing: Spacing.s12) {
                Image(systemName: selection.icon)
                    .font(.system(size: 16, weight: .medium))
                    .foregroundStyle(AppColor.conifer)
                    .frame(width: 22)
                Text(selection.label)
                    .appText(.body)
                    .foregroundStyle(AppColor.ink)
                Spacer()
                Image(systemName: "chevron.up.chevron.down")
                    .font(.footnote.weight(.semibold))
                    .foregroundStyle(AppColor.inkTertiary)
            }
            .padding(.horizontal, Spacing.s16)
            .frame(height: 54)
            .glassEffect(
                .regular,
                in: RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
            )
            .overlay(
                RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
                    .strokeBorder(AppColor.glassStroke, lineWidth: 0.5)
            )
        }
    }
}
