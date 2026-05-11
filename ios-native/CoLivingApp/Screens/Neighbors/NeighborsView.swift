import SwiftUI

/// Соседи tab — four roommate-scoped modules behind a segmented picker:
/// **Финансы** (Expenses + Balance), **Покупки** (shared Inventory),
/// **Уборка** (Chores rotation), **Чат** (apartment-scoped messaging with
/// delete-own / report / block moderation — added in Phase 7.3).
///
/// Owns `NeighborsStore` (apartment context). Each pane owns its own list
/// store — wired in 6.1 / 6.2 / 6.3 / 7.3.
struct NeighborsView: View {
    @State private var store: NeighborsStore
    @State private var section: NeighborsSection = .finance

    private let auth: AuthStore

    init(auth: AuthStore) {
        self.auth = auth
        _store = State(initialValue: NeighborsStore(auth: auth))
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()
                content
            }
            .navigationTitle("Соседи")
        }
        .task { await store.bootstrap() }
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView().tint(AppColor.conifer)
        } else if !store.hasLoaded, let error = store.errorMessage {
            NeighborsErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else if let apartment = store.apartment {
            loaded(apartment: apartment)
        } else {
            EmptyApartmentState(onRefresh: { await store.refresh() })
        }
    }

    @ViewBuilder
    private func loaded(apartment: MyApartmentContextDto) -> some View {
        VStack(spacing: 0) {
            Picker("", selection: $section) {
                ForEach(NeighborsSection.allCases) { s in
                    Text(s.label).tag(s)
                }
            }
            .pickerStyle(.segmented)
            .padding(.horizontal, Spacing.s20)
            .padding(.top, Spacing.s8)
            .padding(.bottom, Spacing.s12)

            switch section {
            case .finance:
                FinanceView(apartment: apartment, auth: auth)
            case .products:
                ProductsView(apartment: apartment, auth: auth)
            case .cleaning:
                CleaningView(apartment: apartment, auth: auth)
            case .chat:
                ChatView(apartment: apartment, auth: auth)
            }
        }
    }
}

enum NeighborsSection: String, CaseIterable, Identifiable {
    case finance, products, cleaning, chat
    var id: String { rawValue }
    var label: String {
        switch self {
        case .finance:  return "Финансы"
        case .products: return "Покупки"
        case .cleaning: return "Уборка"
        case .chat:     return "Чат"
        }
    }
}

private struct NeighborsErrorState: View {
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
    NeighborsView(auth: AuthStore())
}
