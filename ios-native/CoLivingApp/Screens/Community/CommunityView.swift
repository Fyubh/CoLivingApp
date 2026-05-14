import SwiftUI

/// Сообщество tab. Сегментированный контрол: «Чат» — чат всего здания
/// (8.1), «Эвенты» и «Барахолка» — заглушки до 8.3 / 8.7+. Контекст
/// (`MyApartmentContextDto`) грузится один раз на вкладку и шарится между
/// сегментами; именно из него берётся `buildingId` для чата.
struct CommunityView: View {
    let auth: AuthStore
    let realtime: RealtimeService

    @State private var store: CommunityStore
    @State private var section: CommunitySection = .chat

    init(auth: AuthStore, realtime: RealtimeService) {
        self.auth = auth
        self.realtime = realtime
        _store = State(initialValue: CommunityStore(auth: auth))
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()
                content
            }
            .navigationTitle("Сообщество")
        }
        .task { await store.bootstrap() }
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView().tint(AppColor.conifer)
        } else if !store.hasLoaded, let error = store.errorMessage {
            CommunityErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else if let apartment = store.apartment, let buildingId = apartment.buildingId {
            loaded(apartment: apartment, buildingId: buildingId)
        } else {
            EmptyApartmentState(onRefresh: { await store.refresh() })
        }
    }

    @ViewBuilder
    private func loaded(apartment: MyApartmentContextDto, buildingId: UUID) -> some View {
        VStack(spacing: 0) {
            Picker("", selection: $section) {
                ForEach(CommunitySection.allCases) { s in
                    Text(s.label).tag(s)
                }
            }
            .pickerStyle(.segmented)
            .padding(.horizontal, Spacing.s20)
            .padding(.top, Spacing.s8)
            .padding(.bottom, Spacing.s12)

            switch section {
            case .chat:
                BuildingChatView(
                    buildingId: buildingId,
                    buildingName: apartment.buildingName,
                    currentUserId: apartment.meUserId,
                    auth: auth,
                    realtime: realtime
                )
            case .events:
                TabPlaceholderBody(
                    icon: "calendar",
                    hint: "Ивенты появятся в следующем апдейте."
                )
            case .marketplace:
                TabPlaceholderBody(
                    icon: "bag",
                    hint: "Барахолка появится после релиза."
                )
            }
        }
    }
}

enum CommunitySection: String, CaseIterable, Identifiable {
    case chat, events, marketplace
    var id: String { rawValue }
    var label: String {
        switch self {
        case .chat:        return "Чат"
        case .events:      return "Эвенты"
        case .marketplace: return "Барахолка"
        }
    }
}

private struct CommunityErrorState: View {
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
