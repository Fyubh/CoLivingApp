import SwiftUI

/// Root of the signed-in state. Five tabs, each owning its own
/// `NavigationStack` so push/pop and scroll position are tab-local.
/// Sign-out lives only on Profile and is threaded down from `AuthFlow`;
/// `auth` is forwarded to tabs that need authed network access (Home for
/// Phase 3, more later).
///
/// `RealtimeService` (SignalR) живёт здесь — стартует один раз при появлении
/// этого view (т.е. после логина) и останавливается при размонтировании
/// (logout сбрасывает AuthFlow в `.signedOut` → RootTabView уходит).
struct RootTabView: View {
    let auth: AuthStore
    var onSignOut: () -> Void

    @State private var realtime = RealtimeService()

    var body: some View {
        TabView {
            HomeView(auth: auth)
                .tabItem { Label("Главная", systemImage: "house") }

            ServicesView(auth: auth)
                .tabItem { Label("Услуги", systemImage: "wrench.and.screwdriver") }

            NeighborsView(auth: auth)
                .tabItem { Label("Соседи", systemImage: "person.2") }

            CommunityView(auth: auth, realtime: realtime)
                .tabItem { Label("Сообщество", systemImage: "bubble.left.and.bubble.right") }

            ProfileView(auth: auth, onSignOut: onSignOut)
                .tabItem { Label("Профиль", systemImage: "person.crop.circle") }
        }
        .tint(AppColor.conifer)
        .preferredColorScheme(.light)
        .onAppear { realtime.start(auth: auth) }
        .onDisappear { realtime.stop() }
    }
}

#Preview {
    RootTabView(auth: AuthStore(), onSignOut: {})
}
