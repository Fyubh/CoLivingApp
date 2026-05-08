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
                } else if store.isLoading {
                    ProgressView().tint(AppColor.conifer)
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
