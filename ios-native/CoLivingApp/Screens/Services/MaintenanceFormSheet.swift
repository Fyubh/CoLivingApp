import SwiftUI

/// Sheet form for creating a maintenance request. Backed by the parent
/// `MaintenanceStore` so submit + refresh fold into one async call. The
/// apartment context drives the location picker — we never let the user
/// type room/building ids by hand.
struct MaintenanceFormSheet: View {
    let context: MyApartmentContextDto
    let store: MaintenanceStore
    let onDismiss: () -> Void

    @State private var category: MaintenanceCategory = .plumbing
    @State private var priority: MaintenancePriority = .normal
    @State private var title: String = ""
    @State private var description: String = ""
    @State private var location: LocationChoice = .myRoom

    enum LocationChoice: Hashable {
        case myRoom
        case apartmentCommon
        case buildingCommon

        func label(in ctx: MyApartmentContextDto) -> String {
            switch self {
            case .myRoom:
                if let room = ctx.rooms.first {
                    return "Моя комната (\(room.number))"
                }
                return "Моя комната"
            case .apartmentCommon:
                return "Общая зона квартиры"
            case .buildingCommon:
                if let bn = ctx.buildingName, !bn.isEmpty {
                    return "Общая зона здания (\(bn))"
                }
                return "Общая зона здания"
            }
        }
    }

    private var availableLocations: [LocationChoice] {
        var out: [LocationChoice] = []
        if !context.rooms.isEmpty { out.append(.myRoom) }
        out.append(.apartmentCommon)
        if context.buildingId != nil { out.append(.buildingCommon) }
        return out
    }

    private var canSubmit: Bool {
        !title.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty &&
        !description.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty &&
        !store.isSubmitting
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                ScrollView {
                    VStack(alignment: .leading, spacing: Spacing.s20) {
                        FieldGroup(label: "Категория") {
                            CategoryPicker(selection: $category)
                        }

                        FieldGroup(label: "Заголовок") {
                            AppTextField(
                                icon: nil,
                                placeholder: "Например, «Не закрывается окно»",
                                text: $title
                            )
                        }

                        FieldGroup(label: "Описание") {
                            MultilineField(
                                placeholder: "Опишите проблему подробнее — что и где не работает.",
                                text: $description
                            )
                        }

                        FieldGroup(label: "Где?") {
                            LocationPicker(
                                options: availableLocations,
                                context: context,
                                selection: $location
                            )
                        }

                        FieldGroup(label: "Приоритет") {
                            Picker("", selection: $priority) {
                                ForEach(MaintenancePriority.allCases) { p in
                                    Text(p.label).tag(p)
                                }
                            }
                            .pickerStyle(.segmented)
                        }

                        if let err = store.errorMessage {
                            Text(err)
                                .appText(.body)
                                .foregroundStyle(AppColor.danger)
                                .multilineTextAlignment(.leading)
                        }

                        Button("Отправить заявку") { submit() }
                            .buttonStyle(PrimaryButtonStyle(size: .md))
                            .disabled(!canSubmit)
                            .padding(.top, Spacing.s8)
                    }
                    .padding(Spacing.s20)
                }
            }
            .navigationTitle("Новая заявка")
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
        let payload = buildPayload()
        Task {
            let ok = await store.create(payload)
            if ok { onDismiss() }
        }
    }

    private func buildPayload() -> CreateMaintenancePayload {
        let roomId: UUID? = location == .myRoom ? context.rooms.first?.id : nil
        let apartmentId: UUID? = location == .apartmentCommon ? context.apartmentId : nil
        let buildingId: UUID? = location == .buildingCommon ? context.buildingId : nil
        return CreateMaintenancePayload(
            category: category,
            title: title.trimmingCharacters(in: .whitespacesAndNewlines),
            description: description.trimmingCharacters(in: .whitespacesAndNewlines),
            priority: priority,
            roomId: roomId,
            apartmentId: apartmentId,
            buildingId: buildingId
        )
    }
}

// MARK: - Pieces

/// A labelled vertical group — small uppercase caption + content. Used in
/// place of a system Form so the visual language stays consistent with the
/// rest of the app (Conifer/Linen, GlassCard inputs).
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

private struct CategoryPicker: View {
    @Binding var selection: MaintenanceCategory

    var body: some View {
        Menu {
            ForEach(MaintenanceCategory.allCases) { c in
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

private struct LocationPicker: View {
    let options: [MaintenanceFormSheet.LocationChoice]
    let context: MyApartmentContextDto
    @Binding var selection: MaintenanceFormSheet.LocationChoice

    var body: some View {
        VStack(spacing: Spacing.s8) {
            ForEach(options, id: \.self) { opt in
                Button {
                    selection = opt
                } label: {
                    HStack(spacing: Spacing.s12) {
                        Image(systemName: selection == opt
                              ? "largecircle.fill.circle"
                              : "circle")
                            .font(.system(size: 18, weight: .regular))
                            .foregroundStyle(selection == opt
                                             ? AppColor.conifer
                                             : AppColor.inkTertiary)

                        Text(opt.label(in: context))
                            .appText(.body)
                            .foregroundStyle(AppColor.ink)

                        Spacer(minLength: 0)
                    }
                    .padding(.horizontal, Spacing.s16)
                    .frame(height: 48)
                    .glassEffect(
                        .regular,
                        in: RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
                    )
                    .overlay(
                        RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
                            .strokeBorder(
                                selection == opt
                                    ? AppColor.conifer.opacity(0.55)
                                    : AppColor.glassStroke,
                                lineWidth: selection == opt ? 1.0 : 0.5
                            )
                    )
                    .contentShape(Rectangle())
                }
                .buttonStyle(.plain)
            }
        }
    }
}

/// Multiline text container styled to match `AppTextField`. SwiftUI's
/// TextEditor doesn't support placeholders natively, so we layer one
/// behind the empty state.
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
                .frame(minHeight: 120, alignment: .top)
        }
        .glassEffect(.regular, in: shape)
        .overlay { shape.strokeBorder(strokeColor, lineWidth: strokeWidth) }
        .animation(.spring(response: 0.28, dampingFraction: 0.78), value: focused)
    }
}
