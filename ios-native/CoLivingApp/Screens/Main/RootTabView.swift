import SwiftUI

/// Root of the signed-in state. Five tabs, each owning its own
/// `NavigationStack` so push/pop and scroll position are tab-local.
/// Sign-out lives only on Profile and is threaded down from `AuthFlow`.
/// Phase 2 wires the structure; tab content is filled phase by phase.
struct RootTabView: View {
    var onSignOut: () -> Void

    var body: some View {
        TabView {
            HomeView()
                .tabItem { Label("Главная", systemImage: "house") }

            ServicesView()
                .tabItem { Label("Услуги", systemImage: "wrench.and.screwdriver") }

            NeighborsView()
                .tabItem { Label("Соседи", systemImage: "person.2") }

            CommunityView()
                .tabItem { Label("Сообщество", systemImage: "bubble.left.and.bubble.right") }

            ProfileView(onSignOut: onSignOut)
                .tabItem { Label("Профиль", systemImage: "person.crop.circle") }
        }
        .tint(AppColor.conifer)
        .preferredColorScheme(.light)
    }
}

#Preview {
    RootTabView(onSignOut: {})
}
