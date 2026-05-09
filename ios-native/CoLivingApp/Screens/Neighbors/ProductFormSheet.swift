import SwiftUI

/// Sheet form for adding a shared inventory item. The new item enters the
/// list with `Status = Available`. Expiry is optional — most household
/// items don't have a useful one.
struct ProductFormSheet: View {
    let store: ProductsStore
    let onDismiss: () -> Void

    @State private var name: String = ""
    @State private var quantityText: String = "1"
    @State private var unit: UnitType = .piece
    @State private var category: ItemCategory = .food
    @State private var location: StorageLocation = .pantry
    @State private var hasExpiry: Bool = false
    @State private var expiryDate: Date = Date()

    private var parsedQuantity: Decimal? {
        let trimmed = quantityText
            .replacingOccurrences(of: ",", with: ".")
            .trimmingCharacters(in: .whitespaces)
        guard !trimmed.isEmpty,
              let value = Decimal(string: trimmed, locale: Locale(identifier: "en_US_POSIX")),
              value > 0
        else { return nil }
        return value
    }

    private var canSubmit: Bool {
        !name.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty &&
        parsedQuantity != nil &&
        !store.isSubmitting
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                ScrollView {
                    VStack(alignment: .leading, spacing: Spacing.s20) {
                        FieldGroup(label: "Название") {
                            AppTextField(
                                icon: nil,
                                placeholder: "Например, «Молоко»",
                                text: $name
                            )
                        }

                        HStack(spacing: Spacing.s12) {
                            FieldGroup(label: "Количество") {
                                AppTextField(
                                    icon: nil,
                                    placeholder: "1",
                                    text: $quantityText
                                )
                                #if canImport(UIKit)
                                .keyboardType(.decimalPad)
                                #endif
                            }
                            .frame(maxWidth: .infinity)

                            FieldGroup(label: "Единица") {
                                EnumMenu(selection: $unit) { $0.shortLabel }
                            }
                            .frame(maxWidth: .infinity)
                        }

                        FieldGroup(label: "Категория") {
                            EnumMenu(selection: $category, icon: { $0.icon }) { $0.label }
                        }

                        FieldGroup(label: "Где хранится") {
                            EnumMenu(selection: $location, icon: { $0.icon }) { $0.label }
                        }

                        FieldGroup(label: "Срок годности") {
                            VStack(alignment: .leading, spacing: Spacing.s8) {
                                Toggle("Указать срок", isOn: $hasExpiry)
                                    .tint(AppColor.conifer)
                                if hasExpiry {
                                    DatePicker(
                                        "",
                                        selection: $expiryDate,
                                        in: Date()...,
                                        displayedComponents: .date
                                    )
                                    .labelsHidden()
                                    .tint(AppColor.conifer)
                                }
                            }
                        }

                        if let err = store.errorMessage {
                            Text(err)
                                .appText(.body)
                                .foregroundStyle(AppColor.danger)
                        }

                        Button("Добавить продукт") { submit() }
                            .buttonStyle(PrimaryButtonStyle(size: .md))
                            .disabled(!canSubmit)
                            .padding(.top, Spacing.s8)
                    }
                    .padding(Spacing.s20)
                }
            }
            .navigationTitle("Новый продукт")
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
        guard let qty = parsedQuantity else { return }
        let trimmed = name.trimmingCharacters(in: .whitespacesAndNewlines)
        Task {
            let ok = await store.createItem(
                name: trimmed,
                quantity: qty,
                unit: unit,
                category: category,
                location: location
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

/// Generic Menu picker over a `CaseIterable` enum. Optional `icon` closure
/// renders a leading SF Symbol matching the selected case.
private struct EnumMenu<E: Hashable & CaseIterable & Identifiable>: View where E.AllCases: RandomAccessCollection {
    @Binding var selection: E
    var icon: ((E) -> String)?
    let label: (E) -> String

    init(
        selection: Binding<E>,
        icon: ((E) -> String)? = nil,
        label: @escaping (E) -> String
    ) {
        self._selection = selection
        self.icon = icon
        self.label = label
    }

    var body: some View {
        Menu {
            ForEach(Array(E.allCases)) { c in
                Button {
                    selection = c
                } label: {
                    if let icon {
                        Label(label(c), systemImage: icon(c))
                    } else {
                        Text(label(c))
                    }
                }
            }
        } label: {
            HStack(spacing: Spacing.s12) {
                if let icon {
                    Image(systemName: icon(selection))
                        .font(.system(size: 16, weight: .medium))
                        .foregroundStyle(AppColor.conifer)
                        .frame(width: 22)
                }
                Text(label(selection))
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
