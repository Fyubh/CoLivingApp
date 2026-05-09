import SwiftUI

/// Покупки pane — Apartment-shared inventory. Single bucket: items with
/// `Status = Available`. Each row offers swipe-to-Consume on the leading
/// edge and swipe-to-Remove on the trailing edge; the toolbar `+` opens
/// `ProductFormSheet`.
struct ProductsView: View {
    let apartment: MyApartmentContextDto
    let auth: AuthStore

    @State private var store: ProductsStore
    @State private var showingForm: Bool = false

    init(apartment: MyApartmentContextDto, auth: AuthStore) {
        self.apartment = apartment
        self.auth = auth
        _store = State(initialValue: ProductsStore(apartment: apartment, auth: auth))
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
                ProductFormSheet(
                    store: store,
                    onDismiss: { showingForm = false }
                )
            }
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView().tint(AppColor.conifer)
                .frame(maxWidth: .infinity, maxHeight: .infinity)
        } else if !store.hasLoaded, let error = store.errorMessage {
            ProductsErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else if store.items.isEmpty {
            emptyState
        } else {
            itemList
        }
    }

    @ViewBuilder
    private var emptyState: some View {
        ScrollView {
            VStack(spacing: Spacing.s16) {
                Image(systemName: "shippingbox")
                    .font(.system(size: 36, weight: .light))
                    .foregroundStyle(AppColor.inkSecondary)
                    .padding(.top, Spacing.s32)

                Text("На полке пусто")
                    .appText(.title)
                    .foregroundStyle(AppColor.ink)
                    .multilineTextAlignment(.center)

                Text("Добавьте первый общий продукт — соседи увидят его в этом списке.")
                    .appText(.body)
                    .foregroundStyle(AppColor.inkSecondary)
                    .multilineTextAlignment(.center)
                    .padding(.horizontal, Spacing.s24)
            }
            .frame(maxWidth: .infinity)
            .padding(.top, Spacing.s32)
        }
        .refreshable { await store.refresh() }
    }

    @ViewBuilder
    private var itemList: some View {
        List {
            ForEach(store.items) { item in
                ItemRow(item: item)
                    .swipeActions(edge: .leading, allowsFullSwipe: false) {
                        Button {
                            Task { _ = await store.consume(itemId: item.id) }
                        } label: {
                            Label("Закончилось", systemImage: "xmark.circle")
                        }
                        .tint(AppColor.conifer)
                    }
                    .swipeActions(edge: .trailing, allowsFullSwipe: false) {
                        Button(role: .destructive) {
                            Task { _ = await store.remove(itemId: item.id) }
                        } label: {
                            Label("Удалить", systemImage: "trash")
                        }
                    }
                    .listRowBackground(Color.clear)
                    .listRowSeparator(.hidden)
                    .listRowInsets(EdgeInsets(
                        top: Spacing.s6, leading: Spacing.s20,
                        bottom: Spacing.s6, trailing: Spacing.s20
                    ))
            }
        }
        .listStyle(.plain)
        .scrollContentBackground(.hidden)
        .refreshable { await store.refresh() }
    }
}

// MARK: - Row

private struct ItemRow: View {
    let item: ItemDto

    var body: some View {
        GlassCard(padding: Spacing.s16) {
            HStack(spacing: Spacing.s12) {
                Image(systemName: item.itemCategory?.icon ?? "shippingbox")
                    .font(.system(size: 16, weight: .regular))
                    .foregroundStyle(AppColor.conifer)
                    .frame(width: 36, height: 36)
                    .background(Circle().fill(AppColor.conifer.opacity(0.08)))

                VStack(alignment: .leading, spacing: 2) {
                    Text(item.name)
                        .appText(.bodyMed)
                        .foregroundStyle(AppColor.ink)
                        .lineLimit(1)
                    if let subtitle = subtitle {
                        Text(subtitle)
                            .appText(.footnote)
                            .foregroundStyle(AppColor.inkSecondary)
                            .lineLimit(1)
                    }
                }

                Spacer(minLength: Spacing.s8)

                Text(quantityLabel)
                    .appText(.bodyMed)
                    .foregroundStyle(AppColor.ink)
            }
        }
    }

    private var quantityLabel: String {
        let qty = formatQuantity(item.quantity)
        if let unit = item.unitType?.shortLabel {
            return "\(qty) \(unit)"
        }
        return qty
    }

    private var subtitle: String? {
        var parts: [String] = []
        if let loc = item.storageLocation?.label {
            parts.append(loc)
        }
        if let expiry = item.expiryDate {
            parts.append("до \(formatExpiry(expiry))")
        }
        return parts.isEmpty ? nil : parts.joined(separator: " · ")
    }
}

// MARK: - Helpers

private func formatQuantity(_ value: Decimal) -> String {
    let nf = NumberFormatter()
    nf.locale = Locale(identifier: "ru_RU")
    nf.minimumFractionDigits = 0
    nf.maximumFractionDigits = 2
    return nf.string(from: value as NSDecimalNumber) ?? "\(value)"
}

private func formatExpiry(_ date: Date) -> String {
    let f = DateFormatter()
    f.locale = Locale(identifier: "ru_RU")
    f.dateFormat = "d MMM"
    return f.string(from: date)
}

// MARK: - Error

private struct ProductsErrorState: View {
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
