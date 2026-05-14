import SwiftUI

/// Чат всего здания. Структурно повторяет apartment-`ChatView`, но кормится
/// от `BuildingChatStore` и получает live-апдейты через SignalR, поэтому без
/// pull-to-refresh цикла «отправил → ничего не вижу пока не дёрну».
///
/// Moderation primitives те же: delete-own / report / block — все три эндпоинта
/// бэкенда scope-agnostic.
struct BuildingChatView: View {
    let buildingId: UUID
    let buildingName: String?
    let currentUserId: String
    let auth: AuthStore
    let realtime: RealtimeService

    @State private var store: BuildingChatStore
    @State private var draft: String = ""
    @State private var reportTarget: ChatMessageDto?
    @State private var reportReason: String = ""
    @State private var blockTarget: ChatMessageDto?

    init(
        buildingId: UUID,
        buildingName: String?,
        currentUserId: String,
        auth: AuthStore,
        realtime: RealtimeService
    ) {
        self.buildingId = buildingId
        self.buildingName = buildingName
        self.currentUserId = currentUserId
        self.auth = auth
        self.realtime = realtime
        _store = State(initialValue: BuildingChatStore(
            buildingId: buildingId,
            currentUserId: currentUserId,
            auth: auth,
            realtime: realtime
        ))
    }

    var body: some View {
        VStack(spacing: 0) {
            content
            inputBar
        }
        .task { await store.bootstrap() }
        .alert(
            "Пожаловаться?",
            isPresented: reportAlertBinding,
            presenting: reportTarget
        ) { message in
            TextField("Что не так? (необязательно)", text: $reportReason)
            Button("Отправить", role: .destructive) {
                let reason = reportReason.trimmingCharacters(in: .whitespacesAndNewlines)
                Task {
                    _ = await store.report(
                        messageId: message.id,
                        reason: reason.isEmpty ? nil : reason
                    )
                    reportReason = ""
                }
            }
            Button("Отмена", role: .cancel) {
                reportReason = ""
            }
        } message: { _ in
            Text("Жалоба отправится модерации. Сообщение пока останется в чате.")
        }
        .alert(
            "Заблокировать?",
            isPresented: blockAlertBinding,
            presenting: blockTarget
        ) { message in
            Button("Заблокировать", role: .destructive) {
                Task { _ = await store.block(userId: message.senderId) }
            }
            Button("Отмена", role: .cancel) { }
        } message: { message in
            Text("Сообщения от \(message.senderName) перестанут показываться в чатах.")
        }
    }

    private var reportAlertBinding: Binding<Bool> {
        Binding(
            get: { reportTarget != nil },
            set: { if !$0 { reportTarget = nil } }
        )
    }

    private var blockAlertBinding: Binding<Bool> {
        Binding(
            get: { blockTarget != nil },
            set: { if !$0 { blockTarget = nil } }
        )
    }

    @ViewBuilder
    private var content: some View {
        if store.isLoading && !store.hasLoaded {
            ProgressView()
                .tint(AppColor.conifer)
                .frame(maxWidth: .infinity, maxHeight: .infinity)
        } else if !store.hasLoaded, let error = store.errorMessage {
            BuildingChatErrorState(message: error) {
                Task { await store.refresh() }
            }
        } else if store.messages.isEmpty {
            emptyState
        } else {
            messageList
        }
    }

    @ViewBuilder
    private var emptyState: some View {
        ScrollView {
            VStack(spacing: Spacing.s16) {
                Image(systemName: "bubble.left.and.bubble.right")
                    .font(.system(size: 36, weight: .light))
                    .foregroundStyle(AppColor.inkSecondary)
                    .padding(.top, Spacing.s32)

                Text("Чат пуст")
                    .appText(.title)
                    .foregroundStyle(AppColor.ink)
                    .multilineTextAlignment(.center)

                Text(emptySubtitle)
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

    private var emptySubtitle: String {
        if let name = buildingName, !name.isEmpty {
            return "Напишите первое сообщение — его увидят все жильцы «\(name)»."
        }
        return "Напишите первое сообщение — его увидят все жильцы здания."
    }

    @ViewBuilder
    private var messageList: some View {
        ScrollViewReader { proxy in
            ScrollView {
                LazyVStack(spacing: Spacing.s8) {
                    ForEach(store.messages) { message in
                        BuildingMessageRow(
                            message: message,
                            isMine: message.senderId == store.currentUserId,
                            onDelete: {
                                Task { _ = await store.deleteOwn(messageId: message.id) }
                            },
                            onReport: { reportTarget = message },
                            onBlock: { blockTarget = message }
                        )
                        .id(message.id)
                    }
                }
                .padding(.horizontal, Spacing.s20)
                .padding(.vertical, Spacing.s12)
            }
            .refreshable { await store.refresh() }
            .onChange(of: store.messages.last?.id) { _, newId in
                guard let newId else { return }
                withAnimation(.easeOut(duration: 0.2)) {
                    proxy.scrollTo(newId, anchor: .bottom)
                }
            }
            .onAppear {
                if let last = store.messages.last?.id {
                    proxy.scrollTo(last, anchor: .bottom)
                }
            }
        }
    }

    @ViewBuilder
    private var inputBar: some View {
        HStack(alignment: .bottom, spacing: Spacing.s8) {
            TextField("Сообщение", text: $draft, axis: .vertical)
                .lineLimit(1...4)
                .textFieldStyle(.plain)
                .appText(.body)
                .foregroundStyle(AppColor.ink)
                .padding(.horizontal, Spacing.s12)
                .padding(.vertical, Spacing.s8)
                .background(
                    RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
                        .fill(AppColor.linen)
                        .overlay(
                            RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
                                .stroke(AppColor.hairline, lineWidth: 1)
                        )
                )

            Button {
                let text = draft
                draft = ""
                Task {
                    let ok = await store.send(text: text)
                    if !ok { draft = text }
                }
            } label: {
                Image(systemName: "arrow.up")
                    .font(.body.weight(.semibold))
                    .foregroundStyle(.white)
                    .frame(width: 36, height: 36)
                    .background(
                        Circle().fill(canSend ? AppColor.conifer : AppColor.inkTertiary)
                    )
            }
            .disabled(!canSend)
        }
        .padding(.horizontal, Spacing.s20)
        .padding(.vertical, Spacing.s12)
        .background(.ultraThinMaterial)
    }

    private var canSend: Bool {
        !store.isSending
            && !draft.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
    }
}

// MARK: - Row

private struct BuildingMessageRow: View {
    let message: ChatMessageDto
    let isMine: Bool
    let onDelete: () -> Void
    let onReport: () -> Void
    let onBlock: () -> Void

    var body: some View {
        HStack {
            if isMine { Spacer(minLength: Spacing.s32) }

            bubble

            if !isMine { Spacer(minLength: Spacing.s32) }
        }
    }

    @ViewBuilder
    private var bubble: some View {
        VStack(alignment: .leading, spacing: 4) {
            if !isMine {
                Text(message.senderName)
                    .appText(.footnote)
                    .foregroundStyle(AppColor.inkSecondary)
            }

            Text(displayText)
                .appText(.body)
                .foregroundStyle(textColor)
                .italic(message.isDeleted)

            Text(timeLabel)
                .appText(.footnote)
                .foregroundStyle(isMine ? Color.white.opacity(0.7) : AppColor.inkSecondary)
        }
        .padding(.horizontal, Spacing.s12)
        .padding(.vertical, Spacing.s8)
        .background(
            RoundedRectangle(cornerRadius: 18, style: .continuous)
                .fill(bubbleFill)
        )
        .contextMenu { menuContent }
    }

    @ViewBuilder
    private var menuContent: some View {
        if message.isDeleted {
            EmptyView()
        } else if isMine {
            Button(role: .destructive, action: onDelete) {
                Label("Удалить", systemImage: "trash")
            }
        } else {
            Button(action: onReport) {
                Label("Пожаловаться", systemImage: "flag")
            }
            Button(role: .destructive, action: onBlock) {
                Label("Заблокировать", systemImage: "hand.raised")
            }
        }
    }

    private var displayText: String {
        message.isDeleted ? "Сообщение удалено" : message.text
    }

    private var textColor: Color {
        if message.isDeleted {
            return isMine ? Color.white.opacity(0.7) : AppColor.inkSecondary
        }
        return isMine ? .white : AppColor.ink
    }

    private var bubbleFill: Color {
        isMine ? AppColor.conifer : AppColor.linen
    }

    private var timeLabel: String {
        let f = DateFormatter()
        f.locale = Locale(identifier: "ru_RU")
        f.dateFormat = "HH:mm"
        return f.string(from: message.sentAt)
    }
}

// MARK: - Error

private struct BuildingChatErrorState: View {
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
