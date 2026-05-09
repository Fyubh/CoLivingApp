import SwiftUI

/// Соседи tab — three roommate-scoped modules behind a segmented picker:
/// **Финансы** (Expenses + Balance), **Покупки** (shared Inventory),
/// **Уборка** (Chores rotation). Chat is intentionally not shipped in MVP —
/// the backend lacks the moderation primitives (delete / report / block)
/// that App Review requires for user-generated message surfaces.
///
/// Owns `NeighborsStore` (apartment context). Each pane owns its own list
/// store — wired in 6.1 / 6.2 / 6.3.
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
                NeighborsPendingPane(
                    hint: "Общий список покупок появится в следующей сборке."
                )
            case .cleaning:
                NeighborsPendingPane(
                    hint: "Очерёдность уборки появится в следующей сборке."
                )
            }
        }
    }
}

enum NeighborsSection: String, CaseIterable, Identifiable {
    case finance, products, cleaning
    var id: String { rawValue }
    var label: String {
        switch self {
        case .finance:  return "Финансы"
        case .products: return "Покупки"
        case .cleaning: return "Уборка"
        }
    }
}

private struct NeighborsPendingPane: View {
    let hint: String

    var body: some View {
        VStack(spacing: Spacing.s16) {
            Image(systemName: "ellipsis.circle")
                .font(.system(size: 36, weight: .light))
                .foregroundStyle(AppColor.inkTertiary)
            Text(hint)
                .appText(.body)
                .foregroundStyle(AppColor.inkSecondary)
                .multilineTextAlignment(.center)
                .padding(.horizontal, Spacing.s24)
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
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
