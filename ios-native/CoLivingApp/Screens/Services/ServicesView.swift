import SwiftUI

/// Services tab — the resident-side maintenance flow plus visual-only
/// placeholders for Bookings / VIP / PS5 / Contract (gated by
/// `AppConfig.showVisualPlaceholders` until those backends land).
///
/// Owns its own `MaintenanceStore` as `@State` (TabView keeps the view
/// alive across tab switches, so the store survives). The store fetches
/// `/Apartments/my-context` independently from HomeStore — same trade-off
/// we made for ProfileStore.
struct ServicesView: View {
    @State private var store: MaintenanceStore
    @State private var showingForm: Bool = false
    @State private var selectedRequest: MaintenanceRequestDto?

    init(auth: AuthStore) {
        _store = State(initialValue: MaintenanceStore(auth: auth))
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()
                content
            }
            .navigationTitle("Услуги")
            .toolbar {
                ToolbarItem(placement: .primaryAction) {
                    if store.apartment != nil {
                        Button {
                            showingForm = true
                        } label: {
                            Image(systemName: "plus")
                                .font(.body.weight(.semibold))
                        }
                        .tint(AppColor.conifer)
                    }
                }
            }
        }
        .task { await store.bootstrap() }
        .sheet(isPresented: $showingForm) {
            if let context = store.apartment {
                MaintenanceFormSheet(
                    context: context,
                    store: store,
                    onDismiss: { showingForm = false }
                )
            }
        }
        .sheet(item: $selectedRequest) { request in
            MaintenanceDetailSheet(
                request: request,
                store: store,
                onDismiss: { selectedRequest = nil }
            )
        }
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView().tint(AppColor.conifer)
        } else if !store.hasLoaded, let error = store.errorMessage {
            ErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else {
            ScrollView {
                VStack(alignment: .leading, spacing: Spacing.s24) {
                    MaintenanceSection(
                        requests: store.requests,
                        hasApartment: store.apartment != nil,
                        onTap: { selectedRequest = $0 }
                    )

                    if AppConfig.showVisualPlaceholders {
                        ComingSoonSection()
                    }
                }
                .padding(.horizontal, Spacing.s20)
                .padding(.top, Spacing.s12)
                .padding(.bottom, Spacing.s32)
            }
            .refreshable { await store.refresh() }
        }
    }
}

// MARK: - Sections

private struct MaintenanceSection: View {
    let requests: [MaintenanceRequestDto]
    let hasApartment: Bool
    let onTap: (MaintenanceRequestDto) -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            SectionHeader("Заявки")

            if !hasApartment {
                GlassCard {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        Text("Вы пока не заселены")
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                        Text("Заявки на обслуживание появятся, когда менеджер закрепит за вами квартиру.")
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                    }
                }
            } else {
                MaintenanceList(requests: requests, onTap: onTap)
            }
        }
    }
}

private struct ComingSoonSection: View {
    private let items: [(icon: String, title: String, hint: String)] = [
        ("calendar",     "Бронирования", "Переговорки и общие пространства"),
        ("crown",        "VIP-сервис",   "Консьерж, доставки, прачечная"),
        ("gamecontroller","PS5 / X-Box", "Бронирование игровой приставки"),
        ("doc.text",     "Договор",      "Условия, оплата, продление")
    ]

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            SectionHeader("Скоро")

            GlassCard(padding: 0) {
                VStack(spacing: 0) {
                    ForEach(Array(items.enumerated()), id: \.offset) { idx, item in
                        if idx > 0 {
                            Rectangle()
                                .fill(AppColor.hairline)
                                .frame(height: 0.5)
                        }
                        Row(icon: item.icon, title: item.title, hint: item.hint)
                    }
                }
            }
        }
    }

    private struct Row: View {
        let icon: String
        let title: String
        let hint: String

        var body: some View {
            HStack(spacing: Spacing.s12) {
                Image(systemName: icon)
                    .font(.system(size: 16, weight: .regular))
                    .foregroundStyle(AppColor.inkSecondary)
                    .frame(width: 28, height: 28)
                    .background(Circle().fill(AppColor.inkSecondary.opacity(0.06)))

                VStack(alignment: .leading, spacing: 2) {
                    Text(title)
                        .appText(.bodyMed)
                        .foregroundStyle(AppColor.ink)
                    Text(hint)
                        .appText(.footnote)
                        .foregroundStyle(AppColor.inkSecondary)
                }

                Spacer(minLength: Spacing.s8)

                Pill("Скоро", variant: .neutral)
            }
            .padding(.horizontal, Spacing.s16)
            .padding(.vertical, Spacing.s12)
        }
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

#Preview {
    ServicesView(auth: AuthStore())
}
