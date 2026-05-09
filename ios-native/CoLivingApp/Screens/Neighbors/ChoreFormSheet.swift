import SwiftUI

/// Sheet form for adding a chore. Assignee is one of the apartment's
/// roommates (including the creator) or "anyone" — the latter sends
/// `assignedUserId = nil` and any roommate can pick it up.
struct ChoreFormSheet: View {
    let apartment: MyApartmentContextDto
    let store: ChoresStore
    let onDismiss: () -> Void

    @State private var title: String = ""
    @State private var description: String = ""
    @State private var category: ChoreCategory = .vacuum
    @State private var assigneeChoice: AssigneeChoice = .anyone
    @State private var hasDueDate: Bool = false
    @State private var dueDate: Date = Date().addingTimeInterval(60 * 60 * 24)

    enum AssigneeChoice: Hashable {
        case anyone
        case me
        case roommate(String)

        func label(in apartment: MyApartmentContextDto) -> String {
            switch self {
            case .anyone:
                return "Любой сосед"
            case .me:
                return "Я (\(apartment.meName))"
            case .roommate(let userId):
                if let r = apartment.roommates.first(where: { $0.userId == userId }) {
                    return r.name
                }
                return "Сосед"
            }
        }

        func userId(in apartment: MyApartmentContextDto) -> String? {
            switch self {
            case .anyone: return nil
            case .me: return apartment.meUserId
            case .roommate(let id): return id
            }
        }
    }

    private var assigneeOptions: [AssigneeChoice] {
        var out: [AssigneeChoice] = [.anyone, .me]
        for r in apartment.roommates where !r.isMe {
            out.append(.roommate(r.userId))
        }
        return out
    }

    private var canSubmit: Bool {
        !title.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty &&
        !store.isSubmitting
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                ScrollView {
                    VStack(alignment: .leading, spacing: Spacing.s20) {
                        FieldGroup(label: "Что сделать") {
                            AppTextField(
                                icon: nil,
                                placeholder: "Например, «Помыть посуду»",
                                text: $title
                            )
                        }

                        FieldGroup(label: "Категория") {
                            CategoryMenu(selection: $category)
                        }

                        FieldGroup(label: "Кто делает") {
                            AssigneeMenu(
                                options: assigneeOptions,
                                apartment: apartment,
                                selection: $assigneeChoice
                            )
                        }

                        FieldGroup(label: "Срок") {
                            VStack(alignment: .leading, spacing: Spacing.s8) {
                                Toggle("Указать срок", isOn: $hasDueDate)
                                    .tint(AppColor.conifer)
                                if hasDueDate {
                                    DatePicker(
                                        "",
                                        selection: $dueDate,
                                        in: Date()...,
                                        displayedComponents: .date
                                    )
                                    .labelsHidden()
                                    .tint(AppColor.conifer)
                                }
                            }
                        }

                        FieldGroup(label: "Заметка (опционально)") {
                            MultilineField(
                                placeholder: "Уточните детали — где, чем, на что обратить внимание.",
                                text: $description
                            )
                        }

                        if let err = store.errorMessage {
                            Text(err)
                                .appText(.body)
                                .foregroundStyle(AppColor.danger)
                        }

                        Button("Добавить уборку") { submit() }
                            .buttonStyle(PrimaryButtonStyle(size: .md))
                            .disabled(!canSubmit)
                            .padding(.top, Spacing.s8)
                    }
                    .padding(Spacing.s20)
                }
            }
            .navigationTitle("Новая уборка")
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
        let trimmedTitle = title.trimmingCharacters(in: .whitespacesAndNewlines)
        let trimmedDesc = description.trimmingCharacters(in: .whitespacesAndNewlines)
        Task {
            let ok = await store.create(
                title: trimmedTitle,
                description: trimmedDesc.isEmpty ? nil : trimmedDesc,
                category: category,
                assignedUserId: assigneeChoice.userId(in: apartment),
                dueDate: hasDueDate ? dueDate : nil
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

private struct CategoryMenu: View {
    @Binding var selection: ChoreCategory

    var body: some View {
        Menu {
            ForEach(ChoreCategory.allCases) { c in
                Button {
                    selection = c
                } label: {
                    Label(c.label, systemImage: c.icon)
                }
            }
        } label: {
            HStack(spacing: Spacing.s12) {
                Image(systemName: selection.icon)
                    .font(.system(size: 16, weight: .medium))
                    .foregroundStyle(AppColor.conifer)
                    .frame(width: 22)
                Text(selection.label)
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

private struct AssigneeMenu: View {
    let options: [ChoreFormSheet.AssigneeChoice]
    let apartment: MyApartmentContextDto
    @Binding var selection: ChoreFormSheet.AssigneeChoice

    var body: some View {
        Menu {
            ForEach(options, id: \.self) { opt in
                Button {
                    selection = opt
                } label: {
                    Text(opt.label(in: apartment))
                }
            }
        } label: {
            HStack(spacing: Spacing.s12) {
                Image(systemName: "person")
                    .font(.system(size: 16, weight: .medium))
                    .foregroundStyle(AppColor.conifer)
                    .frame(width: 22)
                Text(selection.label(in: apartment))
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

private struct MultilineField: View {
    let placeholder: String
    @Binding var text: String

    @FocusState private var focused: Bool

    var body: some View {
        let shape = RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
        let strokeColor: Color = focused
            ? AppColor.conifer.opacity(0.55)
            : AppColor.glassStroke
        let strokeWidth: CGFloat = focused ? 1.0 : 0.5

        ZStack(alignment: .topLeading) {
            if text.isEmpty {
                Text(placeholder)
                    .appText(.body)
                    .foregroundStyle(AppColor.inkSecondary.opacity(0.6))
                    .padding(.horizontal, Spacing.s16 + 4)
                    .padding(.top, 14)
                    .allowsHitTesting(false)
            }
            TextEditor(text: $text)
                .focused($focused)
                .appText(.body)
                .foregroundStyle(AppColor.ink)
                .tint(AppColor.conifer)
                .scrollContentBackground(.hidden)
                .padding(.horizontal, Spacing.s12)
                .padding(.vertical, 8)
                .frame(minHeight: 100, alignment: .top)
        }
        .glassEffect(.regular, in: shape)
        .overlay { shape.strokeBorder(strokeColor, lineWidth: strokeWidth) }
        .animation(.spring(response: 0.28, dampingFraction: 0.78), value: focused)
    }
}
