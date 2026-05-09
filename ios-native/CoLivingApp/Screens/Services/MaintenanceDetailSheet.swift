import SwiftUI

/// Detail view for a single maintenance request. Read-mostly: surfaces
/// description, contractor name, completion notes; offers two workflow
/// actions when the status allows them — Cancel (resident, before
/// InProgress) and Rate (resident, after Completed). Both go through
/// `MaintenanceStore` so the list refreshes automatically.
struct MaintenanceDetailSheet: View {
    let request: MaintenanceRequestDto
    let store: MaintenanceStore
    let onDismiss: () -> Void

    @State private var pendingRating: Int = 0
    @State private var feedback: String = ""
    @State private var showCancelConfirm: Bool = false

    private var isUnrated: Bool {
        request.status == .completed && request.residentRating == nil
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                ScrollView {
                    VStack(alignment: .leading, spacing: Spacing.s24) {
                        Header(request: request)

                        Description(text: request.description)

                        if let assignee = request.assignedStaffName {
                            FactRow(
                                icon: "person.crop.square",
                                label: "Мастер",
                                value: assignee
                            )
                        }

                        if let notes = request.completionNotes, !notes.isEmpty {
                            CompletionNotes(notes: notes)
                        }

                        if let rating = request.residentRating {
                            RatedRow(rating: rating)
                        } else if isUnrated {
                            RateInline(
                                rating: $pendingRating,
                                feedback: $feedback,
                                isSubmitting: store.isSubmitting,
                                onSubmit: submitRating
                            )
                        }

                        if request.status.cancellable {
                            Button("Отменить заявку", role: .destructive) {
                                showCancelConfirm = true
                            }
                            .buttonStyle(SecondaryButtonStyle(size: .md))
                        }

                        if let err = store.errorMessage {
                            Text(err)
                                .appText(.body)
                                .foregroundStyle(AppColor.danger)
                        }
                    }
                    .padding(Spacing.s20)
                }
            }
            .navigationTitle("Заявка")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .confirmationAction) {
                    Button("Готово") { onDismiss() }
                        .tint(AppColor.conifer)
                }
            }
            .alert("Отменить заявку?",
                   isPresented: $showCancelConfirm) {
                Button("Не сейчас", role: .cancel) { }
                Button("Отменить", role: .destructive) { submitCancel() }
            } message: {
                Text("Если проблема уже решилась — отметьте заявку отменённой, чтобы мастер не пришёл напрасно.")
            }
        }
    }

    // MARK: - Actions

    private func submitCancel() {
        Task {
            let ok = await store.cancel(id: request.id, reason: nil)
            if ok { onDismiss() }
        }
    }

    private func submitRating() {
        guard pendingRating > 0 else { return }
        let trimmed = feedback.trimmingCharacters(in: .whitespacesAndNewlines)
        let body: String? = trimmed.isEmpty ? nil : trimmed
        Task {
            let ok = await store.rate(id: request.id, rating: pendingRating, feedback: body)
            if ok { onDismiss() }
        }
    }
}

// MARK: - Sections

private struct Header: View {
    let request: MaintenanceRequestDto

    var body: some View {
        GlassCard {
            VStack(alignment: .leading, spacing: Spacing.s12) {
                HStack(alignment: .top, spacing: Spacing.s12) {
                    Image(systemName: request.category.icon)
                        .font(.system(size: 18, weight: .medium))
                        .foregroundStyle(AppColor.conifer)
                        .frame(width: 32, height: 32)
                        .background(Circle().fill(AppColor.conifer.opacity(0.10)))

                    VStack(alignment: .leading, spacing: 2) {
                        Text(request.title)
                            .appText(.title)
                            .foregroundStyle(AppColor.ink)
                        Text(request.category.label)
                            .appText(.footnote)
                            .foregroundStyle(AppColor.inkSecondary)
                    }

                    Spacer(minLength: 0)
                }

                HStack(spacing: Spacing.s8) {
                    StatusPill(status: request.status)
                    if request.priority != .normal {
                        PriorityPill(priority: request.priority)
                    }
                    Spacer(minLength: 0)
                }

                Text(Self.absoluteDate(request.createdAt))
                    .appText(.footnote)
                    .foregroundStyle(AppColor.inkTertiary)
            }
        }
    }

    private static func absoluteDate(_ date: Date) -> String {
        let f = DateFormatter()
        f.locale = Locale(identifier: "ru_RU")
        f.dateStyle = .medium
        f.timeStyle = .short
        return "Создана \(f.string(from: date))"
    }
}

private struct Description: View {
    let text: String
    var body: some View {
        if text.isEmpty {
            EmptyView()
        } else {
            VStack(alignment: .leading, spacing: Spacing.s8) {
                Text("Описание")
                    .font(.caption2.weight(.semibold))
                    .tracking(0.6)
                    .textCase(.uppercase)
                    .foregroundStyle(AppColor.inkSecondary)

                GlassCard {
                    Text(text)
                        .appText(.body)
                        .foregroundStyle(AppColor.ink)
                        .frame(maxWidth: .infinity, alignment: .leading)
                }
            }
        }
    }
}

private struct FactRow: View {
    let icon: String
    let label: String
    let value: String

    var body: some View {
        GlassCard {
            HStack(spacing: Spacing.s12) {
                Image(systemName: icon)
                    .font(.system(size: 16, weight: .medium))
                    .foregroundStyle(AppColor.conifer)
                    .frame(width: 22)
                VStack(alignment: .leading, spacing: 2) {
                    Text(label)
                        .appText(.footnote)
                        .foregroundStyle(AppColor.inkSecondary)
                    Text(value)
                        .appText(.bodyMed)
                        .foregroundStyle(AppColor.ink)
                }
                Spacer(minLength: 0)
            }
        }
    }
}

private struct CompletionNotes: View {
    let notes: String

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            Text("Комментарий мастера")
                .font(.caption2.weight(.semibold))
                .tracking(0.6)
                .textCase(.uppercase)
                .foregroundStyle(AppColor.inkSecondary)

            GlassCard(tint: AppColor.conifer) {
                Text(notes)
                    .appText(.body)
                    .foregroundStyle(AppColor.ink)
                    .frame(maxWidth: .infinity, alignment: .leading)
            }
        }
    }
}

private struct RatedRow: View {
    let rating: Int
    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            Text("Ваша оценка")
                .font(.caption2.weight(.semibold))
                .tracking(0.6)
                .textCase(.uppercase)
                .foregroundStyle(AppColor.inkSecondary)

            GlassCard {
                StarRating(value: rating, size: 24)
            }
        }
    }
}

private struct RateInline: View {
    @Binding var rating: Int
    @Binding var feedback: String
    let isSubmitting: Bool
    let onSubmit: () -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            Text("Оцените работу")
                .font(.caption2.weight(.semibold))
                .tracking(0.6)
                .textCase(.uppercase)
                .foregroundStyle(AppColor.inkSecondary)

            GlassCard {
                VStack(alignment: .leading, spacing: Spacing.s16) {
                    StarRating(value: rating, size: 28) { rating = $0 }

                    AppTextField(
                        icon: nil,
                        placeholder: "Комментарий (необязательно)",
                        text: $feedback
                    )

                    Button("Отправить оценку") { onSubmit() }
                        .buttonStyle(PrimaryButtonStyle(size: .md))
                        .disabled(rating == 0 || isSubmitting)
                }
            }
        }
    }
}
