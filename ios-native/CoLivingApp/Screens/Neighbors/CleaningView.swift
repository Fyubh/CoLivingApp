import SwiftUI

/// Уборка pane — list of apartment chores grouped by status. Swipe actions
/// drive the workflow:
///   * Pending row, `canComplete=true`  → leading swipe «Готово»
///   * NeedsReview row, `canReview=true` → leading swipe «Принять» /
///     trailing swipe «Отклонить» (destructive, opens a confirmation
///     alert because the backend creates a €5 penalty expense)
struct CleaningView: View {
    let apartment: MyApartmentContextDto
    let auth: AuthStore

    @State private var store: ChoresStore
    @State private var showingForm: Bool = false
    @State private var rejectTarget: ChoreDto?

    init(apartment: MyApartmentContextDto, auth: AuthStore) {
        self.apartment = apartment
        self.auth = auth
        _store = State(initialValue: ChoresStore(apartment: apartment, auth: auth))
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
                ChoreFormSheet(
                    apartment: apartment,
                    store: store,
                    onDismiss: { showingForm = false }
                )
            }
            .alert(
                "Отклонить уборку?",
                isPresented: rejectAlertBinding,
                presenting: rejectTarget
            ) { chore in
                Button("Отклонить", role: .destructive) {
                    Task { _ = await store.reject(choreId: chore.id) }
                }
                Button("Отмена", role: .cancel) { }
            } message: { _ in
                Text("Исполнителю автоматически начислится штраф €5 в общих расходах квартиры.")
            }
    }

    private var rejectAlertBinding: Binding<Bool> {
        Binding(
            get: { rejectTarget != nil },
            set: { if !$0 { rejectTarget = nil } }
        )
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView().tint(AppColor.conifer)
                .frame(maxWidth: .infinity, maxHeight: .infinity)
        } else if !store.hasLoaded, let error = store.errorMessage {
            CleaningErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else if store.chores.isEmpty {
            emptyState
        } else {
            choreList
        }
    }

    @ViewBuilder
    private var emptyState: some View {
        ScrollView {
            VStack(spacing: Spacing.s16) {
                Image(systemName: "sparkles")
                    .font(.system(size: 36, weight: .light))
                    .foregroundStyle(AppColor.inkSecondary)
                    .padding(.top, Spacing.s32)

                Text("Уборка не запланирована")
                    .appText(.title)
                    .foregroundStyle(AppColor.ink)
                    .multilineTextAlignment(.center)

                Text("Добавьте первую задачу — соседи увидят её в этом списке.")
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
    private var choreList: some View {
        let pending = store.chores.filter { $0.choreStatus == .pending }
        let review = store.chores.filter { $0.choreStatus == .needsReview }
        let done = store.chores.filter { $0.choreStatus == .completed }

        List {
            if !pending.isEmpty {
                Section("К выполнению") { rows(for: pending) }
            }
            if !review.isEmpty {
                Section("На проверке") { rows(for: review) }
            }
            if !done.isEmpty {
                Section("Сделано") { rows(for: done) }
            }
        }
        .listStyle(.plain)
        .scrollContentBackground(.hidden)
        .refreshable { await store.refresh() }
    }

    @ViewBuilder
    private func rows(for chores: [ChoreDto]) -> some View {
        ForEach(chores) { chore in
            ChoreRow(chore: chore)
                .swipeActions(edge: .leading, allowsFullSwipe: false) {
                    if chore.canComplete {
                        Button {
                            Task { _ = await store.complete(choreId: chore.id) }
                        } label: {
                            Label("Готово", systemImage: "checkmark.circle")
                        }
                        .tint(AppColor.conifer)
                    } else if chore.canReview {
                        Button {
                            Task { _ = await store.confirm(choreId: chore.id) }
                        } label: {
                            Label("Принять", systemImage: "hand.thumbsup")
                        }
                        .tint(AppColor.conifer)
                    }
                }
                .swipeActions(edge: .trailing, allowsFullSwipe: false) {
                    if chore.canReview {
                        Button(role: .destructive) {
                            rejectTarget = chore
                        } label: {
                            Label("Отклонить", systemImage: "hand.thumbsdown")
                        }
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
}

// MARK: - Row

private struct ChoreRow: View {
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

                statusPill
            }
        }
    }

    private var subtitle: String? {
        var parts: [String] = []
        if let assignee = chore.assignedName {
            parts.append("Исполняет: \(assignee)")
        }
        if let due = chore.dueDate {
            parts.append("до \(formatDate(due))")
        }
        return parts.isEmpty ? nil : parts.joined(separator: " · ")
    }

    @ViewBuilder
    private var statusPill: some View {
        switch chore.choreStatus {
        case .pending:
            Pill(chore.choreStatus.label, variant: .neutral)
        case .needsReview:
            Pill(chore.choreStatus.label, variant: .warning)
        case .completed:
            Pill(chore.choreStatus.label, variant: .success)
        }
    }
}

// MARK: - Helpers

private func formatDate(_ date: Date) -> String {
    let f = DateFormatter()
    f.locale = Locale(identifier: "ru_RU")
    f.dateFormat = "d MMM"
    return f.string(from: date)
}

// MARK: - Error

private struct CleaningErrorState: View {
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
