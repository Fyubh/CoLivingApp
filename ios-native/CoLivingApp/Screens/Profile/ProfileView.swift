import SwiftUI

/// Profile tab — identity, apartment summary (solo / shared / not-yet-assigned),
/// support links, and sign-out. Owns its own `ProfileStore` and re-fetches
/// `/Auth/me` + `/Apartments/my-context` independently from HomeStore — the
/// duplicate GET pair is cheaper than threading a shared store across tabs.
struct ProfileView: View {
    @State private var store: ProfileStore
    @Environment(\.openURL) private var openURL

    var onSignOut: () -> Void

    init(auth: AuthStore, onSignOut: @escaping () -> Void) {
        _store = State(initialValue: ProfileStore(auth: auth))
        self.onSignOut = onSignOut
    }

    var body: some View {
        NavigationStack {
            ZStack {
                AppBackground()

                if let me = store.me {
                    LoadedContent(
                        me: me,
                        apartment: store.apartment,
                        hasLoaded: store.hasLoaded,
                        onSupport: openSupport,
                        onPrivacy: openPrivacy,
                        onDelete: openDeleteAccount,
                        onSignOut: onSignOut,
                        onRefresh: { await store.refresh() }
                    )
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
            .navigationTitle("Профиль")
        }
        .task { await store.bootstrap() }
    }

    // MARK: - Link actions

    private func openSupport() {
        guard let url = URL(string: "mailto:\(AppConfig.supportEmail)") else { return }
        openURL(url)
    }

    private func openPrivacy() {
        openURL(AppConfig.privacyPolicyURL)
    }

    /// Account deletion is mailto-driven for MVP — App Store guideline 5.1.1(v)
    /// requires *some* in-app path, not necessarily a self-serve one. The
    /// subject is pre-filled so support can route by template.
    private func openDeleteAccount() {
        let subject = "Удаление аккаунта"
        let encoded = subject.addingPercentEncoding(
            withAllowedCharacters: .urlQueryAllowed
        ) ?? subject
        guard let url = URL(string: "mailto:\(AppConfig.supportEmail)?subject=\(encoded)") else {
            return
        }
        openURL(url)
    }
}

// MARK: - Loaded content

private struct LoadedContent: View {
    let me: MeDto
    let apartment: MyApartmentContextDto?
    let hasLoaded: Bool
    let onSupport: () -> Void
    let onPrivacy: () -> Void
    let onDelete: () -> Void
    let onSignOut: () -> Void
    let onRefresh: () async -> Void

    var body: some View {
        ScrollView {
            VStack(spacing: Spacing.s24) {
                IdentityCard(me: me)

                ApartmentSection(apartment: apartment, hasLoaded: hasLoaded)

                SupportSection(
                    onSupport: onSupport,
                    onPrivacy: onPrivacy,
                    onDelete: onDelete
                )

                Button("Выйти", role: .destructive, action: onSignOut)
                    .buttonStyle(SecondaryButtonStyle(size: .md))
                    .padding(.top, Spacing.s8)
            }
            .padding(.horizontal, Spacing.s20)
            .padding(.top, Spacing.s12)
            .padding(.bottom, Spacing.s32)
        }
        .refreshable { await onRefresh() }
    }
}

// MARK: - Sections

/// Either the same ApartmentSummaryCard Home renders, or a calm inline
/// "not yet assigned" card. Inline (not the full-screen EmptyApartmentState)
/// because the rest of Profile is still useful even without an apartment.
private struct ApartmentSection: View {
    let apartment: MyApartmentContextDto?
    let hasLoaded: Bool

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            SectionHeader("Квартира")

            if let apartment {
                ApartmentSummaryCard(context: apartment)
            } else if hasLoaded {
                GlassCard {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        Text("Вы пока не заселены")
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                        Text("Когда менеджер дома закрепит за вами квартиру, она появится здесь.")
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                    }
                }
            } else {
                // /Auth/me succeeded but apartment-context is still in-flight
                // (rare — they're parallel). Show a tiny placeholder rather
                // than juggling two spinners.
                GlassCard {
                    ProgressView().tint(AppColor.conifer)
                        .frame(maxWidth: .infinity)
                }
            }
        }
    }
}

private struct SupportSection: View {
    let onSupport: () -> Void
    let onPrivacy: () -> Void
    let onDelete: () -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            SectionHeader("Поддержка")

            GlassCard(padding: Spacing.s8) {
                VStack(spacing: 0) {
                    ProfileLinkRow(
                        icon: "envelope",
                        title: "Связаться с поддержкой",
                        subtitle: AppConfig.supportEmail,
                        action: onSupport
                    )
                    Divider().padding(.leading, 36)
                    ProfileLinkRow(
                        icon: "lock.shield",
                        title: "Политика конфиденциальности",
                        action: onPrivacy
                    )
                    Divider().padding(.leading, 36)
                    ProfileLinkRow(
                        icon: "trash",
                        title: "Удалить аккаунт",
                        tint: AppColor.danger,
                        action: onDelete
                    )
                }
            }
        }
    }
}

// MARK: - Error state

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
    ProfileView(auth: AuthStore(), onSignOut: {})
}
