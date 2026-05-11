import SwiftUI

/// Home tab — DoorCard hero, apartment context card, building notifications.
/// Owns the `HomeStore` as `@State`; the store is constructed once with the
/// session's `AuthStore` (passed in by `RootTabView`) and survives tab
/// switches because `TabView` keeps the view alive.
struct HomeView: View {
    @State private var store: HomeStore

    init(auth: AuthStore) {
        _store = State(initialValue: HomeStore(auth: auth))
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                if let context = store.apartment {
                    LoadedContent(context: context, store: store)
                } else if store.hasLoaded {
                    // Loaded successfully but the user has no active
                    // ApartmentMember — backend returned `null`. Distinct
                    // from the error path: refresh, don't retry.
                    EmptyApartmentState {
                        await store.refresh()
                    }
                } else if let error = store.errorMessage {
                    ErrorState(message: error) {
                        Task { await store.refresh() }
                    }
                } else {
                    ProgressView().tint(AppColor.conifer)
                }
            }
            .navigationTitle("Главная")
        }
        .task { await store.bootstrap() }
    }
}

private struct LoadedContent: View {
    let context: MyApartmentContextDto
    let store: HomeStore

    var body: some View {
        ScrollView {
            VStack(spacing: Spacing.s24) {
                DoorCard(roomLabel: Self.doorLabel(for: context))

                ApartmentSummaryCard(context: context)

                VStack(alignment: .leading, spacing: Spacing.s8) {
                    SectionHeader("Уведомления")
                    NotificationsList(notifications: store.notifications) { id in
                        Task { await store.markRead(id: id) }
                    }
                }

                if !store.inventoryPreview.isEmpty {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        SectionHeader("Покупки")
                        InventoryPreviewList(items: store.inventoryPreview)
                    }
                }

                if let chore = store.nextChore {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        SectionHeader("Ближайшая уборка")
                        NextChoreCard(chore: chore)
                    }
                }
            }
            .padding(.horizontal, Spacing.s20)
            .padding(.top, Spacing.s12)
            .padding(.bottom, Spacing.s32)
        }
        .refreshable { await store.refresh() }
    }

    /// Compose a DoorCard label from MyApartmentContextDto. Prefer the short
    /// `unitNumber` ("12B") prefixed by "Квартира" so DoorCard's split logic
    /// peels off the kicker; fall back to `apartmentName` (e.g. "Penthouse")
    /// which DoorCard renders as a non-numeric hero phrase.
    static func doorLabel(for context: MyApartmentContextDto) -> String {
        let lead: String
        if let unit = context.unitNumber, !unit.isEmpty {
            lead = "Квартира \(unit)"
        } else {
            lead = context.apartmentName
        }
        if let building = context.buildingName, !building.isEmpty {
            return "\(lead) · \(building)"
        }
        return lead
    }
}

private struct ErrorState: View {
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
    }
}

// MARK: - Home aggregation tiles

/// Top-N Available inventory rows wrapped in one glass panel (same container
/// shape as `NotificationsList`). Read-only — taps and swipes live in
/// `ProductsView`; this is just a glance for the Home dashboard.
private struct InventoryPreviewList: View {
    let items: [ItemDto]

    var body: some View {
        GlassCard(padding: 0) {
            VStack(spacing: 0) {
                ForEach(Array(items.enumerated()), id: \.element.id) { idx, item in
                    if idx > 0 {
                        Rectangle()
                            .fill(AppColor.hairline)
                            .frame(height: 0.5)
                    }
                    InventoryPreviewRow(item: item)
                }
            }
        }
    }
}

private struct InventoryPreviewRow: View {
    let item: ItemDto

    var body: some View {
        HStack(spacing: Spacing.s12) {
            Image(systemName: item.itemCategory?.icon ?? "shippingbox")
                .font(.system(size: 14, weight: .regular))
                .foregroundStyle(AppColor.conifer)
                .frame(width: 28, height: 28)
                .background(Circle().fill(AppColor.conifer.opacity(0.08)))

            Text(item.name)
                .appText(.bodyMed)
                .foregroundStyle(AppColor.ink)
                .lineLimit(1)
                .frame(maxWidth: .infinity, alignment: .leading)

            Text(quantityLabel)
                .appText(.footnote)
                .foregroundStyle(AppColor.inkSecondary)
        }
        .padding(.horizontal, Spacing.s16)
        .padding(.vertical, Spacing.s12)
    }

    private var quantityLabel: String {
        let qty = formatQuantity(item.quantity)
        if let unit = item.unitType?.shortLabel {
            return "\(qty) \(unit)"
        }
        return qty
    }
}

/// Single-card preview for the earliest-due pending chore. Mirrors the
/// CleaningView row shape so users recognise the same data on both screens.
/// Read-only on Home — completion/review actions live in `CleaningView`.
private struct NextChoreCard: View {
    let chore: ChoreDto

    var body: some View {
        GlassCard(padding: Spacing.s16) {
            HStack(alignment: .top, spacing: Spacing.s12) {
                Image(systemName: chore.choreCategory?.icon ?? "checkmark.circle")
                    .font(.system(size: 16, weight: .regular))
                    .foregroundStyle(AppColor.conifer)
                    .frame(width: 36, height: 36)
                    .background(Circle().fill(AppColor.conifer.opacity(0.08)))

                VStack(alignment: .leading, spacing: 4) {
                    Text(chore.title)
                        .appText(.bodyMed)
                        .foregroundStyle(AppColor.ink)
                        .lineLimit(2)

                    if let subtitle {
                        Text(subtitle)
                            .appText(.footnote)
                            .foregroundStyle(AppColor.inkSecondary)
                            .lineLimit(1)
                    }
                }

                Spacer(minLength: Spacing.s8)
            }
        }
    }

    private var subtitle: String? {
        var parts: [String] = []
        if let assignee = chore.assignedName {
            parts.append("Исполняет: \(assignee)")
        }
        if let due = chore.dueDate {
            parts.append("до \(formatChoreDate(due))")
        }
        return parts.isEmpty ? nil : parts.joined(separator: " · ")
    }
}

private func formatQuantity(_ value: Decimal) -> String {
    let nf = NumberFormatter()
    nf.locale = Locale(identifier: "ru_RU")
    nf.minimumFractionDigits = 0
    nf.maximumFractionDigits = 2
    return nf.string(from: value as NSDecimalNumber) ?? "\(value)"
}

private func formatChoreDate(_ date: Date) -> String {
    let f = DateFormatter()
    f.locale = Locale(identifier: "ru_RU")
    f.dateFormat = "d MMM"
    return f.string(from: date)
}
