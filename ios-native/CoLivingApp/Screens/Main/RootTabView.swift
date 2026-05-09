import SwiftUI

/// Root of the signed-in state. Five tabs, each owning its own
/// `NavigationStack` so push/pop and scroll position are tab-local.
/// Sign-out lives only on Profile and is threaded down from `AuthFlow`;
/// `auth` is forwarded to tabs that need authed network access (Home for
/// Phase 3, more later).
struct RootTabView: View {
    let auth: AuthStore
    var onSignOut: () -> Void

    var body: some View {
        TabView {
            HomeView(auth: auth)
                .tabItem { Label("Главная", systemImage: "house") }

            ServicesView(auth: auth)
                .tabItem { Label("Услуги", systemImage: "wrench.and.screwdriver") }

            NeighborsView()
                .tabItem { Label("Соседи", systemImage: "person.2") }

            CommunityView()
                .tabItem { Label("Сообщество", systemImage: "bubble.left.and.bubble.right") }

            ProfileView(auth: auth, onSignOut: onSignOut)
                .tabItem { Label("Профиль", systemImage: "person.crop.circle") }
        }
        .tint(AppColor.conifer)
        .preferredColorScheme(.light)
    }
}

#Preview {
    RootTabView(auth: AuthStore(), onSignOut: {})
}
